import fs from 'fs-extra';
import { callGeneratorMatchOutput } from './util';
import { genElevenCharId } from './util/genId';
import GenerationStatus from './GenerationStatus';
import { resolveOutputPath } from './config';

type GenerationRequest = {
  settingsString: string;
  seed: string;
};

type QueuedGenerationStatus = {
  id: string;
  requestStatus: GenerationStatus;
};

type GenerationRequestMap = {
  [key: string]: GenerationStatus;
};

const byId: GenerationRequestMap = {};
const fastQueue: string[] = [];
const slowQueue: string[] = [];
let idInProgress: string = null;

function genUniqueId(): string {
  let id;

  while (true) {
    id = genElevenCharId();

    // Check if in queue or if this id has been generated before.
    if (
      !byId[id] &&
      !fs.existsSync(resolveOutputPath('seeds', id, 'input.json'))
    ) {
      return id;
    }
  }
}

function getGenerationProgress(id: string) {
  return {
    obj: byId[id],
    fastQueueLength: fastQueue.length,
    slowQueueLength: slowQueue.length,
  };
}

function makeRequestStatus(
  generationRequest: GenerationRequest
): GenerationStatus {
  if (!generationRequest || !generationRequest.settingsString) {
    return null;
  }

  return new GenerationStatus(
    generationRequest.settingsString,
    generationRequest.seed
  );
}

const numFastItemsPerSlow = 3;
let numFastItemsBeforeNextSlow = numFastItemsPerSlow;

function processQueueItem(isFastQueue: boolean) {
  let idToProcess: string = null;

  if (isFastQueue) {
    idToProcess = fastQueue.shift();
  } else {
    idToProcess = slowQueue.shift();
  }

  if (idToProcess) {
    idInProgress = idToProcess;

    const generationStatus = byId[idToProcess];
    generationStatus.updateProgress('started');

    callGeneratorMatchOutput(
      [
        'generate2',
        'id' + idToProcess,
        generationStatus.settingsString,
        generationStatus.seed,
      ],
      (error, data) => {
        if (error) {
          generationStatus.markError();
        } else {
          generationStatus.markDone();
        }

        processQueueItems();
      }
    );
  }
}

function processQueueItems() {
  let idToProcess: string = null;

  if (numFastItemsBeforeNextSlow > 0 && fastQueue.length > 0) {
    numFastItemsBeforeNextSlow -= 1;
    processQueueItem(true);
    // try to pull from fast queue
    // pull item from fast queue.
  } else if (slowQueue.length > 0) {
    numFastItemsBeforeNextSlow = numFastItemsPerSlow;
    processQueueItem(false);
    // pull item from slowQueue
  } else if (fastQueue.length > 0) {
    numFastItemsBeforeNextSlow = numFastItemsPerSlow - 1;
    processQueueItem(true);
    // If was supposed to pull from slow, but the slow was empty
  } else {
    idInProgress = null;
  }
}

function notifyQueueItemAdded() {
  // If already working on queueItems, don't need to do anything.
  if (idInProgress) {
    return;
  }

  processQueueItems();
}

// add an item to the fast queue
function addToFastQueue(
  generationRequest: GenerationRequest
): QueuedGenerationStatus {
  if (!generationRequest) {
    return null;
  }

  const id = genUniqueId();

  const requestStatus = makeRequestStatus(generationRequest);

  if (!requestStatus) {
    return null;
  }

  byId[id] = requestStatus;
  fastQueue.push(id);

  notifyQueueItemAdded();

  // Check if can process.
  return {
    id,
    requestStatus,
  };
}

// add an item to the slow queue
function addToSlowQueue(generationRequest: GenerationRequest) {
  // this will handle generating the id
}

// move an item from the fast queue to the slow queue
function swapToSlowQueue(id: string) {}

// pass callback to determine which objects to remove from the fast queue
// function filterFastQueue(cb) {}
function filterFastQueue() {}

export {
  getGenerationProgress,
  addToFastQueue,
  addToSlowQueue,
  swapToSlowQueue,
  filterFastQueue,
};

// setInterval(() => {
//   queuesMgr.filterFastQueue((queueItem) => {
//     return true; // true indicates gets to stay
//   });
// }, 1000 * 15);

import fs from 'fs-extra';
import { callGeneratorMatchOutput } from './util';
import { genElevenCharId } from './util/genId';
import GenerationStatus from './GenerationStatus';
import { resolveOutputPath } from './config';

type GenerationRequest = {
  settingsString: string;
  seed: string;
};

type GenerationRequestMap = {
  [key: string]: GenerationStatus;
};

type UserIdToSeedIdMap = {
  [key: string]: string;
};

const seedIdToRequestStatus: GenerationRequestMap = {};
const userIdToSeedId: UserIdToSeedIdMap = {};
const fastQueue: string[] = [];
const slowQueue: string[] = [];
let idInProgress: string = null;

function genUniqueSeedId(): string {
  let id;

  while (true) {
    id = genElevenCharId();

    // Check if in queue or if this id has been generated before.
    if (
      !seedIdToRequestStatus[id] &&
      !fs.existsSync(resolveOutputPath('seeds', id, 'input.json'))
    ) {
      return id;
    }
  }
}

type GenerationProgress = {
  error?: string;
  generationStatus?: GenerationStatus;
  queuePos?: number;
  queueLength?: number;
};

function getGenerationProgress(id: string): GenerationProgress {
  if (!seedIdToRequestStatus[id]) {
    return {
      error: 'No data for that id.',
    };
  }

  return {
    generationStatus: seedIdToRequestStatus[id],
    queuePos: fastQueue.indexOf(id),
    queueLength: fastQueue.length,
  };
}

function makeRequestStatus(
  seedId: string,
  generationRequest: GenerationRequest
): GenerationStatus {
  if (!generationRequest || !generationRequest.settingsString) {
    return null;
  }

  return new GenerationStatus(
    seedId,
    genElevenCharId(),
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

    const generationStatus = seedIdToRequestStatus[idToProcess];
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

type QueuedGenerationStatus = {
  seedId: string;
  requesterHash: string;
  // requestStatus: GenerationStatus;
};

// add an item to the fast queue
function addToFastQueue(
  userId: string,
  generationRequest: GenerationRequest
): QueuedGenerationStatus {
  if (!generationRequest) {
    return null;
  }

  // Check if user already has an item queued for their userId.

  const seedId = genUniqueSeedId();

  const requestStatus = makeRequestStatus(seedId, generationRequest);
  if (!requestStatus) {
    return null;
  }

  seedIdToRequestStatus[seedId] = requestStatus;
  userIdToSeedId[userId] = seedId;
  fastQueue.push(seedId);

  notifyQueueItemAdded();

  // Check if can process.
  return {
    seedId,
    requesterHash: requestStatus.requesterHash,
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

// When we create the seed, we map
// userId => seedId
// seedId => requestStatus
// queue[seedIds....]

// On the server, we update the status as appropriate
// independent of the client.

// After a status item hasn't had its timestamp updated
// for an hour, we will remove it from the maps.

// So for one to be considered in progress,
// meaning we want the requester to make progress calls on it,
// that means it does not exist on disk, and there is a status
// for it.

// The client will only make progress requests on it in the
// case that the requesterHash is present.

// setInterval(() => {
//   queuesMgr.filterFastQueue((queueItem) => {
//     return true; // true indicates gets to stay
//   });
// }, 1000 * 15);

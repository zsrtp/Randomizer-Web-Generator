import fs from 'fs-extra';
import { genElevenCharId } from './util/genId';
const { resolveOutputPath } = require('./config');

type GenerationRequest = {
  settingsString: string;
  seed: string;
};

// The keys are short here because we want to minimize the amount of memory that
// storing a large amount of these uses.
type GenerationStatus = {
  t: number; // timestamp
  p: string; // progress
  d: boolean; // done
  e: string; // error
};

type QueuedGenerationStatus = {
  id: string;
  requestStatus: GenerationStatus;
};

type GenerationRequestMap = {
  [key: string]: GenerationStatus;
};

const byId: GenerationRequestMap = {};
const fastQueue = [];
const slowQueue = [];

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

function getQueueItem(id: string) {
  return byId[id];
}

function makeRequestStatus(): GenerationStatus {
  return {
    t: new Date().getTime(),
    d: null,
    p: 'queued',
    e: null,
  };
}

// add an item to the fast queue
function addToFastQueue(
  generationRequest: GenerationRequest
): QueuedGenerationStatus {
  const id = genUniqueId();

  const requestStatus = makeRequestStatus();
  byId[id] = requestStatus;

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
  getQueueItem,
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

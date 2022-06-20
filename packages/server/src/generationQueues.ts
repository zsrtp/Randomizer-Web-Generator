import fs from 'fs-extra';
import { callGeneratorMatchOutput } from './util';
import { genElevenCharId } from './util/genId';
import SeedGenStatus, { SeedGenProgress } from './SeedGenStatus';
import { resolveOutputPath } from './config';

type GenerationRequest = {
  settingsString: string;
  seed: string;
};

const seedIdToRequestStatus: {
  [key: string]: SeedGenStatus;
} = {};
const userIdToSeedId: {
  [key: string]: string;
} = {};
let queue: string[] = [];
let idInProgress: string | null = null;

function genUniqueSeedId(): string {
  while (true) {
    const id = genElevenCharId();

    // Check if id temporarily reserved or if this id has been generated before.
    if (
      !seedIdToRequestStatus[id] &&
      !fs.existsSync(resolveOutputPath('seeds', id, 'input.json'))
    ) {
      return id;
    }
  }
}

function processQueueItem() {
  const idToProcess: string | undefined = queue.shift();

  if (idToProcess) {
    idInProgress = idToProcess;

    const generationStatus = seedIdToRequestStatus[idToProcess];
    generationStatus.progress = SeedGenProgress.Started;

    callGeneratorMatchOutput(
      [
        'generate2',
        'id' + idToProcess,
        generationStatus.settingsString,
        generationStatus.seed,
      ],
      (error, data) => {
        if (error) {
          generationStatus.progress = SeedGenProgress.Error;
        } else {
          generationStatus.progress = SeedGenProgress.Done;
          // TODO: Instead of marking as done, just remove from status object entirely.
        }

        processQueueItems();
      }
    );
  }
}

function processQueueItems() {
  // filter list
  const currentTime = new Date().getTime();

  queue = queue.filter((seedId) => {
    const seedGenStatus = seedIdToRequestStatus[seedId];
    if (!seedGenStatus) {
      return false;
    }

    // Temp setting to 3 seconds
    if (currentTime - seedGenStatus.lastRefreshed > 1000) {
      seedGenStatus.progress = SeedGenProgress.Abandoned;
      return false;
    }

    return true;
  });

  if (queue.length > 0) {
    processQueueItem();
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
  userId: string,
  generationRequest: GenerationRequest
): {
  seedId: string;
  requesterHash: string;
} | null {
  // TODO: Check if user already has an item queued for their userId.

  if (!generationRequest || !generationRequest.settingsString) {
    return null;
  }

  const seedId = genUniqueSeedId();

  const requestStatus = new SeedGenStatus(
    seedId,
    genElevenCharId(),
    generationRequest.settingsString,
    generationRequest.seed
  );

  seedIdToRequestStatus[seedId] = requestStatus;
  userIdToSeedId[userId] = seedId;
  queue.push(seedId);

  notifyQueueItemAdded();

  return {
    seedId,
    requesterHash: requestStatus.requesterHash,
  };
}

function checkProgress(id: string): {
  error?: string;
  seedGenStatus?: SeedGenStatus;
  queuePos?: number;
} {
  if (
    !seedIdToRequestStatus[id] &&
    !fs.existsSync(resolveOutputPath('seeds', id, 'input.json'))
  ) {
    return {
      error: 'No data for that id.',
    };
  }

  const seedGenStatus = seedIdToRequestStatus[id];
  seedGenStatus.updateRefreshTime();

  if (seedGenStatus.progress === SeedGenProgress.Abandoned) {
    seedGenStatus.progress = SeedGenProgress.Queued;
    queue.push(id);
  }

  return {
    seedGenStatus,
    queuePos: queue.indexOf(id),
  };
}

export { addToFastQueue, checkProgress };

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

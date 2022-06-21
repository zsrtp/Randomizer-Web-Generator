import fs from 'fs-extra';
import { callGeneratorMatchOutput } from './util';
import { genElevenCharId } from './util/genId';
import SeedGenStatus, { SeedGenProgress } from './SeedGenStatus';
import { resolveOutputPath } from './config';
import * as objects from './util/object';

type GenerationRequest = {
  settingsString: string;
  seed: string;
};

type SeedIdToSeedGenStatusType = {
  [key: string]: SeedGenStatus;
};

type UserIdToSeedIdType = {
  [key: string]: string;
};

let seedIdToSeedGenStatus: SeedIdToSeedGenStatusType = {};
let userIdToSeedId: UserIdToSeedIdType = {};
let queue: string[] = [];
let idInProgress: string | null = null;

function genUniqueSeedId(): string {
  while (true) {
    const id = genElevenCharId();

    // Check if id temporarily reserved or if this id has been generated before.
    if (
      !seedIdToSeedGenStatus[id] &&
      !fs.existsSync(resolveOutputPath('seeds', id, 'input.json'))
    ) {
      return id;
    }
  }
}

function handleSeedGenStatusError(seedGenStatus: SeedGenStatus) {
  if (!seedGenStatus) {
    return;
  }

  seedGenStatus.progress = SeedGenProgress.Error;

  userIdToSeedId = objects.filterKeys(userIdToSeedId, [
    seedGenStatus.userId,
  ]) as UserIdToSeedIdType;
}

function handleSeedGenStatusDone(seedGenStatus: SeedGenStatus) {
  if (!seedGenStatus) {
    return;
  }

  seedIdToSeedGenStatus = objects.filterKeys(seedIdToSeedGenStatus, [
    seedGenStatus.seedId,
  ]) as SeedIdToSeedGenStatusType;
  userIdToSeedId = objects.filterKeys(userIdToSeedId, [
    seedGenStatus.userId,
  ]) as UserIdToSeedIdType;
}

function processQueueItem() {
  const idToProcess: string | undefined = queue.shift();

  if (idToProcess) {
    idInProgress = idToProcess;

    const seedGenStatus = seedIdToSeedGenStatus[idToProcess];
    seedGenStatus.progress = SeedGenProgress.Started;

    callGeneratorMatchOutput(
      [
        'generate2',
        'id' + idToProcess,
        seedGenStatus.settingsString,
        seedGenStatus.seed,
      ],
      (error, data) => {
        if (error) {
          handleSeedGenStatusError(seedGenStatus);
        } else {
          handleSeedGenStatusDone(seedGenStatus);
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
    const seedGenStatus = seedIdToSeedGenStatus[seedId];
    if (!seedGenStatus) {
      return false;
    }

    // TODO: Temp setting to 10 seconds
    if (currentTime - seedGenStatus.lastRefreshed > 10000) {
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
):
  | {
      seedId: string;
      requesterHash: string;
    }
  | string {
  // if (userIdToSeedId[userId]) {
  if (
    Object.keys(seedIdToSeedGenStatus).length > 10 &&
    userIdToSeedId[userId]
  ) {
    return userIdToSeedId[userId];
  }

  const seedId = genUniqueSeedId();

  const requestStatus = new SeedGenStatus(
    seedId,
    userId,
    genElevenCharId(),
    generationRequest.settingsString,
    generationRequest.seed
  );

  seedIdToSeedGenStatus[seedId] = requestStatus;
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
  seedGenProgress?: SeedGenProgress;
  seedGenStatus?: SeedGenStatus;
  queuePos?: number;
} {
  if (!seedIdToSeedGenStatus[id]) {
    if (fs.existsSync(resolveOutputPath('seeds', id, 'input.json'))) {
      return {
        seedGenProgress: SeedGenProgress.Done,
        queuePos: -1,
      };
    } else {
      return { error: 'No data for that id.' };
    }
  }

  const seedGenStatus = seedIdToSeedGenStatus[id];

  if (seedGenStatus.progress === SeedGenProgress.Abandoned) {
    seedGenStatus.progress = SeedGenProgress.Queued;
    queue.push(id);
    notifyQueueItemAdded();
  } else if (seedGenStatus.progress === SeedGenProgress.Error) {
    seedIdToSeedGenStatus = objects.filterKeys(seedIdToSeedGenStatus, [
      seedGenStatus.seedId,
    ]) as SeedIdToSeedGenStatusType;
  }

  return {
    seedGenProgress: seedGenStatus.progress,
    seedGenStatus: seedGenStatus,
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

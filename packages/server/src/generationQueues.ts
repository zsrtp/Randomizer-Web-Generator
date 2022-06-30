import fs from 'fs-extra';
import { callGeneratorMatchOutput } from './util';
import { genElevenCharId } from './util/genId';
import SeedGenStatus, { SeedGenProgress } from './SeedGenStatus';
import { resolveOutputPath } from './config';
import * as objects from './util/object';
import logger from './logger/logger';

const hangingRequestDataFilterInterval = 1000 * 60 * 5; // 5 minutes
const seedAbandonedTimeout = 1000 * 60; // 1 minute
const seedForeverAbandonedTimeout = 1000 * 60 * 15; // 15 minutes

type GenerationRequest = {
  settingsString: string;
  seed: string;
};

type SeedIdToSeedGenStatus = {
  [key: string]: SeedGenStatus;
};

type UserIdToSeedId = {
  [key: string]: string;
};

let seedIdToSeedGenStatus: SeedIdToSeedGenStatus = {};
let userIdToSeedId: UserIdToSeedId = {};
let queue: string[] = [];
let idInProgress: string | null = null;
let filterHangingIntervalId: NodeJS.Timer | undefined;

function tryStartHangingRequestCleaner(): void {
  if (filterHangingIntervalId != null) {
    return;
  }
  logger.debug('Starting hangingRequestCleaner...');
  filterHangingIntervalId = setInterval(
    filterHangingRequestData,
    hangingRequestDataFilterInterval
  );
}

/**
 * This filters out any Error and Abandoned seedGenReqeuests which haven't been
 * refreshed for at least 15 minutes.
 */
function filterHangingRequestData() {
  const now = new Date().getTime();
  const foreverAbandonedSeedIds: string[] = [];
  const maxFullyAbandonable = Object.values(seedIdToSeedGenStatus).filter(
    (seedGenStatus) => seedGenStatus.isHanging()
  ).length;

  if (maxFullyAbandonable < 1) {
    logger.debug(
      'Stopping hangingRequestCleaner because there are 0 hanging requests.'
    );
    clearInterval(filterHangingIntervalId);
    filterHangingIntervalId = undefined;
    return;
  }

  seedIdToSeedGenStatus = objects.filter(
    seedIdToSeedGenStatus,
    (seedId: string, seedGenStatus: SeedGenStatus) => {
      if (
        seedGenStatus.isHanging() &&
        now > seedGenStatus.lastRefreshed + seedForeverAbandonedTimeout
      ) {
        // Filter out Abandoned requests which will likely never be checked on
        // again.
        foreverAbandonedSeedIds.push(seedGenStatus.seedId);
        return false;
      }
      return true;
    }
  ) as SeedIdToSeedGenStatus;

  let logMsg = `Ran filterHangingRequestData. Filtered ${foreverAbandonedSeedIds.length} of ${maxFullyAbandonable}.`;
  if (foreverAbandonedSeedIds.length > 0) {
    logMsg += ` SeedIds: ${foreverAbandonedSeedIds.join(',')}`;
  }
  logger.debug(logMsg);

  // Filter out any userIdToSeedId entries which no longer point to a valid
  // SeedGenStatus.
  userIdToSeedId = objects.filter(
    userIdToSeedId,
    (userId: string, seedId: string) => Boolean(seedIdToSeedGenStatus[seedId])
  ) as UserIdToSeedId;
}

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

  logger.error(
    `Error occurred while generating seed with seedId: ${seedGenStatus.seedId}`
  );

  seedGenStatus.progress = SeedGenProgress.Error;

  userIdToSeedId = objects.filterKeys(userIdToSeedId, [
    seedGenStatus.userId,
  ]) as UserIdToSeedId;

  tryStartHangingRequestCleaner();
}

function handleSeedGenStatusDone(seedGenStatus: SeedGenStatus) {
  if (!seedGenStatus) {
    return;
  }

  logger.debug(
    `Successfully generated seed with seedId: ${seedGenStatus.seedId}`
  );

  seedIdToSeedGenStatus = objects.filterKeys(seedIdToSeedGenStatus, [
    seedGenStatus.seedId,
  ]) as SeedIdToSeedGenStatus;
  userIdToSeedId = objects.filterKeys(userIdToSeedId, [
    seedGenStatus.userId,
  ]) as UserIdToSeedId;
}

function processQueueItem() {
  const idToProcess: string | undefined = queue.shift();

  if (idToProcess) {
    idInProgress = idToProcess;

    const seedGenStatus = seedIdToSeedGenStatus[idToProcess];
    seedGenStatus.progress = SeedGenProgress.Started;

    logger.debug(
      `Generating seed with seedId: ${idToProcess} , settingsString: ${seedGenStatus.settingsString} , seed: ${seedGenStatus.seed}`
    );

    callGeneratorMatchOutput(
      [
        'generate2',
        'id' + idToProcess,
        seedGenStatus.settingsString,
        seedGenStatus.seed,
      ],
      (error, data) => {
        // TODO: temp setting a timeout to make easier to test
        setTimeout(() => {
          if (error) {
            handleSeedGenStatusError(seedGenStatus);
          } else {
            handleSeedGenStatusDone(seedGenStatus);
          }

          processQueueItems();
        }, 10000);
      }
    );
  }
}

function processQueueItems() {
  // filter list
  const currentTime = new Date().getTime();

  const seedIdsMarkedAsAbandoned: string[] = [];

  queue = queue.filter((seedId) => {
    const seedGenStatus = seedIdToSeedGenStatus[seedId];
    if (!seedGenStatus) {
      return false;
    }

    // TODO: Temp setting to 60 seconds
    if (currentTime - seedGenStatus.lastRefreshed > seedAbandonedTimeout) {
      // Mark a seedGenStatus as Abandoned if no one has checked on it for more
      // than 60 seconds.
      seedGenStatus.progress = SeedGenProgress.Abandoned;
      seedIdsMarkedAsAbandoned.push(seedGenStatus.seedId);
      return false;
    }

    return true;
  });

  if (seedIdsMarkedAsAbandoned.length > 0) {
    logger.debug(
      `Marked ${
        seedIdsMarkedAsAbandoned.length
      } seeds as Abandoned with seedIds: ${seedIdsMarkedAsAbandoned.join(',')}`
    );

    tryStartHangingRequestCleaner();
  }

  if (queue.length > 0) {
    logger.debug(`Queue length: ${queue.length}. Processing next item...`);
    processQueueItem();
  } else {
    logger.debug('Queue length: 0. No items to process.');
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
  // TODO: temp code for testing. Allow same user to make multiple requests at
  // the same time.

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

  logger.debug(
    `Queued seed request with seedId: ${seedId}. Queue length is ${queue.length}`
  );

  notifyQueueItemAdded();

  return {
    seedId,
    requesterHash: requestStatus.requesterHash,
  };
}

function checkProgress(
  id: string,
  canCauseRequeue?: boolean
): {
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
    if (canCauseRequeue) {
      logger.debug(`Requeuing request with seedId: ${seedGenStatus.seedId}`);
      seedGenStatus.progress = SeedGenProgress.Queued;
      queue.push(id);
      notifyQueueItemAdded();
    }
  } else if (seedGenStatus.progress === SeedGenProgress.Error) {
    seedIdToSeedGenStatus = objects.filterKeys(seedIdToSeedGenStatus, [
      seedGenStatus.seedId,
    ]) as SeedIdToSeedGenStatus;
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

import fs from 'fs-extra';
import { ExecFileException } from 'child_process';
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

function handleSeedGenStatusError(
  error: ExecFileException | string | null,
  seedGenStatus: SeedGenStatus
) {
  if (!seedGenStatus) {
    return;
  }

  logger.error(
    `Error occurred while generating seed with seedId: ${seedGenStatus.seedId}`
  );
  logger.error(error);

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
        // setTimeout(() => {
        if (error) {
          handleSeedGenStatusError(error, seedGenStatus);
        } else {
          handleSeedGenStatusDone(seedGenStatus);
        }

        processQueueItems();
        // }, 10000);
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

type QueueRequestResult = {
  success: boolean;
  // If success is true, seedId is the new seedId for the queued request.
  // Else seedId references the existing request for this user which blocks the
  // new request.
  seedId: string;
  // If success is true, requesterHash is a string which the client will
  // use to determine whether or not to make progress API calls.
  requesterHash?: string;
  // If success is false, canCancel indicates to the client that a `cancel`
  // API call with the same userId and requesterHash will succeed or fail.
  canCancel?: boolean;
};

// add an item to the fast queue
function addToFastQueue(
  userId: string,
  generationRequest: GenerationRequest,
  requesterHash: string | null | undefined
): QueueRequestResult {
  // TODO: temp code for testing. Allow same user to make multiple requests at
  // the same time.
  // if (userIdToSeedId[userId]) {
  if (
    Object.keys(seedIdToSeedGenStatus).length > 10 &&
    userIdToSeedId[userId]
  ) {
    const seedGenStatus = seedIdToSeedGenStatus[userIdToSeedId[userId]];

    if (seedGenStatus) {
      const canCancel = Boolean(
        userId &&
          requesterHash &&
          seedGenStatus.userId === userId &&
          seedGenStatus.requesterHash === requesterHash
      );

      return {
        success: false,
        seedId: seedGenStatus.seedId,
        canCancel,
      };
    } else {
      // If userId points to an invalid seedId for some reason, just filter out
      // the userId and pretend there is no issue.
      userIdToSeedId = objects.filterKeys(userIdToSeedId, [
        userId,
      ]) as UserIdToSeedId;
    }
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
    success: true,
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

export const enum CancelRequestResult {
  NotFound = 'NotFound',
  Unauthorized = 'Unauthorized',
  Success = 'Success',
}

function cancelRequest(
  seedId: string,
  userId: string,
  requesterHash: string
): CancelRequestResult {
  const seedGenStatus = seedIdToSeedGenStatus[seedId];

  if (!seedGenStatus) {
    logger.debug(
      `User ${userId} tried to cancel a request with seedId ${seedId} which was not found.`
    );
    return CancelRequestResult.NotFound;
  }

  if (
    userId &&
    requesterHash &&
    seedGenStatus.userId === userId &&
    seedGenStatus.requesterHash === requesterHash
  ) {
    // Filter out userId so that user is able to create a new request.
    userIdToSeedId = objects.filterKeys(userIdToSeedId, [
      seedGenStatus.userId,
    ]) as UserIdToSeedId;

    // If in the queue, remove from the queue.
    const index = queue.indexOf(seedId);
    if (index >= 0) {
      const newQueue = new Array(queue.length - 1);
      for (let i = 0; i < queue.length; i++) {
        if (i !== index) {
          newQueue.push(queue[i]);
        }
      }
      queue = newQueue;

      // Filter out of seedIdMap so that it doesn't hang around forever.
      seedIdToSeedGenStatus = objects.filterKeys(seedIdToSeedGenStatus, [
        seedId,
      ]) as SeedIdToSeedGenStatus;

      logger.debug(
        `User ${userId} canceled request with seedId ${seedId} at queue pos ${index}.`
      );
    } else {
      logger.debug(
        `User ${userId} canceled a request with seedId ${seedId} not in the queue with progress: ${seedGenStatus.progress}.`
      );
    }

    return CancelRequestResult.Success;
  } else {
    logger.debug(
      `User ${userId} tried to cancel a request with seedId ${seedId}, but failed authorization.`
    );
    return CancelRequestResult.NotFound;
  }

  // Only allow the user to cancel the request if they provide the correct
  // userId and requesterHash
  //
  // Possible responses to the caller:
  // - call failed because bad auth (first priority, so as to not give away any
  //   information about whether or not the seedId is/was a valid target for
  //   cancellation) ---- The below assume auth was passed:
  //
  // - Always returns success assuming auth passed. Even if the seed request
  //   from the same user was currently being generated, only a single user at a
  //   time can cause any weirdness here, and even then there is no real
  //   problem.
  //
  // Canceling the request will filter out that user's userId from the userId
  // map. Filtering out their userId is enough to allow them to make another
  // request.
  //
  // We still want to prevent unnecessary work, so if the seedId is in the
  // queue, we should filter it out of the queue and remove it from the
  // seedIdToSeedGenRequest map as well. If it is not in the queue, we will
  // leave it alone because the server might want to mark its status as Error.
}

export { addToFastQueue, checkProgress, cancelRequest };

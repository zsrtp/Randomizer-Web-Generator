import express from 'express';
import {
  cancelRequest,
  checkProgress,
  CancelRequestResult,
} from 'src/generationQueues';
import { SeedGenProgress } from 'src/SeedGenStatus';

enum SeedCancelError {
  NotFound = 'NotFound',
  GenerationError = 'GenerationError',
}

function apiSeedCancel(req: express.Request, res: express.Response) {
  const { userId } = req;

  if (!userId) {
    res.status(403).send({ error: 'Forbidden' });
    return;
  }

  const { seedId, requesterHash } = req.body;

  if (!requesterHash || !seedId || seedId.indexOf('/') > 0) {
    // Can update this to be more robust, but slash check is meant to prevent
    // possibility of client trying to check for files on the server machine.
    res.status(400).send({ error: 'Malformed request.' });
    return;
  }

  const result = cancelRequest(seedId, userId, requesterHash);

  if (result === CancelRequestResult.Unauthorized) {
    res.status(403).send({ error: 'Forbidden' });
  } else {
    // NotFound is not considered since the client doesn't really care either
    // way.
    res.send({ data: result });
  }
}

export default apiSeedCancel;

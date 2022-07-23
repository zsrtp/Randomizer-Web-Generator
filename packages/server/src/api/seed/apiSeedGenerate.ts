import express from 'express';
import { addToFastQueue } from 'src/generationQueues';
import { normalizeStringToMax128Bytes } from 'src/util/string';

function apiSeedGenerate(req: express.Request, res: express.Response) {
  const { userId } = req;
  const { settingsString, seed, isRaceSeed, requesterHash } = req.body;

  if (!userId) {
    res.status(403).send({ error: { message: 'Forbidden' } });
    return;
  }

  if (!settingsString || typeof settingsString !== 'string') {
    res.status(400).send({ error: { message: 'Malformed request.' } });
    return;
  }

  if (seed && typeof seed !== 'string') {
    res.status(400).send({ error: { message: 'Malformed request.' } });
    return;
  }

  if (typeof isRaceSeed !== 'boolean') {
    res.status(400).send({ error: { message: 'Malformed request.' } });
    return;
  }

  const seedStr = seed ? normalizeStringToMax128Bytes(seed) : '';

  const {
    success,
    seedId,
    requesterHash: newRequesterHash,
    canCancel,
  } = addToFastQueue(
    userId,
    {
      settingsString,
      seed: seedStr,
      isRaceSeed,
    },
    req.body.requesterHash
  );

  if (success) {
    res.send({
      data: {
        seedId,
        requesterHash: newRequesterHash,
      },
    });
  } else {
    res.send({
      error: {
        seedId,
        canCancel,
      },
    });
  }
}

export default apiSeedGenerate;

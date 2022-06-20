import express from 'express';
import { addToFastQueue } from 'src/generationQueues';
import { callGeneratorMatchOutput } from 'src/util';
import { normalizeStringToMax128Bytes } from 'src/util/string';

function apiSeedGenerate(req: express.Request, res: express.Response) {
  const { userId } = req;
  const { settingsString, seed } = req.body;

  if (!userId) {
    res.status(403).send({ error: 'Forbidden' });
    return;
  }

  if (!settingsString || typeof settingsString !== 'string') {
    res.status(400).send({ error: 'Malformed request.' });
    return;
  }

  if (seed && typeof seed !== 'string') {
    res.status(400).send({ error: 'Malformed request.' });
    return;
  }

  const seedStr = seed ? normalizeStringToMax128Bytes(seed) : '';

  const result = addToFastQueue(userId, {
    settingsString,
    seed: seedStr,
  });

  if (typeof result === 'string') {
    res.send({
      data: {
        ongoingSeedId: result,
      },
    });
  } else {
    res.send({
      data: result,
    });
  }
}

export default apiSeedGenerate;

import express from 'express';
import { addToFastQueue } from 'src/generationQueues';
import { callGeneratorMatchOutput } from 'src/util';
import { normalizeStringToMax128Bytes } from 'src/util/string';

function apiSeedGenerate(req: express.Request, res: express.Response) {
  const { settingsString, seed } = req.body;

  if (!settingsString || typeof settingsString !== 'string') {
    res.status(400).send({ error: 'Malformed request.' });
    return;
  }

  if (seed && typeof seed !== 'string') {
    res.status(400).send({ error: 'Malformed request.' });
    return;
  }

  const seedStr = seed ? normalizeStringToMax128Bytes(seed) : '';
  console.log(`seedStr: '${seedStr}'`);

  // Need to
  const queuedGenerationStatus = addToFastQueue({
    settingsString,
    seed,
  });

  console.log(77);

  // Want to say the following:
  // Send error as main obj if there was an error.

  // For data:
  // id of request.
  // progress status (queued)
  // how many items are in the fast and slow queues

  res.send({
    data: {
      id: queuedGenerationStatus.id,
      progress: queuedGenerationStatus.requestStatus.progress,
    },
  });

  // callGeneratorMatchOutput(
  //   ['generate2', settingsString, seedStr],
  //   (error, data) => {
  //     if (error) {
  //       res.status(500).send({ error });
  //     } else {
  //       res.send({ data: { id: data } });
  //     }
  //   }
  // );
}

export default apiSeedGenerate;

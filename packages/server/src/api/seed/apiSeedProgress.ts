import express from 'express';
import { getGenerationProgress } from 'src/generationQueues';

function apiSeedProgress(req: express.Request, res: express.Response) {
  const { userId } = req;
  console.log(`USER ID IS: ${userId}`);

  const { id } = req.params;

  if (!id) {
    res.status(400).send({ error: 'Malformed request.' });
    return;
  }

  const item = getGenerationProgress(id);

  console.log(88);

  // Want to say the following:
  // Send error as main obj if there was an error.

  // For data:
  // id of request.
  // progress status (queued)
  // how many items are in the fast and slow queues

  res.send({
    data: item,
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

export default apiSeedProgress;

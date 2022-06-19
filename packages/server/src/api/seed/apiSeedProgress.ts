import express from 'express';
import { getGenerationProgress } from 'src/generationQueues';

function apiSeedProgress(req: express.Request, res: express.Response) {
  const { userId } = req;
  console.log(`USER ID IS: ${userId}`);

  const { id } = req.params;

  if (!id || id.indexOf('/') > 0) {
    // Can update this to be more robust, but slash check is meant to prevent
    // possibility of client trying to check for files on the server machine.
    res.status(400).send({ error: 'Malformed request.' });
    return;
  }

  const { error, generationStatus, queuePos, queueLength } =
    getGenerationProgress(id);

  if (error) {
    return res.send({
      error: {
        errors: [
          {
            reason: 'not_found',
            message: `Unable to find generation status for id: ${id}`,
          },
        ],
      },
    });
  } else if (generationStatus.error) {
    return res.send({
      error: {
        errors: [
          {
            reason: 'generation_error',
            message: 'Generation failed.',
          },
        ],
      },
    });
  }

  res.send({
    data: {
      done: generationStatus.done,
      progress: generationStatus.progress,
      queuePos,
      queueLength,
    },
  });
}

export default apiSeedProgress;

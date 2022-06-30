import express from 'express';
import { checkProgress } from 'src/generationQueues';
import { SeedGenProgress } from 'src/SeedGenStatus';

enum SeedProgressError {
  NotFound = 'NotFound',
  GenerationError = 'GenerationError',
}

function apiSeedProgress(req: express.Request, res: express.Response) {
  const { id } = req.params;

  if (!id || id.indexOf('/') > 0) {
    // Can update this to be more robust, but slash check is meant to prevent
    // possibility of client trying to check for files on the server machine.
    res.status(400).send({ error: 'Malformed request.' });
    return;
  }

  const { error, seedGenStatus, seedGenProgress, queuePos } = checkProgress(
    id,
    true
  );

  if (error) {
    return res.send({
      error: {
        errors: [
          {
            reason: SeedProgressError.NotFound,
            message: `Unable to find generation status for id: ${id}`,
          },
        ],
      },
    });
  } else if (seedGenProgress === SeedGenProgress.Error) {
    return res.send({
      error: {
        errors: [
          {
            reason: SeedProgressError.GenerationError,
            message: 'Generation failed.',
          },
        ],
      },
    });
  }

  if (seedGenStatus) {
    seedGenStatus.updateRefreshTime();
  }

  res.send({
    data: {
      progress: seedGenProgress,
      queuePos,
    },
  });
}

export default apiSeedProgress;

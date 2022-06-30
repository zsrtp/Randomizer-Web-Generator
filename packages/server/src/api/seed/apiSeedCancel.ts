import express from 'express';
import { checkProgress } from 'src/generationQueues';
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
            reason: SeedCancelError.NotFound,
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
            reason: SeedCancelError.GenerationError,
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

export default apiSeedCancel;

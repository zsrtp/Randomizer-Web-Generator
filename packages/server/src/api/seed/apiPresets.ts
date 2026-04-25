import express from 'express';
import {
  cancelRequest,
  checkProgress,
  CancelRequestResult,
} from 'src/generationQueues';
import { SeedGenProgress } from 'src/SeedGenStatus';
import { PRESETS } from './presets';

export function apiPresets(req: express.Request, res: express.Response) {
  const { apiToken } = req;

  if (!apiToken) {
    res.status(403).send({ error: 'Forbidden' });
    return;
  }

  res.send({ data: PRESETS });
}

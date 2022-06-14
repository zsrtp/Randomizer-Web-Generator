const fs = require('fs-extra');
import path from 'path';
import searchUpFileTree from './util/searchUpFileTree';

let rootPath: string;
let outputPath: string;

function initConfig() {
  const outputVolumePath: string = process.env.OUTPUT_VOLUME_PATH;
  if (!outputVolumePath) {
    throw new Error('Did not find `OUTPUT_VOLUME_PATH` in process.env.');
  }

  searchUpFileTree(__dirname, (currPath) => {
    const outputConfigPath = path.join(currPath, '.env');
    if (fs.existsSync(outputConfigPath)) {
      rootPath = currPath;
      outputPath = path.resolve(currPath, outputVolumePath);
      return true;
    }
    return false;
  });

  if (!rootPath) {
    throw new Error('Failed to determine paths.');
  }

  fs.mkdirp(outputPath);
}

path.resolve();

function resolveRootPath(...args: string[]): string {
  return path.resolve(rootPath, ...args);
}

function resolveOutputPath(...args: string[]): string {
  return path.resolve(outputPath, ...args);
}

module.exports = {
  initConfig,
  resolveRootPath,
  resolveOutputPath,
};

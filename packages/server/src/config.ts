import fs from 'fs-extra';
import path from 'path';
import searchUpFileTree from './util/searchUpFileTree';

let rootPath: string;
let outputPath: string;
let generatorExePath: string;

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

  let exePath = process.env.TPR_GENERATOR_EXE_PATH;
  if (process.platform === 'win32') {
    // This does not mean 32-bit Windows only. Should work for all of them.
    exePath += '.exe';
  }

  generatorExePath = resolveRootPath(exePath);
}

function resolveRootPath(...str: string[]): string {
  return path.resolve(rootPath, ...str);
}

function resolveOutputPath(...str: string[]): string {
  return path.resolve(outputPath, ...str);
}

export { initConfig, resolveRootPath, resolveOutputPath, generatorExePath };

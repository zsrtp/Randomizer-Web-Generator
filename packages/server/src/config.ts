import fs from 'fs-extra';
import path from 'path';
import searchUpFileTree from './util/searchUpFileTree';

let rootPath: string;
let outputPath: string;
let generatorExePath: string;

function initConfig() {
  const outputVolumePath = process.env.TPRGEN_VOLUME_ROOT;
  if (!outputVolumePath) {
    throw new Error('Did not find `TPRGEN_VOLUME_ROOT` in process.env.');
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

  let exePath = process.env.TPRGEN_GENERATOR_EXE;
  if (!exePath) {
    throw new Error('Did not find `TPRGEN_GENERATOR_EXE` in process.env.');
  }

  if (process.platform === 'win32') {
    // This does not mean 32-bit Windows only. Should work for all of them.
    exePath += '.exe';
  }

  generatorExePath = resolveRootPath(exePath);
}

function logConfig(logFn: Function) {
  const varNames = [
    'IMAGE_VERSION',
    'GIT_COMMIT',
    'TPRGEN_VOLUME_ROOT',
    'TPRGEN_GENERATOR_ROOT',
    'TPRGEN_GENERATOR_EXE',
  ];

  logFn('=====Environment Variables=====');
  varNames.forEach((varName) => {
    logFn(`${varName}=${process.env[varName]}`);
  });

  logFn('=====Resolved Paths=====');
  logFn(`rootPath=${rootPath}`);
  logFn(`outputPath=${outputPath}`);
  logFn(`generatorExePath=${generatorExePath}`);
}

function resolveRootPath(...str: string[]): string {
  return path.resolve(rootPath, ...str);
}

function resolveOutputPath(...str: string[]): string {
  return path.resolve(outputPath, ...str);
}

export {
  initConfig,
  logConfig,
  resolveRootPath,
  resolveOutputPath,
  generatorExePath,
};

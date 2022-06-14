const fs = require('fs-extra');
const path = require('path');
const searchUpFileTree = require('./util/searchUpFileTree');

let rootPath;
let outputPath;

function initConfig() {
  searchUpFileTree(__dirname, (currPath) => {
    const outputConfigPath = path.join(currPath, '.env');
    if (fs.existsSync(outputConfigPath)) {
      rootPath = currPath;
      outputPath = path.resolve(currPath, process.env.OUTPUT_VOLUME_PATH);
    }
  });

  if (!rootPath) {
    throw new Error('Failed to determine paths.');
  }

  fs.mkdirp(outputPath);
}

function resolveRootPath(...args) {
  return path.resolve(rootPath, ...args);
}

function resolveOutputPath(...args) {
  return path.resolve(outputPath, ...args);
}

module.exports = {
  initConfig,
  resolveRootPath,
  resolveOutputPath,
};

const fs = require('fs');
const path = require('path');
const searchUpFileTree = require('./searchUpFileTree');

let rootDir = null;

function findRootDir() {
  if (rootDir) {
    return rootDir;
  }

  return searchUpFileTree(__dirname, (currPath) => {
    const outputConfigPath = path.join(currPath, '.env');
    return fs.existsSync(outputConfigPath);
  });
}

module.exports = findRootDir;

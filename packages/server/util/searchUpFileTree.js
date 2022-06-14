const path = require('path');

function searchUpFileTree(startDir, cb) {
  let prevPath = null;
  let currPath = startDir;

  while (true) {
    if (currPath === prevPath) {
      return null;
    }

    if (cb(currPath)) {
      return currPath;
    }

    prevPath = currPath;
    currPath = path.dirname(currPath);
  }
}

module.exports = searchUpFileTree;

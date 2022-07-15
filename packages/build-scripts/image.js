const fs = require('node:fs');
const path = require('node:path');
const { spawnSync } = require('node:child_process');

function searchUpFileTree(startDir, cb) {
  let prevPath = null;
  let currPath = startDir;

  while (true) {
    if (currPath === prevPath) {
      return null;
    }

    if (cb(currPath)) {
      return path.resolve(currPath);
    }

    prevPath = currPath;
    currPath = path.dirname(currPath);
  }
}

const rootPath = searchUpFileTree(__dirname, (currPath) =>
  fs.existsSync(path.join(currPath, '.env'))
);

spawnSync('docker', ['compose', 'build'], {
  stdio: 'inherit',
  cwd: rootPath,
});

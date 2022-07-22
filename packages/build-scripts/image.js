const fs = require('node:fs');
const path = require('node:path');
const { execSync, spawnSync } = require('node:child_process');
const envFromYaml = require('./util/envFromYaml');

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

const rootDir = searchUpFileTree(__dirname, (currPath) =>
  fs.existsSync(path.join(currPath, '.env'))
);
const stackFilePath = path.join(rootDir, 'stack.yml');

require('dotenv').config({ path: path.join(rootDir, '.env') });

function loadGitCommitHash() {
  const gitCommitHash = execSync('git rev-parse HEAD', {
    cwd: rootDir,
    encoding: 'utf8',
  });

  if (gitCommitHash) {
    process.env.GIT_COMMIT = gitCommitHash.substring(0, 12);
  } else {
    throw new Error('Failed to determine git commit hash.');
  }
}

envFromYaml(stackFilePath);

loadGitCommitHash();

spawnSync(
  'docker',
  ['compose', '-f', 'stack.yml', '-f', 'docker-compose.build.yml', 'build'],
  {
    stdio: 'inherit',
    cwd: rootDir,
  }
);

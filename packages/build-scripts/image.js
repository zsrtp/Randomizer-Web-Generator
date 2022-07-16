const fs = require('node:fs');
const path = require('node:path');
const { spawnSync } = require('node:child_process');
const YAML = require('yaml');
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
const dockerFilePath = path.join(rootDir, 'Dockerfile');

require('dotenv').config({ path: path.join(rootDir, '.env') });

function verifyImageVersionsMatch() {
  const dockerFileContents = fs.readFileSync(dockerFilePath, 'utf8');
  const matches = dockerFileContents.match(
    /^ENV TPRGEN_VERSION=([a-zA-Z0-9.]+)$/gm
  );
  if (!matches) {
    throw new Error(
      'Dockerfile is missing a `ENV TPRGEN_VERSION=([a-zA-Z0-9.]+)` declaration. This should go at the bottom of the file, right above the CMD line.'
    );
  }

  if (matches.length > 1) {
    throw new Error(
      'Dockerfile has multiple `ENV TPRGEN_VERSION=([a-zA-Z0-9.]+)` declarations. The only declaration should go at the bottom of the file, right above the CMD line.'
    );
  }

  const verInDockerFile = matches[0].replace('ENV TPRGEN_VERSION=', '');

  if (!verInDockerFile || verInDockerFile !== process.env.IMAGE_VERSION) {
    throw new Error(
      `TPRGEN_VERSION in Dockerfile does not match process.env.IMAGE_VERSION. Dockerfile has '${verInDockerFile}' and process.env.IMAGE_VERSION is ${process.env.IMAGE_VERSION}.`
    );
  }

  // Make sure stack.yml will reference the IMAGE_VERSION env var.
  const stackFileContents = fs.readFileSync(stackFilePath, 'utf8');
  const { services } = YAML.parse(stackFileContents);

  if (
    !services ||
    !services['tpr-generator'] ||
    services['tpr-generator'].image !== 'tpr-generator:${IMAGE_VERSION}'
  ) {
    throw new Error(
      "stack.yml must have services['tpr-generator'].image set to tpr-generator:${IMAGE_VERSION}"
    );
  }
}

verifyImageVersionsMatch();

envFromYaml(stackFilePath);

spawnSync(
  'docker',
  ['compose', '-f', 'stack.yml', '-f', 'docker-compose.build.yml', 'build'],
  {
    stdio: 'inherit',
    cwd: rootDir,
  }
);

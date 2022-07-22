const path = require('node:path');
const { spawnSync } = require('node:child_process');
const envFromYaml = require('./util/envFromYaml');

const rootDir = path.resolve(__dirname, '../..');
const stackFilePath = path.join(rootDir, 'stack.yml');

// TODO: should do a check before trying to deploy if the image version already
// exists on the machine? And throw an error if it does? Local builds can use an
// epoch for the version?

require('dotenv').config({ path: path.join(rootDir, '.env') });

envFromYaml(stackFilePath);

let useSwarm = true;

const args = process.argv.slice(2);
for (let i = 0; i < args.length; i++) {
  if (args[i] === '--no-swarm') {
    useSwarm = false;
  }
}

if (useSwarm) {
  spawnSync('docker', ['stack', 'deploy', '-c', stackFilePath, 'demo'], {
    stdio: 'inherit',
    cwd: path.join(__dirname, '..'),
  });
  console.log('deployyy');
} else {
  process.env.HOST_PORT = 80;

  spawnSync('docker', ['compose', '-f', stackFilePath, 'up', '-d'], {
    stdio: 'inherit',
    cwd: path.join(__dirname, '..'),
  });

  console.log('deployyy non-swarm');
}

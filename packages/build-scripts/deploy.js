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

spawnSync('docker', ['stack', 'deploy', '-c', stackFilePath, 'demo'], {
  stdio: 'inherit',
  cwd: path.join(__dirname, '..'),
});

console.log('deployyy');

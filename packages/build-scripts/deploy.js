const path = require('node:path');
const fs = require('node:fs');
const crypto = require('node:crypto');
const { spawnSync } = require('node:child_process');
const YAML = require('yaml');

const rootDir = path.resolve(__dirname, '../..');
const stackFilePath = path.join(rootDir, 'bb-stack.yml');

require('dotenv').config({ path: path.join(rootDir, '.env') });

const stackYml = fs.readFileSync(stackFilePath, 'utf8');

// https://github.com/moby/moby/issues/35048#issuecomment-575661677

const { secrets } = YAML.parse(stackYml);
if (secrets) {
  Object.keys(secrets).forEach((secretKey) => {
    const secretVal = secrets[secretKey];
    if (secretVal.name && secretVal.file) {
      const match = secretVal.name.match(/\${([^}]+)}/);
      if (match) {
        const envVarName = match[1];

        const secretFilePath = path.resolve(rootDir, secretVal.file);
        const fileBuffer = fs.readFileSync(secretFilePath);
        const hashSum = crypto.createHash('sha256');
        hashSum.update(fileBuffer);
        const hex = hashSum.digest('hex').substring(0, 8);

        process.env[envVarName] = hex;
      }
    }
  });
}

// spawnSync('docker', ['stack', 'deploy', '-c', 'bb-stack.yml', 'demo'], {
spawnSync('docker', ['stack', 'deploy', '-c', stackFilePath, 'demo'], {
  stdio: 'inherit',
  cwd: path.join(__dirname, '..'),
});

console.log('deployyy');

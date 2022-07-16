const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const YAML = require('yaml');

function updateEnvFromConfigOrSecrets(rootDir, yamlRoot) {
  if (!yamlRoot) {
    return;
  }

  Object.keys(yamlRoot).forEach((key) => {
    const value = yamlRoot[key];
    if (value.name && value.file) {
      const match = value.name.match(/\${([^}]+)}/);
      if (match) {
        const envVarName = match[1];

        const filePath = path.resolve(rootDir, value.file);
        const fileBuffer = fs.readFileSync(filePath);
        const hashSum = crypto.createHash('sha256');
        hashSum.update(fileBuffer);
        const hex = hashSum.digest('hex').substring(0, 8);

        process.env[envVarName] = hex;
      }
    }
  });
}

// https://github.com/moby/moby/issues/35048#issuecomment-575661677

// Fills in any ${VALUES_LIKE_THIS} in the configs and secrets sections using a
// hash of the file contents which that config or secret points to. Docker needs
// different content to use a different name, and this handles this
// automatically.
function envFromYaml(yamlPath) {
  const yamlContent = fs.readFileSync(yamlPath, 'utf8');
  const { configs, secrets } = YAML.parse(yamlContent);

  const rootDir = path.dirname(yamlPath);

  updateEnvFromConfigOrSecrets(rootDir, configs);
  updateEnvFromConfigOrSecrets(rootDir, secrets);
}

module.exports = envFromYaml;

import fs from 'fs-extra';

let jwtSecret: string = '';
let apiSecret: string = '';

function initSecrets(): void {
  if (process.env.NODE_ENV === 'production') {
    jwtSecret = fs.readFileSync('/run/secrets/jwt_secret', 'utf8').trim();
    apiSecret = fs.readFileSync('/run/secrets/api_secret', 'utf8').trim();
  } else {
    jwtSecret = 'example_secret_key';
    apiSecret = 'example_secret_key';
  }
}

function getJwtSecret(): string {
  if (!jwtSecret) {
    throw new Error('Invalid configuration');
  }
  return jwtSecret;
}

function getApiSecret(): string {
  if (!apiSecret) {
    throw new Error('Invalid configuration');
  }
  return apiSecret;
}

export { initSecrets, getJwtSecret, getApiSecret };

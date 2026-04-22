import fs from 'fs-extra';

let jwtSecret: string = '';
let apiSecret: string = '';

function initSecrets(): void {
  if (process.env.NODE_ENV === 'production') {
    jwtSecret = fs.readFileSync('/run/secrets/jwt_secret', 'utf8').trim();
    jwtSecret = fs.readFileSync('/run/secrets/api_secret', 'utf8').trim();
  } else {
    jwtSecret = 'example_secret_key';
    apiSecret = 'example_secret_key';
  }
}

function getJwtSecret(): string {
  return jwtSecret;
}

function getApiSecret(): string {
  return apiSecret;
}

export { initSecrets, getJwtSecret, getApiSecret };

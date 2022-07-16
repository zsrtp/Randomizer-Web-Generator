import fs from 'fs-extra';

let jwtSecret: string = '';

function initSecrets(): void {
  if (process.env.NODE_ENV === 'production') {
    jwtSecret = fs.readFileSync('/run/secrets/jwt_secret', 'utf8').trim();
  } else {
    jwtSecret = 'example_secret_key';
  }
}

function getJwtSecret(): string {
  return jwtSecret;
}

export { initSecrets, getJwtSecret };

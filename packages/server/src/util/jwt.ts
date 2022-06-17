import jwt from 'jsonwebtoken';
import { genUserId } from './genId';

function genUserJwt(): string {
  const token = jwt.sign(
    {
      uid: genUserId(),
    },
    'example_secret_key'
  );

  return token;
}

// function

export { genUserJwt };

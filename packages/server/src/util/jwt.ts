import jwt from 'jsonwebtoken';
import { getJwtSecret } from 'src/secret';
import { genUserId } from './genId';

function genUserJwt(): string {
  const token = jwt.sign(
    {
      uid: genUserId(),
    },
    getJwtSecret()
  );

  return token;
}

export { genUserJwt };

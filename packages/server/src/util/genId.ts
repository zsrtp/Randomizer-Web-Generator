import { randomBytes } from 'crypto';

const charMap =
  '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_';

function getBits(
  byte: number,
  start: number,
  length: number,
  newStart: number
): number {
  const unpositionedBits = byte & ((0xff & (0xff << (8 - length))) >> start);

  if (start < newStart) {
    return unpositionedBits >> (newStart - start);
  }
  return unpositionedBits << (start - newStart);
}

function genElevenCharId(): string {
  const bytes = randomBytes(9);

  let id = '';
  let byteIndex = 0;
  let bitIndex = 0;
  let bitsNeeded = 6;
  let prevRes = 0;

  while (id.length < 11) {
    let newStart = 8 - bitsNeeded;

    const res = getBits(bytes[byteIndex], bitIndex, bitsNeeded, newStart);

    let bitsActuallyRead = bitsNeeded;

    if (bitsNeeded + bitIndex > 8) {
      // Still need to read more bits
      bitsActuallyRead = 8 - bitIndex;
      bitsNeeded = bitIndex - 2;
      prevRes = res;
    } else {
      // We read all of the bits we needed, so we create a char now.
      id += charMap[bitsActuallyRead === 6 ? res : prevRes + res];
      bitsNeeded = 6;
    }

    bitIndex = bitIndex + bitsActuallyRead;
    if (bitIndex >= 8) {
      byteIndex += 1;
      bitIndex = 0;
    }
  }

  return id;
}

export { genElevenCharId };

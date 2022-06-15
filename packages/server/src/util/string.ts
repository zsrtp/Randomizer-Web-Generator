import toArray from 'lodash.toarray';

function normalizeStringToMax128Bytes(inputStr: string): string {
  // substring to save lodash some work potentially. 256 because some
  // characters like emojis have length 2, and we want to leave at least 128
  // glyphs. Normalize is to handle writing the same unicode chars in
  // different ways.
  // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/normalize
  let seedStr = inputStr
    .normalize()
    .trim()
    .replace(/\s+/g, ' ')
    .substring(0, 256);

  // The whitespace replacement already handles removing \n, \r, \f, and \t,
  // so the only characters left which have to be escaped are double-quote,
  // backslash, and backspace (\b or \x08). Even though they are only 1 byte in
  // UTF-8, we say those ones are 2 bytes because they will take 2 bytes in
  // the json file due to the backslash escape character.

  // Allow 128 bytes max
  const textEncoder = new TextEncoder();
  let len = 0;
  let str = '';

  // We use the lodash.toarray method because it handles chars like 👩‍❤️‍💋‍👩.
  // Another approach that almost works is str.match(/./gu), but this returns
  // [ "👩", "‍", "❤", "️", "‍", "💋", "‍", "👩" ] for 👩‍❤️‍💋‍👩.
  const chars = toArray(seedStr);
  for (let i = 0; i < chars.length; i++) {
    const char = chars[i];

    let byteLength;
    if (char === '"' || char === '\\' || char === '\b') {
      byteLength = 2; // Will use 2 chars in the json file
    } else {
      byteLength = textEncoder.encode(char).length;
    }

    if (len + byteLength <= 128) {
      str += char;
      len += byteLength;
    } else {
      break;
    }
  }

  return str.trim();
}

export { normalizeStringToMax128Bytes };

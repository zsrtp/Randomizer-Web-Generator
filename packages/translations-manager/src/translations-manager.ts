import { XMLBuilder, XMLParser } from 'fast-xml-parser';
import fs from 'fs-extra';
import path, { join } from 'node:path';

function searchUpFileTree(startDir: string, cb: (path: string) => boolean) {
  let prevPath: string | null = null;
  let currPath: string | null = startDir;

  while (true) {
    if (currPath == null || currPath === prevPath) {
      return null;
    }

    if (cb(currPath as string)) {
      return path.resolve(currPath);
    }

    prevPath = currPath;
    currPath = path.dirname(currPath);
  }
}

const rootDirRaw = searchUpFileTree(__dirname, (currPath) =>
  fs.existsSync(path.join(currPath, '.env'))
);
if (typeof rootDirRaw !== 'string')
  throw new Error('rootDirRaw was not a string.');

const rootDir = rootDirRaw as string;
const filePath = path.join(rootDir, 'Generator/Translations/Translations.resx');

const contents = fs.readFileSync(filePath, { encoding: 'utf8' });

const options = {
  commentPropName: '#comment',
  ignoreAttributes: false,
  allowBooleanAttributes: true,
  preserveOrder: true,
  format: true,
};
const parser = new XMLParser(options);
const jObj = parser.parse(contents);

const arr = jObj[1].root;

const newArr = [];
const unsortedData = [];

for (let i = 0; i < arr.length; i++) {
  const el = arr[i];
  if (el.hasOwnProperty('data')) {
    unsortedData.push(el);
  } else {
    newArr.push(el);
  }
}

unsortedData.sort((a, b) => {
  const aName = a[':@']['@_name'];
  const bName = b[':@']['@_name'];
  return aName.localeCompare(bName, 'en');
});

jObj[1].root = newArr;
unsortedData.forEach((el) => {
  newArr.push(el);
});

const builder = new XMLBuilder(options);

const xmlOutput = builder.build(jObj);
fs.writeFileSync(filePath, xmlOutput);

"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const fast_xml_parser_1 = require("fast-xml-parser");
const fs_extra_1 = __importDefault(require("fs-extra"));
const node_path_1 = __importDefault(require("node:path"));
function searchUpFileTree(startDir, cb) {
    let prevPath = null;
    let currPath = startDir;
    while (true) {
        if (currPath == null || currPath === prevPath) {
            return null;
        }
        if (cb(currPath)) {
            return node_path_1.default.resolve(currPath);
        }
        prevPath = currPath;
        currPath = node_path_1.default.dirname(currPath);
    }
}
const rootDirRaw = searchUpFileTree(__dirname, (currPath) => fs_extra_1.default.existsSync(node_path_1.default.join(currPath, '.env')));
if (typeof rootDirRaw !== 'string')
    throw new Error('rootDirRaw was not a string.');
const rootDir = rootDirRaw;
const filePath = node_path_1.default.join(rootDir, 'Generator/Translations/Translations.resx');
const contents = fs_extra_1.default.readFileSync(filePath, { encoding: 'utf8' });
const options = {
    commentPropName: '#comment',
    ignoreAttributes: false,
    allowBooleanAttributes: true,
    preserveOrder: true,
    format: true,
};
const parser = new fast_xml_parser_1.XMLParser(options);
const jObj = parser.parse(contents);
console.log(jObj);
const arr = jObj[1].root;
const newArr = [];
const unsortedData = [];
for (let i = 0; i < arr.length; i++) {
    const el = arr[i];
    if (el.hasOwnProperty('data')) {
        unsortedData.push(el);
    }
    else {
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
const builder = new fast_xml_parser_1.XMLBuilder(options);
const xmlOutput = builder.build(jObj);
const outputPath = node_path_1.default.join(__dirname, 'dog.xml');
fs_extra_1.default.writeFileSync(outputPath, xmlOutput);
console.log('dog');
//# sourceMappingURL=translations-manager.js.map
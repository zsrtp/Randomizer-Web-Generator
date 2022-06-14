const fs = require('fs-extra');
const path = require('path');
const { execSync, execFile } = require('child_process');
const spawn = require('cross-spawn');
const { resolveRootPath } = require('./config');

let exePath = process.env.TPR_GENERATOR_EXE_PATH;
if (process.platform === 'win32') {
  // This does not mean 32-bit Windows only. Should work for all of them.
  exePath += '.exe';
}

const generatorExePath = resolveRootPath(exePath);

// const generatorExePath = path.join(
//   __dirname,
//   // 'Generator/bin/release/net5.0/TPRandomizer.exe'
//   'Generator/bin/Debug/net5.0/TPRandomizer.exe'
// );

function callGenerator(...args) {
  const command = [generatorExePath].concat(args).join(' ');

  // const buf = execSync(`${generatorExePath} generate2 ${args[0]} abcdef`);
  const buf = execSync(command);
  return buf.toString();
}

function callGeneratorBuf(args, cb) {
  // callGeneratorAsync(args, cb);

  // const command = [generatorExePath].concat(args).join(' ');

  // // const buf = execSync(`${generatorExePath} generate2 ${args[0]} abcdef`);
  // const buf = execSync(command);
  // return buf;

  const childProcess = execFile(
    generatorExePath,
    args,
    { encoding: 'buffer' },
    (error, stdout, stderr) => {
      if (error) {
        cb(error.message);
      } else {
        cb(null, stdout);
      }
    }
  );

  return childProcess;
}

function callGeneratorAsync(args, cb) {
  const childProcess = execFile(
    generatorExePath,
    args,
    (error, stdout, stderr) => {
      if (error) {
        cb(error.message);
      } else {
        cb(null, stdout);
      }
    }
  );

  return childProcess;
}

function callGeneratorMatchOutput(args, cb) {
  callGeneratorAsync(args, (error, output) => {
    if (error) {
      cb(error);
    } else {
      const match = output.match(/SUCCESS:(\S+)/);
      if (match) {
        cb(null, match[1]);
      } else {
        cb(output);
      }
    }
  });
}

// function getGeneratorExePath() {
//   return generatorExePath;
// }

module.exports = {
  callGenerator,
  callGeneratorBuf,
  callGeneratorMatchOutput,
};

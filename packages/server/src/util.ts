import { execSync, execFile, ExecFileException } from 'child_process';
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

function callGenerator(...args: string[]) {
  const command = [generatorExePath].concat(args).join(' ');

  // const buf = execSync(`${generatorExePath} generate2 ${args[0]} abcdef`);
  const buf = execSync(command);
  return buf.toString();
}

interface callGeneratorCb {
  (error: string, data?: string): void;
}

interface callGeneratorBufCb {
  (error: string, buffer?: Buffer): void;
}

function callGeneratorBuf(args: string[], cb: callGeneratorBufCb) {
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

interface execFileCb {
  (error: ExecFileException | null, stdout: string, stderr: string): void;
}

function callGeneratorAsync(args: string[], cb: callGeneratorCb) {
  const childProcess = execFile(generatorExePath, args, <execFileCb>((
    error,
    stdout,
    stderr
  ) => {
    if (error) {
      cb(error.message);
    } else {
      cb(null, stdout);
    }
  }));

  return childProcess;
}

function callGeneratorMatchOutput(args: string[], cb: callGeneratorCb) {
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

export { callGenerator, callGeneratorBuf, callGeneratorMatchOutput };

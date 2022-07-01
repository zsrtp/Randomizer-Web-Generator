import { execSync, execFile, ExecFileException } from 'child_process';
import { resolveRootPath, generatorExePath } from 'src/config';

function callGenerator(...args: string[]) {
  const command = [generatorExePath].concat(args).join(' ');

  // const buf = execSync(`${generatorExePath} generate2 ${args[0]} abcdef`);
  const buf = execSync(command);
  return buf.toString();
}

interface callGeneratorCb {
  (error: ExecFileException | string | null, data?: string): void;
}

interface callGeneratorBufCb {
  (error: string | null, buffer?: Buffer): void;
}

interface execFileCb {
  (error: ExecFileException | null, stdout: string, stderr: string): void;
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

function callGeneratorAsync(args: string[], cb: callGeneratorCb) {
  const childProcess = execFile(generatorExePath, args, <execFileCb>((
    error,
    stdout,
    stderr
  ) => {
    if (error) {
      cb(error);
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
      if (!output) {
        cb('No output.');
        return;
      }

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

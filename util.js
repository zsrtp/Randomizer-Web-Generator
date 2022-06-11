const fs = require('fs-extra');
const path = require('path');
const { execSync, execFile } = require('child_process');
const spawn = require('cross-spawn');

let outputPath = null;
initOutputPath();

function initOutputPath() {
  let prevPath = null;
  let currPath = __dirname;

  while (true) {
    if (currPath === prevPath) {
      throw new Error('Unable to find output.config.json');
    }

    const outputConfigPath = path.join(currPath, '.env');
    if (fs.existsSync(outputConfigPath)) {
      // const fileContents = fs.readFileSync(outputConfigPath);
      // const json = JSON.parse(fileContents);

      // const relativeOutputPath = json.outputPath;
      // const outputDir = path.resolve(currPath, relativeOutputPath);
      const outputDir = path.resolve(currPath, process.env.OUTPUT_VOLUME_PATH);

      fs.mkdirp(outputDir);

      outputPath = outputDir;
      return;
    }

    prevPath = currPath;
    currPath = path.dirname(currPath);
  }
}

function genOutputPath(...args) {
  return path.join(outputPath, ...args);
}

let exePath = process.env.TPR_GENERATOR_EXE_PATH;
if (process.platform === 'win32') {
  // This does not mean 32-bit Windows only. Should work for all of them.
  exePath += '.exe';
}

const generatorExePath = path.resolve(__dirname, exePath);

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

function callGeneratorBuf(...args) {
  const command = [generatorExePath].concat(args).join(' ');

  // const buf = execSync(`${generatorExePath} generate2 ${args[0]} abcdef`);
  const buf = execSync(command);
  return buf;
}

function callGeneratorAsync(args, cb) {
  execFile(generatorExePath, args, (error, stdout, stderr) => {
    if (error) {
      cb(error.message);
    } else {
      cb(null, stdout);
    }
  });
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

function getGeneratorExePath() {
  return generatorExePath;
}

module.exports = {
  callGenerator,
  callGeneratorBuf,
  callGeneratorMatchOutput,
  genOutputPath,
  getGeneratorExePath,
};

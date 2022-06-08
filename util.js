const path = require('path');
const { execSync } = require('child_process');

const generatorExePath = path.join(
  __dirname,
  'Generator/bin/release/net5.0/TPRandomizer.exe'
);

function callGenerator(...args) {
  const command = [generatorExePath].concat(args).join(' ');

  // const buf = execSync(`${generatorExePath} generate2 ${args[0]} abcdef`);
  const buf = execSync(command);
  return buf.toString();
}

function callGeneratorMatchOutput(...args) {
  const output = callGenerator(args);

  const match = output.match(/SUCCESS:(\S+)/);
  if (match) {
    return {
      data: match[1],
    };
  }

  return {
    error: output,
  };
}

module.exports = {
  callGenerator,
  callGeneratorMatchOutput,
};

const path = require('path');

// Note that this has a warning caused by a dynamic import in express. This does
// not appear to actually cause any issues.

// From express/lib.view.js:

// ```
// var mod = this.ext.slice(1)
// debug('require "%s"', mod)

// // default engine export
// var fn = require(mod).__express
// ```

// The other concern is references to __dirname, but with how those are used
// right now (finding the root dir), they work as intended post-bundling.

module.exports = {
  mode: 'production',
  target: 'node',
  entry: './src/server.ts',
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js'],
    alias: {
      src: [path.resolve('./src')],
    },
  },
  output: {
    filename: 'bundle.js',
    path: path.resolve(__dirname, 'dist'),
  },
};

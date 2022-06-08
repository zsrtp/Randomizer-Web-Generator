// const path = require('path');
// const cors = require('cors');
// const express = require('express');

// const app = express(); // create express app
// app.use(cors());

// const bodyParser = require('body-parser');
// app.use(bodyParser.json());

// // add middlewares
// const root = require('path').join(__dirname, 'build');
// app.use(express.static(root));

// app.use('/*', (req, res) => {
//   res.sendFile(path.join(__dirname, 'build', 'index.html'));
// });

// // start express server on port 5000
// app.listen(process.env.PORT || 5000, () => {
//   console.log('server started');
// });

const { execSync } = require('child_process');

const fs = require('fs');
const path = require('path');
const cors = require('cors');
const express = require('express');
const { useGenerator, callGenerator } = require('./util');

const app = express(); // create express app
app.use(cors());

const bodyParser = require('body-parser');
app.use(bodyParser.json());

const generatorExePath = path.join(
  __dirname,
  'Generator/bin/release/net5.0/TPRandomizer.exe'
);

let root;
let indexHtmlPath;

// add middlewares
if (process.env.NODE_ENV === 'production') {
  root = path.join(__dirname, '..', 'client', 'build');
  indexHtmlPath = path.join(__dirname, '..', 'build', 'client', 'index.html');
} else {
  // root = path.join(__dirname, 'build');
  root = path.join(__dirname, 'packages', 'client');
  // indexHtmlPath = path.join(__dirname, 'build', 'index.html');
  indexHtmlPath = path.join(__dirname, 'packages', 'client', 'index.html');
}

app.post('/api/example', function (req, res) {
  const { settingsString } = req.body;

  if (!settingsString || typeof settingsString !== 'string') {
    res.status(500).send({ error: 'Malformed request.' });
    return;
  }

  let error;
  let buf;

  try {
    buf = execSync(`${generatorExePath} generate2 ${settingsString} abcdef`);
  } catch (e) {
    error = e.toString();
  }

  if (error) {
    res.status(500).send({ error });
  } else {
    const output = buf.toString();

    const match = output.match(/SUCCESS:(\S+)/);
    if (match) {
      res.send({
        result: match[1],
      });
    } else {
      res.status(500).send({
        error: output,
      });
    }
  }
});

// app.use('*', (req, res) => {
//   res.send('fish');
// });

app.get('/', (req, res) => {
  // $cmd = "dotnet Generator/bin/Debug/net5.0/TPRandomizer.dll print_check_ids";

  fs.readFile(indexHtmlPath, function read(err, data) {
    if (err) {
      console.log(err);
      res.status(500).send({ error: 'Internal server error.' });
    } else {
      const excludedChecksList = JSON.parse(callGenerator('print_check_ids'));
      const arr = Object.keys(excludedChecksList).map((key) => {
        return `<li><label><input type='checkbox' data-checkId='${excludedChecksList[key]}'>${key}</label></li>`;
      });

      let msg = data.toString();
      msg = msg.replace('<!-- CHECK_IDS -->', arr.join('\n'));

      res.send(msg);
    }
  });

  // res.send('fish');
  // res.sendFile(indexHtmlPath);
});

// app.use('*', (req, res) => {
//   // $cmd = "dotnet Generator/bin/Debug/net5.0/TPRandomizer.dll print_check_ids";

//   // fs.readFile(indexHtmlPath, function read(err, data) {
//   //   if (err) {
//   //     console.log(err);
//   //     res.status(500).send({ error: 'Internal server error.' });
//   //   } else {
//   //     let msg = data.toString();
//   //     const a = msg.indexOf('<!-- CHECK_IDS -->');
//   //     msg = msg.replace('<!-- CHECK_IDS -->', '<div>ABCDEFFFF</div>');
//   //     // msg = msg.replace(/%email%/gi, 'example@gmail.com');

//   //     // let temp = 'Hello %NAME%, would you like some %DRINK%?';
//   //     // temp = temp.replace(/%NAME%/gi, 'Myname');
//   //     // temp = temp.replace('%DRINK%', 'tea');
//   //     // console.log('temp: ' + temp);
//   //     console.log('msg: ' + msg);
//   //     // res.send(msg);
//   //     res.send('fish');
//   //   }
//   // });

//   res.send('fish');
//   // res.sendFile(indexHtmlPath);
// });

app.use(express.static(root));

// start express server on port 5000
app.listen(process.env.PORT || 5000, () => {
  console.log('server started');
});

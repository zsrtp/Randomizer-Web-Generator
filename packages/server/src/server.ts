import path from 'path';
import fs from 'fs-extra';
import searchUpFileTree from './util/searchUpFileTree';

if (process.env.NODE_ENV === 'production') {
  // In production, the config is provided by docker swarm configs.
  require('dotenv').config({ path: '/env_config' });
} else {
  // During development, we load config using env variables.
  const envFileDir = searchUpFileTree(__dirname, (currPath) =>
    fs.existsSync(path.join(currPath, '.env'))
  );

  if (!envFileDir) {
    throw new Error('Failed to find env file directory.');
  }

  const { execSync } = require('node:child_process');
  const gitCommitHash = execSync('git rev-parse HEAD', {
    cwd: envFileDir,
    encoding: 'utf8',
  });

  if (gitCommitHash) {
    process.env.GIT_COMMIT = gitCommitHash.substring(0, 12);
  }

  const dotenvPath = path.resolve(path.join(envFileDir, '.env'));

  const dotenvFiles = [
    `${dotenvPath}.development.local`,
    `${dotenvPath}.development`,
    dotenvPath,
  ].filter(Boolean);

  dotenvFiles.forEach((dotenvFile: string) => {
    if (fs.existsSync(dotenvFile)) {
      require('dotenv').config({
        path: dotenvFile,
      });
    }
  });
}

import {
  initConfig,
  resolveRootPath,
  resolveOutputPath,
  logConfig,
} from './config';
initConfig();
// Import the logger as soon as the config is initialized so that it can be used
// after this point.
import logger from './logger/logger';

import { initSecrets, getJwtSecret } from './secret';
initSecrets();

const url = require('url');
const cors = require('cors');
import express from 'express';
import { callGenerator, callGeneratorBuf } from './util';
import apiSeedProgress from './api/seed/apiSeedProgress';
import apiSeedGenerate from './api/seed/apiSeedGenerate';
import apiSeedCancel from './api/seed/apiSeedCancel';
import { genUserJwt } from './util/jwt';
const { normalizeStringToMax128Bytes } = require('./util/string');
import jwt from 'jsonwebtoken';
import { checkProgress } from './generationQueues';

declare global {
  namespace Express {
    interface Request {
      newUserJwt?: string;
      userId?: string;
    }
  }
}

logger.info('Server starting...');

// log config
logConfig(logger.info);

const app = express(); // create express app
app.use(cors());

const bodyParser = require('body-parser');
app.use(bodyParser.json());

app.all(
  '/api/*',
  (req: express.Request, res: express.Response, next: express.NextFunction) => {
    if (
      !req.headers.authorization ||
      !req.headers.authorization.startsWith('Bearer ')
    ) {
      return res.status(403).send({ error: 'Forbidden' });
    }

    const token = req.headers.authorization.substring(7);

    jwt.verify(token, getJwtSecret(), (err, data: jwt.JwtPayload) => {
      if (err || !data.uid) {
        return res.status(403).send({ error: 'Forbidden' });
      }

      req.userId = data.uid;
      next();
    });
  }
);

app.get(
  '*',
  (req: express.Request, res: express.Response, next: express.NextFunction) => {
    if (req.path.indexOf('.') < 0 && !req.path.startsWith('/api/')) {
      const userAgent = req.headers['user-agent'];
      if (!userAgent) {
        return res.send('Unsupported User-Agent.');
      } else if (
        userAgent.indexOf('MSIE ') >= 0 ||
        userAgent.indexOf('Trident/') >= 0
      ) {
        return res.send(
          'Internet Explorer is not supported. Please use a different browser.'
        );
      }

      req.newUserJwt = genUserJwt();
    }
    next();
  }
);

let root: string;
let indexHtmlPath: string;

// add middlewares
if (process.env.NODE_ENV === 'production') {
  root = path.join(__dirname, '..', 'client', 'build');
  indexHtmlPath = path.join(__dirname, '..', 'client', 'build', 'index.html');
} else {
  // root = path.join(__dirname, 'build');
  // indexHtmlPath = path.join(__dirname, 'build', 'index.html');

  // root = path.join(__dirname, 'packages', 'client');
  // indexHtmlPath = path.join(__dirname, 'packages', 'client', 'index.html');

  const rootDir = resolveRootPath();

  root = path.join(rootDir, 'packages', 'client');
  indexHtmlPath = path.join(root, 'index.html');
}

app.post('/api/seed/generate', apiSeedGenerate);
app.get('/api/seed/progress/:id', apiSeedProgress);
app.post('/api/seed/cancel', apiSeedCancel);

interface Aaa {
  name: string;
  length: number;
  bytes: string;
}

interface OutputFileMeta {
  name: string;
  length: number;
}

app.post('/api/final', function (req: express.Request, res: express.Response) {
  const { referer } = req.headers;

  let id = null;

  try {
    if (referer != null) {
      const refUrl = new URL(referer);
      if (refUrl.pathname) {
        const match = refUrl.pathname.match(/^\/s\/([a-zA-Z0-9-_]+)$/);
        if (match && match.length > 1) {
          id = match[1];
        }
      }
    }
  } catch (e) {
    // do nothing
  }

  // const { query } = url.parse(referer, true);
  // const { id } = query;

  if (!id) {
    res.status(400).send({ error: 'Bad referer.' });
    return;
  }

  if (typeof id !== 'string' || !/^[0-9a-z-_]+$/i.test(id)) {
    res.status(400).send({ error: 'Invalid id format.' });
    return;
  }

  const { fileCreationSettings } = req.body;

  if (
    !fileCreationSettings ||
    typeof fileCreationSettings !== 'string' ||
    !/^[0-9a-z-_]+$/i.test(fileCreationSettings)
  ) {
    res.status(400).send({ error: 'Invalid fileCreationSettings format.' });
    return;
  }

  callGeneratorBuf(
    ['generate_final_output2', id, fileCreationSettings],
    (error, buffer) => {
      if (error) {
        res.status(500).send({ error });
        return;
      }

      try {
        if (!buffer) {
          res.status(500).send({ error: 'Output buffer was null.' });
          return;
        }

        const index = buffer.indexOf('BYTES:', 0);

        let currIndex = index;
        if (currIndex < 0) {
          res.status(500).send({ error: 'Failed to find BYTES:' });
          return;
        }

        currIndex += 'BYTES:'.length;
        const jsonLen = parseInt(
          buffer.toString('utf8', currIndex, currIndex + 8),
          16
        );
        currIndex += 8;

        const json: OutputFileMeta[] = JSON.parse(
          buffer.toString('utf8', currIndex, currIndex + jsonLen)
        );
        currIndex += jsonLen;

        const data: Aaa[] = [];
        // const data = [];
        json.forEach(({ name, length }) => {
          data.push({
            name,
            length,
            bytes: buffer
              .subarray(currIndex, currIndex + length)
              .toString('base64'),
          });
          currIndex += length;
        });

        res.send({ data });
      } catch (e) {
        res.status(500).send({ error: e.message });
      }
    }
  );
});

app.get('/', (req: express.Request, res: express.Response) => {
  fs.readFile(indexHtmlPath, function read(err, data) {
    if (err) {
      console.log(err);
      res.status(500).send({ error: 'Internal server error.' });
    } else {
      let msg = data.toString();
      msg = msg.replace(
        '<!-- IMAGE_VERSION -->',
        `<input id="envImageVersion" type="hidden" value="${process.env.IMAGE_VERSION}">`
      );
      msg = msg.replace(
        '<!-- GIT_COMMIT -->',
        `<input id="envGitCommit" type="hidden" value="${process.env.GIT_COMMIT}">`
      );
      msg = msg.replace(
        '<!-- USER_ID -->',
        `<input id="userJwtInput" type="hidden" value="${req.newUserJwt}">`
      );

      const excludedChecksList = JSON.parse(callGenerator('print_check_ids'));
      const arr = Object.keys(excludedChecksList).map((key) => {
        return `<li><label><input type='checkbox' data-checkId='${excludedChecksList[key]}'>${key}</label></li>`;
      });

      msg = msg.replace('<!-- CHECK_IDS -->', arr.join('\n'));

      const startingItems = [
        [50, 'Shadow Crystal'],
        [132, 'Horse Call'],
        [243, 'Gate Keys'],
        [63, 'Progressive Sword'],
        [63, 'Progressive Sword'],
        [63, 'Progressive Sword'],
        [63, 'Progressive Sword'],
        [64, 'Boomerang'],
        [72, 'Lantern'],
        [75, 'Slingshot'],
        [74, 'Progressive Fishing Rod'],
        [74, 'Progressive Fishing Rod'],
        [69, 'Iron Boots'],
        [67, 'Progressive Bow'],
        [67, 'Progressive Bow'],
        [67, 'Progressive Bow'],
        [81, 'Bomb Bag and Bombs'],
        [81, 'Bomb Bag and Bombs'],
        [81, 'Bomb Bag and Bombs'],
        [79, 'Giant Bomb Bag'],
        [49, 'Zora Armor'],
        [68, 'Progressive Clawshot'],
        [68, 'Progressive Clawshot'],
        [144, 'Aurus Memo'],
        [145, 'Asheis Sketch'],
        [65, 'Spinner'],
        [66, 'Ball and Chain'],
        [70, 'Progressive Dominion Rod'],
        [70, 'Progressive Dominion Rod'],
        [96, 'Empty Bottle'],
        [48, 'Magic Armor'],
        [42, 'Ordon Shield'],
        [44, 'Hylian Shield'],
        [62, 'Hawkeye'],
        [233, 'Progressive Sky Book', 7],
        [225, 'Progressive Hidden Skill', 7],
        [0x85, 'Forest Temple Small Key'],
        [0x85, 'Forest Temple Small Key'],
        [0x85, 'Forest Temple Small Key'],
        [0x85, 'Forest Temple Small Key'],
        [0x92, 'Forest Temple Big Key'],
        [0x86, 'Goron Mines Small Key'],
        [0x86, 'Goron Mines Small Key'],
        [0x86, 'Goron Mines Small Key'],
        [0xf9, 'Goron Mines Key Shard'],
        [0xf9, 'Goron Mines Key Shard'],
        [0xf9, 'Goron Mines Key Shard'],
        [0x87, 'Lakebed Temple Small Key'],
        [0x87, 'Lakebed Temple Small Key'],
        [0x87, 'Lakebed Temple Small Key'],
        [0x93, 'Lakebed Temple Big Key'],
        [0x88, "Arbiter's Grounds Small Key"],
        [0x88, "Arbiter's Grounds Small Key"],
        [0x88, "Arbiter's Grounds Small Key"],
        [0x88, "Arbiter's Grounds Small Key"],
        [0x88, "Arbiter's Grounds Small Key"],
        [0x94, "Arbiter's Grounds Big Key"],
        [0x89, 'Snowpeak Ruins Small Key'],
        [0x89, 'Snowpeak Ruins Small Key'],
        [0x89, 'Snowpeak Ruins Small Key'],
        [0x89, 'Snowpeak Ruins Small Key'],
        [0xf4, 'Snowpeak Ruins Ordon Pumpkin'],
        [0xf5, 'Snowpeak Ruins Ordon Goat Cheese'],
        [0xf6, 'Snowpeak Ruins Bedroom Key'],
        [0x8a, 'Temple of Time Small Key'],
        [0x8a, 'Temple of Time Small Key'],
        [0x8a, 'Temple of Time Small Key'],
        [0x95, 'Temple of Time Big Key'],
        [0x8b, 'City in the Sky Small Key'],
        [0x96, 'City in the Sky Big Key'],
        [0x8c, 'Palace of Twilight Small Key'],
        [0x8c, 'Palace of Twilight Small Key'],
        [0x8c, 'Palace of Twilight Small Key'],
        [0x8c, 'Palace of Twilight Small Key'],
        [0x8c, 'Palace of Twilight Small Key'],
        [0x8c, 'Palace of Twilight Small Key'],
        [0x8c, 'Palace of Twilight Small Key'],
        [0x97, 'Palace of Twilight Big Key'],
        [0x8d, 'Hyrule Castle Small Key'],
        [0x8d, 'Hyrule Castle Small Key'],
        [0x8d, 'Hyrule Castle Small Key'],
        [0x98, 'Hyrule Castle Big Key'],
        [0xe0, 'Poe Soul', 60],
      ];

      const startingItemsEls = startingItems.map((item) => {
        if (item.length < 3) {
          return `<li><label><input type='checkbox' data-itemId='${item[0]}'>${item[1]}</label> </li>`;
        } else {
          return `<li class="liSlider"><label><div>${item[1]}</div><div class="liSlider-inputRow"><input type='range' value='0' min='0' max='${item[2]}' data-itemId='${item[0]}'><span class="liSlider-inputRowText"></span></div></label></li>`;
        }
      });

      msg = msg.replace('<!-- STARTING_ITEMS -->', startingItemsEls.join('\n'));

      const plandoItems = [
        // Progression Items
        ['Category', 'Progression Items'],
        [0x32, 'Shadow_Crystal'],
        [0x3f, 'Progressive_Sword'],
        [0x91, 'Asheis_Sketch'],
        [0x90, 'Aurus_Memo'],
        [0x42, 'Ball_and_Chain'],
        [0x40, 'Boomerang'],
        [0x51, 'Filled_Bomb_Bag'],
        [0x45, 'Iron_Boots'],
        [0x48, 'Lantern'],
        [0x43, 'Progressive_Bow'],
        [0x44, 'Progressive_Clawshot'],
        [0x46, 'Progressive_Dominion_Rod'],
        [0x4a, 'Progressive_Fishing_Rod'],
        [0xe9, 'Progressive_Sky_Book'],
        [0x4b, 'Slingshot'],
        [0x41, 'Spinner'],
        [0x31, 'Zora_Armor'],
        // Dungeon Rewards
        ['Category', 'Dungeon Rewards'],
        [0xd8, 'Progressive_Fused_Shadow'],
        [0xa5, 'Progressive_Mirror_Shard'],
        // Small Keys
        ['Category', 'Small Keys'],
        [0x85, 'Forest_Temple_Small_Key'],
        [0x86, 'Goron_Mines_Small_Key'],
        [0x87, 'Lakebed_Temple_Small_Key'],
        [0x88, 'Arbiters_Grounds_Small_Key'],
        [0x89, 'Snowpeak_Ruins_Small_Key'],
        [0xf4, 'Snowpeak_Ruins_Ordon_Pumpkin'],
        [0xf5, 'Snowpeak_Ruins_Ordon_Goat_Cheese'],
        [0x8a, 'Temple_of_Time_Small_Key'],
        [0x8b, 'City_in_The_Sky_Small_Key'],
        [0x8c, 'Palace_of_Twilight_Small_Key'],
        [0x8d, 'Hyrule_Castle_Small_Key'],
        [0xee, 'North_Faron_Woods_Gate_Key'],
        [0xf3, 'Gate_Keys'],
        [0x8e, 'Gerudo_Desert_Bulblin_Camp_Key'],
        // Big Keys
        ['Category', 'Big Keys'],
        [0x92, 'Forest_Temple_Big_Key'],
        [0xf9, 'Goron_Mines_Key_Shard'],
        [0x93, 'Lakebed_Temple_Big_Key'],
        [0x94, 'Arbiters_Grounds_Big_Key'],
        [0xf6, 'Snowpeak_Ruins_Bedroom_Key'],
        [0x95, 'Temple_of_Time_Big_Key'],
        [0x96, 'City_in_The_Sky_Big_Key'],
        [0x97, 'Palace_of_Twilight_Big_Key'],
        [0x98, 'Hyrule_Castle_Big_Key'],
        // Compasses
        ['Category', 'Compasses'],
        [0x99, 'Forest_Temple_Compass'],
        [0x9a, 'Goron_Mines_Compass'],
        [0x9b, 'Lakebed_Temple_Compass'],
        [0xa8, 'Arbiters_Grounds_Compass'],
        [0xa9, 'Snowpeak_Ruins_Compass'],
        [0xaa, 'Temple_of_Time_Compass'],
        [0xab, 'City_in_The_Sky_Compass'],
        [0xac, 'Palace_of_Twilight_Compass'],
        [0xad, 'Hyrule_Castle_Compass'],
        //Maps
        ['Category', 'Dungeon Maps'],
        [0xb6, 'Forest_Temple_Dungeon_Map'],
        [0xb7, 'Goron_Mines_Dungeon_Map'],
        [0xb8, 'Lakebed_Temple_Dungeon_Map'],
        [0xb9, 'Arbiters_Grounds_Dungeon_Map'],
        [0xba, 'Snowpeak_Ruins_Dungeon_Map'],
        [0xbb, 'Temple_of_Time_Dungeon_Map'],
        [0xbc, 'City_in_The_Sky_Dungeon_Map'],
        [0xbd, 'Palace_of_Twilight_Dungeon_Map'],
        [0xbe, 'Hyrule_Castle_Dungeon_Map'],
        // Golden Bugs
        ['Category', 'Golden Bugs'],
        [0xc0, 'Male_Beetle'],
        [0xc1, 'Female_Beetle'],
        [0xc2, 'Male_Butterfly'],
        [0xc3, 'Female_Butterfly'],
        [0xc4, 'Male_Stag_Beetle'],
        [0xc5, 'Female_Stag_Beetle'],
        [0xc6, 'Male_Grasshopper'],
        [0xc7, 'Female_Grasshopper'],
        [0xc8, 'Male_Phasmid'],
        [0xc9, 'Female_Phasmid'],
        [0xca, 'Male_Pill_Bug'],
        [0xcb, 'Female_Pill_Bug'],
        [0xcc, 'Male_Mantis'],
        [0xcd, 'Female_Mantis'],
        [0xce, 'Male_Ladybug'],
        [0xcf, 'Female_Ladybug'],
        [0xd0, 'Male_Snail'],
        [0xd1, 'Female_Snail'],
        [0xd2, 'Male_Dragonfly'],
        [0xd3, 'Female_Dragonfly'],
        [0xd4, 'Male_Ant'],
        [0xd5, 'Female_Ant'],
        [0xd6, 'Male_Dayfly'],
        [0xd7, 'Female_Dayfly'],
        // Rupees
        ['FirstCategory', 'Rupees'],
        [0x01, 'Green_Rupee'],
        [0x02, 'Blue_Rupee'],
        [0x03, 'Yellow_Rupee'],
        [0x04, 'Red_Rupee'],
        [0x05, 'Purple_Rupee'],
        [0x06, 'Orange_Rupee'],
        [0x07, 'Silver_Rupee'],
        [0xed, 'Purple_Rupee_Links_House'],
        // Ammo
        ['Category', 'Ammo'],
        [0x0a, 'Bombs_5'],
        [0x0b, 'Bombs_10'],
        [0x0c, 'Bombs_20'],
        [0x0d, 'Bombs_30'],
        [0x0e, 'Arrows_10'],
        [0x0f, 'Arrows_20'],
        [0x10, 'Arrows_30'],
        [0x12, 'Seeds_50'],
        [0x16, 'Water_Bombs_5'],
        [0x17, 'Water_Bombs_10'],
        [0x18, 'Water_Bombs_15'],
        [0x1a, 'Bomblings_5'],
        [0x1b, 'Bomblings_10'],
        // Misc
        ['Category', 'Misc'],
        [0x2a, 'Ordon_Shield'],
        [0x2b, 'Wooden_Shield'],
        [0x2c, 'Hylian_Shield'],
        [0x30, 'Magic_Armor'],
        [0xe0, 'Poe_Soul'],
        [0x21, 'Piece_of_Heart'],
        [0x22, 'Heart_Container'],
        [0xe1, 'Progressive_Hidden_Skill'],
        [0x35, 'Progressive_Wallet'],
        [0x4f, 'Giant_Bomb_Bag'],
        [0x60, 'Empty_Bottle'],
        [0x65, 'Sera_Bottle'],
        [0x75, 'Jovani_Bottle'],
        [0x9d, 'Coro_Bottle'],
        [0x3e, 'Hawkeye'],
        [0x84, 'Horse_Call'],
        [0x13, 'Foolish_Item'],
      ];

      const plandoItemEls = plandoItems
        .map((item) => {
          if (item[0] === 'FirstCategory') {
            return `<optgroup label='${item[1]}'>`;
          } else if (item[0] === 'Category') {
            return `</optgroup><optgroup label='${item[1]}'>`;
          } else {
            return `<option value='${item[0]}'>${item[1]}</option>`;
          }
        })
        .join('\n');
      const plandoChecksEls = Object.keys(excludedChecksList)
        .map((key) => {
          return `<option value='${excludedChecksList[key]}'>${key}</option>`;
        })
        .join('\n');
      const plandoStr = `<select id=plandoCheckSelect>${plandoChecksEls}</select>
                        <select id=plandoItemSelect>${plandoItemEls}</select>`;
      msg = msg.replace('<!-- PLANDO -->', plandoStr);

      res.send(msg);
    }
  });
});

// const escapeHtml = (str: string) =>
//   str.replace(
//     /[&<>'"]/g,
//     (tag: string) =>
//       ({
//         '&': '&amp;',
//         '<': '&lt;',
//         '>': '&gt;',
//         "'": '&#39;',
//         '"': '&quot;',
//       }[tag])
//   );

type HtmlCharMap = {
  [key: string]: string;
};

const abc: HtmlCharMap = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  "'": '&#39;',
  '"': '&quot;',
};

function escapeHtml(str: string) {
  console.log(77);

  return str.replace(/[&<>'"]/g, (tag: string) => {
    return abc[tag] || '';
  });
}

function genSpoilerData(inputJsonObj: string) {
  return JSON.parse(JSON.stringify(inputJsonObj));
}

app.get('/s/:id', (req: express.Request, res: express.Response) => {
  fs.readFile(path.join(root, 'getseed.html'), function read(err, data) {
    if (err) {
      console.log(err);
      res.status(500).send({ error: 'Internal server error.' });
    } else {
      let msg = data.toString();

      // const { id } = <{ id: string }>req.query;
      // if (!id || typeof id )

      const { id } = req.params;

      if (!id) {
        res.status(400).send({ error: 'Malformed request.' });
        return;
      }

      msg = msg.replace(
        '<!-- USER_ID -->',
        `<input id="userJwtInput" type="hidden" value="${req.newUserJwt}">`
      );

      // const filePath = path.join(__dirname, `seeds/${id}/input.json`);
      const filePath = resolveOutputPath(`seeds/${id}/input.json`);

      if (fs.existsSync(filePath)) {
        // Completely done generating
        const json = JSON.parse(
          fs.readFileSync(filePath, { encoding: 'utf8' })
        );

        json.output.seedHash = undefined;
        json.output.itemPlacement = undefined;
        // Stringifying will get rid of these undefined values. We don't want to
        // expose certain values, especially if it is a race seed.
        const fileContents = escapeHtml(JSON.stringify(json));

        msg = msg.replace(
          '<!-- INPUT_JSON_DATA -->',
          `<input id='inputJsonData' type='hidden' value='${fileContents}'>`
        );
        msg = msg.replace('<!-- REQUESTER_HASH -->', '');

        const spoilerData = escapeHtml(
          callGenerator('print_seed_gen_results', id)
        );

        msg = msg.replace(
          '<!-- SEED_GEN_RESULTS -->',
          `<input id='spoilerData' type='hidden' value='${spoilerData}'>`
        );
      } else {
        const { seedGenStatus } = checkProgress(id);
        if (seedGenStatus) {
          // Generation requested but not completed and not 100% forgotten about
          // by the server.
          msg = msg.replace('<!-- INPUT_JSON_DATA -->', '');
          msg = msg.replace(
            '<!-- REQUESTER_HASH -->',
            `<input id='requesterHash' type='hidden' value='${seedGenStatus.requesterHash}'>`
          );
          msg = msg.replace('<!-- SEED_GEN_RESULTS -->', '');
        } else {
          // Have no idea what the client is talking about with that seedId.
          msg = msg.replace('<!-- INPUT_JSON_DATA -->', '');
          msg = msg.replace('<!-- REQUESTER_HASH -->', '');
          msg = msg.replace('<!-- SEED_GEN_RESULTS -->', '');
        }
      }

      res.send(msg);
    }
  });
});

app.use(express.static(root));

app.use(function (req: express.Request, res: express.Response, next) {
  res.status(404);

  const accepts = req.accepts(['text/html', 'application/json']);

  if (accepts === 'applicaton/json') {
    res.json({ error: 'Not found' });
  } else {
    res.type('txt').send('Not found');
  }
});

const port = process.env.SERVER_PORT || 3500;
logger.info(`Server will listen on port: ${port}`);

// start express server
app.listen(port, () => {
  logger.info('Server started.');
});

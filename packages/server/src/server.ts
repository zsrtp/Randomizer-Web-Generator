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
        // Let the generator decide based on other settings
        [-1, 'Random'],

        // Rupees
        ['FirstCategory', 'Rupees'],
        [1, 'Green_Rupee'],
        [2, 'Blue_Rupee'],
        [3, 'Yellow_Rupee'],
        [4, 'Red_Rupee'],
        [5, 'Purple_Rupee'],
        [6, 'Orange_Rupee'],
        [7, 'Silver_Rupee'],
        [237, 'Purple_Rupee_Links_House'],

        // Ammo
        ['Category', 'Ammo'],
        [10, 'Bombs_5'],
        [11, 'Bombs_10'],
        [12, 'Bombs_20'],
        [13, 'Bombs_30'],
        [17, 'Arrows_1'],
        [14, 'Arrows_10'],
        [15, 'Arrows_20'],
        [16, 'Arrows_30'],
        [18, 'Seeds_50'],
        [25, 'Water_Bombs_3'],
        [22, 'Water_Bombs_5'],
        [23, 'Water_Bombs_10'],
        [24, 'Water_Bombs_15'],
        [29, 'Bombling_1'],
        [28, 'Bomblings_3'],
        [26, 'Bomblings_5'],
        [27, 'Bomblings_10'],

        // Progression Items
        ['Category', 'Progression Items'],
        [48, 'Magic_Armor'],
        [49, 'Zora_Armor'],
        [50, 'Shadow_Crystal'],
        [53, 'Progressive_Wallet'],
        [62, 'Hawkeye'],
        [63, 'Progressive_Sword'],
        [64, 'Boomerang'],
        [65, 'Spinner'],
        [66, 'Ball_and_Chain'],
        [67, 'Progressive_Bow'],
        [68, 'Progressive_Clawshot'],
        [69, 'Iron_Boots'],
        [70, 'Progressive_Dominion_Rod'],
        [72, 'Lantern'],
        [74, 'Progressive_Fishing_Rod'],
        [75, 'Slingshot'],
        [81, 'Filled_Bomb_Bag'],
        [144, 'Aurus_Memo'],
        [145, 'Asheis_Sketch'],
        [233, 'Progressive_Sky_Book'],

        // Small Keys
        ['Category', 'Small Keys'],
        [133, 'Forest_Temple_Small_Key'],
        [134, 'Goron_Mines_Small_Key'],
        [135, 'Lakebed_Temple_Small_Key'],
        [136, 'Arbiters_Grounds_Small_Key'],
        [137, 'Snowpeak_Ruins_Small_Key'],
        [138, 'Temple_of_Time_Small_Key'],
        [139, 'City_in_The_Sky_Small_Key'],
        [140, 'Palace_of_Twilight_Small_Key'],
        [141, 'Hyrule_Castle_Small_Key'],
        [142, 'Gerudo_Desert_Bulblin_Camp_Key'],
        [238, 'North_Faron_Woods_Gate_Key'],
        [243, 'Gate_Keys'],
        [244, 'Snowpeak_Ruins_Ordon_Pumpkin'],
        [245, 'Snowpeak_Ruins_Ordon_Goat_Cheese'],

        // Big Keys
        ['Category', 'Big Keys'],
        [146, 'Forest_Temple_Big_Key'],
        [147, 'Lakebed_Temple_Big_Key'],
        [148, 'Arbiters_Grounds_Big_Key'],
        [246, 'Snowpeak_Ruins_Bedroom_Key'],
        [149, 'Temple_of_Time_Big_Key'],
        [150, 'City_in_The_Sky_Big_Key'],
        [151, 'Palace_of_Twilight_Big_Key'],
        [152, 'Hyrule_Castle_Big_Key'],
        [249, 'Goron_Mines_Key_Shard'],

        // Compasses
        ['Category', 'Compasses'],
        [153, 'Forest_Temple_Compass'],
        [154, 'Goron_Mines_Compass'],
        [155, 'Lakebed_Temple_Compass'],
        [168, 'Arbiters_Grounds_Compass'],
        [169, 'Snowpeak_Ruins_Compass'],
        [170, 'Temple_of_Time_Compass'],
        [171, 'City_in_The_Sky_Compass'],
        [172, 'Palace_of_Twilight_Compass'],
        [173, 'Hyrule_Castle_Compass'],

        //Maps
        ['Category', 'Maps'],
        [182, 'Forest_Temple_Dungeon_Map'],
        [183, 'Goron_Mines_Dungeon_Map'],
        [184, 'Lakebed_Temple_Dungeon_Map'],
        [185, 'Arbiters_Grounds_Dungeon_Map'],
        [186, 'Snowpeak_Ruins_Dungeon_Map'],
        [187, 'Temple_of_Time_Dungeon_Map'],
        [188, 'City_in_The_Sky_Dungeon_Map'],
        [189, 'Palace_of_Twilight_Dungeon_Map'],
        [190, 'Hyrule_Castle_Dungeon_Map'],

        // Bugs
        ['Category', 'Bugs'],
        [192, 'Male_Beetle'],
        [193, 'Female_Beetle'],
        [194, 'Male_Butterfly'],
        [195, 'Female_Butterfly'],
        [196, 'Male_Stag_Beetle'],
        [197, 'Female_Stag_Beetle'],
        [198, 'Male_Grasshopper'],
        [199, 'Female_Grasshopper'],
        [200, 'Male_Phasmid'],
        [201, 'Female_Phasmid'],
        [202, 'Male_Pill_Bug'],
        [203, 'Female_Pill_Bug'],
        [204, 'Male_Mantis'],
        [205, 'Female_Mantis'],
        [206, 'Male_Ladybug'],
        [207, 'Female_Ladybug'],
        [208, 'Male_Snail'],
        [209, 'Female_Snail'],
        [210, 'Male_Dragonfly'],
        [211, 'Female_Dragonfly'],
        [212, 'Male_Ant'],
        [213, 'Female_Ant'],
        [214, 'Male_Dayfly'],
        [215, 'Female_Dayfly'],

        // Skills
        ['Category', 'Skills'],
        [225, 'Progressive_Hidden_Skill'],
        [226, 'Shield_Attack'],
        [227, 'Back_Slice'],
        [228, 'Helm_Splitter'],
        [229, 'Mortal_Draw'],
        [230, 'Jump_Strike'],
        [231, 'Great_Spin'],

        // Boss Items
        ['Category', 'Dungeon Rewards'],
        [165, 'Progressive_Mirror_Shard'],
        [216, 'Progressive_Fused_Shadow'],

        // Misc
        ['Category', 'Misc'],
        [42, 'Ordon_Shield'],
        [43, 'Wooden_Shield'],
        [44, 'Hylian_Shield'],
        [79, 'Giant_Bomb_Bag'],
        [96, 'Empty_Bottle'],
        [132, 'Horse_Call'],
        [224, 'Poe_Soul'],
        [0, 'Recovery_Heart'],
        [33, 'Piece_of_Heart'],
        [34, 'Heart_Container'],
        [19, 'Foolish_Item'],
        [20, 'Foolish_Item_2'],
        [21, 'Foolish_Item_3'],
      ];

      const plandoItemEls = plandoItems
        .map((item) => {
          if (item[0] == 'FirstCategory') {
            return `<optgroup label='${item[1]}'>`;
          } else if (item[0] == 'Category') {
            return `</optgroup><optgroup label='${item[1]}'>`;
          } else {
            return `<option value='${item[0]}'>${item[1]}</option>`;
          }
        })
        .join('\n');
      const plandoEls = Object.keys(excludedChecksList).map((key) => {
        return `<li class='plandoListItem'>
              <label>${key}</label>
              <select class='plandoCheckSelect' data-checkId='${excludedChecksList[key]}'>${plandoItemEls}</optgroup></select>
            </li>`;
      });

      msg = msg.replace('<!-- PLANDO -->', plandoEls.join('\n'));

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

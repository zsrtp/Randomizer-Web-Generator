const express = require('express');
const { queueSeedGeneration } = require('./controllers');
const SlowFastQueue = require('./SlowFastQueue');

const queuesMgr = new SlowFastQueue();

setInterval(() => {
  queuesMgr.filterFastQueue((queueItem) => {
    return true; // true indicates gets to stay
  });
}, 1000 * 15);

const app = express();

app.post('/api/generateseed', queueSeedGeneration);

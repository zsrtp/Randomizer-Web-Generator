const fs = require('fs-extra');
const { resolveOutputPath } = require('./config');

const byId = {};
const fastQueue = [];
const slowQueue = [];

function genUniqueId() {
  let id;

  while (true) {
    // id = Util.Hash.GenId();
    id = '';

    // Check if in queue or if this id has been generated before.
    if (
      !byId[id] &&
      !fs.existsSync(resolveOutputPath('seeds', id, 'input.json'))
    ) {
      return id;
    }
  }
}

function getQueueItem(id) {
  return byId[id];
}

// add an item to the fast queue
function addToFastQueue(def) {
  // this will handle generating the id
}

// add an item to the slow queue
function addToSlowQueue(def) {
  // this will handle generating the id
}

// move an item from the fast queue to the slow queue
function swapToSlowQueue(id) {}

// pass callback to determine which objects to remove from the fast queue
function filterFastQueue(cb) {}

module.exports = {
  getQueueItem,
  addToFastQueue,
  addToSlowQueue,
  swapToSlowQueue,
  filterFastQueue,
};

// setInterval(() => {
//   queuesMgr.filterFastQueue((queueItem) => {
//     return true; // true indicates gets to stay
//   });
// }, 1000 * 15);

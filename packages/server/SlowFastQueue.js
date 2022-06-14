class SlowFastQueue {
  constructor() {
    this.byId = {};
    this.fastQueue = [];
    this.slowQueue = [];
  }

  getQueueItem(id) {
    return this.byId[id];
  }

  // add an item to the fast queue
  addToFastQueue(def) {
    // this will handle generating the id
  }

  // add an item to the slow queue
  addToSlowQueue(def) {
    // this will handle generating the id
  }

  // move an item from the fast queue to the slow queue
  swapToSlowQueue(id) {}

  // pass callback to determine which objects to remove from the fast queue
  filterFastQueue(cb) {}
}

module.exports = SlowFastQueue;

// In the main file, want to generate the queues there and also start the
// process which handles cleaning them up.

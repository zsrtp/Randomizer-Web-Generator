import path from 'path';

interface searchUpFileTreeCb {
  (currentPath: string): boolean;
}

function searchUpFileTree(startDir: string, cb: searchUpFileTreeCb): string {
  let prevPath = null;
  let currPath = startDir;

  while (true) {
    if (currPath === prevPath) {
      return null;
    }

    if (cb(currPath)) {
      return currPath;
    }

    prevPath = currPath;
    currPath = path.dirname(currPath);
  }
}

export default searchUpFileTree;

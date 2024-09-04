import { spawnChild } from './spawn-child.js';

const addonDir = import.meta.dirname.split('\\').slice(0, -4).join('\\');
const exePath =
  import.meta.dirname.split('\\').slice(0, -1).join('\\') +
  '\\bin\\ValveTypeScriptConverter.exe';

const relDstPath = import.meta.dirname.split('\\').slice(-2, -1).join('\\');
const relSrcPath = relDstPath + '\\src';

await spawnChild(exePath, [relSrcPath, relDstPath, addonDir]);

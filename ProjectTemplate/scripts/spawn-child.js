import { spawn } from 'child_process';

export const spawnChild = async (path, args) => {
  console.log(`Running: ${path} ${args.join(' ')}`);
  const child = spawn(path, args);

  child.stdout.setEncoding('utf8');
  child.stdout.on('data', (data) => process.stdout.write(data.toString()));

  child.stderr.setEncoding('utf8');
  child.stderr.on('data', (data) => process.stderr.write(data.toString()));

  return new Promise((resolve) => child.on('close', resolve));
};

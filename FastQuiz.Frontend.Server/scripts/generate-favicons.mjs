// Optional asset maintenance: install sharp locally or set FASTQUIZ_SHARP_MODULE
// to its absolute module entry. Normal application builds need no image tooling.
import { createRequire } from 'node:module';
import { readFile, writeFile } from 'node:fs/promises';

const require = createRequire(import.meta.url);
const sharp = require(process.env.FASTQUIZ_SHARP_MODULE || 'sharp');
const directory = new URL('../public/', import.meta.url);
const svg = await readFile(new URL('favicon.svg', directory));
const sizes = [16, 32, 192, 512, 2048];
for (const size of sizes) {
  await sharp(svg, { density: 300 }).resize(size, size).png()
    .toFile(new URL(`favicon-${size}x${size}.png`, directory).pathname);
}
// Apple masks touch icons itself; fill the tile to avoid a second rounded edge.
const touchSvg = Buffer.from(svg.toString().replace('rx="18"', 'rx="0"'));
await sharp(touchSvg, { density: 300 }).resize(180, 180).png()
  .toFile(new URL('apple-touch-icon.png', directory).pathname);

// ICO directory with embedded lossless PNG frames; no platform-specific converter.
const icoSizes = [16, 32, 48, 64, 128, 256];
const frames = await Promise.all(icoSizes.map(size =>
  sharp(svg, { density: 300 }).resize(size, size).png().toBuffer()));
const header = Buffer.alloc(6 + frames.length * 16);
header.writeUInt16LE(1, 2);
header.writeUInt16LE(frames.length, 4);
let offset = header.length;
frames.forEach((frame, index) => {
  const entry = 6 + index * 16;
  header[entry] = header[entry + 1] = icoSizes[index] === 256 ? 0 : icoSizes[index];
  header.writeUInt16LE(1, entry + 4);
  header.writeUInt16LE(32, entry + 6);
  header.writeUInt32LE(frame.length, entry + 8);
  header.writeUInt32LE(offset, entry + 12);
  offset += frame.length;
});
await writeFile(new URL('favicon.ico', directory), Buffer.concat([header, ...frames]));
console.log('Generated black FastQuiz favicon, ICO and touch icons from favicon.svg.');

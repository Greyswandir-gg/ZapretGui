import { createReadStream, existsSync, statSync } from 'node:fs';
import { createServer } from 'node:http';
import { extname, join, normalize } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('.', import.meta.url));
const port = Number(process.env.PORT ?? 4173);
const types = new Map([
  ['.html', 'text/html; charset=utf-8'],
  ['.css', 'text/css; charset=utf-8'],
  ['.js', 'text/javascript; charset=utf-8'],
  ['.json', 'application/json; charset=utf-8'],
  ['.svg', 'image/svg+xml; charset=utf-8'],
]);

function resolvePath(url) {
  const pathname = decodeURIComponent(new URL(url, `http://localhost:${port}`).pathname);
  const safePath = normalize(pathname).replace(/^([.][.][/\\])+/, '');
  const filePath = join(root, safePath === '/' ? 'index.html' : safePath);

  return existsSync(filePath) && statSync(filePath).isFile() ? filePath : join(root, 'index.html');
}

createServer((request, response) => {
  const filePath = resolvePath(request.url ?? '/');
  response.setHeader('Content-Type', types.get(extname(filePath)) ?? 'application/octet-stream');
  createReadStream(filePath).pipe(response);
}).listen(port, () => {
  // Keep this message stable: deploy agents can parse it to detect the local preview URL.
  console.log(`artist-site listening on http://localhost:${port}`);
});

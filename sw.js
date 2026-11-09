const appRoot = new URL(self.registration.scope);
const home = appRoot.href;
const indexPath = new URL('index.html', appRoot).pathname;
const audioPath = new URL('__musiko_audio', appRoot).pathname;
const cachePrefix = `musiko-shell:${appRoot.pathname}:`;
const cacheName = cachePrefix + 'v2';
const assets = new Set(['manifest.webmanifest', 'icon-192.png', 'icon-512.png']
  .map(file => new URL(file, appRoot).href));

self.addEventListener('install', event => {
  event.waitUntil((async () => {
    const cache = await caches.open(cacheName);
    await cache.addAll([home, ...assets].map(url => new Request(url, { cache: 'reload' })));
    await self.skipWaiting();
  })());
});

self.addEventListener('activate', event => {
  event.waitUntil((async () => {
    const keys = await caches.keys();
    await Promise.all(keys.filter(key => key.startsWith(cachePrefix) && key !== cacheName)
      .map(key => caches.delete(key)));
    await self.clients.claim();
  })());
});

self.addEventListener('message', event => {
  if (event.data === 'musiko:local-audio') event.ports[0]?.postMessage(true);
});

let libraryDatabase;
function openLibrary() {
  if (!libraryDatabase) {
    libraryDatabase = new Promise((resolve, reject) => {
      const request = indexedDB.open('MusikoLibrary');
      request.onupgradeneeded = () => request.transaction.abort();
      request.onerror = () => reject(request.error);
      request.onsuccess = () => {
        const db = request.result;
        db.onversionchange = () => { db.close(); libraryDatabase = undefined; };
        db.onclose = () => { libraryDatabase = undefined; };
        resolve(db);
      };
    }).catch(error => { libraryDatabase = undefined; throw error; });
  }
  return libraryDatabase;
}

async function readTrack(id) {
  const db = await openLibrary();
  return new Promise((resolve, reject) => {
    const transaction = db.transaction('tracks', 'readonly');
    const request = transaction.objectStore('tracks').get(id);
    transaction.onabort = () => reject(transaction.error);
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
}

async function loadAudio(request, id) {
  if (!id) return new Response(null, { status: 404 });
  let track;
  try {
    track = await readTrack(id);
  } catch {
    return new Response(null, { status: 503 });
  }
  const blob = track?.blob;
  if (!blob || !blob.size) return new Response(null, { status: 404 });
  const size = blob.size;
  const headers = new Headers({
    'Accept-Ranges': 'bytes',
    'Cache-Control': 'no-store',
    'Content-Type': track.type || blob.type || 'application/octet-stream',
    'Content-Length': String(size)
  });
  if (request.method === 'HEAD') return new Response(null, { headers });

  const range = /^bytes=(\d*)-(\d*)$/i.exec(request.headers.get('Range') || '');
  if (!range || (!range[1] && !range[2])) return new Response(blob, { headers });

  const start = range[1] ? Number(range[1]) : Math.max(0, size - Number(range[2]));
  const end = range[1] && range[2] ? Math.min(Number(range[2]), size - 1) : size - 1;
  if (start >= size || start > end) {
    headers.set('Content-Range', `bytes */${size}`);
    headers.set('Content-Length', '0');
    return new Response(null, { status: 416, headers });
  }
  headers.set('Content-Range', `bytes ${start}-${end}/${size}`);
  headers.set('Content-Length', String(end - start + 1));
  return new Response(blob.slice(start, end + 1), { status: 206, headers });
}

async function loadHome(request) {
  const response = await fetch(request).catch(() => null);
  if (response && response.status < 500) {
    if (response.ok && !response.redirected && response.headers.get('Content-Type')?.includes('text/html')) {
      try {
        const cache = await caches.open(cacheName);
        await cache.put(home, response.clone());
      } catch (error) {
        console.warn('Musiko could not update its offline page:', error);
      }
    }
    return response;
  }
  const cached = await caches.match(home, { cacheName });
  return cached || response || Response.error();
}

self.addEventListener('fetch', event => {
  const request = event.request;
  const url = new URL(request.url);
  if (url.origin !== appRoot.origin) return;
  if (url.pathname === audioPath && (request.method === 'GET' || request.method === 'HEAD')) {
    event.respondWith(loadAudio(request, url.searchParams.get('id')));
    return;
  }
  if (request.method !== 'GET') return;

  if (request.mode === 'navigate' && (url.pathname === appRoot.pathname || url.pathname === indexPath)) {
    event.respondWith(loadHome(request));
  } else if (assets.has(url.href)) {
    event.respondWith(caches.match(request, { cacheName }).then(cached => cached || fetch(request)));
  }
});

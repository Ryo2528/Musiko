const appRoot = new URL(self.registration.scope);
const home = appRoot.href;
const indexPath = new URL('index.html', appRoot).pathname;
const cachePrefix = `musiko-shell:${appRoot.pathname}:`;
const cacheName = cachePrefix + 'v1';
const assets = new Set(['manifest.webmanifest', 'icon-192.png', 'icon-512.png']
  .map(file => new URL(file, appRoot).href));

self.addEventListener('install', event => {
  event.waitUntil((async () => {
    const cache = await caches.open(cacheName);
    await cache.addAll([home, ...assets].map(url => new Request(url, { cache: 'reload' })));
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
  if (request.method !== 'GET' || url.origin !== appRoot.origin) return;

  if (request.mode === 'navigate' && (url.pathname === appRoot.pathname || url.pathname === indexPath)) {
    event.respondWith(loadHome(request));
  } else if (assets.has(url.href)) {
    event.respondWith(caches.match(request, { cacheName }).then(cached => cached || fetch(request)));
  }
});

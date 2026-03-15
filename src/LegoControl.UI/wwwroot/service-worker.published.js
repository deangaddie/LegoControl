// Production service worker for LegoControl PWA.
// Caches all app assets listed in service-worker-assets.js for offline use.
// Blazor's publish tooling generates service-worker-assets.js automatically.

self.importScripts('./service-worker-assets.js');

const cacheNamePrefix = 'legocontrol-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

const offlineAssetsInclude = [
    /\.dll$/, /\.pdb$/, /\.wasm/, /\.html$/, /\.js$/, /\.json$/,
    /\.css$/, /\.woff2?$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.svg$/,
    /\.blat$/, /\.dat$/
];
const offlineAssetsExclude = [/^service-worker\.js$/];

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => {
    // Sent by pwa-registration.js when the user clicks "Refresh"
    if (event.data?.type === 'SKIP_WAITING') self.skipWaiting();
});

async function onInstall() {
    console.info('[SW] Installing LegoControl service worker...');

    const assetsRequests = self.assetsManifest.assets
        .filter(a => offlineAssetsInclude.some(p => p.test(a.url)))
        .filter(a => !offlineAssetsExclude.some(p => p.test(a.url)))
        .map(a => new Request(a.url, { integrity: a.hash, cache: 'no-cache' }));

    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
    console.info(`[SW] Cached ${assetsRequests.length} assets.`);
}

async function onActivate() {
    console.info('[SW] Activating...');
    const cacheKeys = await caches.keys();
    await Promise.all(
        cacheKeys
            .filter(k => k.startsWith(cacheNamePrefix) && k !== cacheName)
            .map(k => caches.delete(k))
    );
    await clients.claim();
}

async function onFetch(event) {
    if (event.request.method !== 'GET') return fetch(event.request);

    // Navigation requests → serve cached index.html for SPA routing
    const request = event.request.mode === 'navigate' ? 'index.html' : event.request;
    const cache = await caches.open(cacheName);
    const cached = await cache.match(request);
    return cached ?? fetch(event.request);
}

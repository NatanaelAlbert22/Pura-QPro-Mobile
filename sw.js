const CACHE_NAME = "mqpro-cache-v1";
const urlsToCache = [
    "Default.aspx",
    "Login.aspx",
    "Laporan.aspx",
    "Master.aspx",
    "manifest.json",
    "Icon/pura192.png",
    "Icon/pura512.png",
    "Content/bootstrap.css",
    "Content/bootstrap.min.css",
    "Content/Site.css",
    "Scripts/jquery-3.7.0.min.js",
    "Scripts/bootstrap.min.js"
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(cacheName)
            .then((cache) => {
                console.log('Opened cache');
                return cache.addAll(urlsToCache);
            })
    );
});

self.addEventListener('fetch', (event) => {
    event.respondWith(
        caches.match(event.request)
            .then((response) => {
                // Serve from cache or fetch from network
                return response || fetch(event.request);
            })
            .catch(() => {
                // Optional: Add fallback for offline (e.g., offline.html)
            })
    );
});

self.addEventListener('activate', (event) => {
    const cacheWhitelist = [cacheName];
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cache) => {
                    if (!cacheWhitelist.includes(cache)) {
                        return caches.delete(cache);
                    }
                })
            );
        })
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(self.clients.claim());
});
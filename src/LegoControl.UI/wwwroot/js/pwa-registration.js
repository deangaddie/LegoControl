// PWA service worker registration and update detection.
// Imported as an ES module by PwaUpdateChecker.razor.

let _waitingWorker = null;
let _dotNetRef = null;

function notifyUpdate(worker) {
    _waitingWorker = worker;
    window._pwaUpdateWaiting = true;
    if (_dotNetRef) {
        _dotNetRef.invokeMethodAsync('OnUpdateAvailable');
    }
}

export function registerAndWatch(dotNetRef) {
    _dotNetRef = dotNetRef;

    if (!('serviceWorker' in navigator)) return;

    navigator.serviceWorker.register('service-worker.js').then(reg => {
        // A new SW is already waiting (e.g. user refreshed the tab)
        if (reg.waiting) {
            notifyUpdate(reg.waiting);
        }

        // A new SW starts installing after the page loads
        reg.addEventListener('updatefound', () => {
            const newWorker = reg.installing;
            newWorker.addEventListener('statechange', () => {
                // 'installed' + controller present means an update is ready
                if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                    notifyUpdate(newWorker);
                }
            });
        });
    });
}

export function applyUpdate() {
    if (_waitingWorker) {
        // Tell the waiting SW to activate immediately
        _waitingWorker.postMessage({ type: 'SKIP_WAITING' });
    }
    // Reload once the new SW takes control
    navigator.serviceWorker.addEventListener('controllerchange', () => location.reload());
}

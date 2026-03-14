const SERVICE_UUID        = '00001623-1212-efde-1623-785feabcd123';
const CHARACTERISTIC_UUID = '00001624-1212-efde-1623-785feabcd123';

let device = null;
let characteristic = null;

export function isSupported() {
    return !!navigator.bluetooth;
}

export async function connect(dotNetCallback) {
    device = await navigator.bluetooth.requestDevice({
        filters: [{ services: [SERVICE_UUID] }]
    });

    device.addEventListener('gattserverdisconnected', () => {
        dotNetCallback.invokeMethodAsync('OnDisconnected');
    });

    const server = await device.gatt.connect();
    const service = await server.getPrimaryService(SERVICE_UUID);
    characteristic = await service.getCharacteristic(CHARACTERISTIC_UUID);

    await characteristic.startNotifications();
    characteristic.addEventListener('characteristicvaluechanged', (e) => {
        const bytes = Array.from(new Uint8Array(e.target.value.buffer));
        dotNetCallback.invokeMethodAsync('OnNotification', bytes);
    });

    return device.name ?? 'Lego Device';
}

export async function disconnect() {
    if (device?.gatt.connected) {
        device.gatt.disconnect();
    }
}

export async function sendCommand(data) {
    if (characteristic) {
        await characteristic.writeValueWithoutResponse(new Uint8Array(data));
    }
}

export function hasTouchSupport() {
    return navigator.maxTouchPoints > 0 || 'ontouchstart' in window;
}

export function capturePointer(element, pointerId) {
    element.setPointerCapture(pointerId);
}

export function getBoundingRect(element) {
    const r = element.getBoundingClientRect();
    return { left: r.left, top: r.top, width: r.width, height: r.height };
}

export function hasGamepadSupport() {
    return 'getGamepads' in navigator;
}

export function pollGamepad() {
    for (const gp of navigator.getGamepads()) {
        if (gp) return {
            id: gp.id,
            axes: Array.from(gp.axes),
            buttons: gp.buttons.map(b => ({ pressed: b.pressed, value: b.value }))
        };
    }
    return null;
}

let _gpConnHandler = null, _gpDiscHandler = null;

export function listenForGamepad(dotNetCallback) {
    _gpConnHandler = e => dotNetCallback.invokeMethodAsync('OnGamepadConnected', e.gamepad.index);
    _gpDiscHandler = () => dotNetCallback.invokeMethodAsync('OnGamepadDisconnected');
    window.addEventListener('gamepadconnected', _gpConnHandler);
    window.addEventListener('gamepaddisconnected', _gpDiscHandler);
}

export function unlistenGamepad() {
    if (_gpConnHandler) window.removeEventListener('gamepadconnected', _gpConnHandler);
    if (_gpDiscHandler) window.removeEventListener('gamepaddisconnected', _gpDiscHandler);
    _gpConnHandler = _gpDiscHandler = null;
}

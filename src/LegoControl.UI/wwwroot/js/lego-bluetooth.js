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

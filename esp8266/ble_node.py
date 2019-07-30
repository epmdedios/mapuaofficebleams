import Mcu
import Streams
import Socket
from Espressif.Esp32ble import Esp32ble as BleDriver
from Espressif.Esp32net import Esp32wifi as WifiDriver
from Wireless import Ble, Ble_beacons as BleTools, Wifi

DEVICE_ID     = "PN1"
NETWORK_SSID  = "dediosnet"
NETWORK_KEY   = "DeD!os24180501"
SERVER_IP     = "192.168.1.5"
SERVER_PORT   = 55556
SCAN_DURATION = 10000
SOCKET_STREAM = None
MAX_RETRIES   = 10

def start_scan(data):
    pass_count += 1
    if pass_count == 6:
        Mcu.reset()
    else:
        print("STARTED", pass_count)

def restart_scan(data):
    print("STOPPED")
    Ble.start_scanning(SCAN_DURATION)
    print("RESTARTED")

def report_scan(data):
    try:
        _, _, _, tx_power = BleTools.ibeacon_decode(data[3])
        stream = (DEVICE_ID
                  + " "
                  + str(tx_power)
                  + " "
                  + str(data[2])
                  + " "
                  + Ble.btos(data[4])
                  + " ")
        SOCKET_STREAM.sendall(stream)
        print(stream)
    except ValueError:
        pass
    except Exception as e:
        print("Can't connect to", SERVER_IP)
        Mcu.reset()

pass_count = 0
Streams.serial()

try:
    BleDriver.init()
    WifiDriver.auto_init()

    for retries in range(MAX_RETRIES):
        try:
            Wifi.link(NETWORK_SSID, Wifi.WIFI_WPA2, NETWORK_KEY)
            print("Connected to", NETWORK_SSID)
            break
        except Exception as e:
            print("Can't connect to", NETWORK_SSID)
            sleep(1000)
    else:
        Mcu.reset()

    for retries in range(MAX_RETRIES):
        try:
            SOCKET_STREAM = Socket.socket()
            SOCKET_STREAM.connect((SERVER_IP, SERVER_PORT))
            print("Connected to", SERVER_IP)
            break
        except Exception as e:
            print("Can't connect to", SERVER_IP)
    else:
        Mcu.reset()

    Ble.gap("Partex")
    Ble.add_callback(Ble.EVT_SCAN_REPORT, report_scan)
    Ble.add_callback(Ble.EVT_SCAN_STARTED, start_scan)
    Ble.add_callback(Ble.EVT_SCAN_STOPPED, restart_scan)
    Ble.scanning(50, 50)
    Ble.start()
    Ble.start_scanning(SCAN_DURATION)

except Exception as e:
    print("Unexpected error")
    Mcu.reset()

while True:
    sleep(1000)

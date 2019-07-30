import mcu
import streams
import socket
from espressif.esp8266wifi import esp8266wifi as wifidriver
from wireless import wifi

NETWORK_SSID  = "dediosnet"
NETWORK_KEY   = "DeD!os24180501"
SERVER_IP     = "192.168.1.5"
SERVER_PORT   = 55555
SOCKET_STREAM = None

streams.serial()

wifidriver.auto_init()

for retries in range(10):
    try:
        wifi.link(NETWORK_SSID, wifi.WIFI_WPA2, NETWORK_KEY)
        print("Connected to Wi-Fi")
        break
    except Exception:
        print("Can't connect to Wi-Fi")
        sleep(1000)
else:
    mcu.reset()
for retries in range(10):
    try:
        SOCKET_STREAM = socket.socket()
        SOCKET_STREAM.connect((SERVER_IP, SERVER_PORT))
        print("Connected to server")
        break
    except Exception:
        print("Can't connect to server")
else:
    mcu.reset()
while True:
    print(str(digitalRead(D1)))
    try:
        SOCKET_STREAM.sendall(str(digitalRead(D1)))
        sleep(100)
    except IOError:
        mcu.reset()

import serial
import time
import utils

s = serial.Serial('COM6', timeout=1, baudrate=115200)

if __name__ == '__main__':

   print("start")
   
   while True:
      print("******")
      #req = bytearray([1, 0x2B, 0xE, 0x1, 0x1])
      req = bytearray([1, 0x6, 0x0, 0x0, 0x0, 0x4])
      body = list(req)
      crc =utils.computeCRC(body)
      body.append((0xFF00&crc)>>8)
      body.append(0x00FF&crc)
      print("-->",str(body))
      s.write(body)	
      time.sleep(0.3)
      res = s.read_all()
      print("<--",str(res))
      print("size:", len(res))
      time.sleep(0.5)







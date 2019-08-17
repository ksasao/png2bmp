import audio
import gc
import image
import lcd
import sensor
import time
import uos
from fpioa_manager import *
from machine import I2C
from Maix import I2S, GPIO

lcd.init()
lcd.rotation(2)
i2c = I2C(I2C.I2C0, freq=400000, scl=28, sda=29)

img_overlay = image.Image("/sd/circle.bmp")
img_mask = image.Image("/sd/circle.m.bmp")

err_counter = 0
while 1:
    try:
        sensor.reset() #Reset sensor may failed, let's try some times
        break
    except:
        err_counter = err_counter + 1
        if err_counter == 20:
            lcd.draw_string(lcd.width()//2-100,lcd.height()//2-4, "Error: Sensor Init Failed", lcd.WHITE, lcd.RED)
        time.sleep(0.1)
        continue

sensor.set_pixformat(sensor.RGB565)
sensor.set_framesize(sensor.QVGA) #QVGA=320x240
sensor.set_windowing((224, 224))
sensor.run(1)

try:
    while(True):
        img = sensor.snapshot()
        img.draw_image(img_overlay,80,80,mask=img_mask)
        lcd.display(img)
except KeyboardInterrupt:
    kpu.deinit(task)
    sys.exit()

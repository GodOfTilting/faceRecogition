import cv2
import time
import numpy as np

##########
wCam, hCam = 1280,720
##########

cap = cv2.VideoCapture(0)
cap.set(3,wCam)
cap.set(4,hCam)

while True:
    siccess, img = cap.read()
    
    cv2.imshow("Img", img)
    cv2.waitKey(1)
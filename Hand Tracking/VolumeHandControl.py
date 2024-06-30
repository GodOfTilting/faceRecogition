import cv2
import time
import numpy as np
import math
import HandTrackingModule as htm
from comtypes import CLSCTX_ALL
from pycaw.pycaw import AudioUtilities, IAudioEndpointVolume

class HandVolumeControl:
    def __init__(self, wCam=640, hCam=480, detectionCon=0.8):
        self.wCam = wCam
        self.hCam = hCam
        self.detectionCon = detectionCon
        self.cap = cv2.VideoCapture(0)
        self.cap.set(3, self.wCam)
        self.cap.set(4, self.hCam)
        self.pTime = 0

        self.detector = htm.handDetector(detectionCon=self.detectionCon)

        devices = AudioUtilities.GetSpeakers()
        interface = devices.Activate(
            IAudioEndpointVolume._iid_, CLSCTX_ALL, None)
        self.volume = interface.QueryInterface(IAudioEndpointVolume)
        volRange = self.volume.GetVolumeRange()
        self.minVol = volRange[0]
        self.maxVol = volRange[1]
        self.vol = 0
        self.volBar = 400
        self.volPer = 0

    def run(self):
        while True:
            success, img = self.cap.read()
            if not success:
                break

            img = self.detector.findHands(img)
            lmList = self.detector.findPosition(img, draw=False)
            if len(lmList) != 0:
                x1, y1 = lmList[4][1], lmList[4][2]
                x2, y2 = lmList[8][1], lmList[8][2]
                cx, cy = (x1 + x2) // 2, (y1 + y2) // 2

                cv2.circle(img, (x1, y1), 15, (255, 0, 255), cv2.FILLED)
                cv2.circle(img, (x2, y2), 15, (255, 0, 255), cv2.FILLED)
                cv2.line(img, (x1, y1), (x2, y2), (255, 0, 255), 3)
                cv2.circle(img, (cx, cy), 15, (255, 0, 255), cv2.FILLED)

                length = math.hypot(x2 - x1, y2 - y1)

                self.vol = np.interp(length, [50, 300], [self.minVol, self.maxVol])
                self.volBar = np.interp(length, [50, 300], [400, 150])
                self.volPer = np.interp(length, [50, 300], [0, 100])

                # Sicherstellen, dass der Lautstärkewert im gültigen Bereich liegt
                if self.minVol <= self.vol <= self.maxVol:
                    self.volume.SetMasterVolumeLevel(self.vol, None)

                if length < 50:
                    cv2.circle(img, (cx, cy), 15, (0, 255, 0), cv2.FILLED)

            cv2.rectangle(img, (50, 150), (85, 400), (0, 255, 0), 3)
            cv2.rectangle(img, (50, int(self.volBar)), (85, 400), (0, 255, 0), cv2.FILLED)
            cv2.putText(img, f'{int(self.volPer)}%', (40, 450), cv2.FONT_HERSHEY_COMPLEX, 1, (0, 250, 0), 3)

            cTime = time.time()
            fps = 1 / (cTime - self.pTime)
            self.pTime = cTime

            cv2.putText(img, f'FPS: {int(fps)}', (40, 50), cv2.FONT_HERSHEY_COMPLEX, 1, (255, 0, 0), 1)
            cv2.imshow("Img", img)
            cv2.waitKey(1)

    def release(self):
        self.cap.release()
        cv2.destroyAllWindows()

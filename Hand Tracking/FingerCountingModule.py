import cv2
import os
import time
import HandTrackingModule as htm

class FingerCounter:
    def __init__(self, folderpath, detectionCon=0.75, wCam=640, hCam=480):
        self.overlayList = self.load_images_from_folder(folderpath)
        self.detector = htm.handDetector(detectionCon=detectionCon)
        self.tipIDs = [4, 8, 12, 16, 20]
        self.pTime = 0
        self.wCam = wCam
        self.hCam = hCam

    def load_images_from_folder(self, folderpath):
        myList = os.listdir(folderpath)
        overlayList = []
        for imPath in myList:
            image = cv2.imread(f"{folderpath}/{imPath}")
            overlayList.append(image)
        return overlayList

    def count_fingers(self, lmList):
        fingers = []
        if lmList[self.tipIDs[0]][1] > lmList[self.tipIDs[0] - 1][1]:
            fingers.append(1)
        else:
            fingers.append(0)
        for id in range(1, 5):
            if lmList[self.tipIDs[id]][2] < lmList[self.tipIDs[id] - 2][2]:
                fingers.append(1)
            else:
                fingers.append(0)
        return fingers

    def draw_results(self, img, totalFingers, fps):
        if totalFingers > 0:
            h, w, c = self.overlayList[totalFingers-1].shape
            img[0:h, 0:w] = self.overlayList[totalFingers-1]
        cv2.rectangle(img, (20, 255), (170, 425), (0, 255, 0), cv2.FILLED)
        cv2.putText(img, str(totalFingers), (45, 375), cv2.FONT_HERSHEY_COMPLEX, 3, (255, 0, 0), 3)
        cv2.putText(img, f"FPS: {int(fps)}", (400, 70), cv2.FONT_HERSHEY_PLAIN, 3, (255, 0, 0), 3)

    def calculate_fps(self):
        cTime = time.time()
        fps = 1 / (cTime - self.pTime)
        self.pTime = cTime
        return fps

    def detect_hands(self, img):
        img = self.detector.findHands(img)
        lmList = self.detector.findPosition(img, draw=False)
        return img, lmList

    def start(self):
        cap = cv2.VideoCapture(0)
        cap.set(3, self.wCam)
        cap.set(4, self.hCam)

        while True:
            success, img = cap.read()
            if not success:
                continue

            img, lmList = self.detect_hands(img)
            if lmList:
                fingers = self.count_fingers(lmList)
                totalFingers = fingers.count(1)
                fps = self.calculate_fps()
                self.draw_results(img, totalFingers, fps)

            cv2.imshow("Image", img)
            cv2.waitKey(1)

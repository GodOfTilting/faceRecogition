import cv2
import mediapipe as mp
import time

class FaceMeshDetector():
    def __init__(self, staticMode=False, maxFaces=2, minDetectionCon=0.5, minTrackCon=0.5):
        self.staticMode = staticMode
        self.maxFaces = maxFaces
        self.minDetectionCon = minDetectionCon
        self.minTrackCon = minTrackCon

        self.mpDraw = mp.solutions.drawing_utils
        self.mpFaceMesh = mp.solutions.face_mesh
        self.faceMesh = self.mpFaceMesh.FaceMesh(static_image_mode=self.staticMode, max_num_faces=self.maxFaces, min_detection_confidence=self.minDetectionCon, min_tracking_confidence=self.minTrackCon)
        self.drawSpec = self.mpDraw.DrawingSpec(thickness=1, circle_radius=2)

    def FindFaceMesh(self, img, draw=True):
        self.imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        self.results = self.faceMesh.process(self.imgRGB)
        
        faces = []
        if self.results.multi_face_landmarks:
            for faceLMS in self.results.multi_face_landmarks:
                if draw:
                    self.mpDraw.draw_landmarks(img, faceLMS, self.mpFaceMesh.FACE_CONNECTIONS, self.drawSpec, self.drawSpec)
                face = []
                for id, lm in enumerate(faceLMS.landmark):
                    ih, iw, ic = img.shape
                    x, y = int(lm.x * iw), int(lm.y * ih)
                    cv2.putText(img, str(id), (x, y), cv2.FONT_HERSHEY_COMPLEX, 0.7, (0, 255, 0), 1)
                    face.append([x, y])
                faces.append(face)
        
        return img, faces
    
    def Start():
        cap = cv2.VideoCapture(0)
        pTime = 0
        detector = FaceMeshDetector(maxFaces=2, minDetectionCon=0.5, minTrackCon=0.5)
        while True:
            success, img = cap.read()
            img, faces = detector.FindFaceMesh(img, False)
            if len(faces) != 0:
                print(len(faces))
            cTime = time.time()
            fps = 1 / (cTime - pTime)
            pTime = cTime

            cv2.putText(img, f'FPS: {int(fps)}', (20, 70), cv2.FONT_HERSHEY_COMPLEX, 3, (0, 255, 0), 3)
            cv2.imshow("Image", img)
            cv2.waitKey(1)

            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

        cap.release()
        cv2.destroyAllWindows()

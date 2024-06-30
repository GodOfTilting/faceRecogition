import cv2
import time
from FaceMeshModule import FaceMeshDetector

def main():
    cap = cv2.VideoCapture(0)
    pTime = 0
    detector = FaceMeshDetector(maxFaces=2, minDetectionCon=0.5, minTrackCon=0.5)
    
    while True:
        success, img = cap.read()
        img, faces = detector.FindFaceMesh(img, draw=False)
        
        if faces is not None:
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

if __name__ == "__main__":
    main()

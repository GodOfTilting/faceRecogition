import cv2
from FaceMeshModule import FaceMeshDetector

def main():
    cap = cv2.VideoCapture(0)
    pTime = 0
    detector = FaceMeshDetector(maxFaces=2)

    while True:
        success, img = cap.read()
        img, faces = detector.FindFaceMesh(img, draw=False)

        if len(faces) != 0:
            print(len(faces))

        cTime = time.time()
        fps = 1 / (cTime - pTime)
        pTime = cTime

        cv2.putText(img, f'FPS: {int(fps)}', (20, 70), cv2.FONT_HERSHEY_COMPLEX, 1, (0, 255, 0), 2)
        cv2.imshow("Image", img)
        cv2.waitKey(1)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    main()

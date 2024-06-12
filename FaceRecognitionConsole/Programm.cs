using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

namespace Cam
{
    public class Programm
    {
        public static readonly string dataFolderPath = Directory.GetCurrentDirectory() + "/data/haarcascades";
        public static void Main(string[] args)
        {
            int width = 1520;
            int height = 950;

            String win1 = "Face Detection";
            CvInvoke.NamedWindow(win1);

            var faceCascades = new List<CascadeClassifier>();
            foreach (var cascadePath in Directory.GetFiles(dataFolderPath, "*.xml"))
                faceCascades.Add(new CascadeClassifier(cascadePath));

            int lastFaceCenterX = 0;
            int lastFaceCenterY = 0;
            int lastFaceWidth = 0;
            int lastFaceHeight = 0;

            Mat frame = new();
            VideoCapture capture = new(0);
            capture.Set(CapProp.FrameWidth, width);
            capture.Set(CapProp.FrameHeight, height);

            while (CvInvoke.WaitKey(1) == -1)
            {
                capture.Read(frame);

                Mat grayFrame = new();
                CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);

                Rectangle[] faces = null;
                foreach (var cascade in faceCascades)
                {
                    faces = cascade.DetectMultiScale(grayFrame, 1.1, 3);
                    if (faces.Length > 0)
                    {
                        break; // Break if faces are detected using any cascade
                    }
                }

                if (faces != null && faces.Length > 0)
                {
                    Rectangle face = faces[0];
                    foreach (var element in faces)
                    {
                        CvInvoke.Rectangle(frame, element, new MCvScalar(0, 255, 0), 2);
                    }

                    lastFaceCenterX = face.X + face.Width / 2;
                    lastFaceCenterY = face.Y + face.Height / 2;
                    lastFaceWidth = face.Width;
                    lastFaceHeight = face.Height;
                }

                Console.WriteLine($"Last Face Center: X={lastFaceCenterX}, Y={lastFaceCenterY}");

                CvInvoke.Imshow(win1, frame);
            }
        }
    }
}

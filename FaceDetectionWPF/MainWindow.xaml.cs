using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FaceDetection
{
    public partial class MainWindow : Window
    {
        private readonly string dataFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "haarcascades");
        private readonly VideoCapture capture;
        private readonly List<CascadeClassifier> faceCascades;
        private bool isCapturing = false;

        public MainWindow()
        {
            InitializeComponent();

            faceCascades = new List<CascadeClassifier>();
            foreach (var cascadePath in Directory.GetFiles(dataFolderPath, "*.xml"))
                faceCascades.Add(new CascadeClassifier(cascadePath));

            capture = new VideoCapture();
            capture.ImageGrabbed += ProcessFrame;
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            if (isCapturing)
            {
                using (var frame = new Mat())
                {
                    capture.Retrieve(frame);

                    using (var grayFrame = new Mat())
                    {
                        CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);

                        Rectangle[] faces = null;
                        foreach (var cascade in faceCascades)
                        {
                            faces = cascade.DetectMultiScale(grayFrame, 1.1, 3);
                            if (faces.Length > 0)
                                break;
                        }

                        if (faces != null && faces.Length > 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                DrawFaces(frame, faces);
                            });
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        DisplayFrame(frame);
                    });
                }
            }
        }

        private void DrawFaces(Mat frame, Rectangle[] faces)
        {
            foreach (var face in faces)
            {
                CvInvoke.Rectangle(frame, face, new MCvScalar(0, 255, 0), 2);
            }
        }

        private void DisplayFrame(Mat frame)
        {
            var bitmap = frame.ToImage<Bgr, byte>().ToBitmap();
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            MainImage.Source = bitmapSource;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isCapturing)
            {
                isCapturing = true;
                capture.Start();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (isCapturing)
            {
                isCapturing = false;
                capture.Stop();
            }
        }
    }
}

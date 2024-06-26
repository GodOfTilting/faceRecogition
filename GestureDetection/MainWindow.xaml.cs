using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GestureDetection
{
    public partial class MainWindow : Window
    {
        private VideoCapture _capture;
        private CascadeClassifier _faceCascade;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _capture = new VideoCapture();
            _faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += ProcessFrame;
            _timer.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.Stop();
            _capture.Dispose();
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            using (var frame = _capture.QueryFrame())
            {
                if (frame != null)
                {
                    var grayFrame = new Mat();
                    CvInvoke.CvtColor(frame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                    var faces = _faceCascade.DetectMultiScale(grayFrame, 1.1, 10, new System.Drawing.Size(20, 20));

                    foreach (var face in faces)
                    {
                        CvInvoke.Rectangle(frame, face, new MCvScalar(255, 0, 0), 2);
                    }

                    cameraImage.Source = ToBitmapSource(frame);
                }
            }
        }

        private BitmapSource ToBitmapSource(Mat image)
        {
            using (var source = image.ToBitmap())
            {
                var hBitmap = source.GetHbitmap();
                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(hBitmap);
                return bitmapSource;
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}

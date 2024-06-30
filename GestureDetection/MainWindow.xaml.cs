using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GestureDetection
{
    public partial class MainWindow : Window
    {
        private VideoCapture _capture;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _capture = new VideoCapture();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            _timer.Tick += ProcessFrame;
            _timer.Start();
        }
   
        private void ProcessFrame(object sender, EventArgs e)
        {
            using (Mat frame = _capture.QueryFrame())
            {
                if (frame != null)
                {
                    // Hand-Tracking-Logik hier einfügen
                    using (Mat hsv = new Mat())
                    {
                        CvInvoke.CvtColor(frame, hsv, ColorConversion.Bgr2Hsv);

                        // Erstellen Sie eine Maske für die Hautfarbe
                        using (Mat mask = new Mat())
                        {
                            CvInvoke.InRange(hsv, new ScalarArray(new MCvScalar(0, 48, 80)), new ScalarArray(new MCvScalar(20, 255, 255)), mask);

                            // Optional: Wenden Sie Morphologieoperationen an, um das Rauschen zu reduzieren
                            CvInvoke.Erode(mask, mask, null, new System.Drawing.Point(-1, -1), 2, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                            CvInvoke.Dilate(mask, mask, null, new System.Drawing.Point(-1, -1), 2, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

                            // Finden Sie Konturen in der Maske
                            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                            {
                                CvInvoke.FindContours(mask, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                                // Zeichnen Sie die Konturen und Landmarken (optional)
                                for (int i = 0; i < contours.Size; i++)
                                {
                                    var contour = contours[i];
                                    var hull = new VectorOfPoint();

                                    // Überprüfen Sie, ob die Kontur und die konvexe Hülle genügend Punkte haben
                                    if (contour.Size > 3)
                                    {
                                        CvInvoke.ConvexHull(contour, hull, false);

                                        // Stellen Sie sicher, dass die konvexe Hülle genügend Punkte hat
                                        if (hull.Size >= 3)
                                        {
                                            // Finden Sie die konvexen Defekte (Punkte zwischen den Fingern)
                                            using (var defects = new Mat())
                                            {
                                                try
                                                {
                                                    CvInvoke.ConvexityDefects(contour, hull, defects);

                                                    if (!defects.IsEmpty)
                                                    {
                                                        // Extrahieren Sie die Defektdaten
                                                        Matrix<int> defectsData = new Matrix<int>(defects.Rows, 4);
                                                        defects.CopyTo(defectsData);

                                                        var handPoints = new List<System.Drawing.Point>();

                                                        for (int j = 0; j < defects.Rows; j++)
                                                        {
                                                            int startIdx = defectsData[j, 0];
                                                            int endIdx = defectsData[j, 1];
                                                            int farIdx = defectsData[j, 2];

                                                            if (startIdx < 0 || endIdx < 0 || farIdx < 0 ||
                                                                startIdx >= contour.Size || endIdx >= contour.Size || farIdx >= contour.Size)
                                                            {
                                                                continue; // Überspringen Sie ungültige Indizes
                                                            }

                                                            var startPoint = contour[startIdx];
                                                            var endPoint = contour[endIdx];
                                                            var farPoint = contour[farIdx];

                                                            // Zeichnen Sie Punkte für Fingerkuppen und Gelenke
                                                            CvInvoke.Circle(frame, startPoint, 5, new MCvScalar(0, 255, 0), -1);
                                                            CvInvoke.Circle(frame, endPoint, 5, new MCvScalar(0, 255, 0), -1);
                                                            CvInvoke.Circle(frame, farPoint, 5, new MCvScalar(0, 0, 255), -1);

                                                            // Fügen Sie die Punkte zur Liste hinzu
                                                            handPoints.Add(startPoint);
                                                            handPoints.Add(endPoint);
                                                            handPoints.Add(farPoint);
                                                        }

                                                        // Zeichnen Sie Linien zwischen den Handpunkten
                                                        for (int j = 0; j < handPoints.Count; j++)
                                                        {
                                                            for (int k = j + 1; k < handPoints.Count; k++)
                                                            {
                                                                CvInvoke.Line(frame, handPoints[j], handPoints[k], new MCvScalar(255, 0, 0), 2);
                                                            }
                                                        }
                                                    }

                                                }
                                                catch (Exception)
                                                {
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Konvertieren Sie das Bild zur Anzeige in WPF
                    cameraImage.Source = ToBitmapSource(frame);
                }
            }
        }

        private BitmapSource ToBitmapSource(Mat mat)
        {
            using (Bitmap bitmap = mat.ToBitmap())
            {
                IntPtr hBitmap = bitmap.GetHbitmap();
                try
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            _capture.Dispose();
            base.OnClosed(e);
        }
    }
}

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextDetection
{
    public partial class Form1 : Form
    {
        VideoCapture capture;
        Tesseract ocr;
        string filePath;
        public Form1()
        {
            InitializeComponent();
            capture = new VideoCapture();
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
            ocr = new Tesseract(path, "eng", OcrEngineMode.TesseractOnly);
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                capture = new VideoCapture(ofd.FileName);
                Mat m = new Mat();
                capture.Read(m);
                pictureBox1.Image = m.ToBitmap();
                filePath = ofd.FileName;
                m.Dispose();
            }
        }

        private async void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (capture == null)
            {
                return;
            }

            try
            {

                while (true)
                {
                    Mat m = new Mat();
                    capture.Read(m);

                    if (!m.IsEmpty)
                    {
                        pictureBox1.Image = m.ToBitmap();
                        DetectText(m);
                        double fps = capture.GetCaptureProperty(CapProp.Fps);

                        m.Dispose();
                    }
                    await Task.Delay(1000 / Convert.ToInt32(40));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DetectText(Mat mat)
        {
            mat = new Mat(@"C:\Users\Huy\Pictures\Camera Roll\version.jpg");

            var firstVersion = new Mat(mat, new Rectangle(0, 0, 840, 1080)).ToImage<Gray, byte>().ThresholdBinary(new Gray(140), new Gray(150));
            var secondVersion = new Mat(mat, new Rectangle(840, 0, 1000, 1080)).ToImage<Gray, byte>().ThresholdBinary(new Gray(119), new Gray(150));

            ExtractTextFromImage(firstVersion);
            ExtractTextFromImage(secondVersion);

        }

        void ExtractTextFromImage(IInputArray img)
        {
            ocr.SetImage(img);
            ocr.Recognize();
            var result = ocr.GetUTF8Text();
            richTextBox1.AppendText(result);
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            DetectText(new Mat(filePath));
        }

        private void btnDetectShape_Click(object sender, EventArgs e)
        {
            DetectShape(new Image<Bgr, byte>(filePath));
        }

        private void DetectShape(Image<Bgr, byte> imgInput)
        {
            if (imgInput == null)
            {
                return;
            }

            try
            {
                var temp = imgInput.SmoothGaussian(5).Convert<Gray, byte>().ThresholdBinaryInv(new Gray(30), new Gray(50));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat m = new Mat();

                CvInvoke.FindContours(temp, contours, m, RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < contours.Size; i++)
                {
                    double perimeter = CvInvoke.ArcLength(contours[i], true);
                    VectorOfPoint approx = new VectorOfPoint();
                    CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);


                    //moments  center of the shape

                    var moments = CvInvoke.Moments(contours[i]);
                    int x = (int)(moments.M10 / moments.M00);
                    int y = (int)(moments.M01 / moments.M00);



                    if (approx.Size == 4)
                    {
                        var con = contours[i];
                        if (con.Size < 9)
                        {
                            Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                            double ar = (double)rect.Width / rect.Height;

                            if (ar >= 0.95 && ar <= 1.05)
                            {
                                CvInvoke.PutText(imgInput, "Square", new Point(x, y),
                                Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                            }
                            else
                            {
                                CvInvoke.PutText(imgInput, "Rectangle", new Point(x, y),
                                Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                            }
                            CvInvoke.DrawContours(imgInput, contours, i, new MCvScalar(0, 0, 255), 2);

                        }


                    }


                    pictureBox2.Image = imgInput.ToBitmap();

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void FindContours(Image<Bgr, byte> imgInput)
        {
            Image<Gray, byte> imgOutput = imgInput.SmoothGaussian(5).Convert<Gray, byte>().ThresholdBinaryInv(new Gray(230), new Gray(255));
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();

            CvInvoke.FindContours(imgOutput, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            if (contours.Size > 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    double perimeter = CvInvoke.ArcLength(contours[i], true);
                    var approx = new VectorOfPoint();
                    CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);
                    CvInvoke.DrawContours(imgInput, contours, i, new MCvScalar(0, 255, 255));
                    var a = approx.Size;
                    pictureBox2.Image = imgInput.ToBitmap();

                }
            }



        }


        private void btnFindContours_Click(object sender, EventArgs e)
        {
            FindContours(new Image<Bgr, byte>(filePath));
        }
    }
}

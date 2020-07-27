// Simple Player sample application
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2006-2011
// contacts@aforgenet.com
//

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using AForge.Video;
using AForge.Video.DirectShow;
using TobiiAgent;
using Tobii.Interaction;
using System.IO;
using Common;
using System.Linq;
using System.Configuration;
using System.Reactive.Subjects;

namespace UI
{
    public partial class MainForm : Form
    {
        private IAgentAnalyzer agent;
        private Stopwatch stopWatch;
        readonly ObjectDetector detector;

        // Class constructor//
        public MainForm()
        {
            InitializeComponent();
            detector = new ObjectDetector();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCurrentVideoSource();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            localVideoCaptureDeviceToolStripMenuItem_Start();
            var subject = new Subject<StreamData<GazePointData>>();
            agent = new TobiiAgentAnalyzer(subject);
            subject.Subscribe(value =>
            {
                OnDetection(value.Data.X, value.Data.Y);
            });
            //m_Agent = new MockAgentAnalyzer();
            agent.StartWatching(this.OnDetection);
        }

        // "Exit" menu item clicked
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void localVideoCaptureDeviceToolStripMenuItem_Start()
        {
            var form = new VideoCaptureDeviceForm();
            form.CaptureSize = new System.Drawing.Size(1280, 720);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                form.CaptureSize = new System.Drawing.Size(1280, 720);
                var videoSource = form.VideoDevice;

                // open it
                OpenVideoSource(videoSource);
            }
        }

        // Open local video capture device
        private void localVideoCaptureDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new VideoCaptureDeviceForm();
            form.CaptureSize = new System.Drawing.Size(1280, 720);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                form.CaptureSize = new System.Drawing.Size(1280, 720);
                var videoSource = form.VideoDevice;

                // open it
                OpenVideoSource(videoSource);
            }
        }

        internal void OnDetection(double x, double y)
        {
            Action action = () =>
            {
                double ratio = OS.GetScalingFactor(Handle);
                var gazeLocation = new Point((int)(x / ratio), (int)(y / ratio));

                var pt = this.videoSourcePlayer.PointToClient(gazeLocation);
                var normalizeX = pt.X / (float)videoSourcePlayer.Width;
                var normalizeY = pt.Y / (float)videoSourcePlayer.Height;
                detector.PointX = normalizeX;
                detector.PointY = normalizeY;

                var focusedButton = this.DescendentsFromPoint(pt).OfType<Button>().LastOrDefault();
                if (focusedButton != null)
                {
                    this.txtStatus.Text = $"clicking {focusedButton.Text}";
                    focusedButton.PerformClick();
                }
                if (Bounds.Contains(pt))
                {
                    panelDetectionFrame.Location = gazeLocation;
                    panelDetectionFrame.Visible = true;
                }
                else
                {
                    panelDetectionFrame.Visible = false;
                }
            };

            if (!this.IsDisposed)
            {
                this.Invoke(action);
            }

            //MemoryStream frame_MS = CaptureSnapshot();
            //saveImageLocally(frame_MS);
            //tryToDetect(frame_MS);
        }

        internal void SaveImageLocally(MemoryStream ms)
        {
            var path = string.Concat(ConfigurationManager.AppSettings["SolutionDirectory"], @"\PointerAppAlyn\YOLOv3-Object-Detection-with-OpenCV\temp\");
            //var imageStream = Image.FromStream(ms);
            //imageStream.Save(outStream, ImageFormat.Jpeg);
            var imgSave = System.Drawing.Image.FromStream(ms);
            var bmSave = new Bitmap(imgSave);
            var bmTemp = new Bitmap(bmSave);

            var grSave = Graphics.FromImage(bmTemp);
            grSave.DrawImage(imgSave, 0, 0, imgSave.Width, imgSave.Height);

            bmTemp.Save(path + "\\" + "image" + ".jpeg");
            imgSave.Dispose();
            bmSave.Dispose();
            bmTemp.Dispose();
            grSave.Dispose();
        }

        internal MemoryStream CaptureSnapshot()
        {
            if (videoSourcePlayer != null)
            {
                var memoryStream = new MemoryStream();
                var varBmp = videoSourcePlayer.GetCurrentVideoFrame();
                varBmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                varBmp.Dispose();
                memoryStream.Seek(0L, SeekOrigin.Begin);
                return memoryStream;
                //varBmp.Save(@"C:\a.png", ImageFormat.Png);
            }
            return null;
        }

        internal void TryToDetect(MemoryStream stream)
        {
            detector.DetectFromImagePath();
            // m_Detector.Detect(i_MS);
        }

        // Open video file using DirectShow
        private void OpenVideofileusingDirectShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // create video source
                var fileSource = new FileVideoSource(openFileDialog.FileName);
                fileSource.VideoSourceError += FileSource_VideoSourceError;

                // open it
                OpenVideoSource(fileSource);
            }
        }

        private void FileSource_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
        }

        // Open JPEG URL
        private void openJPEGURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new UrlForm();

            form.Description = "Enter URL of an updating JPEG from a web camera:";
            form.Urls = new string[]
                {
                    "http://195.243.185.195/axis-cgi/jpg/image.cgi?camera=1",
                };

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                var jpegSource = new JPEGStream(form.URL);

                // open it
                OpenVideoSource(jpegSource);
            }
        }

        // Open MJPEG URL
        private void openMJPEGURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new UrlForm();

            form.Description = "Enter URL of an MJPEG video stream:";
            form.Urls = new string[]
                {
                    "http://195.243.185.195/axis-cgi/mjpg/video.cgi?camera=4",
                    "http://195.243.185.195/axis-cgi/mjpg/video.cgi?camera=3",
                };

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                var mjpegSource = new MJPEGStream(form.URL);

                // open it
                OpenVideoSource(mjpegSource);
            }
        }

        // Capture 1st display in the system
        private void capture1stDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenVideoSource(new ScreenCaptureStream(Screen.AllScreens[0].Bounds, 100));
        }

        // Open video source
        private void OpenVideoSource(IVideoSource source)
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            // stop current video source
            CloseCurrentVideoSource();

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();

            // reset stop watch
            stopWatch = null;

            this.Cursor = Cursors.Default;
        }

        // Close video source if it is running
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();

                // wait ~ 3 seconds
                for (var i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (videoSourcePlayer.IsRunning)
                {
                    videoSourcePlayer.Stop();
                }

                videoSourcePlayer.VideoSource = null;
            }
        }

        // New frame received by the player
        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            var now = DateTime.Now;
            var g = Graphics.FromImage(image);

            // paint current time
            var brush = new SolidBrush(Color.Red);
            g.DrawString(now.ToString(), this.Font, brush, new PointF(5, 5));
            brush.Dispose();

            g.Dispose();
        }

        // On timer event - gather statistics
        private void timer_Tick(object sender, EventArgs e)
        {
            var videoSource = videoSourcePlayer.VideoSource;

            if (videoSource != null)
            {
                // get number of frames since the last timer tick
                var framesReceived = videoSource.FramesReceived;

                if (stopWatch == null)
                {
                    stopWatch = new Stopwatch();
                    stopWatch.Start();
                }
                else
                {
                    stopWatch.Stop();

                    stopWatch.Reset();
                    stopWatch.Start();
                }
            }
        }

        private void buttonTakeMeThere_Click(object sender, EventArgs e)
        {

        }

        private void buttonStopTobii_Click(object sender, EventArgs e)
        {

        }

        private void buttonIWhatsThis_Click(object sender, EventArgs e)
        {

        }

        private void buttonWantThis_Click(object sender, EventArgs e)
        {

        }

        private void buttonWhoIsThis_Click(object sender, EventArgs e)
        {

        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            var settings = new SettingsForm(agent.UpdateDelayThreshold);
            settings.ShowDialog();
        }
    }
}

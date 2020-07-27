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
using AForge.Video;
using AForge.Video.DirectShow;
using TobiiAgent;
using Tobii.Interaction;
using System.IO;
using Common;
using System.Linq;
using System.Configuration;
using System.Drawing.Imaging;
using System.Reactive.Subjects;
using System.Threading;
using Size = System.Windows.Size;

namespace UI
{
    public partial class MainForm : Form
    {
        private readonly ObjectDetector detector;
        private IAgentAnalyzer agent;

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
            agent = new TobiiAgentAnalyzer();
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
            var form = new VideoCaptureDeviceForm
            {
                CaptureSize = new System.Drawing.Size(1280, 720)
            };

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
            var imgSave = Image.FromStream(ms);
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
                varBmp.Save(memoryStream, ImageFormat.Jpeg);
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

            this.Cursor = Cursors.Default;
        }

        // Close video source if it is running
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource == null)
            {
                return;
            }

            videoSourcePlayer.SignalToStop();

            // wait ~3 seconds
            for (var i = 0; i < 30; i++)
            {
                if (!videoSourcePlayer.IsRunning)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            if (videoSourcePlayer.IsRunning)
            {
                videoSourcePlayer.Stop();
            }

            videoSourcePlayer.VideoSource = null;
        }

        // New frame received by the player
        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            var now = DateTime.Now;
            var g = Graphics.FromImage(image);

            // paint current time
            var brush = new SolidBrush(Color.Red);
            g.DrawString(now.ToString("O"), this.Font, brush, new PointF(5, 5));
            brush.Dispose();

            g.Dispose();
        }

        private void buttonTakeMeThere_Click(object sender, EventArgs e)
        {
            //var simpleSound = new SoundPlayer(@"c:\Windows\Media\chimes.wav");
            //simpleSound.Play();
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

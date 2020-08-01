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
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Media;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using Alyn.Pointer.App.Properties;
using Alyn.Pointer.Common;
using Alyn.Pointer.DetectionService;
using Alyn.Pointer.TobiiAgent;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;

namespace Alyn.Pointer.App
{
    public partial class MainForm : Form
    {
        private IAgentAnalyzer agent;
        private bool mockTobii = Settings.Default.TobiiMock;
        private (DateTime timestamp, int x, int y) lastLock;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCurrentVideoSource();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LocalVideoCaptureDeviceToolStripMenuItem_Start();

            if (mockTobii)
            {
                var subject = new Subject<(double x, double y)>();
                this.videoSourcePlayer.MouseClick += (_, args) =>
                {
                    if (args.Button == MouseButtons.Left)
                    {
                        subject.OnNext((args.X, args.Y));
                    }
                };

                agent = new MockAgentAnalyzer(subject);
            }
            else
            {
                agent = new TobiiAgentAnalyzer();
            }

            agent.StartWatching(this.OnDetection);
        }

        private void LocalVideoCaptureDeviceToolStripMenuItem_Start()
        {

            var form = new VideoCaptureDeviceForm
            {
                CaptureSize = new Size(1280, 720)
            };

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                form.CaptureSize = new Size(1280, 720);
                var videoSource = form.VideoDevice;

                // open it
                OpenVideoSource(videoSource);
            }
        }

        internal void OnDetection(double x, double y)
        {
            void Action()
            {
                var ratio = mockTobii ? 1d : OS.GetScalingFactor(Handle);
                var gazeLockLocation = new Point(Math.Max((int)(x / ratio), 0), Math.Max((int)(y / ratio), 0));

                Trace.WriteLine($"Alyn:: {gazeLockLocation.X} {gazeLockLocation.Y}");

                var point = this.videoSourcePlayer.PointToClient(gazeLockLocation);

                this.DescendentsFromPoint(point).OfType<Button>().LastOrDefault()?.PerformClick();

                if (Bounds.Contains(point))
                {
                    this.lastLock = (DateTime.Now, gazeLockLocation.X, gazeLockLocation.Y);
                    panelDetectionFrame.Location = gazeLockLocation;
                    panelDetectionFrame.Visible = true;
                }
                else
                {
                    panelDetectionFrame.Visible = false;
                }
            }

            if (!this.IsDisposed)
            {
                this.Invoke((Action)Action);
                using var frame = CaptureSnapshot();
                var result = GetDetections(frame);
            }
        }

        public static async Task<DetectionResult[]> GetDetections(Stream image)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Detection.DetectionClient(channel);
            var request = new Alyn.Pointer.DetectionService.Payload {Image = ByteString.FromStream(image)};
            var reply = await client.GetDetectionsAsync(request, new CallOptions());
            return reply.Detections.ToArray();
        }

        internal MemoryStream CaptureSnapshot()
        {
            if (videoSourcePlayer != null)
            {
                var memoryStream = new MemoryStream();
                using (var varBmp = videoSourcePlayer.GetCurrentVideoFrame())
                {
                    varBmp.Save(memoryStream, ImageFormat.Jpeg);
                }
                memoryStream.Seek(0L, SeekOrigin.Begin);
                return memoryStream;
            }

            return null;
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
            using (var g = Graphics.FromImage(image))
            {
                // paint current time
                using (var brush = new SolidBrush(Color.Red))
                {
                    g.DrawString(now.ToString("O"), this.Font, brush, new PointF(10, this.Height / 2));
                }

                if (DateTime.Now - this.lastLock.timestamp < TimeSpan.FromSeconds(5))
                {
                    const int CircleRadius = 15;

                    var rect = new Rectangle(this.lastLock.x - CircleRadius, this.lastLock.y - CircleRadius, CircleRadius * 2, CircleRadius * 2);
                    g.DrawEllipse(Pens.Red, rect);
                }
            }
        }

        private void buttonTakeMeThere_Click(object sender, EventArgs e) => PlaySound("there.wav");

        private void buttonStopTobii_Click(object sender, EventArgs e) => PlaySound("stop.wav");

        private void buttonIWhatsThis_Click(object sender, EventArgs e) => PlaySound("what_is_this.wav");

        private void buttonWantThis_Click(object sender, EventArgs e) => PlaySound("want.wav");

        private void buttonWhoIsThis_Click(object sender, EventArgs e) => PlaySound("who_is_this.wav");

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            var settings = new SettingsForm(agent.UpdateDelayThreshold);
            settings.ShowDialog();
        }

        private static void PlaySound(string fileName)
        {
            using var simpleSound = new SoundPlayer(Path.Combine("Sounds", fileName));
            simpleSound.Play();
        }
    }
}

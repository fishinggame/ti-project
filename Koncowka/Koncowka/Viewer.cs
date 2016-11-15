using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Koncowka
{
    public partial class Viewer : Form
    {
        private System.Timers.Timer timer;
        private System.Timers.Timer timerSync;
        private int counter = 0;
        private List<Bitmap> images = new List<Bitmap>();
        private string url;
        private string userName;

        public Viewer(string _url, string _userName)
        {
            InitializeComponent();
            url = _url;
            userName = _userName;

            ImageList(userName);

            if (images.Count > 0)
            {
                TimerInit();
                TimerSyncInit();
            }
        }

        private void TimerInit()
        {
            picBox.Image = images[0];
            counter++;
            timer = new System.Timers.Timer();
            timer.Interval = 5000;
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (images.Count > 0)
            {
                if (counter == images.Count)
                    counter = 0;
                picBox.Image = images[counter];
                counter++;
            }
        }
        private void TimerSyncInit()
        {
            timerSync = new System.Timers.Timer();
            timerSync.Interval = 30000;
            timerSync.Elapsed += TimerElapsedSync;
            timerSync.Start();
        }
        private async void TimerElapsedSync(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                await Sync.SynchronizeFiles(url, userName, false);
                picBox.Image = null;
                images.Clear();
                ImageList(userName);
                if (images.Count > 0)
                {
                    picBox.Image = images[0];
                    counter = 1;
                }
            }
        }

        private void Viewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void ImageList(string dirName)
        {
            var fileEntries = Directory.EnumerateFiles(dirName, "*.*")
            .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg"));

            foreach (var file in fileEntries)
            {
                using (var fs = new System.IO.FileStream(file, System.IO.FileMode.Open))
                {
                    var bmp = new Bitmap(fs);
                    images.Add(ReSize((Bitmap)bmp.Clone()));
                }
            }
        }

        private Bitmap ReSize(Bitmap image)
        {
            if (image.Height > Screen.PrimaryScreen.Bounds.Height)
            {
                float scaleHeight = image.Height / (float)Screen.PrimaryScreen.Bounds.Height;
                image = new Bitmap(image, (int)(image.Width / scaleHeight), Screen.PrimaryScreen.Bounds.Height);
            }
            if (image.Width > Screen.PrimaryScreen.Bounds.Width)
            {
                float scaleWidth = image.Width / (float)Screen.PrimaryScreen.Bounds.Width;
                image = new Bitmap(image, Screen.PrimaryScreen.Bounds.Width, (int)(image.Height / scaleWidth));
            }

            return image;
        }
    }
}

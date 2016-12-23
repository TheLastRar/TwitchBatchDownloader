using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TwitchVodDownloaderSharp.Download;
using TwitchVodDownloaderSharp.Merge;
using TwitchVodDownloaderSharp.TwitchAPI;

namespace TwitchVodDownloaderSharp.UI
{
    public partial class VodDownloadPanel : UserControl
    {
        VideoData data;
        VodDownloader vD;
        VodMerger vM;

        public volatile bool Cancel = false;
        List<Chunk> parts;

        public event EventHandler DownloadCompleted;
        public event EventHandler ConvertCompleted;
        public event EventHandler Canceled;

        public VodDownloadPanel()
        {
            InitializeComponent();
            statusLabel.Visible = false;
            downloadProgress.Visible = false;
            cancelButton.Visible = false;
            cancelButton.Enabled = false;
        }

        public VideoData StreamInfo
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                Populate();
            }
        }

        public void Queued()
        {
            //Called on Load to resume incomplete download (TODO)
            //Called on button press to start new download
            SetStatus("Queued");
            SetProgressMarquee(true);
            PopulateQuality("chunked");

            statusLabel.Visible = true;
            //
            downloadProgress.Visible = true;
            cancelButton.Enabled = true;
        }

        public void StartDownload()
        {
            SetStatus("Downloading");
            SetProgressMarquee(false);

            vD = new VodDownloader();
            vD.ProgressSetMax += (object sender, ProgessEventArgs e) => SetProgressMax(e.Progress);
            vD.ProgressUpdated += (object sender, ProgessEventArgs e) => SetProgress(e.Progress);
            vD.VODCompleted += VD_VODCompleted;
            vD.StartVodDownload(GetFolderName(data), data, "chunked");
        }

        public void StartConvert()
        {
            //Called on Load to resume incomplete convert
            //Called after Download completeted
            SetStatus("Merging");
            vM = new VodMerger();
            vM.VODCompleted += VM_VODCompleted;
            vM.Start(GetFolderName(data), parts, Format.MKV, false);
            //vM.Start(GetFolderName(data), parts, Format.MP4, false);
        }

        private void VD_VODCompleted(object sender, VodDownloadCompleted e)
        {
            vD = null;
            if (!Cancel)
            {
                SetStatus("Queued");
                SetProgressMarquee(true);
            }
            parts = e.Chunks;
            DownloadCompleted?.Invoke(this, new EventArgs());
        }

        private void VM_VODCompleted(object sender, EventArgs e)
        {
            vM = null;
            if (!Cancel)
            {
                SetStatus("Completed");
            }
            SetProgressMarquee(false);
            ConvertCompleted?.Invoke(this, new EventArgs());
        }

        private void Populate()
        {
            titleLabel.Text = data.title;
            channelLabel.Text = data.channel.display_name;
            gameLabel.Text = data.game;
            recordedLabel.Text = DateTime.Parse(data.recorded_at).ToString();
            //Select Max Quality
            PopulateQuality("chunked");
            TimeSpan t = TimeSpan.FromSeconds(data.length);
            lengthLabel.Text = t.ToString(@"hh\:mm\:ss");
            previewBox.ImageLocation = data.preview;
        }

        private void PopulateQuality(string quality)
        {
            if (data.resolutions.ContainsKey(quality))
            {
                if (data.fps.ContainsKey(quality))
                {
                    qualityLabel.Text = data.resolutions[quality] + "@" + data.fps[quality].ToString("0.##");
                }
                else
                {
                    qualityLabel.Text = data.resolutions[quality];
                }
            }
            else
            {
                qualityLabel.Text = "Audio Only";
            }
        }

        public string GetFolderName(VideoData Video)
        {
            //The TwitchAPI and the TwitchAPI had diffrent ideas about spaces
            string folderName = Video.recorded_at + "_" + Video.title;
            folderName = folderName.Replace("\\\\", "_").Replace("/", "_").Replace("\"", "_").Replace("*", "_").Replace(":", "_").Replace("?", "_").Replace("<", "_").Replace(">", "_").Replace("|", "_").Trim().TrimEnd('.');

            //Deal with streams that have stupid long titles
            const int maxFullName = 260 - 1;
            int maxDirLen = 248 - 1;
            //                           "Merged.xyz
            //                           "Merged.tmp2
            //                           "Done.Convert
            //                           "Done.Download"
            //                           "Part 10000.ts"
            //                           "_StreamURL.txt"
            //                           "Temp100.mkv" 'FLV convert
            int maxFileLen = "_StreamURL.txt".Length + 1;
            //+1 to Account for "/"
            int maxFullNameRemaining = maxFullName - maxFileLen;
            if (maxFullNameRemaining < maxDirLen)
            {
                maxDirLen = maxFullNameRemaining;
            }
            maxDirLen -= (Directory.GetCurrentDirectory().Length + 1);
            //+1 to Account for "/"

            if ((folderName.Length) > maxDirLen)
            {
                folderName = folderName.Substring(0, maxDirLen);
            }
            return folderName;
        }

        private void SetProgressMax(int max)
        {
            if (downloadProgress.InvokeRequired)
            {
                downloadProgress.Invoke(new Action(() => SetProgressMax(max)));
            }
            else
            {
                downloadProgress.Maximum = max;
            }
        }
        private void SetProgress(int value)
        {
            if (downloadProgress.InvokeRequired)
            {
                downloadProgress.Invoke(new Action(() => SetProgress(value)));
            }
            else
            {
                downloadProgress.Value = value;
            }
        }
        private void SetProgressMarquee(bool value)
        {
            if (downloadProgress.InvokeRequired)
            {
                downloadProgress.Invoke(new Action(() => SetProgressMarquee(value)));
            }
            else
            {
                if (value)
                {
                    downloadProgress.Style = ProgressBarStyle.Marquee;
                    //downloadProgress.MarqueeAnimationSpeed = 30;
                }
                else
                {
                    downloadProgress.Style = ProgressBarStyle.Continuous;
                }
            }
        }

        private void SetStatus(string value)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action(() => SetStatus(value)));
            }
            else
            {
                statusLabel.Text = value;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            cancelButton.Enabled = false;
            vD?.Cancel();
            vM?.Cancel();

            SetProgressMarquee(false);
            //SetProgress(0);

            Cancel = true;
            Canceled?.Invoke(this, new EventArgs());
        }

        private void CancelButton_MouseLeave(object sender, EventArgs e)
        {
            if (!panel1.Bounds.Contains(panel1.PointToClient(MousePosition)))
            {
                cancelButton.Visible = false;
            }
        }

        private void VodDownloadPanel_MouseEnter(object sender, EventArgs e)
        {
            if (cancelButton.Enabled)
            {
                cancelButton.Visible = true;
            }
        }

        private void VodDownloadPanel_MouseLeave(object sender, EventArgs e)
        {
            if (!cancelButton.Bounds.Contains(panel1.PointToClient(MousePosition)))
            {
                cancelButton.Visible = false;
            }
        }
    }
}

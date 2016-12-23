using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TwitchVodDownloaderSharp.TwitchAPI;
using TwitchVodDownloaderSharp.UI;

namespace TwitchVodDownloaderSharp
{
    public partial class Form1 : Form
    {
        //Search Panel
        List<VodDownloadPanel> searchPanels = new List<VodDownloadPanel>();
        //Download Panel
        List<VodDownloadPanel> downloadPanels = new List<VodDownloadPanel>();

        List<VodDownloadPanel> queueDownload = new List<VodDownloadPanel>();
        VodDownloadPanel currentDownload;
        List<VodDownloadPanel> queueConvert = new List<VodDownloadPanel>();
        VodDownloadPanel currentConvert;
        List<VodDownloadPanel> completed = new List<VodDownloadPanel>();

        public Form1()
        {
            InitializeComponent();
        }

        //TODO
        //figure out handling Close()

        private void button1_Click(object sender, EventArgs e)
        {
            //Get Vid or Channel
            string entry = urlBox.Text;
            if (entry.StartsWith("https://"))
            {
                entry = entry.Substring("https://".Length);
            }
            else if (entry.StartsWith("http://"))
            {
                entry = entry.Substring("http://".Length);
            }

            string[] entrySplit = entry.Split('/');

            ulong id = 0;

            switch (entrySplit.Length)
            {
                case 1:
                    if (entry.StartsWith("v") &&
                        ulong.TryParse(entry.Substring(1), out id))
                    {
                        //Populate with single video
                        HandleSingleVod(entry);
                    }
                    else
                    {
                        //Populate with channel videos
                    }
                    break;
                case 2:
                    if (entrySplit[0] == "www.twitch.tv")
                    {
                        //Populate with channel videos
                    }
                    break;
                case 4:
                    if (entrySplit[0] == "www.twitch.tv" &&
                        //Don't bother validating channel
                        entrySplit[2] == "v" &&
                        ulong.TryParse(entrySplit[3], out id))
                    {
                        HandleSingleVod("v" + entrySplit[3]);
                    }
                    break;
            }
        }

        private void HandleSingleVod(string id)
        {
            //TODO
            //Present Options to user
            //Then download after option selected

            //Take the presented Stream Info,
            //Convert it to a Stream Download Box
            //And move it to the Download panel
            VideoData vi = Twitch.GetVideoInfo(id);
            VodDownloadPanel vdp = new VodDownloadPanel();
            vdp.StreamInfo = vi;
            lock (downloadPanels)
            {
                AddUIEntry(tabDownload, downloadPanels, vdp);
            }

            tabControl1.SelectedIndex = 1;

            queueDownload.Add(vdp);
            vdp.Queued();
            PopDownloadQueue();
        }

        private void AddUIEntry(TabPage uiPanel, List<VodDownloadPanel> uiList, VodDownloadPanel vdp)
        {
            vdp.DownloadCompleted += DownloadCompleted;
            vdp.ConvertCompleted += ConvertCompleted;
            vdp.Canceled += Canceled;

            if (downloadPanels.Count != 0)
            {
                vdp.Location = new Point(0, 3 + vdp.Size.Height + downloadPanels[downloadPanels.Count - 1].Location.Y);
            }
            uiList.Add(vdp);
            uiPanel.Controls.Add(vdp);
        }

        private void RemoveUIEntry(TabPage uiPanel, List<VodDownloadPanel> uiList, VodDownloadPanel vdp)
        {
            vdp.DownloadCompleted -= DownloadCompleted;
            vdp.ConvertCompleted -= ConvertCompleted;
            vdp.Canceled -= Canceled;

            uiList.Remove(vdp);
            if (uiPanel.InvokeRequired)
            {
                uiPanel.Invoke(new Action(() => 
                {
                    uiPanel.Controls.Remove(vdp);
                    vdp.Dispose();
                }));
            }
            else
            {
                uiPanel.Controls.Remove(vdp);
                vdp.Dispose();
            }
            ReDrawUIList(uiList);
        }

        private void ReDrawUIList(List<VodDownloadPanel> uiList)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ReDrawUIList(uiList)));
                return;
            }

            lock (uiList)
            {
                for (int i = 0; i < downloadPanels.Count; i++)
                {
                    if (i == 0)
                    {
                        downloadPanels[0].Location = new Point(0, 0);
                    }
                    else
                    {
                        downloadPanels[i].Location = new Point(0, 3 + downloadPanels[i].Height + downloadPanels[i - 1].Location.Y);
                    }
                }
            }
        }

        private void PopDownloadQueue()
        {
            lock (downloadPanels)
            {
                if (currentDownload == null)
                {
                    if (queueDownload.Count > 0)
                    {
                        currentDownload = queueDownload[0];
                        queueDownload.RemoveAt(0);
                        currentDownload.StartDownload();
                    }
                }
            }
        }

        private void PopConvertQueue()
        {
            lock (downloadPanels)
            {
                if (currentConvert == null)
                {
                    if (queueConvert.Count > 0)
                    {
                        currentConvert = queueConvert[0];
                        queueConvert.RemoveAt(0);
                        currentConvert.StartConvert();
                    }
                }
            }
        }

        private void DownloadCompleted(object sender, EventArgs e)
        {
            lock (downloadPanels)
            {
                VodDownloadPanel vdp = currentDownload;
                currentDownload = null;

                if (vdp.Cancel)
                {
                    RemoveUIEntry(tabDownload, downloadPanels, vdp);
                }
                else
                {
                    queueConvert.Add(vdp);
                }
                PopDownloadQueue();
                PopConvertQueue();
            }
        }

        private void ConvertCompleted(object sender, EventArgs e)
        {
            lock (downloadPanels)
            {
                VodDownloadPanel vdp = currentConvert;
                currentConvert = null;

                if (vdp.Cancel)
                {
                    RemoveUIEntry(tabDownload, downloadPanels, vdp);
                }

                PopConvertQueue();
            }
        }

        private void Canceled(object sender, EventArgs e)
        {
            if (sender is VodDownloadPanel)
            {
                lock (downloadPanels)
                {
                    VodDownloadPanel s = sender as VodDownloadPanel;

                    if (queueDownload.Contains(s))
                    {
                        queueDownload.Remove(s);
                        RemoveUIEntry(tabDownload, downloadPanels, s);
                    }
                    if (queueConvert.Contains(s))
                    {
                        queueConvert.Remove(s);
                        RemoveUIEntry(tabDownload, downloadPanels, s);
                    }
                    if (completed.Contains(s))
                    {
                        queueConvert.Remove(s);
                        RemoveUIEntry(tabDownload, downloadPanels, s);
                    }
                }
            }
        }
    }
}

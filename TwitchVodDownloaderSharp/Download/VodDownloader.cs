using System;
using System.Collections.Generic;
using System.IO;
using TwitchVodDownloaderSharp.TwitchAPI;

namespace TwitchVodDownloaderSharp.Download
{
    class VodDownloader
    {
        string directory;
        List<Chunk> videoParts;
        volatile bool cancel = false;

        ChunkTheardDispatcher threadDispatcher = new ChunkTheardDispatcher();

        public event EventHandler<ProgessEventArgs> ProgressSetMax; //Total Number of Parts
        public event EventHandler<ProgessEventArgs> ProgressUpdated;//Part Compleated
        public event EventHandler<VodDownloadCompleted> VODCompleted;

        public VodDownloader()
        {
            threadDispatcher.ProgressUpdated += HandleProgressUpdated;
            threadDispatcher.VODCompleated += HandleVodCompleted;
        }

        public void StartVodDownload(string vodDirectory, VideoData vodInfo, string quality)
        {
            //VideoData vodInfo = Twitch.GetVideoInfo(id);

            directory = vodDirectory;

            if (!File.Exists(Path.Combine(directory, "Download.Done")))
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string tempPath = Path.Combine(directory, "Temp");
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                else
                {
                    Directory.Delete(tempPath,true);
                    Directory.CreateDirectory(tempPath);
                }

                File.WriteAllText(Path.Combine(directory, "_StreamURL.txt"), vodInfo.url);

                Dictionary<string, string> videoQ = Twitch.GetTSVideoQualities(vodInfo._id);
                //List<SuperChunk> videoParts = Twitch.GetTSVideoParts(videoQ[quality]);
                videoParts = Twitch.GetTSVideoParts(videoQ[quality]);

                ProgressSetMax?.Invoke(this, new ProgessEventArgs(videoParts.Count));
                ProgressUpdated?.Invoke(this, new ProgessEventArgs(0));

                threadDispatcher.StartChunkDownloads(directory, tempPath, videoParts);
            }
            else
            {
                try
                {
                    Dictionary<string, string> videoQ = Twitch.GetTSVideoQualities(vodInfo._id);
                    //List<SuperChunk> videoParts = Twitch.GetTSVideoParts(videoQ[quality]);
                    List<Chunk> videoParts = Twitch.GetTSVideoParts(videoQ[quality]);

                    ProgressUpdated?.Invoke(this, new ProgessEventArgs(0));
                    ProgressSetMax?.Invoke(this, new ProgessEventArgs(videoParts.Count));
                    ProgressUpdated?.Invoke(this, new ProgessEventArgs(videoParts.Count));

                    VODCompleted?.Invoke(this, new VodDownloadCompleted(videoParts));
                }
                catch
                {
                    Console.WriteLine("Stream Load Error");

                    ProgressUpdated?.Invoke(this, new ProgessEventArgs(0));
                    ProgressSetMax?.Invoke(this, new ProgessEventArgs(0));

                    VODCompleted?.Invoke(this, new VodDownloadCompleted(null));
                }
            }
        }

        public void Cancel()
        {
            threadDispatcher.Cancel();
            cancel = true;
        }

        private void HandleVodCompleted(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(directory, "Temp");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }

            if (!cancel)
            {
                File.WriteAllText(Path.Combine(directory, "Download.Done"), "");
            }

            VODCompleted?.Invoke(this, new VodDownloadCompleted(videoParts));
        }

        private void HandleProgressUpdated(object sender, ProgessEventArgs e)
        {
            ProgressUpdated?.Invoke(this, e);
        }
    }
}

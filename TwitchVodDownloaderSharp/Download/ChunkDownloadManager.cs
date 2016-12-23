using System.IO;
using TwitchVodDownloaderSharp.TwitchAPI;

namespace TwitchVodDownloaderSharp.Download
{
    class ChunkPartManager
    {
        ChunkDownloader downloader = new ChunkDownloader();
        //public void Download(string directory, string fileName, SuperChunk chunkurl)
        public void Download(string directory, string tempDirectory, string fileName, Chunk chunkurl)
        {
            //Download to temp path, then copy into place
            string tempPath = Path.Combine(tempDirectory, fileName);
            string targetPath = Path.Combine(directory, fileName);
            //string failedPath = Path.Combine(directory, fileName);
            //failedPath = Path.ChangeExtension(failedPath, "failed");

            if (File.Exists(targetPath))
            {
                return;
            }

            ChunkDownloader.DownloadStatus state = downloader.Download(tempPath, chunkurl);

            switch (state)
            {
                case ChunkDownloader.DownloadStatus.Compleated:
                    File.Move(tempPath, targetPath);
                    break;
                case ChunkDownloader.DownloadStatus.Canceled:
                    break;
                case ChunkDownloader.DownloadStatus.Failed:
                    break;
            }
        }

        public void Cancel()
        {
            downloader.Cancel();
        }
    }
}

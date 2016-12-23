using System;
using System.IO;
using System.Net;
using System.Threading;
using TwitchVodDownloaderSharp.TwitchAPI;

namespace TwitchVodDownloaderSharp.Download
{
    class ChunkDownloader
    {
        const int BUFFER_SIZE = 81920;
        byte[] buffer = new byte[BUFFER_SIZE];
        const int CONNECT_ATTEMPTS = 3;

        public enum DownloadStatus
        {
            Compleated,
            Failed,
            Canceled
        }

        private volatile bool cancel = false;

        public DownloadStatus Download(string file, Chunk chunk)
        {
            //Retry 3 times to start download
            //Retry endlessly once we have started download

            WebResponse response = null;
            for (int x = 0; x <= CONNECT_ATTEMPTS; x++)
            {
                response = GetResponse(chunk.url);
                if (response != null) { break; }
                Thread.Sleep(100);
            }
            if (response == null) { return DownloadStatus.Failed; }

            long contentLength = response.ContentLength;

            //File exists, do infinite retries as needed
            while (!cancel)
            {
                using (Stream outFile = File.Create(file))
                {
                    outFile.SetLength(contentLength);

                    Stream inNet = response.GetResponseStream();

                    try
                    {
                        inNet.CopyTo(outFile);
                        if (outFile.Position != contentLength)
                        {
                            throw new Exception("Incomplete Download");
                        }
                        return DownloadStatus.Compleated;
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine("ChunkDownloaderDown: " + Path.GetFileName(file) + ": " + e.Message);

                        outFile.Seek(0, SeekOrigin.Begin);

                        inNet.Dispose();
                        response.Dispose();
                        response = null;
                        while (response == null)
                        {
                            Thread.Sleep(100);
                            if (cancel)
                            {
                                break;
                            }
                            response = GetResponse(chunk.url);
                        }
                        contentLength = response.ContentLength;
                    }
                    inNet.Dispose();
                    response.Dispose();
                    response = null;

                    outFile.Flush();
                } //end using
            }

            File.Delete(file);
            return DownloadStatus.Canceled;
        }

        public void Cancel()
        {
            cancel = true;
        }

        private WebResponse GetResponse(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                return request.GetResponse();
            }
            catch (Exception e)
            {
                Console.WriteLine("ChunkDownloader: " + e.Message);
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using TwitchVodDownloaderSharp.TwitchAPI;

namespace TwitchVodDownloaderSharp
{
    class ProgessEventArgs : EventArgs
    {
        public int Progress;
        public ProgessEventArgs(int parProgress)
        {
            Progress = parProgress;
        }
    }
    class VodDownloadCompleted : EventArgs
    {
        public List<Chunk> Chunks = new List<Chunk>();
        public VodDownloadCompleted(List<Chunk> parChunks)
        {
            Chunks = parChunks;
        }
    }
    class StringEvent : EventArgs
    {
        public string Value;
        public StringEvent(string parValue)
        {
            Value = parValue;
        }
    }
}

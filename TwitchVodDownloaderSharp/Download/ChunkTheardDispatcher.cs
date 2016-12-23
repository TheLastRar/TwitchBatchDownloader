using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TwitchVodDownloaderSharp.TwitchAPI;

namespace TwitchVodDownloaderSharp.Download
{
    class ChunkTheardDispatcher
    {
        const int THREADS = 10;

        int partsNext = -1;
        int partsDone = 0;
        int threadsDone = 0;
        volatile bool cancel = false;
        //Set before thread is started
        string directory;
        string tempDirectory;
        //List<SuperChunk> parts;
        List<Chunk> parts;
        List<string> partNames = new List<string>();
        
        private event EventHandler PartCompleted;
        private event EventHandler ThreadCompleted;

        public event EventHandler<ProgessEventArgs> ProgressUpdated;
        public event EventHandler VODCompleated;

        //public void StartChunkDownloads(string vodDirectory, List<SuperChunk> vodParts)
        //TODO test Download performance
        public void StartChunkDownloads(string vodDirectory, string tempDirectory, List<Chunk> vodParts)
        {
            directory = vodDirectory;
            this.tempDirectory = tempDirectory;
            parts = vodParts;

            PartCompleted += HandlePartCompleted;
            ThreadCompleted += HandleThreadCompleted;

            Thread[] threads = new Thread[THREADS];

            for (int i = 0; i < parts.Count; i++)
            {
                partNames.Add("Part" + (i + 1).ToString() + ".ts");
            }


            //int partsAllocated = 0;

            //List<int>[] allocatedParts = new List<int>[THREADS];
            //for (int i = 0; i < THREADS; i++)
            //{
            //    allocatedParts[i] = new List<int>();
            //}

            //while (partsAllocated != parts.Count)
            //{
            //    for (int i = 0; i < THREADS; i++)
            //    {
            //        if (partsAllocated != parts.Count)
            //        {
            //            partNames.Add("Part" + (partsAllocated + 1).ToString() + ".ts");

            //            string targetPath = Path.Combine(directory, partNames[partsAllocated]);

            //            if (File.Exists(targetPath))
            //            {
            //                partsAllocated++;
            //                partsDone++;
            //                continue;
            //            }

            //            allocatedParts[i].Add(partsAllocated);
            //            partsAllocated++;
            //        }
            //    }
            //}

            ProgressUpdated?.Invoke(this, new ProgessEventArgs(partsDone));

            for (int i = 0; i < THREADS; i++)
            {
                int iAP = i;
                threads[i] = new Thread(() => worker(/*allocatedParts[iAP]*/));
                threads[i].IsBackground = true;
                threads[i].Start();
            }
        }

        public void Cancel()
        {
            cancel = true;
        }

        private void HandlePartCompleted(object sender, EventArgs e)
        {
            int done = Interlocked.Increment(ref partsDone);
            Console.WriteLine(done);
            if (!cancel)
            {
                ProgressUpdated?.Invoke(this, new ProgessEventArgs(done));
            }
        }

        private void HandleThreadCompleted(object sender, EventArgs e)
        {
            int done = Interlocked.Increment(ref threadsDone);
            if (done == THREADS)
            {
                VODCompleated?.Invoke(this, new EventArgs());
            }
        }

        private int GetNextPart()
        {
            int Next = Interlocked.Increment(ref partsNext);
            if (Next > parts.Count - 1)
            {
                return -1;
            }
            return Next;
        }

        private void worker()//(List<int> ids)
        {
            ChunkPartManager cpm = new ChunkPartManager();

            //foreach (int i in ids)
            //{
            int i = GetNextPart();
            while (i != -1)
            { 
                if (cancel)
                {
                    break;
                }
                cpm.Download(directory, tempDirectory, partNames[i], parts[i]);
                PartCompleted(cpm, new EventArgs());
                i = GetNextPart();
            }
            ThreadCompleted(Thread.CurrentThread, new EventArgs());
        }
    }
}

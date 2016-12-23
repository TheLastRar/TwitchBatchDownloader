using MediaInfoLib;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TwitchVodDownloaderSharp.Merge
{
    class MediaInfoUtils
    {
        const int BUFFER_SIZE = 81920;
        public static void OpenInIsolation(MediaInfo mi, string directory, string file, out long fileSize)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            using (Stream infile = File.OpenRead(Path.Combine(directory, file)))
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    mi.Open_Buffer_Init(infile.Length, 0);
                    int bytesRead = 0;
                    do
                    {
                        bytesRead = infile.Read(buffer, 0, BUFFER_SIZE);
                        int status = mi.Open_Buffer_Continue(handle.AddrOfPinnedObject(), new IntPtr(bytesRead));
                        if ((status & 0x08) != 0)
                        {
                            break;
                        }

                        if (mi.Open_Buffer_Continue_GoTo_Get() != -1L)
                        {
                            infile.Seek(mi.Open_Buffer_Continue_GoTo_Get(), SeekOrigin.Begin);
                            mi.Open_Buffer_Init(infile.Length, infile.Position);
                        }
                    } while (bytesRead > 0);
                    mi.Open_Buffer_Finalize();
                    fileSize = infile.Length;
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        public static bool IsPartValid(MediaInfo mi, string directory, string file, bool isAudioOnly, out long fileSize, out decimal durationSeconds)
        {
            OpenInIsolation(mi, directory, file, out fileSize);
            durationSeconds = decimal.Parse(mi.Get(StreamKind.General, 0, "Duration")) / 1000m;

            string AudioTrack = mi.Get(StreamKind.Audio, 0, "Format");
            if (string.IsNullOrEmpty(AudioTrack))
            {
                return false;
            }

            if (isAudioOnly)
            {
                return true;
            }
            string VideoTrack = mi.Get(StreamKind.Video, 0, "Format");
            if (string.IsNullOrEmpty(VideoTrack))
            {
                return false;
            }
            return true;
        }

        //public static decimal GetDurationSeconds(MediaInfo mi, string directory, string file)
        //{
        //    try
        //    {
        //        OpenInIsolation(mi, directory, file);
        //        //MediaInfo returns duration as milliseconds
        //        return decimal.Parse(mi.Get(StreamKind.General, 0, "Duration")) / 1000m;
        //    }
        //    finally
        //    {
        //        mi.Close();
        //    }
        //}
    }
}

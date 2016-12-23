using System.Collections.Generic;
using System.IO;
using TwitchVodDownloaderSharp.TwitchAPI;

namespace TwitchVodDownloaderSharp.Merge
{
    class M3UPreProcesser
    {
        public static void ProcessAndSaveM3U(string directory, List<string> partNames, List<Chunk> partInfo,
            out List<List<string>> splitPartNames)
        {
            List<List<Chunk>> splitPartInfo = HandleDiscontinuity(partInfo);
            splitPartNames = new List<List<string>>();

            //Match the filenames with stream chunks
            int index = 0; 
            foreach (List<Chunk> subVod in splitPartInfo)
            {
                List<string> subPartNames = new List<string>();
                subPartNames.AddRange(partNames.GetRange(index, subVod.Count));
                splitPartNames.Add(subPartNames);
                index += subVod.Count;
            }

            for (index = 0; index < splitPartInfo.Count; index++)
            {
                using (StreamWriter f = File.CreateText(Path.Combine(directory, "list" + (index + 1).ToString() + ".m3u8")))
                {
                    //Write the m3u file
                    for (int pi = 0; pi < splitPartInfo[index].Count; pi++)
                    {
                        Chunk part = splitPartInfo[index][pi];
                        foreach (string m3u in part.m3u_params)
                        {
                            f.WriteLine(m3u);
                        }
                        f.WriteLine(splitPartNames[index][pi]);
                    }
                    f.WriteLine("#EXT-X-ENDLIST");
                }
            }
        }

        private static List<List<Chunk>> HandleDiscontinuity(List<Chunk> partInfo)
        {
            //Seach for DISCONTINUITY tags
            List<List<Chunk>> splitVod = new List<List<Chunk>>();
            List<Chunk> subVod = new List<Chunk>();
            List<string> headerPart = new List<string>();

            bool discontinuity = false;

            long length = 0;

            int xLengthIndex = -1;
            for (int x = 0; x < partInfo[0].m3u_params.Count; x++)
            {
                string str = partInfo[0].m3u_params[x];
                if (str.StartsWith("#EXTINF:"))
                {
                    continue;
                }
                if (str.StartsWith("#EXT-X-TWITCH-TOTAL-SECS:"))
                {
                    //keep track of the TOTAL-SECS tag so we can
                    //update it for the sub m3u files needed
                    //to handle DISCONTINUITY
                    xLengthIndex = x;
                }
                headerPart.Add(str);
            }

            foreach (Chunk part in partInfo)
            {
                if (part.m3u_params.Contains("#EXT-X-DISCONTINUITY"))
                {
                    discontinuity = true;
                    //split the m3u file at DISCONTINUITY tags.
                    subVod[0] = PrepHeader(subVod[0], headerPart, xLengthIndex, length);
                    length = 0;

                    splitVod.Add(subVod);
                    subVod = new List<Chunk>();
                }
                subVod.Add(part);
                length += (part.end_timestamp_ms - part.start_timestamp_ms);
            }

            //Add the last entry into the list
            if (discontinuity)
            {
                subVod[0] = PrepHeader(subVod[0], headerPart, xLengthIndex, length);

                splitVod.Add(subVod);
            }
            else
            {
                //No DISCONTINUITY, so no changes to the M3U needed.
                splitVod.Add(partInfo);
            }
            return splitVod;
        }

        private static Chunk PrepHeader(Chunk parChunk, List<string> header, int lengthIndex, long length)
        {
            //Change the m3u information from a DISCONTINUITY marker to 
            //a m3u file header, and set length as appropriate.
            decimal lengthSec = length / 1000m;
            Chunk newHeader = parChunk.Clone();
            if (!newHeader.m3u_params.Contains("#EXTM3U"))
            {
                newHeader.m3u_params.Remove("#EXT-X-DISCONTINUITY");
                List<string> newHeaderText = new List<string>();
                newHeaderText.AddRange(header);
                newHeaderText.AddRange(newHeader.m3u_params);
                newHeader.m3u_params = newHeaderText;
            }
            newHeader.m3u_params[lengthIndex] = "#EXT-X-TWITCH-TOTAL-SECS:" + lengthSec.ToString();
            return newHeader;
        }
    }
}

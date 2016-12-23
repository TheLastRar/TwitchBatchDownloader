using MediaInfoLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TwitchVodDownloaderSharp.TwitchAPI;

namespace TwitchVodDownloaderSharp.Merge
{
    public enum Format
    {
        AsSource, //No Convert
        TS,       //Merge to single TS not supported
        MKV,
        MP4
    }

    class VodMerger
    {
        //
        volatile bool cancel = false;
        //Process ff;
        FFmpeg ff = new FFmpeg();
        //Inputs
        string directory;
        List<Chunk> partInfo;
        List<string> partFileNames;
        bool isAudioOnly;
        Format format;

        public event EventHandler VODCompleted;

        public void Start(string vodDirectory, List<Chunk> vodParts, Format tarFormat, bool audioOnly = false)
        {
            directory = vodDirectory;
            partInfo = vodParts;
            partFileNames = new List<string>();
            for (int i = 0; i < vodParts.Count; i++)
            {
                partFileNames.Add("Part" + (i + 1).ToString() + ".ts");
            }

            format = tarFormat;
            isAudioOnly = audioOnly;

            Thread thread = new Thread(Worker);
            thread.Start();
        }

        public void Cancel()
        {
            cancel = true;
            ff.Cancel();
        }

        private void Worker()
        {
            //parts list needed
            //target format needed
            //folder needed
            //if audio only

            //need to check if start has both video and audio channels
            //or audio for audio only vods
            foreach (string part in partFileNames)
            {
                if (!File.Exists(Path.Combine(directory, part)))
                {
                    MessageBox.Show("ERROR: Download Incomplete");
                    VODCompleted?.Invoke(this, new EventArgs());
                    return;
                }
            }
            //Completed Convert
            if (File.Exists(Path.Combine(directory, "Convert.Done")))
            {
                VODCompleted?.Invoke(this, new EventArgs());
                return;
            }
            //Failed Convert
            if (File.Exists(Path.Combine(directory, "Merged." + format.ToString().ToLower())))
            {
                File.Delete(Path.Combine(directory, "Merged." + format.ToString().ToLower()));
            }

            switch (format)
            {
                case Format.AsSource:
                    VODCompleted?.Invoke(this, new EventArgs());
                    return;
                case Format.MKV:
                    JoinTSToMKV();
                    break;
                case Format.MP4:
                    JoinTSToMP4();
                    break;
            }

            if (File.Exists(Path.Combine(directory, "Merged." + format.ToString().ToLower())))
            {
                File.WriteAllText(Path.Combine(directory, "Convert.Done"), "");
            }

            VODCompleted?.Invoke(this, new EventArgs());
        }

        public bool AdjustAnalysisParams(string directory, List<string> partNames, out long mutedSize, out decimal mutedDuration)
        {
            //This function handles streams that start with 
            //delayed audio/video or where the vod was muted
            //by removing the audio channel (twitch used to
            //to do this)

            //Returns true if audio was found.

            //Sets mutedSize and mutedDuration to the Size/
            //Duration needed to seek into the segment to
            //detect audio/video
            //if no audio is found, returns the size of the
            //segment

            //for segmented vods, this is done per segment
            //with the largest delay being used.

            //EditZP stream fix V3
            MediaInfo mi = new MediaInfo();

            //Detect VODs starting with muted files
            int index = 0;

            mutedSize = 0;
            mutedDuration = 0;

            long retSize;
            decimal retDuration;

            while (!MediaInfoUtils.IsPartValid(mi, directory, partNames[index], isAudioOnly, out retSize, out retDuration))
            {
                index += 1;
                mutedSize += retSize;
                mutedDuration += retDuration;

                if (index == partNames.Count)
                {
                    //Whole segment is missing audio or video
                    return false;
                }
            }

            //Pad to pickup streams after muted section
            mutedSize += retSize;
            mutedDuration += retDuration;

            //Ensure We detect the audio channel even when VOD starts with muted files
            long newProbeSizeM = mutedSize / 1000000L + 1L; //Round up
            long newAnalyzeDurationM = Convert.ToInt64(Math.Ceiling(mutedDuration)); //Round up

            if (newProbeSizeM > ff.ProbeSizeM)
            {
                ff.ProbeSizeM = newProbeSizeM;
            }
            if (newAnalyzeDurationM > ff.AnalyzeDurationM)
            {
                ff.AnalyzeDurationM = newAnalyzeDurationM;
            }

            return true;
        }

        private List<string> GenerateParams(out long vodMutedStartSize, out decimal vodMutedStartDuration, out int vodMutedSegments)
        {
            //m3u streams need to be merged on a file level similar to the
            //ffmpeg's Concat protocol, but with more accurate timings.
            //but when a DISCONTINUITY tag is present, the stream cannont
            //be merge across this tag using Concat Protocol/File Merging.
            //ffmpeg appears to not understant the DISCONTINUITY tag correctly
            //and does this anyway.

            //I belive this is due to overlapping timestamps in the stream.

            //ffmpged adjusts the timestamps when it merges the segments,
            //allowing us to then merge across the DISCONTINUITY with the concat
            //protocol using these larger merged segments

            //when a DISCONTINUITY tag is found, we split the m3u file at each
            //DISCONTINUITY, and use that to do a partial merge of the stream
            //(M3U / Concat Protocol) and then merge these smaller parts to
            //form the final stream (Concat Demuxer)

            //the reason these tags occur is because these used to be flv
            //files that where 30 mins long. merging these often needed
            //to use a Concat Demuxer-ish approach.

            //Some of these older vods have a muted/corrupted segments. if
            //this occurs at the start of the vod, than the analyze params 
            //will need to be adjuested to pick up the audio channel, or else
            //a compleatly muted file will be created.

            //vodMutedStartDuration, vodMutedStartDuration and
            //vodMutedSegments are set to the values needed to detect audio/
            //video in the whole stream, including any muted/currupted segments
            //that might occur in older segmented vods

            List<List<string>> splitPartNames;

            M3UPreProcesser.ProcessAndSaveM3U(directory, partFileNames, partInfo, out splitPartNames);

            vodMutedSegments = 0;
            vodMutedStartSize = 0;
            vodMutedStartDuration = 0;

            bool foundValid = false;

            foreach (List<string> subPartNames in splitPartNames)
            {
                long retSize;
                decimal retDuration;
                bool isValid = AdjustAnalysisParams(directory, subPartNames, out retSize, out retDuration);
                //increase Muted counters until audio is found within the whole vod
                if (!foundValid)
                {
                    vodMutedStartSize += retSize;
                    vodMutedStartDuration += retDuration;
                    if (isValid)
                    {
                        foundValid = true;
                    }
                    else
                    {
                        vodMutedSegments += 1;
                    }
                }
            }

            //Create ffmpeg arguments for m3u input
            List<string> argsList = new List<string>();
            for (int index = 0; index < splitPartNames.Count; index++)
            {
                string argsProtocol2 = "-i list" + (index + 1).ToString() + ".m3u8 ";
                string argsProtocol3 = "-copyinkf -c:v copy -c:a copy -y "; //-copyts

                argsList.Add(argsProtocol2 + argsProtocol3);
            }
            return argsList;
        }

        private void JoinTSToX(string additinalParams, string file)
        {
            long vodMutedStartSize;
            decimal vodMutedStartDuration;
            int vodMutedSegments;
            List<string> argsList = GenerateParams(out vodMutedStartSize, out vodMutedStartDuration, out vodMutedSegments);

            StringBuilder argsProtocol = new StringBuilder();

            #region NewVod
            if (argsList.Count == 1)
            {
                //Fully continuous stream
                //Merge directly to final file
                argsProtocol.AppendFormat("{0}{1} {2}", argsList[0], additinalParams, file);
                ff.StartFFMPEG(directory, argsProtocol.ToString());
                if (File.Exists(Path.Combine(directory, "list1.m3u8")))
                {
                    File.Delete(Path.Combine(directory, "list1.m3u8"));
                }
                return;
            }
            #endregion
            #region LegacyVod
            //Discontinuous stream,
            //perform merge in multiple steps
            //merge to larger segments using the M3U files
            for (int i = 0; i < argsList.Count; i++)
            {
                argsProtocol.AppendFormat("{0}Temp{1}.ts", argsList[i], (i + 1).ToString());

                ff.StartFFMPEG(directory, argsProtocol.ToString());
                argsProtocol.Clear();

                if (cancel)
                {
                    break;
                }
            }

            if (cancel)
            {
#if !DEBUG
                DeleteTempSegments(argsList.Count);
#endif
                return;
            }

            //Get Aspect ratio from a known good segment.
            //Otherwise, the AR will be set to the corrupted
            //segment and will look squashed.
            MediaInfo mi = new MediaInfo();
            long retSize;
            MediaInfoUtils.OpenInIsolation(mi, directory, "Temp" + (vodMutedSegments + 1).ToString() + ".ts", out retSize);
            string dar = mi.Get(StreamKind.Video, 0, "DisplayAspectRatio/String");
            mi.Close();

            //Merge these larger segments to the final file
            argsProtocol.Append("-i \"concat:");
            for (int i = 0; i < argsList.Count; i++)
            {
                argsProtocol.AppendFormat("Temp{0}.ts|", i + 1);
            }
            argsProtocol.Length = argsProtocol.Length - 1;
            argsProtocol.Append("\" ");
            argsProtocol.Append("-copyinkf -c:v copy -c:a copy -y ");
            if (vodMutedSegments != 0)
            {
                //force correct aspect ratio
                argsProtocol.AppendFormat("-aspect \"{0}\" ", dar); 
            }

            //FFmpeg concat to mkv is bugged in this situation, it's surposed to work with -bsf:a aac_adtstoasc?
            //this makes a complete reversal to TwithBatchDownloader, where mp4's would always bug out.

            //TODO, try adding -bsf:a aac_adtstoasc to parts before and then see if we can merge to mkv

            argsProtocol.AppendFormat("-bsf:v h264_mp4toannexb -bsf:a aac_adtstoasc {0} Temp.mp4", additinalParams.Replace("-bsf:a aac_adtstoasc", ""));

            long newProbeSizeM = vodMutedStartSize / 1000000L + 2L; //Pad to pickup streams after muted section
            long newAnalyzeDurationM = Convert.ToInt64(Math.Floor(vodMutedStartDuration)) + 2L; //Pad to pickup streams after muted section

            ff.ProbeSizeM = newProbeSizeM;
            ff.AnalyzeDurationM = newAnalyzeDurationM;

            ff.StartFFMPEG(directory, argsProtocol.ToString());
            argsProtocol.Clear();

#if !DEBUG
            DeleteTempSegments(argsList.Count);
#endif

            if (file.EndsWith(".mp4"))
            {
                File.Move(Path.Combine(directory, "Temp.mp4"), Path.Combine(directory, file));
            }
            else
            {
                argsProtocol.AppendFormat("-i Temp.mp4 -copyinkf -c:v copy -c:a copy -y {0}", file);

                ff.StartFFMPEG(directory, argsProtocol.ToString());
                argsProtocol.Clear();
#if !DEBUG
                if (File.Exists(Path.Combine(directory, "Temp.mp4")))
                {
                    File.Delete(Path.Combine(directory, "Temp.mp4"));
                }
#endif
            }

            //Reset to defaults
            ff.ProbeSizeM = 2L;
            ff.AnalyzeDurationM = 2L;
#endregion
        }

#if !DEBUG
        private void DeleteTempSegments(int count)
        {

            for (int i = 0; i < count; i++)
            {
                if (File.Exists(Path.Combine(directory, "Temp" + (i + 1).ToString() + ".mkv")))
                {
                    File.Delete(Path.Combine(directory, "Temp" + (i + 1).ToString() + ".mkv"));
                }
                if (File.Exists(Path.Combine(directory, "list" + (i + 1).ToString() + ".m3u8")))
                {
                    File.Delete(Path.Combine(directory, "list" + (i + 1).ToString() + ".m3u8"));
                }
            }
        }
#endif

        private void JoinTSToMKV()
        {
            JoinTSToX("", "Merged.mkv");

            if (cancel)
            {
                if (File.Exists(Path.Combine(directory, "Merged.mkv")))
                {
                    File.Delete(Path.Combine(directory, "Merged.mkv"));
                }
            }
        }

        private void JoinTSToMP4()
        {
            JoinTSToX("-bsf:a aac_adtstoasc", "Merged.mp4");

            if (cancel)
            {
                if (File.Exists(Path.Combine(directory, "Merged.mp4")))
                {
                    File.Delete(Path.Combine(directory, "Merged.mp4"));
                }
            }
        }
    }
}

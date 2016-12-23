//using MediaInfoLib;
//using System.Text;

//namespace TwitchVodDownloaderSharp.Merge
//{
//    class VodFudger
//    {
//        public static bool SegmentedHandleCurruptedStart(FFmpeg ff, string directory, int segmentCount, bool isAudioOnly)
//        {
//            //Check for corrupted parts at the start of the stream
//            //TODO test with muted streams that where recorded after 
//            //Twitch started muting vods (6 august 2014), but before
//            //the hls system was brouhgt in to the relevent channel
//            //(The stream will have has DISCONTINUITY tags), finding
//            //a suitable stream might be difficult as there would be
//            //little reason to save a muted vod and not appeal it.

//            //This function checks if the stream starts with a currupted segment
//            //and creates a fake header segment to start the stream with so that
//            //ffmpeg can remux it with audio and the correct resolution.
//            MediaInfo mi = new MediaInfo();
//            int firstValidPart = 0;
//            while (!MediaInfoUtils.IsPartValid(mi, directory, "Temp" + (firstValidPart + 1).ToString() + ".mkv", isAudioOnly))
//            {
//                firstValidPart += 1;
//                if (firstValidPart == segmentCount)
//                {
//                    //Whole stream is missing audio or video
//                    return false;
//                }
//            }
//            //is starting segment missing audio/video
//            if (firstValidPart == 0)
//            {
//                return false;
//            }

//            //Use some of the imformation from a valid segment to make
//            //a fake header with the correct audio/video info
//            MediaInfoUtils.OpenInIsolation(mi, directory, "Temp" + (firstValidPart + 1).ToString() + ".mkv");

//            string audioSample = mi.Get(StreamKind.Audio, 0, "SamplingRate");
//            string audioC = mi.Get(StreamKind.Audio, 0, "Channels");
//            int audioCint = int.Parse(audioC);
//            string audioCPos = mi.Get(StreamKind.Audio, 0, "ChannelPositions");
//            string audioCff = ToffmpegChannelPos(audioCPos);

//            string videoWidth = mi.Get(StreamKind.Video, 0, "Width");
//            string videoHeight = mi.Get(StreamKind.Video, 0, "Height");
//            string DisplayAspectRatio = mi.Get(StreamKind.Video, 0, "DisplayAspectRatio/String");

//            mi.Close();

//            StringBuilder args = new StringBuilder();
//            //ffmpeg remuxing to mp4 wants audio stream to be large enough.
//            //with silence, the audio stream is to small, so generate sound
//            //to pad out the audio stream using ffmpegs aevalsrc for a sinwave.
//            args.Append("-f lavfi -i \"aevalsrc=");
//            for (int i = 0; i < audioCint; i++)
//            {
//                args.Append("sin(440*2*PI*t)|");
//            }
//            args.Length -= 1;
//            args.AppendFormat(":s={0}:c={1}:d={2}\" ", audioSample, audioCff, ff.AnalyzeDurationM);
//            args.Append("-t 10 -y Temp.wav");
//            //Save dummy audio to temporty wav file
//            ff.StartFFMPEG(directory, args.ToString());

//            args.Clear();
//            args.Append("-i Temp1.mkv ");
//            //args.Append("-i Temp1.mkv -i Temp.wav ");
//            args.AppendFormat("-c:v libx264 -vf scale={0}:{1} -aspect \"{2}\" ", videoWidth, videoHeight, DisplayAspectRatio);
//            //Bring the volumne down to almost nothing.
//            //args.Append("-af \"volume=0.000001\" -c:a aac -b:a 128k ");
//            //args.AppendFormat("-map 0:v:0 -map 1:a:0 -y -t {0}  Temp0.mkv", ff.AnalyzeDurationM);
//            args.AppendFormat("-y -t {0}  Temp0.mkv", ff.AnalyzeDurationM);
//            //Save the dummy file to start the vod with.
//            ff.StartFFMPEG(directory, args.ToString());
//#if !DEBUG
//            if (File.Exists(Path.Combine(directory, "Temp.wav")))
//            {
//                File.Delete(Path.Combine(directory, "Temp.wav"));
//            }
//#endif
//            return true;
//        }

//        private static string ToffmpegChannelPos(string miChannelPos)
//        {
//            string[] channels = miChannelPos.Split();
//            StringBuilder ret = new StringBuilder();
//            string prefix = "";
//            foreach (string channel in channels)
//            {
//                string ch = channel.TrimEnd(',');
//                switch (ch)
//                {
//                    case "Front:":
//                        prefix = "F";
//                        break;
//                    case "Back:":
//                        prefix = "B";
//                        break;
//                    case "L":
//                        ret.Append(prefix);
//                        ret.Append("L|");
//                        break;
//                    case "C":
//                        ret.Append(prefix);
//                        ret.Append("C|");
//                        break;
//                    case "R":
//                        ret.Append(prefix);
//                        ret.Append("R|");
//                        break;
//                }
//            }
//            ret.Length -= 1;
//            return ret.ToString();
//        }
//    }
//}

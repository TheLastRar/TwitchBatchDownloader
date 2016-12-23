using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace TwitchVodDownloaderSharp.TwitchAPI
{
    class Twitch
    {
        //Thread saftey?
        static Random rng = new Random();

        public static List<VideoData> GetVideos(string ChannelName, bool isHighLight)
        {
            string url = "https://api.twitch.tv/kraken/channels/" + ChannelName + "/videos?limit=100&offset=0&broadcasts=" + (!isHighLight).ToString();

            List<VideoData> AllVideos = new List<VideoData>();
            bool Done = false;

            do
            {
                XElement jsonResponse;

                try
                {
                    jsonResponse = GetTwitchFile(url);
                }
                catch
                {
                    Thread.Sleep(1000);
                    continue;
                }

                DataContractJsonSerializer js = CreateSurrogateSerializer(typeof(VideoList));

                VideoList videoReply = (VideoList)js.ReadObject(jsonResponse.CreateReader());

                AllVideos.AddRange(videoReply.videos);

                url = videoReply._links["next"];

                if (AllVideos.Count == videoReply._total)
                {
                    Done = true;
                }
            } while (!(Done));

            return AllVideos;
        }

        public static VideoData GetVideoInfo(string _id)
        {
            string url = "https://api.twitch.tv/kraken/videos/" + _id + ".json";
            XElement jsonResponse = GetTwitchFile(url);

            DataContractJsonSerializer js = CreateSurrogateSerializer(typeof(VideoData));

            VideoData videoReply = (VideoData)js.ReadObject(jsonResponse.CreateReader());

            return videoReply;
        }

        public static bool IsTSVideo(string _id)
        {
            return _id.StartsWith("v");
        }

        //Twitch appears to have comverted c/d (flv) VODs to v (ts) format
        //the converted files seems cause issues with ffmpeg
        public static List<Chunk> GetTSVideoParts(string vodSourceM3UURL)
        {
            string[] SURLSplit = vodSourceM3UURL.Split('/');

            string BaseSourceURL = vodSourceM3UURL.Substring(0, vodSourceM3UURL.Length - SURLSplit[SURLSplit.Length - 1].Length);

            List<string> hlsResponse = GetM3UFile(vodSourceM3UURL);

            List<Chunk> reqQChunks = new List<Chunk>();

            List<string> chunkHeader = new List<string>();

            long pastTimeMilliseconds = 0;
            long currentTimeMilliseconds = 0;

            bool CleanListEnd = false;

            for (int x = 0; x <= hlsResponse.Count - 1; x++)
            {
                if (hlsResponse[x] == "#EXT-X-ENDLIST")
                {
                    CleanListEnd = true;
                    break;
                }

                if (hlsResponse[x] == "")
                    continue;

                if (hlsResponse[x].StartsWith("#"))
                {
                    string[] ext_X_Split = hlsResponse[x].Split(':');
                    if (ext_X_Split[0] == "#EXTINF")
                    {
                        decimal timeSeconds = decimal.Parse(ext_X_Split[1].TrimEnd(','));
                        pastTimeMilliseconds = currentTimeMilliseconds;
                        currentTimeMilliseconds += (long)(timeSeconds * 1000);
                    }
                    chunkHeader.Add(hlsResponse[x]);
                    continue;
                }

                Chunk currentChunk = new Chunk();
                currentChunk.m3u_params = chunkHeader;
                chunkHeader = new List<string>();

                currentChunk.url = Path.Combine(BaseSourceURL,hlsResponse[x]);

                currentChunk.start_timestamp_ms = pastTimeMilliseconds;
                currentChunk.end_timestamp_ms = currentTimeMilliseconds;

                reqQChunks.Add(currentChunk);
            }

            if ((CleanListEnd == false))
            {
                //Check if downloaded parts matches TOTAL-SECS
                string hlsLengthFeild = hlsResponse.Find(x => x.StartsWith("#EXT-X-TWITCH-TOTAL-SECS:"));
                string lenStr = hlsLengthFeild.Substring("#EXT-X-TWITCH-TOTAL-SECS:".Length);

                long hlsLength = (long)(decimal.Parse(lenStr) * 1000m);
                long chunkLength = reqQChunks[reqQChunks.Count - 1].end_timestamp_ms;

                if (hlsLength != chunkLength)
                    throw new Exception("Incomplete M3U8");
            }

            //return reqQ;
            return reqQChunks;
        }

        public static Dictionary<string, string> GetTSVideoQualities(string _id)
        {
            AccessToken at = GetAccessToken(_id);
            //remove 'v' from id"
            string url = "http://usher.twitch.tv/vod/" + _id.Substring(1) + "?player=twitchweb&p=" + rng.Next() + "&type=any&allow_source=true&allow_audio_only=true&nauthsig=" + at.sig + "&nauth=" + at.token;

            List<string> hlsResponse = GetM3UFile(url);

            //Find Source Q
            Dictionary<string, string> groupToQuality = new Dictionary<string, string>();
            Dictionary<string, string> qualityLinks = new Dictionary<string, string>();
            for (int x = 0; x <= hlsResponse.Count - 1; x++)
            {
                string[] ext_X_Split = hlsResponse[x].Split(':');
                if (ext_X_Split.Length > 1)
                {
                    string[] ext_Params = ext_X_Split[1].Split(',');
                    Dictionary<string, string> Params = new Dictionary<string, string>();
                    for (int pID = 0; pID <= ext_Params.Length - 1; pID++)
                    {
                        string[] pSplit = ext_Params[pID].Split('=');

                        if (pSplit[1].StartsWith("\"") & !pSplit[1].EndsWith("\""))
                        {
                            //deal with CODEC entry
                            string str = pSplit[1];
                            str = str + "," + ext_Params[pID + 1];
                            Params.Add(pSplit[0], str);
                            pID++; //Skip other half of CODEC entry
                        }
                        else {
                            Params.Add(pSplit[0], pSplit[1]);
                        }
                    }

                    if (ext_X_Split[0] == "#EXT-X-MEDIA")
                    {
                        groupToQuality.Add(Params["GROUP-ID"], Params["NAME"]);
                    }
                    if (ext_X_Split[0] == "#EXT-X-STREAM-INF")
                    {
                        //qualityLinks.Add(groupToQuality[Params["VIDEO"]].Trim('"'), hlsResponse[x + 1]);
                        qualityLinks.Add(Params["VIDEO"].Trim('"'), hlsResponse[x + 1]);
                        x++; //Skip URL
                    }
                }
            }
            return qualityLinks;
        }

        public static AccessToken GetAccessToken(string _id)
        {
            string url = "https://api.twitch.tv/api/vods/" + _id.Substring(1) + "/access_token.json";
            //remove 'v' from id

            XElement jsonResponse = GetTwitchFile(url);

            AccessToken TokenReply = new AccessToken();

            TokenReply.token = (string)jsonResponse.Element("token");
            TokenReply.sig = (string)jsonResponse.Element("sig");

            return TokenReply;
        }

        static DataContractJsonSerializer CreateSurrogateSerializer(Type type)
        {
            // Create an instance of the DataContractSerializer. The 
            // constructor demands a knownTypes and surrogate. 
            // Create a Generic List for the knownTypes. 
            List<Type> knownTypes = new List<Type>();
            DictionaryTwitch surrogate = new DictionaryTwitch();
            DataContractJsonSerializer surrogateSerializer = new DataContractJsonSerializer(type,
                                                    knownTypes, int.MaxValue, false, surrogate, true);
            return surrogateSerializer;
        }

        private static XElement GetTwitchFile(string url)
        {
            bool PartListDownloadDone = false;
            XDocument apiReply = null;
            do
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Accept = "application/vnd.twitchtv.v3+json";
                request.Method = "GET";
                RequestCachePolicy CP = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                request.CachePolicy = CP;

                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();


                //using (StreamReader StreamReader = new StreamReader(stream))
                //{
                //    while (!StreamReader.EndOfStream)
                //    {
                //        JSONResponse += StreamReader.ReadLine();
                //    }
                //}

                XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(stream, new XmlDictionaryReaderQuotas());
                apiReply = XDocument.Load(reader);
                XElement xReply = apiReply.Root;

                if (xReply.Element("error") == null)
                {
                    PartListDownloadDone = true;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            } while (!(PartListDownloadDone == true));
            return apiReply.Root;
        }

        private static List<string> GetM3UFile(string url)
        {
            //This can return an empty list sometimes
            //TODO Investigate
            List<string> HLSResponse = new List<string>();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Accept = "application/x-mpegurl";
            request.Method = "GET";
            RequestCachePolicy CP = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            request.CachePolicy = CP;
            WebResponse response = request.GetResponse();
            Stream StreamNet = response.GetResponseStream();

            //response.ContentLength
            MemoryStream StreamTemp = new MemoryStream();

            while (StreamTemp.Length < response.ContentLength)
            {
                StreamNet.CopyTo(StreamTemp);
            }

            StreamTemp.Seek(0, SeekOrigin.Begin);

            using (StreamReader StreamReader = new StreamReader(StreamTemp))
            {
                while (!StreamReader.EndOfStream)
                {
                    HLSResponse.Add(StreamReader.ReadLine());
                }
            }

            //StreamTemp.Dispose() (Disposed in StreamReader?)
            StreamNet.Dispose();

            return HLSResponse;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TwitchVodDownloaderSharp.TwitchAPI
{
    //Data Types sent out by API for the downloader to use
    [DataContract]
    public class VideoList
    {
        [DataMember]
        public int _total;
        [DataMember]
        public IList<VideoData> videos;
        [DataMember]
        public Dictionary<string, string> _links;
    }
    [DataContract]
    public class VideoData
    {
        [DataMember]
        public string title;
        [DataMember]
        public string description;
        [DataMember]
        public string broadcast_id;
        [DataMember]
        public string broadcast_type;
        [DataMember]
        public string status;
        [DataMember]
        public string tag_list; //?
        [DataMember]
        public int views;
        [DataMember]
        public string created_at;
        [DataMember]
        public string url;
        [DataMember]
        public string _id;
        [DataMember]
        public string recorded_at;
        [DataMember]
        public string game;
        [DataMember] //seconds
        public long length;
        [DataMember]
        public string preview;
        [DataMember]
        public string animated_preview;
        [DataMember]
        public VideoThumbnail[] thumbnails;
        [DataMember] //includes audio_only
        public Dictionary<string, double> fps;
        [DataMember] //Excludes audio_only
        public Dictionary<string, string> resolutions;
        [DataMember]
        public Dictionary<string, string> _links;
        [DataMember]
        public VideoChannel channel;
    }
    [DataContract]
    public class VideoChannel
    {
        [DataMember]
        public string name;
        [DataMember]
        public string display_name;
    }
    [DataContract]
    public class VideoThumbnail
    {
        [DataMember]
        public string url;
        [DataMember]
        public string type;
    }
    //AccessToken for new TS Vods
    public class AccessToken
    {
        public string token;
        public string sig;
    }

    public class Chunk
    {
        public List<string> m3u_params;
        public string url;

        public long start_timestamp_ms;
        public long end_timestamp_ms;

        public Chunk Clone()
        {
            Chunk nC = new Chunk();
            nC.m3u_params = new List<string>(m3u_params);
            nC.url = url;
            nC.start_timestamp_ms = start_timestamp_ms;
            nC.end_timestamp_ms = end_timestamp_ms;

            return nC;
        }
    }
}

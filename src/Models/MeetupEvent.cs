using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace dotnetfoundation.Models
{
    public partial class MeetupEventData
    {
        [JsonProperty("results")]
        public List<MeetupEvent> Results { get; set; }

        [JsonProperty("meta")]
        public MeetupEventQueryMeta Meta { get; set; }
    }

    public partial class MeetupEventQueryMeta
    {
        [JsonProperty("next")]
        public Uri Next { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("total_count")]
        public long TotalCount { get; set; }

        [JsonProperty("link")]
        public Uri Link { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("lon")]
        public string Lon { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("signed_url")]
        public Uri SignedUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }

        [JsonProperty("lat")]
        public string Lat { get; set; }
    }

    public partial class MeetupEvent
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("time")]
        public long TimeMilliseconds { get; set; }

        public DateTimeOffset Date { get { return DateTimeOffset.FromUnixTimeMilliseconds(TimeMilliseconds); } }

        [JsonProperty("event_url")]
        public Uri EventUrl { get; set; }

        [JsonProperty("group")]
        public MeetupGroup Group { get; set; }
    }

    public partial class MeetupGroup
    {
        [JsonProperty("join_mode")]
        public JoinMode JoinMode { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("group_lon")]
        public double GroupLon { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("urlname")]
        public string Urlname { get; set; }

        [JsonProperty("group_lat")]
        public double GroupLat { get; set; }

        [JsonProperty("who")]
        public string Who { get; set; }
    }
    public enum JoinMode { Approval, Open };

}


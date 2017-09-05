using Newtonsoft.Json;
using System;

namespace dotnetfoundation.Models
{
    public class MeetupEvent
    {
        [JsonProperty(PropertyName = "group_name")]
        public string GroupName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "group_url")]
        public string GroupUrl { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "event_url")]
        public string EventUrl { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "time")]
        public DateTime Time { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; } = string.Empty;



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlinkDotnet.DTO{

    // TODO: Fix member names with proper casing
    public class NetworkDetail
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        public DateTime updated_at { get; set; }
        public string name { get; set; }
        public object network_key { get; set; }
        public string description { get; set; }
        public string network_origin { get; set; }
        public string locale { get; set; }
        public string time_zone { get; set; }
        public bool dst { get; set; }
        public int ping_interval { get; set; }
        public object encryption_key { get; set; }
        public bool armed { get; set; }
        public bool autoarm_geo_enable { get; set; }
        public bool autoarm_time_enable { get; set; }
        public string lv_mode { get; set; }
        public int lfr_channel { get; set; }
        public string video_destination { get; set; }
        public int storage_used { get; set; }
        public int storage_total { get; set; }
        public int video_count { get; set; }
        public int video_history_count { get; set; }
        public string arm_string { get; set; }
        public bool busy { get; set; }
        public bool camera_error { get; set; }
        public bool sync_module_error { get; set; }
        public object feature_plan_id { get; set; }
        public int account_id { get; set; }

        public static IEnumerable<NetworkDetail> GetNetworks(string networkResponse){
            var parsed = JObject.Parse(networkResponse);
            var networks = parsed.SelectToken("networks");

            return networks.Children().Select(n => n.ToObject<NetworkDetail>());
        }
    }


}
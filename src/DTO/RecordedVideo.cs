using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BlinkDotnet.DTO{

public class Envelope{
    public int Limit {get; set;}
    public RecordedVideo[] Videos {get; set;}
}
public class Description
{
    public bool partial { get; set; }
    public string camera_name { get; set; }
    public string network_name { get; set; }
    public string time_zone { get; set; }
}


    public class RecordedVideo
{
    public int id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public string camera_name { get; set; }
    public Description description { get; set; }
    public int length { get; set; }
    public int size { get; set; }
    public int upload_time { get; set; }
    public DateTime viewed { get; set; }
    public bool locked { get; set; }
    public bool ready { get; set; }
    public string encryption { get; set; }
    public object encryption_key { get; set; }
    public string storage_location { get; set; }
    public string thumbnail { get; set; }
    public string address { get; set; }
    public int account_id { get; set; }
    public int network_id { get; set; }
    public int camera_id { get; set; }
    public object event_id { get; set; }
    public bool partial { get; set; }
    public string network_name { get; set; }
    public string time_zone { get; set; }

        internal static IEnumerable<RecordedVideo> GetVideoList(string responseContents)
        {
            var result = new List<RecordedVideo>();
            // return JsonConvert.DeserializeObject<List<RecordedVideo>>(responseContents);
            JArray array;
            responseContents = responseContents.Replace("\"", "'");
            try {
                //array = JArray.Parse(responseContents);}
                JObject payload = JObject.Parse(responseContents);
                var videos = payload.SelectToken("videos").ToString();
                array = JArray.Parse(videos);
            }
            catch (JsonReaderException e){
                return new List<RecordedVideo>();
            }
    
            foreach(var v in array){
                var descriptionString = v.SelectToken("description").ToString();
                var videoObject = JObject.Parse(v.ToString());
                videoObject.Property("description").Remove();

            var settings = new JsonSerializerSettings
                {
                    Error = delegate(object sender, ErrorEventArgs args)
                    {            
                        args.ErrorContext.Handled = true;
                    }
                };

                var video = JsonConvert.DeserializeObject<RecordedVideo>(v.ToString(), settings);
                
                video.description = JsonConvert.DeserializeObject<Description>(descriptionString);
                result.Add(video);
            }

            return result;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace BlinkDotnet.DTO{

    public class Network {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool OnBoarded { get; set; }
    }

    public class Region {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class LoginResponse
    {
        public LoginResponse(string response)
        {
            // var regex = new Regex("\"[0-9]+\"");
            // var matches = regex.Matches(response);
            // foreach(var match in matches.OfType<Match>()){
            //     var network = match.Value.Replace("\"", "");
            //     response = response.Replace(network, $"N{network}");
            // }

            var parsed = JObject.Parse(response);

            this.Authtoken = (string) parsed.SelectToken("authtoken.authtoken");

            var regionProperty = parsed.SelectToken("region").ToObject<JObject>().Properties().First();
            
            this.Region = new Region() {
                Code = regionProperty.Name, 
                Name = (string) regionProperty.Value
            };

            var networksProperty = parsed.SelectToken("networks").ToObject<JObject>().Properties();
            this.Networks = networksProperty.Select(p => {
                var id = p.Name;
                return new Network() {
                    Id = id,
                    Name = (string) parsed.SelectToken($"networks.{id}.name"),
                    OnBoarded = (bool) parsed.SelectToken($"networks.{id}.onboarded")
                };
            });
        }
        public string Authtoken { get; set; }    

        public IEnumerable<Network> Networks {get; set;}

        public Region Region { get; set; }
    }
}
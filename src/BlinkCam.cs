using BlinkDotnet.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlinkDotnet
{
    // Implementation is based on the unofficial Blink protocol by MattTW
    // https://github.com/MattTW/BlinkMonitorProtocol

    public class BlinkCam : IBlinkCam
    {
        private readonly string userName;
        private readonly string password;
        private readonly ILogger<BlinkCam> logger;
        private const string hostTemplate = "rest.{0}.immedia-semi.com";
        private string urlBaseTemplate = "https://{0}";

        public BlinkCam(IConfiguration configuration, ILogger<BlinkCam> logger) : 
            this(configuration["AppSettings:BlinkUserName"], configuration["AppSettings:BlinkPassword"], logger)
        {
        }
        
        public BlinkCam(string userName, string password, ILogger<BlinkCam> logger)
        {
            this.userName = userName;
            this.password = password;
            this.logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            var host = "prod";
            HttpClient client = GetHttpClient(host);

            var loginBody = new { email = this.userName, password = this.password, client_specifier = "'iPhone 9.2 | 2.2 | 222'" };

            var content = new StringContent(JsonConvert.SerializeObject(loginBody), Encoding.UTF8, "application/json");

            var urlBase = GetUrlBase(host);

            try
            {
                using (var response = await client.PostAsync($"{urlBase}/login", content, cancelToken))
                {
                    response.EnsureSuccessStatusCode();
                    var contents = await response.Content.ReadAsStringAsync();
                    this.logger.LogInformation("Logged on");
                    return new LoginResponse(contents);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(new EventId(1, "Login"), ex, "logon error");
                throw ex;
            }
        }

        public async Task<IEnumerable<NetworkDetail>> GetNetworksAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            var loginResponse = await this.LoginAsync();
            var client = GetHttpClient(loginResponse);
            var urlBase = GetUrlBase(loginResponse);
            using (var response = await client.GetAsync($"{urlBase}/networks", cancelToken))
            {
                response.EnsureSuccessStatusCode();
                var responseContents = await response.Content.ReadAsStringAsync();
                return NetworkDetail.GetNetworks(responseContents);
            }
        }

        public async Task<IEnumerable<CameraEvent>> GetEventsAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            var loginResponse = await this.LoginAsync();
            var client = GetHttpClient(loginResponse);
            var urlBase = GetUrlBase(loginResponse);
            var networks = await this.GetNetworksAsync();
            var events = new List<CameraEvent>();
            foreach (var network in networks)
            {
                using (var response = await client.GetAsync($"{urlBase}/events/network/{network.Id}", cancelToken))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContents = await response.Content.ReadAsStringAsync();
                    events.AddRange(CameraEvent.GetEvents(responseContents));
                }
            }
            var eventsList = events.ToList();
            this.logger.LogInformation($"Found {eventsList.Count} events.");
            return eventsList;
        }

        public async Task<IEnumerable<RecordedVideo>> GetAllVideosListAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            var loginResponse = await this.LoginAsync();
            var client = GetHttpClient(loginResponse);
            var urlBase = GetUrlBase(loginResponse);
            var page = 1;
            var videos = new List<RecordedVideo>();
            while (true)
            {
                using (var response =
                    await client.GetAsync(
                        $"{urlBase}/api/v2/videos/changed?since=2016-01-01T23:11:21+0000&page={page++}", cancelToken))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContents = await response.Content.ReadAsStringAsync();
                    var videosList = RecordedVideo.GetVideoList(responseContents).ToList();
                    this.logger.LogInformation($"Found {videosList.Count} videos.");
                    if (!videosList.Any())
                    {
                        break;
                    }

                    videos.AddRange(videosList);
                }
            }
            this.logger.LogInformation($"Found a total of {videos.Count} videos.");
            return videos;
        }

        private string GetUrlBase(string host)
        {
            return string.Format(urlBaseTemplate, string.Format(hostTemplate, host));
        }

        private string GetUrlBase(LoginResponse loginResponse)
        {
            return string.Format(urlBaseTemplate, string.Format(hostTemplate, loginResponse.Region.Code));
        }

        private static HttpClient GetHttpClient(string host)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Host", string.Format(hostTemplate, host));
            return client;
        }

        private static HttpClient GetHttpClient(LoginResponse loginResponse)
        {
            var client = GetHttpClient(loginResponse.Region.Code);
            client.DefaultRequestHeaders.Add("TOKEN_AUTH", loginResponse.Authtoken);
            return client;
        }

        public async Task<MemoryStream> GetVideoAsMemoryStreamAsync(string videoAddress, CancellationToken cancelToken = default(CancellationToken))
        {
            var loginResponse = await this.LoginAsync();
            var client = GetHttpClient(loginResponse);
            var urlBase = GetUrlBase(loginResponse);

#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
            videoAddress = videoAddress.StartsWith("/") ? videoAddress.Substring(1, videoAddress.Length - 1) : videoAddress;
#pragma warning restore RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.

            using (var response = await client.GetAsync($"{urlBase}/{videoAddress}", cancelToken))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default(MemoryStream);
                }

                response.EnsureSuccessStatusCode();
              
                using(var stream = await response.Content.ReadAsStreamAsync())
                {
                    var buffer = new byte[stream.Length];
                    stream.Position = 0;
                    var length = await stream.ReadAsync(buffer, 0, buffer.Length);
                    return new MemoryStream(buffer);
                }

            }
        }
    }
}

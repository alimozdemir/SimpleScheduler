using Newtonsoft.Json;
using System;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleScheduler
{
    //ref : https://gist.github.com/jogleasonjr/7121367
    //Modified for dotnet core
    public class SlackClient
    {
        private string url;
        private readonly Encoding _encoding = new UTF8Encoding();

        public SlackClient(string urlWithAccessToken)
        {
            url = urlWithAccessToken;
        }

        //Post a message using simple strings
        public async Task PostMessage(string text, string username = null, string channel = null)
        {
            Payload payload = new Payload()
            {
                Channel = channel,
                Username = username,
                Text = text
            };

            await PostMessage(payload);
        }

        //Post a message using a Payload object
        public async Task PostMessage(Payload payload)
        {
            string payloadJson = JsonConvert.SerializeObject(payload);

            List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();
            data.Add(new KeyValuePair<string, string>("payload", payloadJson));

            using (var response =  await Utility.Client.PostAsync(url, new FormUrlEncodedContent(data)))
            {
                var result = await response.Content.ReadAsStringAsync();
                
                await Console.Out.WriteAsync(string.Format("Result from slack {0}", result));
            }
        }
    }

    //This class serializes into the Json payload required by Slack Incoming WebHooks
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
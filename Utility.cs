using System;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

namespace SimpleScheduler
{
    public class Utility
    {
        public static bool Notify { get; set; }
        public static string SlackKey { get; set; }
        public static int Period { get; set; } = 15;
        public static HttpClient Client { get; set; }

        public static void HttpInitialize()
        {
            Client = new HttpClient();
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            Client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });
            Client.DefaultRequestHeaders.Add("User-Agent", ua);
        }

        public static DateTimeOffset NextPeriod(int period = 0)
        {
            if (period == 0)
                period = Period;

            var date = DateTimeOffset.Now;
            var sec = date.Second;
            var min = date.Minute;
            var times = min / period;
            var surplus = min % period;
            var next = ((times + 1) * period) % 60;

            if (next == 0) //if hour increase
            {
                date = date.AddMinutes(min * -1);
                date = date.AddHours(1);
            }
            else
            {
                date = date.AddMinutes(surplus * -1);
                date = date.AddMinutes(period);
            }
            //clear seconds
            date = date.AddSeconds(sec * -1);

            return date;
        }


    }
}
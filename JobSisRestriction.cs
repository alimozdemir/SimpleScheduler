using System;
using System.Threading.Tasks;
using Quartz;
using System.Net.Http;
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;
using AngleSharp.Dom;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SimpleScheduler
{
    public class JobSisRestriction : IJob
    {
        private string url = "http://www.sis.itu.edu.tr/tr/ders_programlari/LSprogramlar/prg.php?fb={0}";
        async Task IJob.Execute(IJobExecutionContext context)
        {
            string fb = string.Empty;
            object fb_o;
            var map = context.JobDetail.JobDataMap;
            if (map.TryGetValue("fb", out fb_o))
            {
                fb = fb_o.ToString();
            }
            else
            {
                await Console.Error.WriteLineAsync(string.Format("Parameter error {0}.", DateTimeOffset.Now));
                return;
            }

            int crn = 0;
            object crn_o;

            if (map.TryGetValue("crn", out crn_o))
            {
                crn = int.Parse(crn_o.ToString());
            }
            else
            {
                await Console.Error.WriteLineAsync(string.Format("Parameter error {0}.", DateTimeOffset.Now));
                return;
            }

            try
            {

                var url_fb = string.Format(url, fb);

                using (var response = await Utility.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url_fb)))
                {
                    response.EnsureSuccessStatusCode();

                    using (var content = await response?.Content.ReadAsStreamAsync())
                    {
                        var document = await new HtmlParser().ParseAsync(content);
                        await Process(document, crn);
                    }

                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(string.Format("Http Error {0} {1}.", DateTimeOffset.Now, ex));
                return;
            }

        }
        private async Task Process(IHtmlDocument doc, int crn)
        {
            var trs = doc.QuerySelectorAll(".dersprg > tbody > tr");
            IElement theTr = null;
            foreach (var item in trs)
            {
                if (item.FirstChild.TextContent == crn.ToString())
                {
                    theTr = item;
                    break;
                }
            }

            if (theTr == null)
            {
                await Console.Error.WriteLineAsync(string.Format("No crn found {0} {1}.", DateTimeOffset.Now, crn));
                return;
            }
            Quartz.Logging.ILog logger = Quartz.Logging.LogProvider.GetLogger(typeof(JobSisRestriction));
            var crnT = theTr.FirstChild.TextContent;
            if (theTr.ChildNodes.Length > 12)
            {
                var rest = theTr.ChildNodes.ElementAt(11).TextContent;

                var fetch = MemoryRestriction.GetData(crn);

                if (!fetch.Equals("None") && !fetch.Equals(rest))
                {
                    logger.Log(Quartz.Logging.LogLevel.Info, () => $"There is an update on {crn}");
                    await Notify(crn, rest);
                }

                MemoryRestriction.SetData(crn, rest);
            }
        }

        private async Task Notify(int crn, string rest)
        {
            if (Utility.Notify && !string.IsNullOrEmpty(Utility.SlackKey))
            {
                SlackClient client = new SlackClient(Utility.SlackKey);
                await client.PostMessage($"[{DateTime.Now.ToString("T")}][{crn}][{rest}]");
            }
        }
    }

    public class MemoryRestriction
    {
        private static Dictionary<int, string> Data = new Dictionary<int, string>();

        public static void SetData(int crn, string restriction)
        {
            Data[crn] = restriction;
        }

        public static string GetData(int crn)
        {
            string res = string.Empty;
            if (!Data.TryGetValue(crn, out res))
            {
                res = "None";
            }

            return res;

        }
    }

}
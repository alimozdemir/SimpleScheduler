using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz.Logging;
using System.IO;
using System.Threading;

namespace SimpleScheduler
{
    public class Program
    {
        static AutoResetEvent autoEvent = new AutoResetEvent(false);
        public static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
            Utility.HttpInitialize();
            //true
            Utility.Notify = false;
            Utility.SlackKey = "https://hooks.slack.com/services/X";

            Start().GetAwaiter().GetResult();
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("Type 'exit' for exit.");
                string key = string.Empty;
                while (!key.Equals("exit"))
                {
                    key = Console.ReadLine();
                }
            }
            else
            {
                autoEvent.WaitOne();
            }
        }

        public static async Task Start()
        {
            QuartzConfig config = new QuartzConfig("Itu restrict");

            await config.Initialize();

            //Notify the program still working..
            await config.ScheduleJobNow<JobNotify>(30);

            if (File.Exists("listen.txt"))
            {
                var lines = File.ReadAllLines("listen.txt");
                foreach (var item in lines)
                {
                    var array = item.Split(',');
                    if (array.Length >= 2)
                    {
                        await config.Once<JobSisRestriction>(0,
                            new KeyValuePair<string, object>("fb", array[0]),
                            new KeyValuePair<string, object>("crn", array[1]));

                        await config.ScheduleJob<JobSisRestriction>(0, 2,
                            new KeyValuePair<string, object>("fb", array[0]),
                            new KeyValuePair<string, object>("crn", array[1]));
                    }

                }
            }
            else
            {
                Console.WriteLine("Not found : listen.txt");
            }

        }
    }
}

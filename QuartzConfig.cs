using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace SimpleScheduler
{
    public class QuartzConfig
    {
        string name;
        IScheduler scheduler;
        public QuartzConfig(string _name)
        {
            name = _name;
        }
        public async Task Initialize()
        {
            NameValueCollection cfg = new NameValueCollection();
            cfg.Add("quartz.scheduler.instanceName", name);
            cfg.Add("quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz");

            StdSchedulerFactory factory = new StdSchedulerFactory();
            factory.Initialize(cfg);
            scheduler = await factory.GetScheduler();
            // and start it off
            await scheduler.Start();
        }

        public async Task ScheduleJob<T>(int timeInterval = 0, int offsetMin = 0, params KeyValuePair<string, object>[] pms) where T : class
        {
            if (timeInterval == 0)
                timeInterval = Utility.Period;

            //parameter of the job
            JobDataMap map = new JobDataMap();
            Quartz.Logging.ILog logger = Quartz.Logging.LogProvider.GetLogger(typeof(JobSisRestriction));

            for (int i = 0; i < pms.Length; i++)
            {
                map.Add(pms[i]);
            }

            IJobDetail detail = JobBuilder.Create(typeof(T))
                    .WithIdentity("job1", "group1")
                    .SetJobData(map)
                    .Build();

            var startAt = Utility.NextPeriod(timeInterval);
            startAt = startAt.AddMinutes(offsetMin);
            logger.Log(Quartz.Logging.LogLevel.Info, () => $"Schedule will start at {startAt}");

            ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartAt(startAt)
                    .WithSimpleSchedule(i => i.WithIntervalInMinutes(timeInterval).RepeatForever())
                    .Build();

            await scheduler.ScheduleJob(detail, trigger);
        }
        public async Task ScheduleJobNow<T>(int timeInterval = 0, params KeyValuePair<string, object>[] pms) where T : class
        {
            if (timeInterval == 0)
                timeInterval = Utility.Period;

            //parameter of the job
            JobDataMap map = new JobDataMap();
            Quartz.Logging.ILog logger = Quartz.Logging.LogProvider.GetLogger(typeof(JobSisRestriction));

            for (int i = 0; i < pms.Length; i++)
            {
                map.Add(pms[i]);
            }

            IJobDetail detail = JobBuilder.Create(typeof(T))
                    .WithIdentity("job3", "group1")
                    .SetJobData(map)
                    .Build();

            logger.Log(Quartz.Logging.LogLevel.Info, () => $"Schedule will start now.");

            ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger3", "group1")
                    .StartNow()
                    .WithSimpleSchedule(i => i.WithIntervalInMinutes(timeInterval).RepeatForever())
                    .Build();

            await scheduler.ScheduleJob(detail, trigger);
        }

        public async Task Once<T>(int timeInterval = 0, params KeyValuePair<string, object>[] pms) where T : class
        {
            if (timeInterval == 0)
                timeInterval = Utility.Period;

            //parameter of the job
            JobDataMap map = new JobDataMap();

            for (int i = 0; i < pms.Length; i++)
            {
                map.Add(pms[i]);
            }

            IJobDetail detail = JobBuilder.Create(typeof(T))
                    .WithIdentity("once", "group1")
                    .SetJobData(map)
                    .Build();

            ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("onceTrigger", "group1")
                    .StartNow()
                    .Build();

            await scheduler.ScheduleJob(detail, trigger);
        }
    }
    public class ConsoleLogProvider : ILogProvider
    {
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= LogLevel.Info && func != null)
                {
                    Console.WriteLine("[" + DateTime.Now.ToString("T") + "] [" + level + "] " + func(), parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
    public class testJob : IJob
    {
        async Task IJob.Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync(string.Format("Greetings from HelloJob! {0}", DateTimeOffset.Now));
        }
    }
}
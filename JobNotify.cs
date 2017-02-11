using System;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace SimpleScheduler
{
    public class JobNotify : IJob
    {
        async Task IJob.Execute(IJobExecutionContext context)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now.ToString("T")}] I'm alive..");

            SlackClient client = new SlackClient(Utility.SlackKey);
            await client.PostMessage(sb.ToString());
        }
    }
}
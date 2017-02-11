# Simple job scheduler

It is designed for ITU website (http://www.sis.itu.edu.tr/). Basically it checks the website for what you need.
The notification of the system is Slack webhook for now, maybe you can integrate your notification system.
And it is a dotnet core program
##Slack Setup

At ~Program.cs there exists, 

```javascript
            Utility.Notify = false;
            Utility.SlackKey = "https://hooks.slack.com/services/X";
```

You can get your key from 'incoming-webhook' app in slack

###Libraries
1. Quartz
2. AngleSharp

###Jobs
For now, JobSisRestriction.cs is looking for a lesson if the restriction changes.


##Build and run

Publish the program
```
dotnet publish
```

for linux background run on terminal
```
nohup dotnet SimpleScheduler.dll &
```


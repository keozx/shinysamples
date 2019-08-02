using System;
using Shiny.Jobs;
using Samples.Models;
using Shiny;
using Shiny.Infrastructure;
using Samples.Jobs;

namespace Samples.ShinySetup
{
    public class JobLoggerTask : IShinyStartupTask
    {
        readonly IMyJobManager jobManager;
        readonly SampleSqliteConnection conn;
        readonly ISerializer serializer;


        public JobLoggerTask(IMyJobManager jobManager,
                             ISerializer serializer,
                             SampleSqliteConnection conn)
        {
            this.jobManager = jobManager;
            this.serializer = serializer;
            this.conn = conn;
        }


        public void Start()
        {
            this.jobManager.JobStarted += JobAndTaskStartedHandler;
            this.jobManager.JobFinished += JobAndTaskStartedHandler;
            this.jobManager.TaskStarted += JobAndTaskStartedHandler;
            this.jobManager.TaskFinished += JobAndTaskStartedHandler;

        }

        public async void JobAndTaskStartedHandler(object sender, JobInfo args)
        {
            await this.conn.InsertAsync(new JobLog
            {
                JobIdentifier = args.Identifier,
                JobType = args.Type.FullName,
                Started = true,
                Timestamp = DateTime.Now,
                Parameters = this.serializer.Serialize(args.Parameters)
            });
        }

        public async void JobAndTaskStartedHandler(object sender, JobRunResult args)
        {
            await this.conn.InsertAsync(new JobLog
            {
                JobIdentifier = args.Job.Identifier,
                JobType = args.Job.Type.FullName,
                Error = args.Exception?.ToString(),
                Parameters = this.serializer.Serialize(args.Job.Parameters),
                Timestamp = DateTime.Now
            });
        }
    }
}

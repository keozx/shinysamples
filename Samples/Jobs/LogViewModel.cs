﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs.Forms;
using ReactiveUI;
using Samples.Models;
using Samples.Infrastructure;
using Shiny;
using Shiny.Jobs;
using Shiny.Infrastructure;
using Prism.Navigation;

namespace Samples.Jobs
{
    public class LogViewModel : AbstractLogViewModel<CommandItem>
    {
        readonly IMyJobManager jobManager;
        readonly SampleSqliteConnection conn;
        readonly ISerializer serializer;


        public LogViewModel(IMyJobManager jobManager,
                            IUserDialogs dialogs,
                            ISerializer serializer,
                            SampleSqliteConnection conn) : base(dialogs)
        {
            this.jobManager = jobManager;
            this.serializer = serializer;
            this.conn = conn;
        }


        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);
            this.jobManager.JobStarted += this.OnJobStarted;
            this.jobManager.JobFinished += this.OnJobFinished;
            this.jobManager.TaskStarted += this.OnJobStarted;
            this.jobManager.TaskFinished += this.OnJobFinished;
        }


        public override void Destroy()
        {
            base.Destroy();
            this.jobManager.JobStarted -= this.OnJobStarted;
            this.jobManager.JobFinished -= this.OnJobFinished;
            this.jobManager.TaskStarted -= this.OnJobStarted;
            this.jobManager.TaskFinished -= this.OnJobFinished;
        }


        void OnJobStarted(object sender, JobInfo job) => this.Load.Execute();
        void OnJobFinished(object sender, JobRunResult job) => this.Load.Execute();


        protected override async Task<IEnumerable<CommandItem>> LoadLogs()
        {
            var logs = await this.conn
                .JobLogs
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();

            return logs.Select(x =>
            {
                var title = $"{x.JobIdentifier} ({x.JobType})";
                var msg = x.Started ? "Started" : "Finished";
                if (x.Error != null)
                    msg = $"ERROR - {x.Error}";

                msg += $" @ {x.Timestamp:G}";

                return new CommandItem
                {
                    Text = title,
                    Detail = msg,
                    PrimaryCommand = ReactiveCommand.CreateFromTask(async () =>
                    {
                        var job = await this.jobManager.GetJob(x.JobIdentifier);

                        var sb = new StringBuilder()
                            .AppendLine(msg)
                            .AppendLine($"Battery: {job.BatteryNotLow}")
                            .AppendLine($"Internet: {job.RequiredInternetAccess}")
                            .AppendLine($"Charging: {job.DeviceCharging}")
                            .AppendLine($"Repeat: {job.Repeat}");

                        if (!x.Parameters.IsEmpty())
                        {
                            var parameters = this.serializer.Deserialize<Dictionary<string, object>>(x.Parameters);
                            foreach (var p in parameters)
                                sb.AppendLine().Append($"{p.Key}: {p.Value}");
                        }
                        this.Dialogs.Alert(sb.ToString(), title);
                    })
                };
            });
        }

        protected override Task ClearLogs() => this.conn.DeleteAllAsync<JobLog>();
    }
}

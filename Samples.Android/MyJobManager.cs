using Android;
using Shiny;
using Shiny.Infrastructure;
using Shiny.Jobs;
using Shiny.Net;
using Shiny.Power;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Jobs
{
    public class MyJobManager : AbstractJobManager, IMyJobManager
    {
        readonly AndroidContext context;

        public MyJobManager(AndroidContext context, IServiceProvider container, IRepository repository, IPowerManager powerManager, IConnectivity connectivity) : base(container, repository, powerManager, connectivity)
        {
            this.context = context;
        }

        public override async void RunTask(string taskName, Func<CancellationToken, Task> task)
        {
            var jobInfo = new JobInfo() { Identifier = taskName, Type = typeof(ShinyDelegates.SampleJob) };

            try
            {
                this.LogTask(JobState.Start, taskName);
                TaskStarted?.Invoke(this, jobInfo);

                await task(CancellationToken.None).ConfigureAwait(false);

                TaskFinished?.Invoke(this, new JobRunResult(true, jobInfo, null));
                this.LogTask(JobState.Finish, taskName);
            }
            catch (Exception ex)
            {
                this.LogTask(JobState.Error, taskName, ex);
                TaskFinished?.Invoke(this, new JobRunResult(false, jobInfo, ex));
                throw;
            }
        }

        public event EventHandler<JobInfo> TaskStarted;
        public event EventHandler<JobRunResult> TaskFinished;

        public override Task<AccessState> RequestAccess()
        {
            var permission = AccessState.Available;

            if (!this.context.IsInManifest(Manifest.Permission.AccessNetworkState))
                permission = AccessState.NotSetup;

            if (!this.context.IsInManifest(Manifest.Permission.BatteryStats))
                permission = AccessState.NotSetup;

            //if (!this.context.IsInManifest(Manifest.Permission.ReceiveBootCompleted, false))
            //    permission = AccessState.NotSetup;

            return Task.FromResult(permission);
        }
    }
}

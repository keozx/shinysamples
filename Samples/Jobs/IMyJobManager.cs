using Shiny.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samples.Jobs
{
    public interface IMyJobManager : IJobManager
    {
        event EventHandler<JobInfo> TaskStarted;
        event EventHandler<JobRunResult> TaskFinished;
    }
}

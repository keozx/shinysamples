using System;
using Shiny;
using Android.App;
using Android.Runtime;
using Samples.ShinySetup;


namespace Samples.Droid
{
    [Application]
    public class MainApplication : ShinyAndroidApplication<SampleStartup>
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }


//        public override void OnCreate()
//        {
//            base.OnCreate();

//            AndroidShinyHost.Init(
//                this,
//                new SampleStartup()
//#if DEBUG
//                , services =>
//                {
//                    // TODO: make android great again - by running jobs faster for debugging purposes ;)
//                    services.ConfigureJobService(TimeSpan.FromMinutes(1));
//                    services.AddSingleton<MyJobManager>(); // We must explicitly register MyJobManager
//                    services.AddSingleton<IJobManager>(x => x.GetRequiredService<MyJobManager>()); // Forward requests to MyJobManager
//                    services.AddSingleton<IMyJobManager>(x => x.GetRequiredService<MyJobManager>()); // Forward requests to MyJobManager
//                }
//#endif
//            );
//        }
    }
}
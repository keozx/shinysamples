using System;
using Shiny;
using Shiny.Logging;
using Microsoft.Extensions.DependencyInjection;
using Samples.Settings;
using Samples.ShinyDelegates;
using Acr.UserDialogs.Forms;


namespace Samples.ShinySetup
{
    public class SampleStartup : ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //services.UseAppCenterLogging(Constants.AppCenterTokens, true, false);
            //services.UseSqliteLogging(true, true);
            //services.UseSqliteCache();
            //services.UseSqliteSettings();
            //services.UseSqliteStorage();
#if DEBUG
            Log.UseConsole();
            Log.UseDebug();
#endif

            // create your infrastructures
            // jobs, connectivity, power, filesystem, are installed automatically
            services.AddSingleton<SampleSqliteConnection>();
            services.AddSingleton<CoreDelegateServices>();
            services.AddSingleton<GlobalExceptionHandler>();
            services.AddSingleton<IFullService, FullService>();
            services.AddSingleton<IUserDialogs, UserDialogs>();

            // startup tasks
            services.AddSingleton<IFullService, FullService>();
            services.AddSingleton<JobLoggerTask>();

            // configuration
            services.AddSingleton<IAppSettings, AppSettings>();

            // register all of the shiny stuff you want to use
            services.UseHttpTransfers<HttpTransferDelegate>();
            services.UseBeacons<BeaconDelegate>();
            services.UseBleCentral<BleCentralDelegate>();
            services.UseBlePeripherals();

            //builder.UseGeofencing<LocationDelegates>(new GeofenceRegion("Test", new Position(1, 1), Distance.FromKilometers(1)));
            services.UseGeofencing<LocationDelegates>();
            services.UseGps<LocationDelegates>();
            services.UseMotionActivity();

            services.UseNotifications<NotificationDelegate>(true);
            services.UseSpeechRecognition();

            services.UseAccelerometer();
            services.UseAmbientLightSensor();
            services.UseBarometer();
            services.UseCompass();
            services.UseMagnetometer();
            services.UsePedometer();
            services.UseProximitySensor();
            services.UseHeartRateMonitor();
            services.UseTemperature();
            services.UseHumidity();
        }


#if DEBUG
        public override IServiceProvider CreateServiceProvider(IServiceCollection services)
            => services.BuildServiceProvider(true);
#endif
    }
}

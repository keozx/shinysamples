using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Acr.UserDialogs.Forms;
using ReactiveUI.Fody.Helpers;
using Shiny.Locations;


namespace Samples.MotionActivity
{
    public class OtherExtensionsViewModel : ViewModel
    {
        readonly IMotionActivity activityManager;
        readonly IUserDialogs dialogs;


        public OtherExtensionsViewModel(IMotionActivity activityManager, IUserDialogs dialogs)
        {
            this.activityManager = activityManager;
            this.dialogs = dialogs;
        }


        public override void OnAppearing()
        {
            base.OnAppearing();
            Observable
                .Interval(TimeSpan.FromSeconds(5))
                .SubOnMainThread(async _ =>
                {
                    try
                    {
                        var current = await this.activityManager.GetCurrentActivity();
                        this.CurrentText = $"{current.Types} ({current.Confidence})";

                        this.IsCurrentAuto = await this.activityManager.IsCurrentAutomotive();
                        this.IsCurrentStationary = await this.activityManager.IsCurrentStationary();
                    }
                    catch (Exception ex)
                    {
                        await this.dialogs.Alert(ex.ToString());
                    }
                })
                .DisposeWith(this.DeactivateWith);
        }


        [Reactive] public string CurrentText { get; private set; }
        [Reactive] public bool IsCurrentAuto { get; private set; }
        [Reactive] public bool IsCurrentStationary { get; private set; }
    }
}

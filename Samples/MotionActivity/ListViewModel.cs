using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Acr.UserDialogs.Forms;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny.Locations;


namespace Samples.MotionActivity
{
    public class ListViewModel : ViewModel
    {
        readonly IMotionActivity motionActivity;


        public ListViewModel(IUserDialogs dialogs, IMotionActivity motionActivity = null)
        {
            this.motionActivity = motionActivity;

            this.Load = ReactiveCommand.CreateFromTask(async () =>
            {
                if (this.motionActivity == null)
                {
                    await dialogs.Alert("MotionActivity is not supported on this platform");
                    return;
                }

                if (!this.motionActivity.IsSupported)
                {
                    await dialogs.Alert("Motion Activity is not available");
                    return;
                }

                var activities = await motionActivity.QueryByDate(this.Date);
                this.Events = activities
                    .OrderByDescending(x => x.Timestamp)
                    .Select(x => new CommandItem
                    {
                        Text = $"({x.Confidence}) {x.Types}",
                        Detail = $"{x.Timestamp.LocalDateTime}"
                    })
                    .ToList();

                this.EventCount = this.Events.Count;
            });
            this.BindBusyCommand(this.Load);

            this.WhenAnyValue(x => x.Date)
                .DistinctUntilChanged()
                .Select(_ => Unit.Default)
                .InvokeCommand((ICommand)this.Load)
                .DisposeWith(this.DestroyWith);
        }


        public override void OnAppearing()
        {
            base.OnAppearing();
            this.motionActivity?
                .WhenActivityChanged()
                .SubOnMainThread(x => this.CurrentActivity = $"({x.Confidence}) {x.Types}")
                .DisposeWith(this.DeactivateWith);
        }

        public IReactiveCommand Load { get; }
        [Reactive] public DateTime Date { get; set; } = DateTime.Now;
        [Reactive] public int EventCount { get; private set; }
        [Reactive] public string CurrentActivity { get; private set; }
        [Reactive] public IList<CommandItem> Events { get; private set; }
    }
}

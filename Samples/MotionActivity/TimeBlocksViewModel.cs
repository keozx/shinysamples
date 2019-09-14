using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny.Locations;


namespace Samples.MotionActivity
{
    public class TimeBlocksViewModel : ViewModel
    {
        public TimeBlocksViewModel(IMotionActivity activityManager)
        {
            this.Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var data = await activityManager.GetTimeBlocksForRange(this.Date.Date, this.Date.Date.AddDays(1));
                this.List = data
                    .OrderBy(x => x.Start)
                    .Select(x => new CommandItem
                    {
                        Text = $"{x.Start.LocalDateTime.TimeOfDay} - {x.End.LocalDateTime.TimeOfDay}",
                        Detail = x.Type.ToString()
                    })
                    .ToList();
            });
            this.BindBusyCommand(this.Load);
            this.WhenAnyValue(x => x.Date)
                .Skip(1)
                .DistinctUntilChanged()
                .Select(_ => Unit.Default)
                .InvokeCommand((ICommand)this.Load)
                .DisposeWith(this.DestroyWith);
        }


        public override void OnAppearing()
        {
            base.OnAppearing();
            ((ICommand)this.Load).Execute(null);
        }

        public IReactiveCommand Load { get; }
        [Reactive] public DateTime Date { get; set; } = DateTime.Now;
        [Reactive] public IList<CommandItem> List { get; private set; }
    }
}

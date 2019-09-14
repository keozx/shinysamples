using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Acr.UserDialogs.Forms;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using Shiny.BluetoothLE.Central;


namespace Samples.BluetoothLE
{
    public class AdapterViewModel : ViewModel
    {
        readonly ICentralManager centralManager;
        IDisposable scan;


        public AdapterViewModel(ICentralManager central,
                                INavigationService navigator,
                                IUserDialogs dialogs)
        {
            this.centralManager = central;

            this.SelectPeripheral = navigator.NavigateCommand<PeripheralItemViewModel>(
                "Peripheral",
                (x, p) => p.Add("Peripheral", x.Peripheral)
            );

            this.OpenSettings = ReactiveCommand.Create(() =>
            {
                var settings = this.centralManager as ICanOpenAdapterSettings;
                settings.OpenSettings();
            });

            this.ToggleAdapterState = ReactiveCommand.Create(
                () =>
                {
                    var poweredOn = central.Status == AccessState.Available;
                    if (!central.TrySetAdapterState(!poweredOn))
                        dialogs.Alert("Cannot change bluetooth adapter state");
                }
            );

            this.ScanToggle = ReactiveCommand.Create(
                () =>
                {
                    if (this.IsScanning)
                    {
                        this.IsScanning = false;
                        this.scan?.Dispose();
                    }
                    else
                    {
                        this.Peripherals.Clear();

                        this.scan = central
                            .Scan()
                            .Buffer(TimeSpan.FromSeconds(1))
                            .Synchronize()
                            .SubOnMainThread(
                                results =>
                                {
                                    var list = new List<PeripheralItemViewModel>();
                                    foreach (var result in results)
                                    {
                                        var peripheral = this.Peripherals.FirstOrDefault(x => x.Equals(result.Peripheral));
                                        if (peripheral == null)
                                            peripheral = list.FirstOrDefault(x => x.Equals(result.Peripheral));

                                        if (peripheral != null)
                                            peripheral.Update(result);
                                        else
                                        {
                                            peripheral = new PeripheralItemViewModel(result.Peripheral);
                                            peripheral.Update(result);
                                            list.Add(peripheral);
                                        }
                                    }
                                    if (list.Any())
                                        this.Peripherals.AddRange(list);
                                },
                                ex => dialogs.Alert(ex.ToString(), "ERROR")
                            )
                            .DisposeWith(this.DeactivateWith);

                        this.IsScanning = true;
                    }
                }
            );
        }


        public override void OnDisappearing()
        {
            base.OnDisappearing();
            this.IsScanning = false;
        }

        public ICommand ScanToggle { get; }
        public ICommand OpenSettings { get; }
        public ICommand ToggleAdapterState { get; }
        public ICommand SelectPeripheral { get; }
        public ObservableList<PeripheralItemViewModel> Peripherals { get; } = new ObservableList<PeripheralItemViewModel>();
        [Reactive] public bool IsScanning { get; private set; }
    }
}
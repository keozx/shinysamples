using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Acr.UserDialogs.Forms;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Central;


namespace Samples.BluetoothLE
{
    public class PeripheralViewModel : ViewModel
    {
        readonly IUserDialogs dialogs;
        readonly ICentralManager centralManager;
        IPeripheral peripheral;


        public PeripheralViewModel(ICentralManager centralManager, IUserDialogs dialogs)
        {
            this.centralManager = centralManager;
            this.dialogs = dialogs;

            this.SelectCharacteristic = ReactiveCommand.Create<GattCharacteristicViewModel>(x => x.Select());

            this.ConnectionToggle = ReactiveCommand.Create(() =>
            {
                // don't cleanup connection - force user to d/c
                if (this.peripheral.Status == ConnectionState.Disconnected)
                {
                    this.peripheral.Connect();
                }
                else
                {
                    this.peripheral.CancelConnection();
                }
            });

            this.PairToDevice = ReactiveCommand.Create(() =>
            {
                var pair = this.centralManager as ICanPairPeripherals;

                if (pair.PairingStatus == PairingState.Paired)
                {
                    dialogs.Toast("Peripheral is already paired");
                }
                else
                {
                    pair
                        .PairingRequest()
                        .Subscribe(x =>
                        {
                            var txt = x ? "Peripheral Paired Successfully" : "Peripheral Pairing Failed";
                            dialogs.Toast(txt);
                            this.RaisePropertyChanged(nameof(this.PairingText));
                        });
                }
            });

            this.RequestMtu = ReactiveCommand.CreateFromTask(
                async x =>
                {
                    var mtu = this.centralManager as ICanRequestMtu;
                    var result = await dialogs.Prompt("Range 20-512", "MTU Request");
                    if (result.Ok)
                    {

                        var actual = await mtu.RequestMtu(Int32.Parse(result.Value));
                        dialogs.Toast("MTU Changed to " + actual);
                    }
                },
                this.WhenAny(
                    x => x.ConnectText,
                    x => x.GetValue().Equals("Disconnect")
                )
            );
        }


        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            this.peripheral = parameters.GetValue<IPeripheral>("Peripheral");
            this.Name = this.peripheral.Name;
            this.Uuid = this.peripheral.Uuid;
            this.PairingText = this.peripheral.TryGetPairingStatus() == PairingState.Paired ? "Peripheral Paired" : "Pair Peripheral";

            this.peripheral
                .WhenReadRssiContinuously(TimeSpan.FromSeconds(3))
                .SubOnMainThread(x => this.Rssi = x)
                .DisposeWith(this.DeactivateWith);

            this.peripheral
                .WhenStatusChanged()
                .SubOnMainThread(status =>
                {
                    switch (status)
                    {
                        case ConnectionState.Connecting:
                            this.ConnectText = "Cancel Connection";
                            break;

                        case ConnectionState.Connected:
                            this.ConnectText = "Disconnect";
                            break;

                        case ConnectionState.Disconnected:
                            this.ConnectText = "Connect";
                            try
                            {
                                this.GattCharacteristics.Clear();
                            }
                            catch (Exception ex)
                            {
                                // eat this for now
                                Console.WriteLine(ex);
                            }

                            break;
                    }
                })
                .DisposeWith(this.DeactivateWith);

            this.peripheral
                .WhenAnyCharacteristicDiscovered()
                .SubOnMainThread(
                    chs =>
                    {
                        var service = this.GattCharacteristics.FirstOrDefault(x => x.ShortName.Equals(chs.Service.Uuid.ToString()));
                        if (service == null)
                        {
                            service = new Group<GattCharacteristicViewModel>(
                                $"{chs.Service.Description} ({chs.Service.Uuid})",
                                chs.Service.Uuid.ToString()
                            );
                            this.GattCharacteristics.Add(service);
                        }

                        service.Add(new GattCharacteristicViewModel(chs, this.dialogs));
                    },
                    ex => this.dialogs.Alert(ex.ToString())
                )
                .DisposeWith(this.DeactivateWith);
        }


        public ICommand ConnectionToggle { get; }
        public ICommand PairToDevice { get; }
        public ICommand RequestMtu { get; }
        public ICommand SelectCharacteristic { get; }

        [Reactive] public string Name { get; private set; }
        [Reactive] public Guid Uuid { get; private set; }
        [Reactive] public string PairingText { get; private set; }
        public ObservableCollection<Group<GattCharacteristicViewModel>> GattCharacteristics { get; } = new ObservableCollection<Group<GattCharacteristicViewModel>>();

        [Reactive] public string ConnectText { get; private set; } = "Connect";
        [Reactive] public int Rssi { get; private set; }

        // TODO
        //public bool IsMtuVisible => this.peripheral?.IsMtuRequestsAvailable() ?? false;
        //public bool IsPairingVisible => this.centralManager.IsPairingRequestsAvailable();
    }
}

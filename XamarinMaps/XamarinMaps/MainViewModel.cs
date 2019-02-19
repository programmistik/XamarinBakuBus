using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;
using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using static XamarinMaps.Model;

namespace XamarinMaps
{
	public class MainViewModel : INotifyPropertyChanged
	{
       
        private ObservableCollection<Pin> _pinCollection = new ObservableCollection<Pin>();
        public ObservableCollection<Pin> PinCollection { get { return _pinCollection; } set { _pinCollection = value; OnPropertyChanged(); } }

        private Position _myPosition = new Position(40.409264, 49.867092); // Baku
        public Position MyPosition { get { return _myPosition; } set { _myPosition = value; OnPropertyChanged(); } }

        private ObservableCollection<string> busNumbers = new ObservableCollection<string>();
        public ObservableCollection<string> BusNumbers { get { return busNumbers; } set { busNumbers = value; OnPropertyChanged(); } }

        private string selectedBus; 
        public string SelectedBus { get { return selectedBus; } set { selectedBus = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
        
        public List<BusPins> Items { get; private set; }
        public List<string> Numbers { get; private set; }

        public Task<List<BusPins>> RefreshDataAsync()
        {
            return Task.Run(async () =>
           {
               var uri = new Uri($"https://www.bakubus.az/az/ajax/apiNew1");
               var client = new HttpClient();
               client.MaxResponseContentBufferSize = 256000;
               Numbers = new List<string>();

               try
               {
                   var response = await client.GetAsync(uri);
                   if (response.IsSuccessStatusCode)
                   {
                       var content = await response.Content.ReadAsStringAsync();
                       var count = JsonConvert.DeserializeObject<BusList>(content).BUS.Count();

                       var jsObj = JObject.Parse(content);
                       for (int i = 0; i < count; i++)
                       {
                           var Lat = (string)jsObj["BUS"][i]["@attributes"]["LATITUDE"];
                           var Long = (string)jsObj["BUS"][i]["@attributes"]["LONGITUDE"];
                           var Number = (string)jsObj["BUS"][i]["@attributes"]["DISPLAY_ROUTE_CODE"];

                           var enLat = Lat.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                           var enLong = Long.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                           var newBusPin = new BusPins();
                           newBusPin.BusNumber = Number;                           
                           
                           newBusPin.Latitude = double.Parse(enLat);
                           newBusPin.Longitude = double.Parse(enLong);

                           if (SelectedBus == null)
                           {
                               if (Number != "0") // NOT SERVICE
                               {
                                   if (Numbers.Where(n => n.Equals(Number)).Any() == false)
                                       Numbers.Add(Number);

                               }
                           }
                           
                           Items.Add(newBusPin);
                       }
                   }
               }
               catch (Exception ex)
               {
                   Debug.WriteLine(@"				ERROR {0}", ex.Message);
               }
               return Items;
           });
        }
        //----------------

        public MainViewModel()
        {
            
            Items = new List<BusPins>();

            Task.Run(async () =>
            {
            Items = await RefreshDataAsync();

                foreach (var item in Items)
                {
                    PinCollection.Add(new Pin() { Position = new Position(item.Latitude, item.Longitude), Type = PinType.Generic, Label = item.BusNumber });
                }

            Numbers.Sort();
            BusNumbers = new ObservableCollection<string>(Numbers);

            });

            Device.StartTimer(TimeSpan.FromSeconds(5), OnTimerTick);
        }

       
        private bool OnTimerTick()
        {
            Task.Run(async () =>
            {
                Items.Clear();
                Items = await RefreshDataAsync();
                PinCollection.Clear();

                if (SelectedBus != null)
                {
                    var qw = Items.Where(n => n.BusNumber == SelectedBus);
                    foreach (var item in qw)
                    {
                        PinCollection.Add(new Pin() { Position = new Position(item.Latitude, item.Longitude), Type = PinType.Generic, Label = item.BusNumber });
                    }
                }
                else
                {
                    foreach (var item in Items)
                    {
                        PinCollection.Add(new Pin() { Position = new Position(item.Latitude, item.Longitude), Type = PinType.Generic, Label = item.BusNumber });
                    }
                }
            });

            return true;
        }

    }
}

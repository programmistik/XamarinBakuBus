using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinMaps
{
    public class Model
    {
        //-------
        public class Attributes
        {
            public string BUS_ID { get; set; }
            public string LATITUDE { get; set; }
            public string LONGITUDE { get; set; }
            public string DISPLAY_ROUTE_CODE { get; set; }
            public string DRIVER_NAME { get; set; }
            public string ROUTE_NAME { get; set; }
        }
        public class Bus
        {
            public Attributes @attributes { get; set; }
        }

        public class BusList
        {
            public List<Bus> BUS { get; set; }
        }

        public class BusPins
        {
            public string BusNumber { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            //public string Route { get; set; }
            //public string DriverName { get; set; }
            //public string CurrentStop { get; set; }
        }
        //------------
    }
}

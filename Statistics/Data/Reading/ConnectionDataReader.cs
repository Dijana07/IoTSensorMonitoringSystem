using IoT.Shared.Contracts;
using IoT.Shared.Models;
using Statistics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Data.Reading
{
    public class ConnectionDataReader : IDataReader
    {
        private readonly string _serviceUrl = "http://localhost:8733/TelemetryService/";

        public List<SensorTelemetry> ReadData(DateTime from, DateTime to)
        {
            // 1. Kreiraj fabriku kanala za interfejs iz Shared projekta
            var binding = new BasicHttpBinding();
            var address = new EndpointAddress(_serviceUrl);
            var factory = new ChannelFactory<ITelemetryService>(binding, address);

            // 2. Kreiraj kanal (klijenta)
            ITelemetryService client = factory.CreateChannel();

            try
            {
                // 3. Pozovi metodu servisa
                var data = client.GetTelemetryData(from, to);

                // 4. Zatvori kanal nakon upotrebe
                ((ICommunicationObject)client).Close();

                return data;
            }
            catch (Exception ex)
            {
                // Loguj grešku ili baci custom izuzetak ako servis nije dostupan
                ((ICommunicationObject)client).Abort(); // Ako pukne, abortiraj kanal
                throw new Exception("Nije moguće povezivanje sa Komponentom 1: " + ex.Message);
            }
        }
    }
}

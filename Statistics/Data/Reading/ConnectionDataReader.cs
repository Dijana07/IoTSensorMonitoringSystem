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
            var binding = new BasicHttpBinding();
            var address = new EndpointAddress(_serviceUrl);
            var factory = new ChannelFactory<ITelemetryService>(binding, address);

            ITelemetryService client = factory.CreateChannel();

            try
            {
                var data = client.GetTelemetryData(from, to);
                ((ICommunicationObject)client).Close();
                return data;
            }
            catch (Exception ex)
            {
                ((ICommunicationObject)client).Abort();
                throw new Exception("Failed to connect with information system: " + ex.Message);
            }
        }
    }
}

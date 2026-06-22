using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace IoT.Shared.Models
{
    public class Sensor : INotifyPropertyChanged
    {
        public Guid Id { get; set; }

        private string name;
        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        private string type;
        public string Type
        {
            get => type;
            set { type = value; OnPropertyChanged(); }
        }

        private string location;
        public string Location
        {
            get => location;
            set { location = value; OnPropertyChanged(); }
        }

        private double minValue;
        public double MinValue
        {
            get => minValue;
            set { minValue = value; OnPropertyChanged(); }
        }

        private double maxValue;
        public double MaxValue
        {
            get => maxValue;
            set { maxValue = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
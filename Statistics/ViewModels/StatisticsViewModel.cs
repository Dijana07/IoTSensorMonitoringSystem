using Statistics.Adapter;
using Statistics.Base;
using Statistics.Commands;
using Statistics.Data.Reading;
using Statistics.Data.Writing;
using Statistics.Interfaces;
using Statistics.Models;
using Statistics.Processing;
using Statistics.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Input;

namespace Statistics.ViewModels
{
    public enum DataSourceType { File, Network }

    public class StatisticsViewModel : ViewModelBase
    {
        private IStatisticsProcessor processor;
        private IDataProvider dataProvider;
        private Dictionary<string, List<Reading>> data = new Dictionary<string, List<Reading>>();
        
        #region Properties

        private DateTime fromDate;
        public DateTime FromDate
        {
            get => fromDate;
            set
            {
                fromDate = value;
                OnPropertyChanged(nameof(FromDate));
            }
        }

        private DateTime toDate;
        public DateTime ToDate
        {
            get => toDate;
            set
            {
                toDate = value;
                OnPropertyChanged(nameof(ToDate));
            }
        }

        private List<Result> results;
        public List<Result> Results
        {
            get => results;
            set
            {
                results = value;
                OnPropertyChanged(nameof(Results));
            }
        }

        private IStatisticsStrategy selectedStrategy;
        public IStatisticsStrategy SelectedStrategy
        {
            get => selectedStrategy;
            set
            {
                selectedStrategy = value;
                if (processor != null)
                {
                    processor.SetStatisticsStrategy(value);
                    CreateProcessor();
                }
                OnPropertyChanged(nameof(SelectedStrategy));
            }
        }

        private DataSourceType selectedDataSource;
        public DataSourceType SelectedDataSource
        {
            get => selectedDataSource;
            set
            {
                selectedDataSource = value;
                CreateDataProvider();
                OnPropertyChanged(nameof(SelectedDataSource));
            }
        }

        private List<Reading> loadedReadings;

        public List<Reading> LoadedReadings
        {
            get => loadedReadings;
            set
            {
                loadedReadings = value;
                OnPropertyChanged(nameof(LoadedReadings));
            }
        }
        #endregion

        #region Lists
        public List<IStatisticsStrategy> Strategies { get; }

        public List<DataSourceType> DataSources { get; }
        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand ProcessDataCommand { get; }
        public ICommand ExportCsvCommand { get; }

        #endregion

        public StatisticsViewModel()
        {
            dataProvider  = new SensorTelemetryAdapter(new FileDataReader("seedData.json"));

            Strategies = new List<IStatisticsStrategy>
            {
                new AverageStrategy(),
                new SumStrategy(),
                new AlarmCountStrategy()
            };
            SelectedStrategy = Strategies.First();

            Results = new List<Result>();

            FromDate = DateTime.Now.AddDays(-7);
            ToDate = DateTime.Now;

            LoadDataCommand = new RelayCommand(LoadData);
            ProcessDataCommand = new RelayCommand(ProcessData);

            ExportCsvCommand = new RelayCommand(ExportData);

            DataSources = Enum
                .GetValues(typeof(DataSourceType))
                .Cast<DataSourceType>()
                .ToList();

            SelectedDataSource = DataSourceType.File;
        }

        private void LoadData()
        {
            CreateDataProvider();

            data.Clear();

            var loadedData = dataProvider.GetData(FromDate, ToDate);
            if (loadedData != null)
            {
                data.Add(loadedData.Item1, loadedData.Item2);

                LoadedReadings = loadedData.Item2.ToList();
            }
            else
            {
                LoadedReadings = new List<Reading>();
            }
        }

        private void ProcessData()
        {
            if (data.Any())
            {
                if (processor == null)
                {
                    CreateProcessor();
                }
                Results = processor.ProcessData(FromDate, ToDate, data);
            }
            else
            {
                MessageBox.Show("You have to load data first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportData()
        {
            if (data.Any())
            {
                if (processor == null)
                {
                    CreateProcessor();
                }
                var decorator = new CsvExportDecorator(processor, new CsvWriter
                    ($"statistics_{processor.GetStatisticsStrategy()}_{FromDate.ToLongDateString()}_{ToDate.ToLongDateString()}.csv"));
                try
                {
                    Results = decorator.ProcessData(fromDate, ToDate, data);
                    MessageBox.Show("Data exported to csv!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                }
            else
            {
                MessageBox.Show("You have to load data first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateProcessor()
        {
            processor = new StatisticsProcessor(SelectedStrategy);
        }

        private void CreateDataProvider()
        {
            IDataReader reader;

            switch (SelectedDataSource)
            {
                case DataSourceType.Network:
                    reader = new ConnectionDataReader();
                    break;

                default:
                    reader = new FileDataReader("seedData.json");
                    break;
            }

            dataProvider = new SensorTelemetryAdapter(reader);
        }
    }
}
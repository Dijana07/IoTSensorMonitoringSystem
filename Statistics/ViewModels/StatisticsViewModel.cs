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
using System.Windows.Input;

namespace Statistics.ViewModels
{
    public enum DataSourceType { File, Network }

    public class StatisticsViewModel : ViewModelBase
    {
        private IStatisticsProcessor processor;

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
            processor = CreateProcessor();
            processor.LoadData(FromDate, ToDate);
            LoadedReadings = processor
                .GetData()
                .SelectMany(x => x.Value)
                .ToList();
        }

        private void ProcessData()
        {
            if (processor == null)
            {
                processor = CreateProcessor();
            }
            Results = processor.ProcessData(FromDate, ToDate);
        }

        private void ExportData()
        {
            if (processor == null)
            {
                processor = CreateProcessor();
            }
            var decorator = new CsvExportDecorator(processor, new CsvWriter("statistics.csv"));
            decorator.ExportData();
        }

        private IStatisticsProcessor CreateProcessor()
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

            IDataProvider provider =
                new SensorTelemetryAdapter(reader);

            return new StatisticsProcessor(
                SelectedStrategy,
                provider);
        }
    }
}
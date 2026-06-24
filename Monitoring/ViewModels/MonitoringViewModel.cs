using IoT.Shared.Enums;
using IoT.Shared.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Monitoring.Commands;
using Monitoring.Data;
using Monitoring.Interfaces;
using Monitoring.Loggers;
using Monitoring.Mappers;
using Monitoring.Models;
using Monitoring.Services;
using Monitoring.Validations;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;

namespace Monitoring.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly Monitoring.Commands.CommandManager commandManager;
        private readonly LoggerTxt loggerTxt;
        private readonly LoggerXml loggerXml;
        private readonly SensorStateMapper stateMapper;
        private readonly SensorValidator sensorValidator;
        private readonly ITelemetryChartService telemetryChartService;
        private readonly IDataLoader dataLoader;

        private readonly string sensorsXmlPath = "sensors.xml";
        private readonly string telemetryXmlPath = "telemetry.xml";
        private readonly string telemetryHistoryXmlPath = "telemetryHistory.xml";

        // Za obrisane telemetrije koje želimo da vratimo ako korisnik klikne Undo nakon RemoveSensor
        private readonly Dictionary<Guid, SensorTelemetryContext> _telemetryCache = new Dictionary<Guid, SensorTelemetryContext>();

        // Obicne kolekcije senzora i telemetrija koje se prikazuju u UI
        public ObservableCollection<Sensor> Sensors { get; set; }
        public ObservableCollection<SensorTelemetryContext> Telemetries { get; set; }

        // Kolekcije koje se koriste za filtriranje senzora i telemetrija u UI
        private readonly ICollectionView _sensorsView;
        private readonly ICollectionView _telemetriesView;
        #endregion

        #region Properties

        private ObservableCollection<ISeries> series;
        public ObservableCollection<ISeries> Series
        {
            get => series;
            set
            {
                series = value;
                OnPropertyChanged();
            }
        }

        private Sensor selectedSensor;
        public Sensor SelectedSensor
        {
            get => selectedSensor;
            set
            {
                selectedSensor = value;
                OnPropertyChanged(nameof(SelectedSensor));
                PopulateFormFromSelected();
            }
        }

        private SensorTelemetryContext selectedTelemetry;
        public SensorTelemetryContext SelectedTelemetry
        {
            get => selectedTelemetry;
            set
            {
                selectedTelemetry = value;
                OnPropertyChanged(nameof(SelectedTelemetry));
            }
        }

        private string editName = string.Empty;
        public string EditName { get => editName; set { editName = value; OnPropertyChanged(); } }

        private string editType = string.Empty;
        public string EditType { get => editType; set { editType = value; OnPropertyChanged(); } }

        private string editLocation = string.Empty;
        public string EditLocation { get => editLocation; set { editLocation = value; OnPropertyChanged(); } }

        private string editMin = "0";
        public string EditMin { get => editMin; set { editMin = value; OnPropertyChanged(); } }

        private string editMax = "100";
        public string EditMax { get => editMax; set { editMax = value; OnPropertyChanged(); } }

        private string searchSensorText = string.Empty;
        public string SearchSensorText
        {
            get => searchSensorText;
            set
            {
                searchSensorText = value;
                OnPropertyChanged();
                _sensorsView.Refresh();
            }
        }

        private string searchTelemetryText = string.Empty;
        public string SearchTelemetryText
        {
            get => searchTelemetryText;
            set
            {
                searchTelemetryText = value;
                OnPropertyChanged();
                _telemetriesView.Refresh();
            }
        }
        #endregion

        #region Commands
        public System.Windows.Input.ICommand AddSensorCommand { get; }
        public System.Windows.Input.ICommand UpdateSensorCommand { get; }
        public System.Windows.Input.ICommand RemoveSensorCommand { get; }
        public System.Windows.Input.ICommand UndoCommand { get; }
        public System.Windows.Input.ICommand RedoCommand { get; }
        public System.Windows.Input.ICommand NextStateCommand { get; }
        #endregion

        #region Constructor

        public MainViewModel()
        {
            loggerTxt = new LoggerTxt("activities.txt");
            loggerXml = new LoggerXml("");
            var systemSensor = new Sensor { Name = "System" };

            commandManager = new Monitoring.Commands.CommandManager(loggerTxt, systemSensor);
            stateMapper = new SensorStateMapper();
            sensorValidator = new SensorValidator();
            telemetryChartService = new TelemetryChartService();
            dataLoader = new DataLoader(sensorsXmlPath, telemetryXmlPath, stateMapper);

            Sensors = new ObservableCollection<Sensor>();
            Telemetries = new ObservableCollection<SensorTelemetryContext>();

            Series = telemetryChartService.BuildChart(Telemetries);

            // Reakcija na izmene u kolekciji senzora radi automatskog upravljanja telemetrijom
            Sensors.CollectionChanged += Sensors_CollectionChanged;

            // Osluškujemo promene u Telemetries kolekciji kako bismo osveživali grafikon
            Telemetries.CollectionChanged += (s, e) => RefreshChart();

            _sensorsView = CollectionViewSource.GetDefaultView(Sensors);
            _sensorsView.Filter = FilterSensorsPredicate;

            _telemetriesView = CollectionViewSource.GetDefaultView(Telemetries);
            _telemetriesView.Filter = FilterTelemetriesPredicate;

            #region Commands Initialization

            AddSensorCommand = new MyICommand(OnAddSensor);
            UpdateSensorCommand = new MyICommand(OnUpdateSensor);
            RemoveSensorCommand = new MyICommand(OnRemoveSensor);

            UndoCommand = new MyICommand(() =>
            {
                commandManager.Undo();
                loggerTxt.Log(new Sensor { Name = "System" }, "Undo operation executed.");
                _sensorsView.Refresh();
                _telemetriesView.Refresh();

                PopulateFormFromSelected();

                RefreshChart();
                SaveAllToXml();
            });

            RedoCommand = new MyICommand(() =>
            {
                commandManager.Redo();
                loggerTxt.Log(new Sensor { Name = "System" }, "Redo operation executed.");

                _sensorsView.Refresh();
                _telemetriesView.Refresh();

                PopulateFormFromSelected();

                RefreshChart(); 
                SaveAllToXml();
            });

            NextStateCommand = new MyICommand(OnNextState);
            #endregion

            LoadAll();
            RefreshChart();
        }
        #endregion

        #region Chart
        private void RefreshChart()
        {
            Series = telemetryChartService.BuildChart(Telemetries);
        }
        #endregion

        #region Sensor Collection Changed Handler
        private void Sensors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Random rand= new Random();
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (Sensor sensor in e.NewItems)
                {
                    sensor.PropertyChanged += Sensor_PropertyChanged;

                    if (_telemetryCache.TryGetValue(sensor.Id, out var cachedTelemetry))
                    {
                        if (!Telemetries.Any(t => t.Telemetry.SensorId == sensor.Id))
                        {
                            Telemetries.Add(cachedTelemetry);
                        }
                    }
                    else
                    {
                        if (!Telemetries.Any(t => t.Telemetry.SensorId == sensor.Id))
                        {
                            var newTelemetry = new SensorTelemetryContext(new SensorTelemetry
                            {
                                SensorId = sensor.Id,
                                DateTime = DateTime.Now,
                                Value = Math.Round(rand.NextDouble() * rand.Next((int)sensor.MinValue, (int)sensor.MaxValue),2),
                                Status = SensorStatus.Stable
                            }, stateMapper);

                            Telemetries.Add(newTelemetry);
                        }
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (Sensor sensor in e.OldItems)
                {
                    sensor.PropertyChanged -= Sensor_PropertyChanged;

                    var telemetryToRemove = Telemetries.FirstOrDefault(t => t.Telemetry.SensorId == sensor.Id);
                    if (telemetryToRemove != null)
                    {
                        _telemetryCache[sensor.Id] = telemetryToRemove;
                        Telemetries.Remove(telemetryToRemove);
                    }
                }
            }
        }

        private void Sensor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _sensorsView.Refresh();
        }
        #endregion

        #region XML Data Loading and Logging
        private void LoadAll()
        {
            Sensors.CollectionChanged -= Sensors_CollectionChanged;

            Sensors.Clear();
            Telemetries.Clear();

            var data = dataLoader.LoadAll();

            foreach (var sensor in data.Sensors)
            {
                sensor.PropertyChanged += Sensor_PropertyChanged;
                Sensors.Add(sensor);
            }

            foreach (var telemetry in data.Telemetries)
            {
                Telemetries.Add(telemetry);
            }

            Sensors.CollectionChanged += Sensors_CollectionChanged;

        }

        private void SaveAllToXml()
        {
            try
            {
                loggerXml.SaveState(Sensors, Telemetries, sensorsXmlPath, telemetryXmlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to XML: {ex.Message}");
            }
        }
        private void SaveTelemetryHistory(SensorTelemetry telemetry)
        {
            try
            {
                loggerXml.SaveSingleTelemetryHistory(telemetry, telemetryHistoryXmlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving telemetry history: {ex.Message}");
            }
        }
        #endregion

        #region Command Management
        private void OnAddSensor()
        {
            if (!sensorValidator.Validate(EditName, EditType, EditLocation, EditMin, EditMax, out string error, out double min, out double max))
            {
                MessageBox.Show(error, "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newSensor = new Sensor
            {
                Id = Guid.NewGuid(),
                Name = EditName,
                Type = EditType,
                Location = EditLocation,
                MinValue = min,
                MaxValue = max
            };

            var cmd = new AddSensorCommand(Sensors, newSensor);
            commandManager.ExecuteCommand(cmd);

            loggerTxt.Log(newSensor, "Added new instance of the subject and its corresponding initial metric.");

            SaveAllToXml();
            ClearForm();
        }

        private void OnUpdateSensor()
        {
            if (SelectedSensor == null)
                return;

            if (!sensorValidator.Validate(EditName, EditType, EditLocation, EditMin, EditMax, out string error, out double min, out double max))
            {
                MessageBox.Show(error, "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var updatedData = new Sensor { Name = EditName, Type = EditType, Location = EditLocation, MinValue = min, MaxValue = max };

            var cmd = new UpdateSensorCommand(SelectedSensor, updatedData);
            commandManager.ExecuteCommand(cmd);

            loggerTxt.Log(SelectedSensor, "Sensor data updated.");

            _sensorsView.Refresh();
            SaveAllToXml();
        }

        private void OnRemoveSensor()
        {
            if (SelectedSensor == null) return;

            var sensorToRemove = SelectedSensor;

            var cmd = new RemoveSensorCommand(Sensors, sensorToRemove);
            commandManager.ExecuteCommand(cmd);

            loggerTxt.Log(sensorToRemove, "Sensor instance removed.");

            SaveAllToXml();
        }

        private void OnNextState()
        {
            if (SelectedTelemetry == null) return;

            var currentTelemetry = SelectedTelemetry;

            SaveTelemetryHistory(new SensorTelemetry
            {
                SensorId = currentTelemetry.Telemetry.SensorId,
                DateTime = currentTelemetry.Telemetry.DateTime,
                Value = currentTelemetry.Telemetry.Value,
                Status = currentTelemetry.Telemetry.Status
            });

            var cmd = new ChangeStateCommand(currentTelemetry);
            commandManager.ExecuteCommand(cmd);

            var sensor = Sensors.FirstOrDefault(
                s => s.Id == currentTelemetry.Telemetry.SensorId);

            if (sensor != null)
            {
                Random rand = new Random();

                currentTelemetry.Telemetry.Value = Math.Round(
                    sensor.MinValue +
                    rand.NextDouble() * (sensor.MaxValue - sensor.MinValue),
                    2);

                currentTelemetry.Telemetry.DateTime = DateTime.Now;
            }

            loggerTxt.Log( new Sensor { Name = "System" }, $"Sensor {currentTelemetry.Telemetry.SensorId} transitioned to state {currentTelemetry.Telemetry.Status}");

            _telemetriesView.Refresh();
            SelectedTelemetry = currentTelemetry;

            RefreshChart();
            SaveAllToXml();
        }
        #endregion

        #region Filtering Methods
        private bool FilterSensorsPredicate(object obj)
        {
            if (obj is Sensor s)
            {
                if (string.IsNullOrEmpty(SearchSensorText)) return true;

                return s.Name.ToLower().Contains(SearchSensorText.ToLower()) ||
                       s.Type.ToLower().Contains(SearchSensorText.ToLower()) ||
                       s.Location.ToLower().Contains(SearchSensorText.ToLower()) ||
                       s.MinValue.ToString().Contains(SearchSensorText) ||
                       s.MaxValue.ToString().Contains(SearchSensorText) ||
                       s.Id.ToString().ToLower().Contains(SearchSensorText.ToLower());
            }
            return false;
        }

        private bool FilterTelemetriesPredicate(object obj)
        {
            if (obj is SensorTelemetryContext t)
            {
                if (string.IsNullOrEmpty(SearchTelemetryText)) return true;

                return t.Telemetry.SensorId.ToString().ToLower().Contains(SearchTelemetryText.ToLower()) ||
                       t.Telemetry.Value.ToString().Contains(SearchTelemetryText) ||
                       t.Telemetry.DateTime.ToString().ToLower().Contains(SearchTelemetryText.ToLower()) ||
                       t.Telemetry.Status.ToString().ToLower().Contains(SearchTelemetryText.ToLower());
            }
            return false;
        }
        #endregion

        #region Form Management

        private void PopulateFormFromSelected()
        {
            if (SelectedSensor == null) 
                return;
            EditName = SelectedSensor.Name;
            EditType = SelectedSensor.Type;
            EditLocation = SelectedSensor.Location;
            EditMin = SelectedSensor.MinValue.ToString();
            EditMax = SelectedSensor.MaxValue.ToString();
        }

        private void ClearForm()
        {
            EditName = string.Empty; EditType = string.Empty; EditLocation = string.Empty;
            EditMin = "0"; EditMax = "100";
        }
        #endregion
    }
}
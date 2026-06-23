using IoT.Shared.Enums;
using IoT.Shared.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Monitoring.Commands;
using Monitoring.Loggers;
using Monitoring.Mappers;
using Monitoring.Models;
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
        private readonly Monitoring.Commands.CommandManager commandManager;
        private readonly LoggerTxt loggerTxt;
        private readonly LoggerXml loggerXml;
        private readonly SensorStateMapper stateMapper;

        private readonly string sensorsXmlPath = "sensors.xml";
        private readonly string telemetryXmlPath = "telemetry.xml";
        private readonly string telemetryHistoryXmlPath = "telemetryHistory.xml";

        // Za obrisane telemetrije koje želimo da vratimo ako korisnik klikne Undo nakon RemoveSensor
        private readonly Dictionary<Guid, SensorTelemetryContext> _telemetryCache = new Dictionary<Guid, SensorTelemetryContext>();

        public ObservableCollection<Sensor> Sensors { get; set; }
        public ObservableCollection<SensorTelemetryContext> Telemetries { get; set; }

        private readonly ICollectionView _sensorsView;
        private readonly ICollectionView _telemetriesView;

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

        public System.Windows.Input.ICommand AddSensorCommand { get; }
        public System.Windows.Input.ICommand UpdateSensorCommand { get; }
        public System.Windows.Input.ICommand RemoveSensorCommand { get; }
        public System.Windows.Input.ICommand UndoCommand { get; }
        public System.Windows.Input.ICommand RedoCommand { get; }
        public System.Windows.Input.ICommand NextStateCommand { get; }

        public MainViewModel()
        {
            loggerTxt = new LoggerTxt("activities.txt");
            loggerXml = new LoggerXml("");
            var systemSensor = new Sensor { Name = "System" };

            commandManager = new Monitoring.Commands.CommandManager(loggerTxt, systemSensor);
            stateMapper = new SensorStateMapper();

            Sensors = new ObservableCollection<Sensor>();
            Telemetries = new ObservableCollection<SensorTelemetryContext>();

            Series = new ObservableCollection<ISeries>
            {
                new PieSeries<int> { Values = new ObservableCollection<int> { 0 }, Name = "Stable" },
                new PieSeries<int> { Values = new ObservableCollection<int> { 0 }, Name = "Alarm" },
                new PieSeries<int> { Values = new ObservableCollection<int> { 0 }, Name = "Error" },
                new PieSeries<int> { Values = new ObservableCollection<int> { 0 }, Name = "Unactive" }
            };

            // Reakcija na izmene u kolekciji senzora radi automatskog upravljanja telemetrijom
            Sensors.CollectionChanged += Sensors_CollectionChanged;

            // Osluškujemo promene u Telemetries kolekciji (dodavanje/brisanje) kako bismo osveživali grafikon
            Telemetries.CollectionChanged += (s, e) => UpdateChart();

            _sensorsView = CollectionViewSource.GetDefaultView(Sensors);
            _sensorsView.Filter = FilterSensorsPredicate;

            _telemetriesView = CollectionViewSource.GetDefaultView(Telemetries);
            _telemetriesView.Filter = FilterTelemetriesPredicate;

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

                UpdateChart();
                SaveAllToXml();
            });

            RedoCommand = new MyICommand(() =>
            {
                commandManager.Redo();
                loggerTxt.Log(new Sensor { Name = "System" }, "Redo operation executed.");

                _sensorsView.Refresh();
                _telemetriesView.Refresh();

                PopulateFormFromSelected();

                UpdateChart(); 
                SaveAllToXml();
            });

            NextStateCommand = new MyICommand(OnNextState);

            LoadAllFromXml();
            UpdateChart(); 
        }

        private void UpdateChart()
        {

            int stableCount = Telemetries.Count(t => t.Telemetry.Status == SensorStatus.Stable);
            int alarmCount = Telemetries.Count(t => t.Telemetry.Status == SensorStatus.Alarm);
            int errorCount = Telemetries.Count(t => t.Telemetry.Status == SensorStatus.Error);
            int unactiveCount = Telemetries.Count(t => t.Telemetry.Status == SensorStatus.Unactive);

            Series = new ObservableCollection<ISeries>
                {
                    new PieSeries<int>
            {
                Values = new[] { stableCount },
                Name = "Stable",
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new PieSeries<int>
            {
                Values = new[] { alarmCount },
                Name = "Alarm",
                Fill = new SolidColorPaint(SKColors.Orange)
            },
            new PieSeries<int>
            {
                Values = new[] { errorCount },
                Name = "Error",
                Fill = new SolidColorPaint(SKColors.Red)
            },
            new PieSeries<int>
            {
                Values = new[] { unactiveCount },
                Name = "Unactive",
                Fill = new SolidColorPaint(SKColors.Gray)
            }
                };
        }

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

        private void LoadAllFromXml()
        {
            Sensors.CollectionChanged -= Sensors_CollectionChanged;

            Sensors.Clear();
            Telemetries.Clear();

            if (File.Exists(sensorsXmlPath))
            {
                try
                {
                    XDocument doc = XDocument.Load(sensorsXmlPath);
                    foreach (XElement el in doc.Root.Elements("Sensor"))
                    {
                        var sensor = new Sensor
                        {
                            Id = Guid.Parse(el.Element("Id").Value),
                            Name = el.Element("Name").Value,
                            Type = el.Element("Type").Value,
                            Location = el.Element("Location").Value,
                            MinValue = double.Parse(el.Element("MinValue").Value),
                            MaxValue = double.Parse(el.Element("MaxValue").Value)
                        };

                        sensor.PropertyChanged += Sensor_PropertyChanged;
                        Sensors.Add(sensor);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"Error loading sensors: {ex.Message}"); }
            }

            if (File.Exists(telemetryXmlPath))
            {
                try
                {
                    XDocument doc = XDocument.Load(telemetryXmlPath);
                    foreach (XElement el in doc.Root.Elements("Telemetry"))
                    {
                        var raw = new SensorTelemetry
                        {
                            SensorId = Guid.Parse(el.Element("SensorId").Value),
                            DateTime = DateTime.Parse(el.Element("DateTime").Value),
                            Value = double.Parse(el.Element("Value").Value),
                            Status = (SensorStatus)Enum.Parse(typeof(SensorStatus), el.Element("Status").Value)
                        };
                        Telemetries.Add(new SensorTelemetryContext(raw, stateMapper));
                    }
                }
                catch (Exception ex) { MessageBox.Show($"Error loading telemetry: {ex.Message}"); }
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

        private void OnAddSensor()
        {
            if (!ValidateForm(out double min, out double max)) return;

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
            if (SelectedSensor == null || !ValidateForm(out double min, out double max)) return;

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

            loggerTxt.Log(
                new Sensor { Name = "System" },
                $"Sensor {currentTelemetry.Telemetry.SensorId} transitioned to state {currentTelemetry.Telemetry.Status}");

            _telemetriesView.Refresh();
            SelectedTelemetry = currentTelemetry;

            UpdateChart();
            SaveAllToXml();
        }

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

        private bool ValidateForm(out double min, out double max)
        {
            min = 0; max = 0;
            if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditType) || string.IsNullOrWhiteSpace(EditLocation))
            {
                MessageBox.Show("All text fields must be filled!", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!double.TryParse(EditMin, out min) || !double.TryParse(EditMax, out max))
            {
                MessageBox.Show("Min and Max fields must be numbers!", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (min >= max)
            {
                MessageBox.Show("Minimum value must be less than maximum value!", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void PopulateFormFromSelected()
        {
            if (SelectedSensor == null) return;
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
    }
}
using Monitoring.Services;
using Monitoring.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monitoring
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
            try
            {
                var host = new ServiceHost(typeof(Monitoring.Services.TelemetryService));
                host.Open();
                if (host.State == CommunicationState.Opened)
                {
                    System.Diagnostics.Debug.WriteLine("Servis je uspešno pokrenut!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri pokretanju servisa: " + ex.Message);
            }
        }
    }
}

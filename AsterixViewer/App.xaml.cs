using AsterixViewer.AsterixMap;
using System.Configuration;
using System.Data;
using System.Security.Policy;
using System.Windows;

namespace AsterixViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public DataStore DataStore { get; }
        public FiltersViewModel FiltersViewModel { get; }
        public TimeSliderViewModel TimeSliderViewModel { get; }

        public App()
        {
            DataStore = new DataStore();
            FiltersViewModel = new FiltersViewModel();
            TimeSliderViewModel = new TimeSliderViewModel(DataStore);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "AAPTxy8BH1VEsoebNVZXo8HurAxi1wZPqpeOQaRgtZPqt-q2X--psSnBObJdvh6A9vVMi0G8nItdldxZ4AXJ-1Q_zSMfIjZgASUFV5r6gmACyd90nYRFH9BjQKCsyfb0ZbINlxvmNRxmX6VpoLq1PMm6XJdpm0AhfKyxnY3MG_L5HajWsBfBjbRXpQzNLGww-oCy53pUkVLmMW3cyAvcPjIa9qc_5jt8aYjRy9PJluYc1iM.AT1_c0CPcTLg";
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense("runtimelite,1000,rud2068828281,none,2K0RJAY3FL7ZFKF60150");
        }
    }

}

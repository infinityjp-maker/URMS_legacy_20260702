using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using URMS.WinUI.Services;

namespace URMS.WinUI.Pages
{
    public sealed partial class WeatherPage : Page
    {
        private readonly LanguageService _lang = LanguageService.Instance;

        public WeatherPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Apply();
        }

        private void Apply()
        {
            // Static placeholder data — replace with live weather API in future
            TxtMainTemp.Text     = "22\u00B0C";
            TxtMainDesc.Text     = "Partly Cloudy";
            TxtMainLocation.Text = "Tokyo, Minato-ku";
            TxtHumidity.Text     = "68%";
            TxtWind.Text         = "12 km/h";
            TxtFeelsLike.Text    = "20\u00B0C";
            TxtUvIndex.Text      = "4";
            TxtTempHigh.Text     = "26\u00B0C";
            TxtTempLow.Text      = "17\u00B0C";
            WxMainAnim.Condition = "partly_cloudy";
        }

        private void OnNavDashboard(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
                Frame.Navigate(typeof(DashboardPage));
        }
    }
}

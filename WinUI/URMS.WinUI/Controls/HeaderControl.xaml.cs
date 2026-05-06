using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace URMS.WinUI.Controls
{
    public sealed partial class HeaderControl : UserControl
    {
        public event RoutedEventHandler? SettingsClicked;
        public event RoutedEventHandler? CloseClicked;

        public StackPanel SpectrumPanelElement => SpectrumPanel;
        public TextBlock ClockTextElement => TxtClock;
        public TextBlock DateTextElement => TxtDate;
        public RotateTransform RadarRotateElement => RadarRotate;
        public Ellipse RadarBlip0Element => RadarBlip0;
        public Ellipse RadarBlip1Element => RadarBlip1;
        public TextBlock StatusTextElement => TxtStatus;
        public Ellipse StatusDotElement => StatusDot;

        public HeaderControl()
        {
            this.InitializeComponent();
        }

        private void OnSettingsClicked(object sender, RoutedEventArgs e)
            => SettingsClicked?.Invoke(this, e);

        private void OnCloseClicked(object sender, RoutedEventArgs e)
            => CloseClicked?.Invoke(this, e);

        private void OnSettingsPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            SettingsGlow.Opacity = 0.25;
            SettingsButton.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(102, 17, 34, 51));
        }

        private void OnSettingsPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            SettingsGlow.Opacity = 0.0;
            SettingsButton.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(102, 10, 15, 26));
        }

        private void OnClosePointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            CloseGlow.Opacity = 0.25;
            CloseButton.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(102, 17, 34, 51));
        }

        private void OnClosePointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            CloseGlow.Opacity = 0.0;
            CloseButton.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(102, 10, 15, 26));
        }
    }
}

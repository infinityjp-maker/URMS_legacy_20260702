using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace URMS.WinUI.Controls
{
    public sealed partial class HeaderControl : UserControl
    {
        public event RoutedEventHandler? MinimizeClicked;
        public event RoutedEventHandler? MaximizeClicked;
        public event RoutedEventHandler? RestoreClicked;
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
            SetMaximizedState(false);
        }

        public void SetMaximizedState(bool isMaximized)
        {
            MaximizeButton.Visibility = isMaximized ? Visibility.Collapsed : Visibility.Visible;
            RestoreButton.Visibility = isMaximized ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnMinimizeClicked(object sender, RoutedEventArgs e)
            => MinimizeClicked?.Invoke(this, e);

        private void OnMaximizeClicked(object sender, RoutedEventArgs e)
            => MaximizeClicked?.Invoke(this, e);

        private void OnRestoreClicked(object sender, RoutedEventArgs e)
            => RestoreClicked?.Invoke(this, e);

        private void OnCloseClicked(object sender, RoutedEventArgs e)
            => CloseClicked?.Invoke(this, e);
    }
}

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
        }

        private void OnMinimizeClicked(object sender, RoutedEventArgs e)
            => MinimizeClicked?.Invoke(this, e);

        private void OnMaximizeClicked(object sender, RoutedEventArgs e)
            => MaximizeClicked?.Invoke(this, e);

        private void OnRestoreClicked(object sender, RoutedEventArgs e)
            => RestoreClicked?.Invoke(this, e);

        private void OnCloseClicked(object sender, RoutedEventArgs e)
            => CloseClicked?.Invoke(this, e);

        private static void ApplyHover(Button button, Rectangle glow)
        {
            glow.Opacity = 0.25;
            button.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(102, 17, 34, 51));
        }

        private static void ClearHover(Button button, Rectangle glow)
        {
            glow.Opacity = 0.0;
            button.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(102, 10, 15, 26));
        }

        private void OnMinimizePointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            => ApplyHover(MinimizeButton, MinimizeGlow);

        private void OnMinimizePointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            => ClearHover(MinimizeButton, MinimizeGlow);

        private void OnMaximizePointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            => ApplyHover(MaximizeButton, MaximizeGlow);

        private void OnMaximizePointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            => ClearHover(MaximizeButton, MaximizeGlow);

        private void OnRestorePointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            => ApplyHover(RestoreButton, RestoreGlow);

        private void OnRestorePointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            => ClearHover(RestoreButton, RestoreGlow);

        private void OnClosePointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            => ApplyHover(CloseButton, CloseGlow);

        private void OnClosePointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            => ClearHover(CloseButton, CloseGlow);
    }
}

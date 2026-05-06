using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace URMS.WinUI.Controls
{
    public sealed partial class WorkflowCard : UserControl
    {
        public static readonly DependencyProperty CurrentStepProperty =
            DependencyProperty.Register(nameof(CurrentStep), typeof(int), typeof(WorkflowCard),
                new PropertyMetadata(0, OnCurrentStepChanged));

        public int CurrentStep
        {
            get => (int)GetValue(CurrentStepProperty);
            set => SetValue(CurrentStepProperty, value);
        }

        public WorkflowCard()
        {
            this.InitializeComponent();
            ApplyStepState(CurrentStep);
        }

        private static void OnCurrentStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WorkflowCard card)
            {
                card.ApplyStepState((int)e.NewValue);
            }
        }

        private void ApplyStepState(int step)
        {
            var dots = new[] { Dot0, Dot1, Dot2, Dot3, Dot4, Dot5 };
            var texts = new[] { Txt0, Txt1, Txt2, Txt3, Txt4, Txt5 };

            for (int i = 0; i < dots.Length; i++)
            {
                if (i < step)
                {
                    dots[i].Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(102, 0, 224, 255));
                    texts[i].Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(102, 0, 224, 255));
                }
                else if (i == step)
                {
                    dots[i].Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 0, 224, 255));
                    texts[i].Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 0, 224, 255));
                }
                else
                {
                    dots[i].Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(51, 255, 255, 255));
                    texts[i].Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(51, 255, 255, 255));
                }
            }
        }
    }
}

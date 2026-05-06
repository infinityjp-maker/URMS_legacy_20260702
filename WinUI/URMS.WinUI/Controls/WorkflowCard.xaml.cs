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
            var dots = new[] { DotCommit, DotBuild, DotTest, DotCI, DotCD, DotPR };
            var texts = new[] { TxtCommit, TxtBuild, TxtTest, TxtCI, TxtCD, TxtPR };
            var currentColor = Microsoft.UI.ColorHelper.FromArgb(255, 0, 224, 255);
            var passedColor = Microsoft.UI.ColorHelper.FromArgb(102, 0, 224, 255);
            var futureColor = Microsoft.UI.ColorHelper.FromArgb(102, 255, 255, 255);

            for (int i = 0; i < dots.Length; i++)
            {
                if (i < step)
                {
                    dots[i].Fill = new SolidColorBrush(passedColor);
                    texts[i].Foreground = new SolidColorBrush(passedColor);
                }
                else if (i == step)
                {
                    dots[i].Fill = new SolidColorBrush(currentColor);
                    texts[i].Foreground = new SolidColorBrush(currentColor);
                }
                else
                {
                    dots[i].Fill = new SolidColorBrush(futureColor);
                    texts[i].Foreground = new SolidColorBrush(futureColor);
                }
            }
        }
    }
}

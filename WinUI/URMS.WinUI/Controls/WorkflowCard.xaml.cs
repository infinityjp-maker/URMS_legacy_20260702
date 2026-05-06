using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Dispatching;
using System;

namespace URMS.WinUI.Controls
{
    public sealed partial class WorkflowCard : UserControl
    {
        private DispatcherQueueTimer? _pulseTimer;
        private double _pulseT;

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
            StartPulseAnimation();
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
            var dots = new[] { DotCommit, DotBuild, DotTest, DotCI, DotCD, DotDeploy, DotPR };
            var texts = new[] { TxtCommit, TxtBuild, TxtTest, TxtCI, TxtCD, TxtDeploy, TxtPR };
            var icons = new[] { IcoCommit, IcoBuild, IcoTest, IcoCI, IcoCD, IcoDeploy, IcoPR };
            var metas = new[] { TxtMetaCommit, TxtMetaBuild, TxtMetaTest, TxtMetaCI, TxtMetaCD, TxtMetaDeploy, TxtMetaPR };
            string[] times = { "12s", "31s", "54s", "19s", "22s", "17s", "9s" };
            string[] counts = { "5 commits", "1 build", "42 tests", "8 jobs", "3 packs", "2 nodes", "1 PR" };
            var currentColor = Microsoft.UI.ColorHelper.FromArgb(255, 0, 224, 255);
            var passedColor = Microsoft.UI.ColorHelper.FromArgb(255, 0, 224, 255);
            var futureColor = Microsoft.UI.ColorHelper.FromArgb(255, 255, 255, 255);
            var failColor = Microsoft.UI.ColorHelper.FromArgb(255, 255, 106, 106);

            for (int i = 0; i < dots.Length; i++)
            {
                if (i < step)
                {
                    bool failed = (i == 2 && step > 3);
                    dots[i].Fill = new SolidColorBrush(passedColor);
                    texts[i].Foreground = new SolidColorBrush(passedColor);
                    icons[i].Foreground = new SolidColorBrush(passedColor);
                    metas[i].Foreground = new SolidColorBrush(passedColor);
                    dots[i].Opacity = 0.4;
                    texts[i].Opacity = 0.4;
                    icons[i].Opacity = 0.4;
                    metas[i].Opacity = 0.4;
                    dots[i].RenderTransform = null;
                    metas[i].Text = failed ? $"FAIL {times[i]} {counts[i]}" : $"OK {times[i]} {counts[i]}";
                    if (failed)
                    {
                        dots[i].Fill = new SolidColorBrush(failColor);
                        texts[i].Foreground = new SolidColorBrush(failColor);
                        icons[i].Foreground = new SolidColorBrush(failColor);
                        metas[i].Foreground = new SolidColorBrush(failColor);
                    }
                }
                else if (i == step)
                {
                    dots[i].Fill = new SolidColorBrush(currentColor);
                    texts[i].Foreground = new SolidColorBrush(currentColor);
                    icons[i].Foreground = new SolidColorBrush(currentColor);
                    metas[i].Foreground = new SolidColorBrush(currentColor);
                    dots[i].Opacity = 1.0;
                    texts[i].Opacity = 1.0;
                    icons[i].Opacity = 1.0;
                    metas[i].Opacity = 1.0;
                    metas[i].Text = $"RUN {times[i]} {counts[i]}";
                }
                else
                {
                    dots[i].Fill = new SolidColorBrush(futureColor);
                    texts[i].Foreground = new SolidColorBrush(futureColor);
                    icons[i].Foreground = new SolidColorBrush(futureColor);
                    metas[i].Foreground = new SolidColorBrush(futureColor);
                    dots[i].Opacity = 0.2;
                    texts[i].Opacity = 0.2;
                    icons[i].Opacity = 0.2;
                    metas[i].Opacity = 0.2;
                    dots[i].RenderTransform = null;
                    metas[i].Text = $"WAIT 0s {counts[i]}";
                }
            }
        }

        private void StartPulseAnimation()
        {
            _pulseTimer?.Stop();
            _pulseTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _pulseTimer.Interval = TimeSpan.FromMilliseconds(40);
            _pulseTimer.Tick += (_, _) =>
            {
                _pulseT += 0.16;
                var dots = new[] { DotCommit, DotBuild, DotTest, DotCI, DotCD, DotDeploy, DotPR };
                int step = Math.Clamp(CurrentStep, 0, dots.Length - 1);
                for (int i = 0; i < dots.Length; i++)
                {
                    if (i != step) continue;
                    double pulse = 1.0 + 0.15 * (0.5 + 0.5 * Math.Sin(_pulseT));
                    dots[i].RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                    dots[i].RenderTransform = new ScaleTransform { ScaleX = pulse, ScaleY = pulse };
                }

                var sweep = new[] { TxtMetaCommit, TxtMetaBuild, TxtMetaTest, TxtMetaCI, TxtMetaCD, TxtMetaDeploy, TxtMetaPR };
                for (int i = 0; i < sweep.Length; i++)
                {
                    if (i == step)
                    {
                        sweep[i].CharacterSpacing = 70;
                    }
                    else
                    {
                        sweep[i].CharacterSpacing = 0;
                    }
                }
            };
            _pulseTimer.Start();
        }
    }
}

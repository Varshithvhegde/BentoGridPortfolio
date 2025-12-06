using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace Portfolio.Presentation;

public sealed partial class HolographicTimeline : UserControl
{
    public HolographicTimeline()
    {
        this.InitializeComponent();
        
        // Initial State
        SetActive(0);
    }

    private void Job_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && int.TryParse(element.Tag?.ToString(), out int index))
        {
            SetActive(index);
        }
    }

    private void Job_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        // Optional: Reset or keep last selected? Let's keep last selected for now.
    }

    private void SetActive(int index)
    {
        if (index == 0)
        {
            AnimateNode(Job1Dot, true);
            AnimateNode(Job2Dot, false);
            
            AnimateText(Job1Details, 1.0);
            AnimateText(Job2Details, 0.5);
            
            Job2Ring.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 45, 51, 72)); // Inactive Color
            Job2Title.Opacity = 0.7;
            Job2Company.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 184, 184, 184));
            Job2Date.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 184, 184, 184));

            // Animate Line
            var anim = new DoubleAnimation
            {
                To = 40, // Height to first node
                Duration = TimeSpan.FromMilliseconds(300),
                EnableDependentAnimation = true
            };
            Storyboard.SetTarget(anim, ActiveLine);
            Storyboard.SetTargetProperty(anim, "Height");
            var sb = new Storyboard();
            sb.Children.Add(anim);
            sb.Begin();
        }
        else
        {
            AnimateNode(Job1Dot, false);
            AnimateNode(Job2Dot, true);

            AnimateText(Job1Details, 0.5);
            AnimateText(Job2Details, 1.0);

            Job2Ring.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 185, 129)); // Active Color
            Job2Title.Opacity = 1.0;
            Job2Company.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 185, 129));
            Job2Date.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 185, 129));

            // Animate Line
            var anim = new DoubleAnimation
            {
                To = 200, // Height to second node (approx)
                Duration = TimeSpan.FromMilliseconds(300),
                EnableDependentAnimation = true
            };
            Storyboard.SetTarget(anim, ActiveLine);
            Storyboard.SetTargetProperty(anim, "Height");
            var sb = new Storyboard();
            sb.Children.Add(anim);
            sb.Begin();
        }
    }

    private void AnimateNode(Ellipse dot, bool isActive)
    {
        var scale = dot.RenderTransform as ScaleTransform;
        if (scale == null) return;

        var animX = new DoubleAnimation
        {
            To = isActive ? 1.0 : 0.0,
            Duration = TimeSpan.FromMilliseconds(200),
            EnableDependentAnimation = true
        };
        var animY = new DoubleAnimation
        {
            To = isActive ? 1.0 : 0.0,
            Duration = TimeSpan.FromMilliseconds(200),
            EnableDependentAnimation = true
        };
        var animOp = new DoubleAnimation
        {
            To = isActive ? 1.0 : 0.0,
            Duration = TimeSpan.FromMilliseconds(200),
            EnableDependentAnimation = true
        };

        Storyboard.SetTarget(animX, scale);
        Storyboard.SetTargetProperty(animX, "ScaleX");
        Storyboard.SetTarget(animY, scale);
        Storyboard.SetTargetProperty(animY, "ScaleY");
        Storyboard.SetTarget(animOp, dot);
        Storyboard.SetTargetProperty(animOp, "Opacity");

        var sb = new Storyboard();
        sb.Children.Add(animX);
        sb.Children.Add(animY);
        sb.Children.Add(animOp);
        sb.Begin();
    }

    private void AnimateText(UIElement element, double opacity)
    {
        var anim = new DoubleAnimation
        {
            To = opacity,
            Duration = TimeSpan.FromMilliseconds(200),
            EnableDependentAnimation = true
        };
        Storyboard.SetTarget(anim, element);
        Storyboard.SetTargetProperty(anim, "Opacity");
        var sb = new Storyboard();
        sb.Children.Add(anim);
        sb.Begin();
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;

namespace Portfolio.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // List of cards to animate
        var cards = new List<FrameworkElement>
        {
            ProfileCard, ExperienceCard, SkillSetCard,
            PoetiqueCard, FreeShareCard, TouchRenoCard, ContactCard,
            DevToCard, NotePageCard, BlogCard, QuoteCard
        };

        var storyboard = new Storyboard();
        int delay = 0;

        foreach (var card in cards)
        {
            if (card == null) continue;

            // Opacity Animation
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
                BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(opacityAnimation, card);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            storyboard.Children.Add(opacityAnimation);

            // TranslateY Animation (Slide Up)
            if (card.RenderTransform is CompositeTransform transform)
            {
                var translateAnimation = new DoubleAnimation
                {
                    From = 50,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(600)),
                    BeginTime = TimeSpan.FromMilliseconds(delay),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                
                Storyboard.SetTarget(translateAnimation, transform);
                Storyboard.SetTargetProperty(translateAnimation, "TranslateY");
                storyboard.Children.Add(translateAnimation);
            }

            delay += 100; // Stagger by 100ms
        }

        storyboard.Begin();
    }

    private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border card && card.RenderTransform is CompositeTransform transform)
        {
            AnimateScale(transform, 1.02); // Slight scale up
            
            // Optional: Add shadow elevation if using Toolkit or just leave as scale
        }
    }

    private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border card && card.RenderTransform is CompositeTransform transform)
        {
            AnimateScale(transform, 1.0); // Scale back to normal
        }
    }

    private void AnimateScale(CompositeTransform transform, double toScale)
    {
        var storyboard = new Storyboard();

        var scaleX = new DoubleAnimation
        {
            To = toScale,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleX, transform);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");

        var scaleY = new DoubleAnimation
        {
            To = toScale,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleY, transform);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");

        storyboard.Children.Add(scaleX);
        storyboard.Children.Add(scaleY);
        storyboard.Begin();
    }
}

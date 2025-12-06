using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;

namespace Portfolio.Presentation;

public sealed partial class InteractiveProfile : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();
    private double _width;
    private double _height;
    private bool _isHovering;
    private double _scanY = -100;
    private double _scanSpeed = 2;

    public InteractiveProfile()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += InteractiveProfile_Loaded;
        this.Unloaded += InteractiveProfile_Unloaded;
        this.SizeChanged += InteractiveProfile_SizeChanged;
        
        this.PointerEntered += (s, e) => _isHovering = true;
        this.PointerExited += (s, e) => 
        {
            _isHovering = false;
            ResetParallax();
        };
        this.PointerMoved += InteractiveProfile_PointerMoved;
    }

    private void InteractiveProfile_Loaded(object sender, RoutedEventArgs e)
    {
        _timer.Start();
    }

    private void InteractiveProfile_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void InteractiveProfile_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _width = e.NewSize.Width;
        _height = e.NewSize.Height;
        
        // Update Clip
        (this.Content as Grid).Clip = new RectangleGeometry { Rect = new Rect(0, 0, _width, _height) };
    }

    private void InteractiveProfile_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pos = e.GetCurrentPoint(this).Position;
        var centerX = _width / 2;
        var centerY = _height / 2;
        
        // Parallax Effect (Move content opposite to mouse)
        var offsetX = (centerX - pos.X) * 0.05;
        var offsetY = (centerY - pos.Y) * 0.05;
        
        ContentTranslate.X = offsetX;
        ContentTranslate.Y = offsetY;
    }

    private void ResetParallax()
    {
        var animX = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(300) };
        var animY = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(300) };
        
        Storyboard.SetTarget(animX, ContentTranslate);
        Storyboard.SetTargetProperty(animX, "X");
        Storyboard.SetTarget(animY, ContentTranslate);
        Storyboard.SetTargetProperty(animY, "Y");
        
        var sb = new Storyboard();
        sb.Children.Add(animX);
        sb.Children.Add(animY);
        sb.Begin();
    }

    private void Timer_Tick(object sender, object e)
    {
        // Rotate Rings
        RingsRotation.Angle += _isHovering ? 2 : 0.5;

        // Scan Line Animation
        if (_isHovering)
        {
            _scanY += _scanSpeed;
            if (_scanY > 100) _scanY = -100;
            
            if (ScanLine.RenderTransform is TranslateTransform trans)
            {
                trans.Y = _scanY;
            }
            
            // Scan Overlay Opacity based on line position
            ScanOverlay.Opacity = Math.Max(0, 0.2 - Math.Abs(_scanY) * 0.005);
        }
        else
        {
            // Reset scan line
            _scanY = -100;
            if (ScanLine.RenderTransform is TranslateTransform trans)
            {
                trans.Y = -100;
            }
            ScanOverlay.Opacity = 0;
        }

        // Background Particles
        if (_particles.Count < 20)
        {
            SpawnParticle();
        }
        UpdateParticles();
    }

    private void SpawnParticle()
    {
        if (_width == 0 || _height == 0) return;

        var particle = new Particle
        {
            Position = new Vector2((float)(_random.NextDouble() * _width), (float)(_random.NextDouble() * _height)),
            Velocity = new Vector2(0, (float)(-_random.NextDouble() * 0.5 - 0.2)), // Float upwards
            Element = new Ellipse
            {
                Width = _random.NextDouble() * 3 + 1,
                Height = _random.NextDouble() * 3 + 1,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)(_random.Next(50, 150)), 255, 255, 255))
            }
        };

        ParticleCanvas.Children.Add(particle.Element);
        _particles.Add(particle);
    }

    private void UpdateParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Position += p.Velocity;
            
            // Wrap around
            if (p.Position.Y < -10)
            {
                p.Position.Y = (float)_height + 10;
                p.Position.X = (float)(_random.NextDouble() * _width);
            }

            Canvas.SetLeft(p.Element, p.Position.X);
            Canvas.SetTop(p.Element, p.Position.Y);
        }
    }

    private class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public FrameworkElement Element;
    }
}

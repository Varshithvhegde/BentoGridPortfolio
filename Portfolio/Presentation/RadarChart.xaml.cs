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

public sealed partial class RadarChart : UserControl
{
    private const double CenterX = 150;
    private const double CenterY = 150;
    private const double Radius = 100;
    
    private readonly List<(string Label, double Value)> _skills = new()
    {
        ("Frontend", 0.9),
        ("Backend", 0.85),
        ("Mobile", 0.75),
        ("DevOps", 0.6),
        ("Database", 0.7),
        ("Creativity", 0.95)
    };

    private readonly DispatcherTimer _animationTimer;
    private double _animationProgress = 0;
    private bool _isHovering = false;
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();

    public RadarChart()
    {
        this.InitializeComponent();
        
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _animationTimer.Tick += AnimationTimer_Tick;

        this.Loaded += RadarChart_Loaded;
        this.Unloaded += RadarChart_Unloaded;
        this.PointerEntered += RadarChart_PointerEntered;
        this.PointerExited += RadarChart_PointerExited;
        this.PointerMoved += RadarChart_PointerMoved;
    }

    private void RadarChart_Loaded(object sender, RoutedEventArgs e)
    {
        DrawBackground();
        _animationProgress = 0;
        _animationTimer.Start();
        PulseStoryboard.Begin();
    }

    private void RadarChart_Unloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        PulseStoryboard.Stop();
    }

    private void RadarChart_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = true;
    }

    private void RadarChart_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = false;
        // Reset tilt
        var animX = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(300) };
        var animY = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(300) };
        Storyboard.SetTarget(animX, ChartProjection); Storyboard.SetTargetProperty(animX, "RotationX");
        Storyboard.SetTarget(animY, ChartProjection); Storyboard.SetTargetProperty(animY, "RotationY");
        var sb = new Storyboard(); sb.Children.Add(animX); sb.Children.Add(animY); sb.Begin();
        
        InfoBorder.Opacity = 0;
    }

    private void RadarChart_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isHovering) return;

        var point = e.GetCurrentPoint(this).Position;
        var dx = point.X - ActualWidth / 2;
        var dy = point.Y - ActualHeight / 2;

        // Max rotation of 10 degrees
        ChartProjection.RotationY = (dx / (ActualWidth / 2)) * 10;
        ChartProjection.RotationX = -(dy / (ActualHeight / 2)) * 10;
    }

    private void AnimationTimer_Tick(object sender, object e)
    {
        // Entry Animation
        if (_animationProgress < 1)
        {
            _animationProgress += 0.02;
            if (_animationProgress > 1) _animationProgress = 1;
            double easedProgress = CubicEaseOut(_animationProgress);
            UpdateDataPolygon(easedProgress);
        }

        // Scan Line Animation
        if (ScanRotation != null)
        {
            ScanRotation.Angle += 2;
            if (ScanRotation.Angle >= 360) ScanRotation.Angle = 0;
        }

        // Particles
        if (_particles.Count < 15) SpawnParticle();
        UpdateParticles();
    }

    private double CubicEaseOut(double t) => 1 - Math.Pow(1 - t, 3);

    private void DrawBackground()
    {
        GridCanvas.Children.Clear();
        LabelsCanvas.Children.Clear();

        // Draw Hexagon Grid (3 levels)
        for (int i = 1; i <= 3; i++)
        {
            var poly = new Polygon
            {
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(30, 255, 255, 255)),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(5, 255, 255, 255))
            };
            
            double r = Radius * (i / 3.0);
            for (int j = 0; j < 6; j++)
            {
                double angle = j * 60 * Math.PI / 180 - Math.PI / 2;
                poly.Points.Add(new Point(CenterX + r * Math.Cos(angle), CenterY + r * Math.Sin(angle)));
            }
            GridCanvas.Children.Add(poly);
        }

        // Draw Axes and Labels
        for (int i = 0; i < 6; i++)
        {
            double angle = i * 60 * Math.PI / 180 - Math.PI / 2;
            double x = CenterX + Radius * Math.Cos(angle);
            double y = CenterY + Radius * Math.Sin(angle);

            // Axis Line
            var line = new Line
            {
                X1 = CenterX, Y1 = CenterY, X2 = x, Y2 = y,
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(20, 255, 255, 255)),
                StrokeThickness = 1
            };
            GridCanvas.Children.Add(line);

            // Label
            var labelText = new TextBlock
            {
                Text = _skills[i].Label,
                FontSize = 11,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 200, 200)),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };
            
            double labelR = Radius + 25;
            double lx = CenterX + labelR * Math.Cos(angle);
            double ly = CenterY + labelR * Math.Sin(angle);

            Canvas.SetLeft(labelText, lx - 20);
            Canvas.SetTop(labelText, ly - 10);
            LabelsCanvas.Children.Add(labelText);
        }
    }

    private void UpdateDataPolygon(double progress)
    {
        DataPolygon.Points.Clear();
        PointsCanvas.Children.Clear();

        for (int i = 0; i < 6; i++)
        {
            double angle = i * 60 * Math.PI / 180 - Math.PI / 2;
            double val = _skills[i].Value * progress;
            double r = Radius * val;
            
            double x = CenterX + r * Math.Cos(angle);
            double y = CenterY + r * Math.Sin(angle);
            
            DataPolygon.Points.Add(new Point(x, y));

            // Draw Interactive Point
            var dotContainer = new Grid
            {
                Width = 16, Height = 16,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(1, 0, 0, 0)), // Transparent hit target
                Tag = i
            };
            
            var dot = new Ellipse
            {
                Width = 8, Height = 8,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 99, 102, 241)),
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                StrokeThickness = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            dotContainer.Children.Add(dot);
            dotContainer.PointerEntered += Dot_PointerEntered;
            dotContainer.PointerExited += Dot_PointerExited;

            Canvas.SetLeft(dotContainer, x - 8);
            Canvas.SetTop(dotContainer, y - 8);
            PointsCanvas.Children.Add(dotContainer);
        }
    }

    private void Dot_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid && grid.Children[0] is Ellipse dot)
        {
            // Scale Up
            dot.RenderTransform = new ScaleTransform { ScaleX = 1.5, ScaleY = 1.5, CenterX = 4, CenterY = 4 };
            dot.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 185, 129)); // Green

            // Show Info
            if (grid.Tag is int index)
            {
                InfoText.Text = $"{_skills[index].Label}: {_skills[index].Value * 100}%";
                InfoBorder.Opacity = 1;
            }
        }
    }

    private void Dot_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid && grid.Children[0] is Ellipse dot)
        {
            // Reset Scale
            dot.RenderTransform = null;
            dot.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 99, 102, 241)); // Original Blue

            // Hide Info
            InfoBorder.Opacity = 0;
        }
    }

    private void SpawnParticle()
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var dist = _random.NextDouble() * Radius * 1.5;
        
        var particle = new Particle
        {
            Position = new Vector2((float)(CenterX + Math.Cos(angle) * dist), (float)(CenterY + Math.Sin(angle) * dist)),
            Velocity = new Vector2((float)((_random.NextDouble() - 0.5) * 0.5), (float)((_random.NextDouble() - 0.5) * 0.5)),
            Element = new Ellipse
            {
                Width = 2, Height = 2,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)_random.Next(50, 150), 255, 255, 255))
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
            
            // Fade out if too far
            if (Vector2.Distance(p.Position, new Vector2((float)CenterX, (float)CenterY)) > Radius * 1.5)
            {
                ParticleCanvas.Children.Remove(p.Element);
                _particles.RemoveAt(i);
                continue;
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

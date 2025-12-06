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

public sealed partial class InteractiveSkillSet : UserControl
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

    private readonly List<string> _tags = new() { "React", "Kotlin", "Python", "AWS", "Next.js", "C#", "Uno" };
    private readonly List<TagItem> _tagItems = new();

    private readonly DispatcherTimer _timer;
    private double _animationProgress = 0;
    private bool _isHovering = false;
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();
    private double _width;
    private double _height;

    public InteractiveSkillSet()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;

        this.Loaded += InteractiveSkillSet_Loaded;
        this.Unloaded += InteractiveSkillSet_Unloaded;
        this.SizeChanged += InteractiveSkillSet_SizeChanged;
        this.PointerEntered += InteractiveSkillSet_PointerEntered;
        this.PointerExited += InteractiveSkillSet_PointerExited;
        this.PointerMoved += InteractiveSkillSet_PointerMoved;
    }

    private void InteractiveSkillSet_Loaded(object sender, RoutedEventArgs e)
    {
        DrawBackground();
        InitializeTags();
        _animationProgress = 0;
        _timer.Start();
        PulseStoryboard.Begin();
    }

    private void InteractiveSkillSet_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        PulseStoryboard.Stop();
    }

    private void InteractiveSkillSet_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _width = e.NewSize.Width;
        _height = e.NewSize.Height;
        ClipRect.Rect = new Rect(0, 0, _width, _height);
    }

    private void InteractiveSkillSet_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = true;
    }

    private void InteractiveSkillSet_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = false;
        ResetTilt();
        InfoOverlay.Opacity = 0;
    }

    private void InteractiveSkillSet_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isHovering) return;

        var point = e.GetCurrentPoint(this).Position;
        var dx = point.X - _width / 2;
        var dy = point.Y - _height / 2;

        // Tilt Effect
        ChartProjection.RotationY = (dx / (_width / 2)) * 5;
        ChartProjection.RotationX = -(dy / (_height / 2)) * 5;
    }

    private void ResetTilt()
    {
        var animX = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(300) };
        var animY = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(300) };
        Storyboard.SetTarget(animX, ChartProjection); Storyboard.SetTargetProperty(animX, "RotationX");
        Storyboard.SetTarget(animY, ChartProjection); Storyboard.SetTargetProperty(animY, "RotationY");
        var sb = new Storyboard(); sb.Children.Add(animX); sb.Children.Add(animY); sb.Begin();
    }

    private void Timer_Tick(object sender, object e)
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
        if (_particles.Count < 20) SpawnParticle();
        UpdateParticles();

        // Floating Tags
        UpdateTags();
    }

    private double CubicEaseOut(double t) => 1 - Math.Pow(1 - t, 3);

    private void DrawBackground()
    {
        GridCanvas.Children.Clear();
        LabelsCanvas.Children.Clear();

        // Draw Hexagon Grid
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

            var line = new Line
            {
                X1 = CenterX, Y1 = CenterY, X2 = x, Y2 = y,
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(20, 255, 255, 255)),
                StrokeThickness = 1
            };
            GridCanvas.Children.Add(line);

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

            var dotContainer = new Grid
            {
                Width = 20, Height = 20,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(1, 0, 0, 0)),
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

            Canvas.SetLeft(dotContainer, x - 10);
            Canvas.SetTop(dotContainer, y - 10);
            PointsCanvas.Children.Add(dotContainer);
        }
    }

    private void Dot_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid && grid.Children[0] is Ellipse dot)
        {
            dot.RenderTransform = new ScaleTransform { ScaleX = 1.5, ScaleY = 1.5, CenterX = 4, CenterY = 4 };
            dot.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 185, 129));

            if (grid.Tag is int index)
            {
                InfoLabel.Text = _skills[index].Label;
                InfoValue.Text = $"{_skills[index].Value * 100}%";
                InfoOverlay.Opacity = 1;
            }
        }
    }

    private void Dot_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid && grid.Children[0] is Ellipse dot)
        {
            dot.RenderTransform = null;
            dot.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 99, 102, 241));
            InfoOverlay.Opacity = 0;
        }
    }

    private void InitializeTags()
    {
        TagsCanvas.Children.Clear();
        _tagItems.Clear();

        foreach (var tag in _tags)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 99, 102, 241)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 4, 10, 4),
                Child = new TextBlock { Text = tag, FontSize = 10, Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 255, 255, 255)) }
            };

            var item = new TagItem
            {
                Element = border,
                Angle = _random.NextDouble() * Math.PI * 2,
                Speed = 0.005 + _random.NextDouble() * 0.01,
                Radius = Radius + 40 + _random.NextDouble() * 30,
                YOffset = (_random.NextDouble() - 0.5) * 50
            };

            TagsCanvas.Children.Add(border);
            _tagItems.Add(item);
        }
    }

    private void UpdateTags()
    {
        foreach (var item in _tagItems)
        {
            item.Angle += item.Speed;
            
            double x = CenterX + item.Radius * Math.Cos(item.Angle);
            // Elliptical orbit
            double z = item.Radius * Math.Sin(item.Angle) * 0.3; // Depth
            double y = CenterY + z + item.YOffset;

            // Simple depth scaling
            double scale = 0.8 + (z / 50) * 0.2;
            item.Element.RenderTransform = new ScaleTransform { ScaleX = scale, ScaleY = scale };
            item.Element.Opacity = 0.5 + (z / 50) * 0.5;

            // Center based on element size
            double width = item.Element.ActualWidth > 0 ? item.Element.ActualWidth : 50;
            double height = item.Element.ActualHeight > 0 ? item.Element.ActualHeight : 24;

            Canvas.SetLeft(item.Element, x - width / 2);
            Canvas.SetTop(item.Element, y - height / 2);
            Canvas.SetZIndex(item.Element, (int)z);
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
            
            if (Vector2.Distance(p.Position, new Vector2((float)CenterX, (float)CenterY)) > Radius * 2)
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

    private class TagItem
    {
        public FrameworkElement Element;
        public double Angle;
        public double Speed;
        public double Radius;
        public double YOffset;
    }
}

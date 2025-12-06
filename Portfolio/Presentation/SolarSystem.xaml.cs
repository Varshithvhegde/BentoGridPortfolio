using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;

namespace Portfolio.Presentation;

public sealed partial class SolarSystem : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly List<Planet> _planets = new();
    private float _speedMultiplier = 1.0f;
    private bool _isWarping = false;
    private double _width;
    private double _height;

    public SolarSystem()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += SolarSystem_Loaded;
        this.Unloaded += SolarSystem_Unloaded;
        this.SizeChanged += SolarSystem_SizeChanged;
    }

    private void SolarSystem_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeSystem();
        _timer.Start();
    }

    private void SolarSystem_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void SolarSystem_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _width = e.NewSize.Width;
        _height = e.NewSize.Height;
        InitializeSystem();
    }

    private void InitializeSystem()
    {
        _planets.Clear();
        SpaceCanvas.Children.Clear();

        if (_width == 0 || _height == 0) return;

        var center = new Vector2((float)_width / 2, (float)_height / 2);

        // Create Planets (Articles/Tags)
        CreatePlanet("React", 60, 0.02f, "#61DAFB", center);
        CreatePlanet("Performance", 90, 0.015f, "#F59E0B", center);
        CreatePlanet("Server", 120, 0.01f, "#10B981", center);
        CreatePlanet("Components", 150, 0.008f, "#EC4899", center);
    }

    private void CreatePlanet(string name, float orbitRadius, float speed, string colorHex, Vector2 center)
    {
        var planet = new Planet
        {
            Angle = (float)(new Random().NextDouble() * Math.PI * 2),
            OrbitRadius = orbitRadius,
            Speed = speed,
            Center = center
        };

        // Orbit Path (Visual)
        var orbit = new Ellipse
        {
            Width = orbitRadius * 2,
            Height = orbitRadius * 2,
            Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(30, 255, 255, 255)),
            StrokeThickness = 1,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(orbit, center.X - orbitRadius);
        Canvas.SetTop(orbit, center.Y - orbitRadius);
        SpaceCanvas.Children.Add(orbit);

        // Planet Body
        var body = new Grid
        {
            Width = 20,
            Height = 20,
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(GetColor(colorHex))
        };
        
        // Label
        var tooltip = new ToolTip { Content = name };
        ToolTipService.SetToolTip(body, tooltip);

        planet.Element = body;
        SpaceCanvas.Children.Add(body);
        _planets.Add(planet);
    }

    private Windows.UI.Color GetColor(string hex)
    {
        return Windows.UI.Color.FromArgb(
            255,
            Convert.ToByte(hex.Substring(1, 2), 16),
            Convert.ToByte(hex.Substring(3, 2), 16),
            Convert.ToByte(hex.Substring(5, 2), 16)
        );
    }

    private void Timer_Tick(object sender, object e)
    {
        if (_isWarping)
        {
            _speedMultiplier = Math.Min(_speedMultiplier + 0.5f, 10.0f);
        }
        else
        {
            _speedMultiplier = Math.Max(_speedMultiplier - 0.5f, 1.0f);
        }

        foreach (var planet in _planets)
        {
            planet.Angle += planet.Speed * _speedMultiplier;
            
            var x = planet.Center.X + Math.Cos(planet.Angle) * planet.OrbitRadius;
            var y = planet.Center.Y + Math.Sin(planet.Angle) * planet.OrbitRadius;

            Canvas.SetLeft(planet.Element, x - 10); // Center the 20x20 body
            Canvas.SetTop(planet.Element, y - 10);
        }
    }

    private void OnCanvasPressed(object sender, PointerRoutedEventArgs e)
    {
        _isWarping = !_isWarping;
    }

    private class Planet
    {
        public float Angle;
        public float OrbitRadius;
        public float Speed;
        public Vector2 Center;
        public FrameworkElement Element;
    }
}

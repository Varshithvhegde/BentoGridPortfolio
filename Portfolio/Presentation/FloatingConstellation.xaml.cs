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

public sealed partial class FloatingConstellation : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly List<Node> _nodes = new();
    private readonly Random _random = new();
    private Vector2 _mousePos;
    private bool _isHovering;
    private double _width;
    private double _height;

    public FloatingConstellation()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += FloatingConstellation_Loaded;
        this.Unloaded += FloatingConstellation_Unloaded;
        this.SizeChanged += FloatingConstellation_SizeChanged;
        this.PointerMoved += FloatingConstellation_PointerMoved;
        this.PointerEntered += (s, e) => _isHovering = true;
        this.PointerExited += (s, e) => _isHovering = false;
    }

    private void FloatingConstellation_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeNodes();
        _timer.Start();
    }

    private void FloatingConstellation_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void FloatingConstellation_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _width = e.NewSize.Width;
        _height = e.NewSize.Height;
        
        if (_nodes.Count == 0 && _width > 0)
        {
            InitializeNodes();
        }
    }

    private void FloatingConstellation_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pos = e.GetCurrentPoint(this).Position;
        _mousePos = new Vector2((float)pos.X, (float)pos.Y);
    }

    private void InitializeNodes()
    {
        _nodes.Clear();
        NodesCanvas.Children.Clear();
        LinesCanvas.Children.Clear();

        if (_width == 0 || _height == 0) return;

        // Create Nodes
        CreateNode("POETRY", "\uE006", "#EC4899"); // Pink
        CreateNode("GITHUB", "\uECA7", "#FFFFFF"); // White
        CreateNode("LINK", "\uE71B", "#3B82F6");   // Blue
        CreateNode("EMAIL", "\uE715", "#10B981");  // Green
    }

    private void CreateNode(string label, string glyph, string colorHex)
    {
        var node = new Node
        {
            Position = new Vector2((float)(_random.NextDouble() * _width), (float)(_random.NextDouble() * _height)),
            Velocity = new Vector2((float)(_random.NextDouble() - 0.5), (float)(_random.NextDouble() - 0.5)) * 2,
            Radius = 30
        };

        // Visual Element
        var grid = new Grid 
        { 
            Width = 60, 
            Height = 60,
            CornerRadius = new CornerRadius(30),
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(30, 255, 255, 255)), // Semi-transparent bg
            BorderBrush = new SolidColorBrush(GetColor(colorHex)),
            BorderThickness = new Thickness(1)
        };

        var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        
        var icon = new FontIcon
        {
            Glyph = glyph,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 20,
            Foreground = new SolidColorBrush(GetColor(colorHex))
        };

        var text = new TextBlock
        {
            Text = label,
            FontSize = 8,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        };

        stack.Children.Add(icon);
        stack.Children.Add(text);
        grid.Children.Add(stack);

        node.Element = grid;
        NodesCanvas.Children.Add(grid);
        _nodes.Add(node);
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
        if (_width == 0) return;

        LinesCanvas.Children.Clear();

        // Update Physics
        for (int i = 0; i < _nodes.Count; i++)
        {
            var node = _nodes[i];

            // Mouse Interaction (Repulsion)
            if (_isHovering)
            {
                var dir = node.Position - _mousePos;
                var dist = dir.Length();
                if (dist < 150 && dist > 0)
                {
                    var force = Vector2.Normalize(dir) * (150 - dist) * 0.05f;
                    node.Velocity += force;
                }
            }

            // Wall Bounce
            if (node.Position.X < node.Radius || node.Position.X > _width - node.Radius) node.Velocity.X *= -1;
            if (node.Position.Y < node.Radius || node.Position.Y > _height - node.Radius) node.Velocity.Y *= -1;

            // Keep inside
            node.Position.X = Math.Clamp(node.Position.X, node.Radius, (float)_width - node.Radius);
            node.Position.Y = Math.Clamp(node.Position.Y, node.Radius, (float)_height - node.Radius);

            // Friction/Damping
            node.Velocity *= 0.98f;
            
            // Minimum movement (Drift)
            if (node.Velocity.Length() < 0.5f)
            {
                node.Velocity += new Vector2((float)(_random.NextDouble() - 0.5f), (float)(_random.NextDouble() - 0.5f)) * 0.1f;
            }

            node.Position += node.Velocity;

            // Update Visual Position
            Canvas.SetLeft(node.Element, node.Position.X - node.Radius);
            Canvas.SetTop(node.Element, node.Position.Y - node.Radius);

            // Draw Lines to neighbors
            for (int j = i + 1; j < _nodes.Count; j++)
            {
                var other = _nodes[j];
                var dist = Vector2.Distance(node.Position, other.Position);
                
                if (dist < 200)
                {
                    var line = new Line
                    {
                        X1 = node.Position.X,
                        Y1 = node.Position.Y,
                        X2 = other.Position.X,
                        Y2 = other.Position.Y,
                        Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)(255 * (1 - dist / 200)), 100, 100, 100)),
                        StrokeThickness = 1
                    };
                    LinesCanvas.Children.Add(line);
                }
            }
        }
    }

    private class Node
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Radius;
        public FrameworkElement Element;
    }
}

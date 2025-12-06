using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace Portfolio.Presentation;

public sealed partial class BugBlaster : UserControl
{
    private readonly DispatcherTimer _gameLoop;
    private readonly Random _random = new();
    private readonly List<Bug> _bugs = new();
    private int _score = 0;
    private double _width;
    private double _height;
    private int _spawnTimer = 0;

    public BugBlaster()
    {
        this.InitializeComponent();
        
        _gameLoop = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _gameLoop.Tick += GameLoop_Tick;
        
        this.Loaded += BugBlaster_Loaded;
        this.Unloaded += BugBlaster_Unloaded;
        this.SizeChanged += BugBlaster_SizeChanged;
    }

    private void BugBlaster_Loaded(object sender, RoutedEventArgs e)
    {
        _gameLoop.Start();
    }

    private void BugBlaster_Unloaded(object sender, RoutedEventArgs e)
    {
        _gameLoop.Stop();
    }

    private void BugBlaster_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _width = e.NewSize.Width;
        _height = e.NewSize.Height;
    }

    private void GameLoop_Tick(object sender, object e)
    {
        if (_width == 0 || _height == 0) return;

        // Spawn Bugs
        _spawnTimer++;
        if (_spawnTimer > 60) // Approx every 1 second
        {
            SpawnBug();
            _spawnTimer = 0;
        }

        // Update Bugs
        for (int i = _bugs.Count - 1; i >= 0; i--)
        {
            var bug = _bugs[i];
            
            // Move
            bug.X += bug.VX;
            bug.Y += bug.VY;
            
            // Bounce
            if (bug.X < 0 || bug.X > _width - 20) bug.VX = -bug.VX;
            if (bug.Y < 0 || bug.Y > _height - 20) bug.VY = -bug.VY;
            
            // Update Visual
            Canvas.SetLeft(bug.Element, bug.X);
            Canvas.SetTop(bug.Element, bug.Y);
            
            // Rotate
            var transform = bug.Element.RenderTransform as RotateTransform;
            if (transform != null)
            {
                transform.Angle += 2;
            }
        }
    }

    private void SpawnBug()
    {
        if (_bugs.Count >= 10) return; // Max bugs

        var bug = new Bug
        {
            X = _random.NextDouble() * (_width - 20),
            Y = _random.NextDouble() * (_height - 20),
            VX = (_random.NextDouble() - 0.5) * 2,
            VY = (_random.NextDouble() - 0.5) * 2
        };

        // Visual Representation (Red "Bug" Icon)
        var grid = new Grid { Width = 20, Height = 20 };
        
        var bugIcon = new FontIcon
        {
            Glyph = "\uEBE8", // Bug icon
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 18,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 239, 68, 68)) // Red-500
        };

        grid.Children.Add(bugIcon);
        
        grid.RenderTransformOrigin = new Point(0.5, 0.5);
        grid.RenderTransform = new RotateTransform();

        bug.Element = grid;
        
        GameCanvas.Children.Add(grid);
        _bugs.Add(bug);
    }

    private void OnGameCanvasPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(InteractionLayer).Position;
        
        // Check hits
        for (int i = _bugs.Count - 1; i >= 0; i--)
        {
            var bug = _bugs[i];
            double dist = Math.Sqrt(Math.Pow(point.X - (bug.X + 10), 2) + Math.Pow(point.Y - (bug.Y + 10), 2));
            
            if (dist < 25) // Hit radius
            {
                // Squash!
                SquashBug(i);
                break; // One per click
            }
        }
    }

    private void SquashBug(int index)
    {
        var bug = _bugs[index];
        GameCanvas.Children.Remove(bug.Element);
        _bugs.RemoveAt(index);
        
        _score++;
        ScoreText.Text = $"BUGS SQUASHED: {_score}";
        ScoreText.Opacity = 1.0;
        
        // Add particle effect? (Maybe later)
    }

    private class Bug
    {
        public double X, Y, VX, VY;
        public FrameworkElement Element;
    }
}

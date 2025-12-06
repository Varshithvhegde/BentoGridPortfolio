using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;

namespace Portfolio.Presentation;

public sealed partial class MorphingBlob : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly Microsoft.UI.Xaml.Shapes.Path _blobPath;
    private readonly Random _random = new();
    
    // Blob Physics
    private const int PointCount = 8;
    private readonly List<Vector2> _basePoints = new();
    private readonly List<Vector2> _offsets = new();
    private readonly List<Vector2> _velocities = new();
    private float _time = 0;
    
    public MorphingBlob()
    {
        this.InitializeComponent();
        
        _blobPath = new Microsoft.UI.Xaml.Shapes.Path
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Windows.UI.Color.FromArgb(100, 168, 85, 247), Offset = 0 }, // Purple
                    new GradientStop { Color = Windows.UI.Color.FromArgb(100, 236, 72, 153), Offset = 1 }  // Pink
                }
            },
            Stretch = Stretch.None
        };
        BlobCanvas.Children.Add(_blobPath);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += MorphingBlob_Loaded;
        this.Unloaded += MorphingBlob_Unloaded;
        this.SizeChanged += MorphingBlob_SizeChanged;
    }

    private void MorphingBlob_Loaded(object sender, RoutedEventArgs e)
    {
        InitializePoints();
        _timer.Start();
    }

    private void MorphingBlob_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void MorphingBlob_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            InitializePoints();
        }
    }

    private void InitializePoints()
    {
        if (ActualWidth == 0 || ActualHeight == 0) return;

        _basePoints.Clear();
        _offsets.Clear();
        _velocities.Clear();

        float centerX = (float)ActualWidth / 2;
        float centerY = (float)ActualHeight / 2;
        float radius = Math.Min(centerX, centerY) * 0.8f;

        for (int i = 0; i < PointCount; i++)
        {
            float angle = (float)(i * 2 * Math.PI / PointCount);
            _basePoints.Add(new Vector2(
                centerX + (float)Math.Cos(angle) * radius,
                centerY + (float)Math.Sin(angle) * radius
            ));
            
            _offsets.Add(Vector2.Zero);
            _velocities.Add(new Vector2(
                (float)(_random.NextDouble() - 0.5) * 2,
                (float)(_random.NextDouble() - 0.5) * 2
            ));
        }
    }

    private void Timer_Tick(object sender, object e)
    {
        _time += 0.05f;

        // Update Points
        for (int i = 0; i < PointCount; i++)
        {
            // Simple harmonic motion + noise
            var vel = _velocities[i];
            var off = _offsets[i];
            
            off += vel;
            
            // Boundary check / Elasticity
            if (off.Length() > 30)
            {
                vel = -vel;
            }
            
            // Randomly perturb velocity
            if (_random.NextDouble() < 0.05)
            {
                vel += new Vector2(
                    (float)(_random.NextDouble() - 0.5) * 0.5f,
                    (float)(_random.NextDouble() - 0.5) * 0.5f
                );
                
                // Dampen
                if (vel.Length() > 2) vel = Vector2.Normalize(vel) * 2;
            }

            _offsets[i] = off;
            _velocities[i] = vel;
        }

        DrawBlob();
    }

    private void DrawBlob()
    {
        if (_basePoints.Count == 0) return;

        var geometry = new PathGeometry();
        var figure = new PathFigure { IsClosed = true };
        
        // Calculate current points
        var currentPoints = new List<Point>();
        for (int i = 0; i < PointCount; i++)
        {
            var p = _basePoints[i] + _offsets[i];
            currentPoints.Add(new Point(p.X, p.Y));
        }

        figure.StartPoint = currentPoints[0];

        // Create smooth bezier curves between points
        for (int i = 0; i < PointCount; i++)
        {
            var p0 = currentPoints[i];
            var p1 = currentPoints[(i + 1) % PointCount];
            
            // Control points (simple smoothing)
            // A robust way is to use Catmull-Rom to Bezier conversion or similar
            // For a blob, we can just use midpoints or simple heuristics
            
            // Let's use a simplified approach: Line segments for now to ensure it works, 
            // or QuadraticBezier to midpoint.
            
            // Better: Catmull-Rom splines converted to Bezier for smoothness.
            // Simplified for this demo:
            
            var mid = new Point((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
            
            // Actually, let's just do straight lines for "low poly" look or proper curves?
            // "Morphing Blob" implies curves.
            
            // Let's try PolyBezierSegment if we had control points.
            // Let's use QuadraticBezierSegment to the midpoint of the next segment?
            
            // Standard approach for smooth loop:
            // Point i is the "knot". Control points are calculated based on neighbors.
            
            var prev = currentPoints[(i - 1 + PointCount) % PointCount];
            var next = currentPoints[(i + 1) % PointCount];
            
            // Tangent at p0
            var tangent = new Vector2((float)(next.X - prev.X), (float)(next.Y - prev.Y));
            tangent = Vector2.Normalize(tangent) * 20; // Tension
            
            var cp1 = new Point(p0.X + tangent.X, p0.Y + tangent.Y);
            var cp2 = new Point(p1.X - tangent.X, p1.Y - tangent.Y); // Approximate
            
            var segment = new BezierSegment
            {
                Point1 = cp1,
                Point2 = cp2,
                Point3 = p1
            };
            figure.Segments.Add(segment);
        }

        geometry.Figures.Add(figure);
        _blobPath.Data = geometry;
    }
}

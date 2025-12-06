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

public sealed partial class WireframeCube : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly List<Vector3> _vertices = new();
    private readonly List<(int, int)> _edges = new();
    private readonly List<Line> _lines = new();
    
    private float _rotationX = 0;
    private float _rotationY = 0;
    private float _targetRotationX = 0;
    private float _targetRotationY = 0;
    
    private bool _isHovering = false;
    private Point _lastMousePos;

    public WireframeCube()
    {
        this.InitializeComponent();
        
        InitializeCube();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += WireframeCube_Loaded;
        this.Unloaded += WireframeCube_Unloaded;
        this.PointerEntered += WireframeCube_PointerEntered;
        this.PointerExited += WireframeCube_PointerExited;
        this.PointerMoved += WireframeCube_PointerMoved;
    }

    private void InitializeCube()
    {
        // Define 8 vertices of a cube
        _vertices.Add(new Vector3(-1, -1, -1));
        _vertices.Add(new Vector3(1, -1, -1));
        _vertices.Add(new Vector3(1, 1, -1));
        _vertices.Add(new Vector3(-1, 1, -1));
        _vertices.Add(new Vector3(-1, -1, 1));
        _vertices.Add(new Vector3(1, -1, 1));
        _vertices.Add(new Vector3(1, 1, 1));
        _vertices.Add(new Vector3(-1, 1, 1));

        // Define 12 edges
        _edges.Add((0, 1)); _edges.Add((1, 2)); _edges.Add((2, 3)); _edges.Add((3, 0)); // Front face
        _edges.Add((4, 5)); _edges.Add((5, 6)); _edges.Add((6, 7)); _edges.Add((7, 4)); // Back face
        _edges.Add((0, 4)); _edges.Add((1, 5)); _edges.Add((2, 6)); _edges.Add((3, 7)); // Connecting lines

        // Create Line elements
        foreach (var edge in _edges)
        {
            var line = new Line
            {
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 185, 129)), // #10B981 (Emerald)
                StrokeThickness = 2,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round
            };
            _lines.Add(line);
            CubeCanvas.Children.Add(line);
        }
    }

    private void WireframeCube_Loaded(object sender, RoutedEventArgs e)
    {
        _timer.Start();
    }

    private void WireframeCube_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void WireframeCube_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = true;
        _lastMousePos = e.GetCurrentPoint(this).Position;
    }

    private void WireframeCube_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = false;
    }

    private void WireframeCube_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isHovering)
        {
            var currentPos = e.GetCurrentPoint(this).Position;
            var deltaX = currentPos.X - _lastMousePos.X;
            var deltaY = currentPos.Y - _lastMousePos.Y;
            
            _targetRotationY += (float)deltaX * 0.01f;
            _targetRotationX += (float)deltaY * 0.01f;
            
            _lastMousePos = currentPos;
        }
    }

    private void Timer_Tick(object sender, object e)
    {
        // Auto rotate if not hovering
        if (!_isHovering)
        {
            _targetRotationY += 0.02f;
            _targetRotationX += 0.01f;
        }

        // Smooth interpolation
        _rotationX += (_targetRotationX - _rotationX) * 0.1f;
        _rotationY += (_targetRotationY - _rotationY) * 0.1f;

        DrawCube();
    }

    private void DrawCube()
    {
        float width = (float)ActualWidth;
        float height = (float)ActualHeight;
        
        if (width == 0 || height == 0) return;

        var center = new Vector2(width / 2, height / 2.5f);
        float scale = Math.Min(width, height) * 0.25f;

        // Create rotation matrices
        var rotationMatrix = Matrix4x4.CreateRotationX(_rotationX) * Matrix4x4.CreateRotationY(_rotationY);

        var projectedPoints = new Vector2[_vertices.Count];

        for (int i = 0; i < _vertices.Count; i++)
        {
            var vertex = _vertices[i];
            
            // Rotate
            var rotated = Vector3.Transform(vertex, rotationMatrix);
            
            // Project (Simple orthographic + perspective-ish)
            float distance = 4;
            float z = 1 / (distance - rotated.Z);
            
            projectedPoints[i] = new Vector2(rotated.X * z * scale + center.X, rotated.Y * z * scale + center.Y);
        }

        // Update Lines
        for (int i = 0; i < _edges.Count; i++)
        {
            var (startIdx, endIdx) = _edges[i];
            var start = projectedPoints[startIdx];
            var end = projectedPoints[endIdx];

            var line = _lines[i];
            line.X1 = start.X;
            line.Y1 = start.Y;
            line.X2 = end.X;
            line.Y2 = end.Y;
            
            // Depth cueing (optional): adjust opacity based on Z?
            // For now, solid lines are fine for wireframe look.
        }
    }
}

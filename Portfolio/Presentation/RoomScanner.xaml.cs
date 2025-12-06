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

public sealed partial class RoomScanner : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly List<Edge> _edges = new();
    private readonly List<Line> _lines = new();
    
    // Rotation State
    private float _rotationX = -0.5f; 
    private float _rotationY = 0.5f;
    private float _targetRotationX = -0.5f;
    private float _targetRotationY = 0.5f;
    
    // Physics State
    private float _velocityX = 0;
    private float _velocityY = 0;
    private float _friction = 0.95f;
    private float _autoRotateSpeed = 0.005f;
    private bool _isAutoRotating = true;
    private int _idleFrames = 0;

    // Interaction State
    private Point _lastMousePos;
    private bool _isDragging;
    
    private double _width;
    private double _height;
    
    private float _scanHeight = -100f;
    private float _scanSpeed = 2f;

    public RoomScanner()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += RoomScanner_Loaded;
        this.Unloaded += RoomScanner_Unloaded;
        this.SizeChanged += RoomScanner_SizeChanged;
    }

    private void RoomScanner_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeScene();
        _timer.Start();
    }

    private void RoomScanner_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void RoomScanner_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _width = e.NewSize.Width;
        _height = e.NewSize.Height;
        ClipRect.Rect = new Rect(0, 0, _width, _height);
    }

    private void InitializeScene()
    {
        _edges.Clear();
        SceneCanvas.Children.Clear();
        _lines.Clear();

        // Floor Grid
        int gridSize = 4;
        float spacing = 40f;
        float offset = (gridSize * spacing) / 2;

        for (int i = 0; i <= gridSize; i++)
        {
            AddEdge(new Vector3(-offset, 0, i * spacing - offset), new Vector3(offset, 0, i * spacing - offset), true);
            AddEdge(new Vector3(i * spacing - offset, 0, -offset), new Vector3(i * spacing - offset, 0, offset), true);
        }

        // Table (Simple Box)
        AddBox(new Vector3(0, -20, 0), 60, 5, 40); // Table Top
        AddBox(new Vector3(-25, -10, -15), 5, 20, 5); // Leg FL
        AddBox(new Vector3(25, -10, -15), 5, 20, 5); // Leg FR
        AddBox(new Vector3(-25, -10, 15), 5, 20, 5); // Leg BL
        AddBox(new Vector3(25, -10, 15), 5, 20, 5); // Leg BR
    }

    private void AddBox(Vector3 center, float w, float h, float d)
    {
        float hw = w / 2; float hh = h / 2; float hd = d / 2;
        Vector3[] v = new Vector3[8];
        v[0] = center + new Vector3(-hw, -hh, -hd); v[1] = center + new Vector3(hw, -hh, -hd);
        v[2] = center + new Vector3(hw, hh, -hd); v[3] = center + new Vector3(-hw, hh, -hd);
        v[4] = center + new Vector3(-hw, -hh, hd); v[5] = center + new Vector3(hw, -hh, hd);
        v[6] = center + new Vector3(hw, hh, hd); v[7] = center + new Vector3(-hw, hh, hd);

        AddEdge(v[0], v[1]); AddEdge(v[1], v[2]); AddEdge(v[2], v[3]); AddEdge(v[3], v[0]);
        AddEdge(v[4], v[5]); AddEdge(v[5], v[6]); AddEdge(v[6], v[7]); AddEdge(v[7], v[4]);
        AddEdge(v[0], v[4]); AddEdge(v[1], v[5]); AddEdge(v[2], v[6]); AddEdge(v[3], v[7]);
    }

    private void AddEdge(Vector3 start, Vector3 end, bool isGrid = false)
    {
        var edge = new Edge { Start = start, End = end, IsGrid = isGrid };
        _edges.Add(edge);
        var line = new Line
        {
            StrokeThickness = isGrid ? 1 : 2,
            Stroke = new SolidColorBrush(isGrid ? Windows.UI.Color.FromArgb(50, 255, 255, 255) : Windows.UI.Color.FromArgb(255, 16, 185, 129))
        };
        _lines.Add(line);
        SceneCanvas.Children.Add(line);
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = true;
        _isAutoRotating = false;
        _idleFrames = 0;
        _velocityX = 0;
        _velocityY = 0;
        _lastMousePos = e.GetCurrentPoint(this).Position;
        (sender as UIElement).CapturePointer(e.Pointer);
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isDragging)
        {
            var pos = e.GetCurrentPoint(this).Position;
            var deltaX = (float)(pos.X - _lastMousePos.X);
            var deltaY = (float)(pos.Y - _lastMousePos.Y);

            // Update Target directly
            _targetRotationY += deltaX * 0.008f; // Sensitivity
            _targetRotationX += deltaY * 0.008f;

            // Clamp Pitch
            _targetRotationX = Math.Clamp(_targetRotationX, -1.5f, 0.5f);

            // Calculate Velocity for momentum
            _velocityY = deltaX * 0.008f;
            _velocityX = deltaY * 0.008f;

            _lastMousePos = pos;
            _idleFrames = 0;
        }
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        (sender as UIElement).ReleasePointerCapture(e.Pointer);
    }

    private void Timer_Tick(object sender, object e)
    {
        if (_width == 0 || _height == 0) return;

        if (_isDragging)
        {
            // Follow target smoothly
            _rotationX += (_targetRotationX - _rotationX) * 0.2f;
            _rotationY += (_targetRotationY - _rotationY) * 0.2f;
        }
        else
        {
            // Apply Momentum
            _targetRotationY += _velocityY;
            _targetRotationX += _velocityX;
            
            // Clamp Pitch
            _targetRotationX = Math.Clamp(_targetRotationX, -1.5f, 0.5f);

            // Friction
            _velocityX *= _friction;
            _velocityY *= _friction;

            // Smoothly transition to rotation
            _rotationX += (_targetRotationX - _rotationX) * 0.1f;
            _rotationY += (_targetRotationY - _rotationY) * 0.1f;

            // Auto Rotate Recovery
            if (Math.Abs(_velocityX) < 0.001f && Math.Abs(_velocityY) < 0.001f)
            {
                _idleFrames++;
                if (_idleFrames > 120) // Wait 2 seconds before auto-rotating
                {
                    _targetRotationY += _autoRotateSpeed;
                }
            }
        }

        // Scan Animation
        _scanHeight += _scanSpeed;
        if (_scanHeight > 50) _scanHeight = -100;

        Render();
    }

    private void Render()
    {
        var center = new Vector2((float)_width / 2, (float)_height / 2);
        var rotationMatrix = Matrix4x4.CreateRotationX(_rotationX) * Matrix4x4.CreateRotationY(_rotationY);

        for (int i = 0; i < _edges.Count; i++)
        {
            var edge = _edges[i];
            var line = _lines[i];

            var p1 = Vector3.Transform(edge.Start, rotationMatrix);
            var p2 = Vector3.Transform(edge.End, rotationMatrix);

            float fov = 300;
            float zOffset = 400;

            if (p1.Z + zOffset <= 0 || p2.Z + zOffset <= 0)
            {
                line.Visibility = Visibility.Collapsed;
                continue;
            }

            float scale1 = fov / (p1.Z + zOffset);
            float scale2 = fov / (p2.Z + zOffset);

            float x1 = p1.X * scale1 + center.X;
            float y1 = p1.Y * scale1 + center.Y;
            float x2 = p2.X * scale2 + center.X;
            float y2 = p2.Y * scale2 + center.Y;

            line.X1 = x1; line.Y1 = y1;
            line.X2 = x2; line.Y2 = y2;
            line.Visibility = Visibility.Visible;

            if (!edge.IsGrid)
            {
                float midY = (edge.Start.Y + edge.End.Y) / 2;
                float dist = Math.Abs(midY - _scanHeight);
                if (dist < 10)
                {
                    line.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                    line.StrokeThickness = 3;
                }
                else
                {
                    line.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 185, 129));
                    line.StrokeThickness = 2;
                }
            }
        }
    }

    private class Edge
    {
        public Vector3 Start;
        public Vector3 End;
        public bool IsGrid;
    }
}

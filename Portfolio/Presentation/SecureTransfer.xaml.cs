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

public sealed partial class SecureTransfer : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly List<DataPacket> _packets = new();
    private readonly Random _random = new();
    private double _width;
    private double _height;
    private double _pulseTime;
    private bool _isHovering;
    private float _rotationSpeed = 1.0f;

    public SecureTransfer()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += SecureTransfer_Loaded;
        this.Unloaded += SecureTransfer_Unloaded;
        this.SizeChanged += SecureTransfer_SizeChanged;
        
        this.PointerEntered += (s, e) => _isHovering = true;
        this.PointerExited += (s, e) => _isHovering = false;
        this.PointerPressed += SecureTransfer_PointerPressed;
    }

    private void SecureTransfer_Loaded(object sender, RoutedEventArgs e)
    {
        _timer.Start();
    }

    private void SecureTransfer_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void SecureTransfer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _width = e.NewSize.Width;
        _height = e.NewSize.Height;
    }

    private void SecureTransfer_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        SpawnBurst();
    }

    private void Timer_Tick(object sender, object e)
    {
        // Update speed based on hover
        float targetSpeed = _isHovering ? 8.0f : 1.0f;
        _rotationSpeed = Lerp(_rotationSpeed, targetSpeed, 0.1f);

        // Rotate Shield
        if (ShieldRing.RenderTransform is RotateTransform rotate)
        {
            rotate.Angle += _rotationSpeed;
        }

        // Pulse Effect
        _pulseTime += 0.05 * (_isHovering ? 2.0 : 1.0);
        var scale = 1.0 + Math.Sin(_pulseTime) * 0.1;
        if (PulseCircle.RenderTransform is ScaleTransform scaleTrans)
        {
            scaleTrans.ScaleX = scale;
            scaleTrans.ScaleY = scale;
        }

        // Spawn Packets
        if (_random.NextDouble() < (_isHovering ? 0.3 : 0.1))
        {
            SpawnPacket();
        }

        // Update Packets
        UpdatePackets();
    }

    private void SpawnPacket()
    {
        if (_width == 0 || _height == 0) return;

        var angle = _random.NextDouble() * Math.PI * 2;
        var radius = Math.Max(_width, _height) / 1.5; // Start outside center area
        
        var packet = new DataPacket
        {
            Position = new Vector2(
                (float)(_width / 2 + Math.Cos(angle) * radius),
                (float)(_height / 2 + Math.Sin(angle) * radius)
            ),
            Velocity = new Vector2(
                (float)(-Math.Cos(angle) * 2 * (_isHovering ? 2.0 : 1.0)),
                (float)(-Math.Sin(angle) * 2 * (_isHovering ? 2.0 : 1.0))
            ),
            Element = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 59, 130, 246)) // Blue
            }
        };

        ParticleCanvas.Children.Add(packet.Element);
        _packets.Add(packet);
    }

    private void SpawnBurst()
    {
        if (_width == 0 || _height == 0) return;

        for (int i = 0; i < 20; i++)
        {
            var angle = _random.NextDouble() * Math.PI * 2;
            var speed = 3.0 + _random.NextDouble() * 3.0;
            
            var packet = new DataPacket
            {
                Position = new Vector2((float)_width / 2, (float)_height / 2),
                Velocity = new Vector2(
                    (float)(Math.Cos(angle) * speed),
                    (float)(Math.Sin(angle) * speed)
                ),
                IsBurst = true,
                Element = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 185, 129)) // Green for success
                }
            };

            ParticleCanvas.Children.Add(packet.Element);
            _packets.Add(packet);
        }
    }

    private void UpdatePackets()
    {
        for (int i = _packets.Count - 1; i >= 0; i--)
        {
            var packet = _packets[i];
            packet.Position += packet.Velocity;

            if (packet.IsBurst)
            {
                // Burst particles fade out and die
                packet.Opacity -= 0.02f;
                packet.Element.Opacity = packet.Opacity;
                
                if (packet.Opacity <= 0)
                {
                    ParticleCanvas.Children.Remove(packet.Element);
                    _packets.RemoveAt(i);
                    continue;
                }
            }
            else
            {
                // Incoming packets die at center
                var dist = Vector2.Distance(packet.Position, new Vector2((float)_width / 2, (float)_height / 2));
                if (dist < 30)
                {
                    ParticleCanvas.Children.Remove(packet.Element);
                    _packets.RemoveAt(i);
                    continue;
                }
            }

            Canvas.SetLeft(packet.Element, packet.Position.X - (packet.Element.Width / 2));
            Canvas.SetTop(packet.Element, packet.Position.Y - (packet.Element.Height / 2));
        }
    }

    private float Lerp(float a, float b, float t) => a + (b - a) * t;

    private class DataPacket
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public FrameworkElement Element;
        public bool IsBurst;
        public float Opacity = 1.0f;
    }
}

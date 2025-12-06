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

public sealed partial class ParticleVortex : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();
    private Vector2 _center;
    private bool _isHovering;
    private Point _mousePos;

    public ParticleVortex()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += ParticleVortex_Loaded;
        this.Unloaded += ParticleVortex_Unloaded;
        this.SizeChanged += ParticleVortex_SizeChanged;
        this.PointerEntered += ParticleVortex_PointerEntered;
        this.PointerExited += ParticleVortex_PointerExited;
        this.PointerMoved += ParticleVortex_PointerMoved;
    }

    private void ParticleVortex_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeParticles();
        _timer.Start();
    }

    private void ParticleVortex_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void ParticleVortex_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            _center = new Vector2((float)e.NewSize.Width / 2, (float)e.NewSize.Height / 2);
        }
    }

    private void ParticleVortex_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = true;
    }

    private void ParticleVortex_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = false;
    }

    private void ParticleVortex_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        _mousePos = e.GetCurrentPoint(this).Position;
    }

    private void InitializeParticles()
    {
        VortexCanvas.Children.Clear();
        _particles.Clear();

        for (int i = 0; i < 100; i++)
        {
            SpawnParticle();
        }
    }

    private void SpawnParticle(bool randomPos = true)
    {
        var particle = new Particle();
        
        // Visual
        var ellipse = new Ellipse
        {
            Width = _random.Next(2, 5),
            Height = _random.Next(2, 5),
            Fill = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)_random.Next(100, 255), 59, 130, 246)) // Blue shades
        };
        particle.Element = ellipse;
        VortexCanvas.Children.Add(ellipse);

        ResetParticle(particle, randomPos);
        _particles.Add(particle);
    }

    private void ResetParticle(Particle p, bool randomPos)
    {
        float angle = (float)(_random.NextDouble() * Math.PI * 2);
        float dist = randomPos ? (float)_random.NextDouble() * 200 + 50 : 250;
        
        p.Position = _center + new Vector2((float)Math.Cos(angle) * dist, (float)Math.Sin(angle) * dist);
        
        // Initial velocity tangent to circle (orbit)
        float speed = (float)_random.NextDouble() * 2 + 1;
        p.Velocity = new Vector2(-(float)Math.Sin(angle), (float)Math.Cos(angle)) * speed;
        
        // Add some inward drift
        p.Velocity += Vector2.Normalize(_center - p.Position) * 0.5f;
    }

    private void Timer_Tick(object sender, object e)
    {
        if (_center == Vector2.Zero) return;

        foreach (var p in _particles)
        {
            // Gravity towards center
            var toCenter = _center - p.Position;
            float distSq = toCenter.LengthSquared();
            float dist = (float)Math.Sqrt(distSq);
            
            if (dist < 10) // Too close, respawn
            {
                ResetParticle(p, false);
                continue;
            }

            var dirToCenter = toCenter / dist;
            
            // Force calculation
            float gravityStrength = 5000 / distSq; // Inverse square law-ish
            p.Velocity += dirToCenter * gravityStrength;

            // Mouse interaction (Repel)
            if (_isHovering)
            {
                var mouseVec = new Vector2((float)_mousePos.X, (float)_mousePos.Y);
                var toMouse = p.Position - mouseVec;
                float mouseDistSq = toMouse.LengthSquared();
                
                if (mouseDistSq < 10000)
                {
                    p.Velocity += Vector2.Normalize(toMouse) * 2.0f;
                }
            }

            // Drag
            p.Velocity *= 0.98f;

            // Update Position
            p.Position += p.Velocity;

            // Update Visuals
            Canvas.SetLeft(p.Element, p.Position.X - p.Element.Width / 2);
            Canvas.SetTop(p.Element, p.Position.Y - p.Element.Height / 2);
            
            // Scale based on speed/dist?
            // Opacity based on life?
        }
    }

    private class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Ellipse Element;
    }
}

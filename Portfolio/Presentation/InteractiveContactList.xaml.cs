using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;
using Windows.Foundation;

namespace Portfolio.Presentation;

public sealed partial class InteractiveContactList : UserControl
{
    private readonly DispatcherTimer _timer;
    private Point _mousePos;
    private bool _isHovering;
    private float _glowScale = 1.0f;
    private float _glowDirection = 0.02f;

    public InteractiveContactList()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += InteractiveContactList_Loaded;
        this.Unloaded += InteractiveContactList_Unloaded;
        this.PointerEntered += InteractiveContactList_PointerEntered;
        this.PointerExited += InteractiveContactList_PointerExited;
        this.PointerMoved += InteractiveContactList_PointerMoved;
    }

    private void InteractiveContactList_Loaded(object sender, RoutedEventArgs e)
    {
        _timer.Start();
    }

    private void InteractiveContactList_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void InteractiveContactList_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = true;
    }

    private void InteractiveContactList_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = false;
        // Reset positions
        ResetTransform(PoetryContainer);
        ResetTransform(GithubBtn);
        ResetTransform(LinkBtn);
        ResetTransform(EmailBtn);
    }

    private void InteractiveContactList_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        _mousePos = e.GetCurrentPoint(this).Position;
    }

    private void Timer_Tick(object sender, object e)
    {
        // Pulse Effect for Poetry Glow
        _glowScale += _glowDirection;
        if (_glowScale > 1.4f || _glowScale < 1.0f) _glowDirection *= -1;
        
        var glowTransform = PoetryGlow.RenderTransform as CompositeTransform;
        if (glowTransform != null)
        {
            glowTransform.ScaleX = _glowScale;
            glowTransform.ScaleY = _glowScale;
        }

        if (_isHovering)
        {
            ApplyMagneticEffect(PoetryContainer, 0.3f);
            ApplyMagneticEffect(GithubBtn, 0.4f);
            ApplyMagneticEffect(LinkBtn, 0.4f);
            ApplyMagneticEffect(EmailBtn, 0.4f);
        }
        else
        {
            // Smooth return to zero handled by simple lerp in ApplyMagneticEffect if we wanted, 
            // but for now we just reset in PointerExited. 
            // Let's add smooth return here if needed.
        }
    }

    private void ApplyMagneticEffect(FrameworkElement element, float strength)
    {
        var transform = element.RenderTransform as CompositeTransform;
        if (transform == null) return;

        // Get element center relative to control
        var transformToRoot = element.TransformToVisual(this);
        var elementPos = transformToRoot.TransformPoint(new Point(0, 0));
        var center = new Vector2((float)(elementPos.X + element.ActualWidth / 2), (float)(elementPos.Y + element.ActualHeight / 2));
        var mouse = new Vector2((float)_mousePos.X, (float)_mousePos.Y);

        var distVec = mouse - center;
        float dist = distVec.Length();

        if (dist < 150) // Magnetic range
        {
            var targetX = distVec.X * strength;
            var targetY = distVec.Y * strength;

            // Lerp for smoothness
            transform.TranslateX += (targetX - transform.TranslateX) * 0.1;
            transform.TranslateY += (targetY - transform.TranslateY) * 0.1;
            
            // Scale up slightly
            transform.ScaleX += (1.1 - transform.ScaleX) * 0.1;
            transform.ScaleY += (1.1 - transform.ScaleY) * 0.1;
        }
        else
        {
            // Return to origin
            transform.TranslateX += (0 - transform.TranslateX) * 0.1;
            transform.TranslateY += (0 - transform.TranslateY) * 0.1;
            
            transform.ScaleX += (1.0 - transform.ScaleX) * 0.1;
            transform.ScaleY += (1.0 - transform.ScaleY) * 0.1;
        }
    }

    private void ResetTransform(FrameworkElement element)
    {
        var transform = element.RenderTransform as CompositeTransform;
        if (transform != null)
        {
            transform.TranslateX = 0;
            transform.TranslateY = 0;
            transform.ScaleX = 1;
            transform.ScaleY = 1;
        }
    }
}

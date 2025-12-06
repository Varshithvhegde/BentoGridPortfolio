using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;

namespace Portfolio.Presentation;

public sealed partial class MatrixRain : UserControl
{
    private const int FontSize = 14;
    private readonly Random _random = new();
    private readonly List<MatrixColumn> _columns = new();
    private readonly DispatcherTimer _timer;
    private double _width;
    private double _height;

    public MatrixRain()
    {
        this.InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += Timer_Tick;
        this.Loaded += MatrixRain_Loaded;
        this.Unloaded += MatrixRain_Unloaded;
        this.SizeChanged += MatrixRain_SizeChanged;
    }

    private void MatrixRain_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeRain();
        _timer.Start();
    }

    private void MatrixRain_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void MatrixRain_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            InitializeRain();
        }
    }

    private void InitializeRain()
    {
        if (ActualWidth == 0 || ActualHeight == 0) return;

        _width = ActualWidth;
        _height = ActualHeight;
        RainCanvas.Children.Clear();
        _columns.Clear();

        int columnCount = (int)(_width / FontSize);

        for (int i = 0; i < columnCount; i++)
        {
            var column = new MatrixColumn
            {
                X = i * FontSize,
                Y = _random.Next(-(int)_height, 0),
                Speed = _random.Next(2, 10),
                Chars = new List<TextBlock>()
            };

            // Create a stream of characters for this column
            int len = _random.Next(5, 20);
            for (int j = 0; j < len; j++)
            {
                var tb = new TextBlock
                {
                    Text = GetRandomChar(),
                    FontSize = FontSize,
                    FontFamily = new FontFamily("Consolas"),
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 255, 65)) // Matrix Green
                };
                
                // Fade out tail
                if (j > len - 5) tb.Opacity = 0.2;
                else if (j > len - 10) tb.Opacity = 0.5;
                
                // Head is brighter
                if (j == 0) 
                {
                    tb.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 255, 200));
                    tb.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
                }

                RainCanvas.Children.Add(tb);
                column.Chars.Add(tb);
            }
            _columns.Add(column);
        }
    }

    private void Timer_Tick(object sender, object e)
    {
        foreach (var col in _columns)
        {
            col.Y += col.Speed;

            // Reset if off screen
            if (col.Y > _height)
            {
                col.Y = _random.Next(-(int)_height / 2, 0);
                col.Speed = _random.Next(2, 10);
            }

            // Update positions
            for (int i = 0; i < col.Chars.Count; i++)
            {
                var tb = col.Chars[i];
                double charY = col.Y - (i * FontSize);
                
                Canvas.SetLeft(tb, col.X);
                Canvas.SetTop(tb, charY);

                // Randomly change character
                if (_random.NextDouble() < 0.05)
                {
                    tb.Text = GetRandomChar();
                }
            }
        }
    }

    private string GetRandomChar()
    {
        // Katakana or standard chars
        // Using standard for simplicity and font compatibility
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$#@%&*";
        return chars[_random.Next(chars.Length)].ToString();
    }

    private class MatrixColumn
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
        public List<TextBlock> Chars { get; set; } = new();
    }
}

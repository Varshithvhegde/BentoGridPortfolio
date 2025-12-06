using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Portfolio.Presentation;

public sealed partial class SnakeGrid : UserControl
{
    private const int CellSize = 16;
    private const int CellMargin = 2;
    private int _rows;
    private int _cols;
    private Rectangle[,]? _gridCells;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();
    
    // Snake State
    private List<(int r, int c)> _snake = new();
    private (int r, int c) _direction = (0, 1); // Moving right initially
    private (int r, int c) _food;
    
    // Colors
    private readonly SolidColorBrush _emptyColor = new(Windows.UI.Color.FromArgb(255, 22, 27, 34)); // #161B22 (GitHub Dark Dimmed bg)
    private readonly SolidColorBrush _snakeHeadColor = new(Windows.UI.Color.FromArgb(255, 255, 255, 255)); // White Head
    private readonly SolidColorBrush _snakeBodyColor = new(Windows.UI.Color.FromArgb(255, 57, 211, 83)); // #39D353 (GitHub Bright Green)
    private readonly SolidColorBrush _foodColorBright = new(Windows.UI.Color.FromArgb(255, 210, 168, 255)); // #D2A8FF (Purple Food)

    public SnakeGrid()
    {
        this.InitializeComponent();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += Timer_Tick;
        
        this.Loaded += SnakeGrid_Loaded;
        this.Unloaded += SnakeGrid_Unloaded;
        this.SizeChanged += SnakeGrid_SizeChanged;
    }

    private void SnakeGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (_gridCells == null)
        {
            InitializeGrid();
            StartGame();
        }
    }

    private void SnakeGrid_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    private void SnakeGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Re-initialize if size changes
        if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            // Simple debounce/check to avoid unnecessary recreations if size is similar (e.g. layout pass noise)
            int newCols = (int)(e.NewSize.Width / (CellSize + CellMargin));
            int newRows = (int)(e.NewSize.Height / (CellSize + CellMargin));

            if (newCols != _cols || newRows != _rows)
            {
                InitializeGrid();
                StartGame();
            }
        }
    }

    private void InitializeGrid()
    {
        if (ActualWidth == 0 || ActualHeight == 0) return;

        _timer.Stop();
        ContainerGrid.Children.Clear();
        ContainerGrid.ColumnDefinitions.Clear();
        ContainerGrid.RowDefinitions.Clear();

        _cols = (int)(ActualWidth / (CellSize + CellMargin));
        _rows = (int)(ActualHeight / (CellSize + CellMargin));

        if (_cols == 0 || _rows == 0) return;

        _gridCells = new Rectangle[_rows, _cols];

        // Create Grid Definitions
        for (int i = 0; i < _cols; i++)
            ContainerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize + CellMargin) });
        
        for (int i = 0; i < _rows; i++)
            ContainerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize + CellMargin) });

        // Populate Cells
        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                var rect = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    RadiusX = 2,
                    RadiusY = 2,
                    Fill = _emptyColor,
                    Margin = new Thickness(CellMargin / 2)
                };
                
                Grid.SetRow(rect, r);
                Grid.SetColumn(rect, c);
                ContainerGrid.Children.Add(rect);
                _gridCells[r, c] = rect;
            }
        }
    }

    private void StartGame()
    {
        if (_rows == 0 || _cols == 0) return;

        _snake.Clear();
        int startR = _rows / 2;
        int startC = _cols / 4;
        
        // Initial snake of length 5
        for (int i = 0; i < 5; i++)
        {
            _snake.Add((startR, startC - i));
        }

        SpawnFood();
        UpdateGridVisuals();
        _timer.Start();
    }

    private void SpawnFood()
    {
        int r, c;
        do
        {
            r = _random.Next(0, _rows);
            c = _random.Next(0, _cols);
        } while (_snake.Contains((r, c)));

        _food = (r, c);
    }

    private void Timer_Tick(object sender, object e)
    {
        MoveSnake();
        UpdateGridVisuals();
    }

    private void MoveSnake()
    {
        var head = _snake[0];
        
        // Simple AI: Move towards food
        // Determine possible moves
        var possibleMoves = new List<(int r, int c)>
        {
            (0, 1), (0, -1), (1, 0), (-1, 0) // Right, Left, Down, Up
        };

        // Filter out moves that hit walls or self (immediate death)
        // Also try to move towards food
        
        var validMoves = possibleMoves.Where(m => IsValidMove(head.r + m.r, head.c + m.c)).ToList();
        
        if (validMoves.Count == 0)
        {
            // Game Over - Restart
            StartGame();
            return;
        }

        // Prefer moves that decrease distance to food
        var bestMove = validMoves.OrderBy(m => GetDistance(head.r + m.r, head.c + m.c, _food.r, _food.c)).First();
        
        // Add some randomness to make it look more "alive" and less robotic, or if stuck
        if (_random.NextDouble() < 0.1 && validMoves.Count > 1)
        {
            bestMove = validMoves[_random.Next(validMoves.Count)];
        }

        _direction = bestMove;
        
        var newHead = (head.r + _direction.r, head.c + _direction.c);
        _snake.Insert(0, newHead);

        // Check Food
        if (newHead == _food)
        {
            SpawnFood();
            // Grow (don't remove tail)
        }
        else
        {
            _snake.RemoveAt(_snake.Count - 1);
        }
    }

    private bool IsValidMove(int r, int c)
    {
        // Check Bounds
        if (r < 0 || r >= _rows || c < 0 || c >= _cols) return false;
        
        // Check Collision with Self (ignore tail as it will move)
        // We can be strict here
        if (_snake.Contains((r, c))) return false;

        return true;
    }

    private double GetDistance(int r1, int c1, int r2, int c2)
    {
        return Math.Abs(r1 - r2) + Math.Abs(c1 - c2); // Manhattan distance
    }

    private void UpdateGridVisuals()
    {
        if (_gridCells == null) return;

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                if (_gridCells[r, c].Fill != _emptyColor)
                    _gridCells[r, c].Fill = _emptyColor;
            }
        }

        // Draw Food
        if (_gridCells[_food.r, _food.c] != null)
            _gridCells[_food.r, _food.c].Fill = _foodColorBright;

        // Draw Snake
        for (int i = 0; i < _snake.Count; i++)
        {
            var (r, c) = _snake[i];
            if (r >= 0 && r < _rows && c >= 0 && c < _cols)
            {
                _gridCells[r, c].Fill = i == 0 ? _snakeHeadColor : _snakeBodyColor;
            }
        }
    }
}

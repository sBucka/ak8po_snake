// Refactor of: https://codereview.stackexchange.com/q/127515 (author Wagacca, CC BY-SA 3.0).
// Original retrieved 2026-05-04. Behavior preserved; structure rewritten per Clean Code principles.

new Game().Run();


internal readonly record struct Position(int X, int Y);

internal enum Direction { Up, Down, Left, Right }


internal sealed class Game
{
    // --- Constants ---

    private const int Width = 32;
    private const int Height = 16;
    private const int TickMilliseconds = 500;
    private const int InitialScore = 5;

    private const ConsoleColor BorderColor = ConsoleColor.Green;
    private const ConsoleColor SnakeColor = ConsoleColor.Red;
    private const ConsoleColor FoodColor = ConsoleColor.Cyan;
    private const ConsoleColor TextColor = ConsoleColor.Gray;
    private const char Block = '■';

    // --- State ---

    private Position _head;
    private Direction _direction = Direction.Right;
    private readonly List<Position> _body = new();
    private Position _food;
    private int _score = InitialScore;
    private readonly Random _random = new();

    // --- Public entry point ---

    public void Run()
    {
        InitializeConsole();
        _head = new Position(Width / 2, Height / 2);
        SpawnFood();

        while (true)
        {
            TryEatFood();
            if (IsGameOver()) break;
            RenderFrame();
            WaitForNextTick();
            AdvanceSnake();
        }

        RenderGameOver();
    }

    // --- Game logic (no Console.* calls) ---

    private bool IsGameOver()
    {
        return HasHitWall() || HasHitItself();
    }

    private bool HasHitWall()
    {
        return _head.X == 0 || _head.X == Width - 1
            || _head.Y == 0 || _head.Y == Height - 1;
    }

    private bool HasHitItself()
    {
        return _body.Contains(_head);
    }

    private bool HasEatenFood()
    {
        return _head == _food;
    }

    private void TryEatFood()
    {
        if (!HasEatenFood()) return;
        _score++;
        SpawnFood();
    }

    private void AdvanceSnake()
    {
        _body.Add(_head);
        _head = NextHeadPosition();
        if (_body.Count > _score)
        {
            _body.RemoveAt(0);
        }
    }

    private Position NextHeadPosition()
    {
        return _direction switch
        {
            Direction.Up    => new Position(_head.X, _head.Y - 1),
            Direction.Down  => new Position(_head.X, _head.Y + 1),
            Direction.Left  => new Position(_head.X - 1, _head.Y),
            Direction.Right => new Position(_head.X + 1, _head.Y),
            _ => _head,
        };
    }

    private void SpawnFood()
    {
        _food = new Position(
            _random.Next(1, Width - 1),
            _random.Next(1, Height - 1));
    }

    private static bool IsOpposite(Direction a, Direction b)
    {
        return (a == Direction.Up    && b == Direction.Down)
            || (a == Direction.Down  && b == Direction.Up)
            || (a == Direction.Left  && b == Direction.Right)
            || (a == Direction.Right && b == Direction.Left);
    }

    // --- Rendering (only Console.* output happens here) ---

    private static void InitializeConsole()
    {
        Console.WindowHeight = Height;
        Console.WindowWidth = Width;
        Console.CursorVisible = false;
    }

    private void RenderFrame()
    {
        Console.Clear();
        RenderBorders();
        RenderBody();
        RenderHead();
        RenderFood();
    }

    private static void RenderBorders()
    {
        Console.ForegroundColor = BorderColor;
        for (int x = 0; x < Width; x++)
        {
            DrawBlock(x, 0);
            DrawBlock(x, Height - 1);
        }
        for (int y = 0; y < Height; y++)
        {
            DrawBlock(0, y);
            DrawBlock(Width - 1, y);
        }
    }

    private void RenderBody()
    {
        Console.ForegroundColor = SnakeColor;
        foreach (var segment in _body)
        {
            DrawBlock(segment.X, segment.Y);
        }
    }

    private void RenderHead()
    {
        Console.ForegroundColor = SnakeColor;
        DrawBlock(_head.X, _head.Y);
    }

    private void RenderFood()
    {
        Console.ForegroundColor = FoodColor;
        DrawBlock(_food.X, _food.Y);
    }

    private void RenderGameOver()
    {
        Console.ForegroundColor = TextColor;
        Console.SetCursorPosition(Width / 5, Height / 2);
        Console.WriteLine($"Game over, Score: {_score}");
    }

    private static void DrawBlock(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        Console.Write(Block);
    }

    // --- Input (only Console.KeyAvailable / ReadKey calls happen here) ---

    private void WaitForNextTick()
    {
        var deadline = DateTime.Now.AddMilliseconds(TickMilliseconds);
        bool keyApplied = false;

        while (DateTime.Now < deadline)
        {
            if (keyApplied || !Console.KeyAvailable) continue;
            var key = Console.ReadKey(intercept: true).Key;
            keyApplied = TryChangeDirection(key);
        }
    }

    private bool TryChangeDirection(ConsoleKey key)
    {
        Direction? next = key switch
        {
            ConsoleKey.UpArrow    => Direction.Up,
            ConsoleKey.DownArrow  => Direction.Down,
            ConsoleKey.LeftArrow  => Direction.Left,
            ConsoleKey.RightArrow => Direction.Right,
            _ => null,
        };

        if (next is null || IsOpposite(next.Value, _direction))
        {
            return false;
        }

        _direction = next.Value;
        return true;
    }
}

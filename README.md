The refactored version keeps the same gameplay (32×16 board, 500 ms ticks,
arrow-key controls, growth on food, game-over on wall or self-collision)
but reorganizes the code per Clean Code principles.

The single class `Game` is internally split into three labeled regions:

| Region         | Responsibility                               | Touches `Console.*`? |
| -------------- | -------------------------------------------- | -------------------- |
| **Game logic** | movement, collisions, scoring, food          | **No**               |
| **Rendering**  | drawing borders, snake, food, game-over text | Yes (output)         |
| **Input**      | reading arrow keys, blocking 180° reversal   | Yes (input)          |

The logic methods do not call `Console` at all — the same code could drive a
WinForms or web renderer by replacing only the rendering and input regions.
This is the core point of the exercise: **logic lives independently of the GUI.**

## Project layout

```text
snake_refactored/
├── snake_refactored.csproj   # net8.0, single console project
├── Program.cs                # Position, Direction, Game
├── README.md
└── .gitignore
```

## How to run

```sh
dotnet run
```

Use the arrow keys to control the snake. Window must be at least 32×16 characters.

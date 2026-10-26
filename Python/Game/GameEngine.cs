using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Python.Game
{
    // All the game rules live here, separate from the WPF screens so they can be
    // tested on their own (see the EngineTests project). The screen calls Step()
    // to advance the snake and TickSecond() once per second.
    public class GameEngine
    {
        public int GridSize { get; private set; }
        public List<Cell> Snake { get; private set; } = new List<Cell>();  // Snake[0] is the head
        public List<Apple> Apples { get; private set; } = new List<Apple>();
        public string QuestionText { get; private set; } = "";

        public int Lives { get; private set; } = 3;
        public int Score { get; private set; } = 0;
        public int SecondsLeft { get; private set; }
        public bool GameOver { get; private set; } = false;

        public PowerUpType ActivePowerUp { get; private set; } = PowerUpType.Geen;
        public int PowerUpSecondsLeft { get; private set; } = 0;

        // A power-up lying on the board, waiting to be picked up.
        public bool HasToken { get; private set; } = false;
        public Cell Token { get; private set; }
        public PowerUpType TokenType { get; private set; } = PowerUpType.Geen;

        public GameSettings Settings { get; private set; }

        private Direction _dir = Direction.Right;
        private readonly Queue<Direction> _inputs = new Queue<Direction>();  // pending turns
        private Direction _lastQueued = Direction.Right;
        private int _nextPowerUpIn;
        private int _startLength = 3;
        private readonly Random _rng;

        public GameEngine(GameSettings settings, Random rng = null)
        {
            Settings = settings;
            _rng = rng ?? new Random();
            StartNew();
        }

        public void StartNew()
        {
            GridSize = Settings.GridSize;
            Lives = 3;
            Score = 0;
            GameOver = false;
            SecondsLeft = 120;
            ActivePowerUp = PowerUpType.Geen;
            PowerUpSecondsLeft = 0;
            HasToken = false;
            TokenType = PowerUpType.Geen;
            _nextPowerUpIn = RandomPowerUpDelay();
            _dir = Direction.Right;
            _inputs.Clear();
            _lastQueued = Direction.Right;

            // Start with a 3-tile snake in the middle, head facing right.
            Snake.Clear();
            int midRow = GridSize / 2;
            Snake.Add(new Cell(2, midRow));
            Snake.Add(new Cell(1, midRow));
            Snake.Add(new Cell(0, midRow));
            _startLength = Snake.Count;

            NewRound();
        }

        // Turns are buffered (up to two) so two quick presses both register.
        public void SetDirection(Direction d)
        {
            // Compare against the last chosen direction so you can never reverse into yourself.
            Direction vorige = _inputs.Count > 0 ? _lastQueued : _dir;
            if (d == vorige || IsOpposite(d, vorige)) return;
            if (_inputs.Count >= 2) return;
            _inputs.Enqueue(d);
            _lastQueued = d;
        }

        // One snake step: move, eat, grow and collision checks.
        public void Step()
        {
            if (GameOver) return;

            if (_inputs.Count > 0) _dir = _inputs.Dequeue();

            Cell head = Snake[0];
            Cell next = Move(head, _dir);

            // Wall.
            if (next.Col < 0 || next.Row < 0 || next.Col >= GridSize || next.Row >= GridSize)
            {
                GameOver = true;
                return;
            }
            // Self. The last tail segment doesn't count, it's about to move away.
            for (int i = 0; i < Snake.Count - 1; i++)
                if (Snake[i].Equals(next)) { GameOver = true; return; }

            Snake.Insert(0, next);
            bool groeien = false;

            if (HasToken && Token.Equals(next))
            {
                ActivatePowerUp(TokenType);
                HasToken = false;
                TokenType = PowerUpType.Geen;
            }
            else
            {
                Apple gegeten = null;
                foreach (Apple a in Apples)
                    if (a.Position.Equals(next)) { gegeten = a; break; }

                if (gegeten != null)
                {
                    bool veilig = EatAllActive(); // with a power-up any apple is safe to eat
                    if (gegeten.IsCorrect)
                    {
                        Score += (ActivePowerUp == PowerUpType.Blauw) ? 20 : 10;
                        groeien = true;
                        NewRound();
                    }
                    else if (veilig)
                    {
                        Score += 2;
                        NewRound();
                    }
                    else
                    {
                        Lives--;
                        if (Lives <= 0) GameOver = true;
                        NewRound();
                    }
                }
            }

            // Keep the tail when growing; otherwise drop it so the snake stays the same length.
            if (!groeien && Snake.Count > 0)
                Snake.RemoveAt(Snake.Count - 1);
        }

        // Clock and power-up timing, once per second.
        public void TickSecond()
        {
            if (GameOver) return;

            SecondsLeft--;
            if (SecondsLeft <= 0) { SecondsLeft = 0; GameOver = true; return; }

            if (ActivePowerUp != PowerUpType.Geen)
            {
                PowerUpSecondsLeft--;
                if (PowerUpSecondsLeft <= 0)
                {
                    ActivePowerUp = PowerUpType.Geen;
                    PowerUpSecondsLeft = 0;
                }
            }

            // Count down to the next power-up only when nothing is on the board or active.
            if (!HasToken && ActivePowerUp == PowerUpType.Geen)
            {
                _nextPowerUpIn--;
                if (_nextPowerUpIn <= 0) SpawnPowerUp();
            }
        }

        // Delay between steps in ms (lower is faster): speeds up as the snake grows,
        // slower on Makkelijk, adjusted by active power-ups.
        public int CurrentMoveDelayMs()
        {
            int gegroeid = Snake.Count - _startLength;
            int delay = Settings.BaseMoveDelayMs - gegroeid * 8;
            delay = (int)(delay * Settings.SpeedFactor); // player-chosen speed
            if (delay < 55) delay = 55;
            if (Settings.Difficulty == Difficulty.Makkelijk) delay = (int)(delay * 4.0); // Snelheid langzaam
            if (ActivePowerUp == PowerUpType.Rood) delay = (int)(delay * 0.6);
            if (ActivePowerUp == PowerUpType.Blauw) delay = (int)(delay * 1.6);
            return delay;
        }

        // Used by the screen to draw the hint on Makkelijk.
        public Apple CorrectApple()
        {
            foreach (Apple a in Apples)
                if (a.IsCorrect) return a;
            return null;
        }

        private bool EatAllActive()
        {
            return ActivePowerUp == PowerUpType.Rood || ActivePowerUp == PowerUpType.Blauw;
        }

        private void ActivatePowerUp(PowerUpType t)
        {
            if (t == PowerUpType.Klok) { SecondsLeft += 15; return; } // extra time is instant, no duration
            ActivePowerUp = t;
            PowerUpSecondsLeft = 10;
        }

        private void SpawnPowerUp()
        {
            Cell plek;
            if (!TryRandomFreeCell(out plek)) { _nextPowerUpIn = 5; return; }
            PowerUpType[] soorten = { PowerUpType.Rood, PowerUpType.Blauw, PowerUpType.Klok };
            TokenType = soorten[_rng.Next(soorten.Length)];
            Token = plek;
            HasToken = true;
            _nextPowerUpIn = RandomPowerUpDelay();
        }

        // 50 to 70 seconds until the next power-up.
        private int RandomPowerUpDelay()
        {
            return 60 + _rng.Next(-10, 11);
        }

        private void NewRound()
        {
            Apples.Clear();
            List<Cell> plekken = PickApplePositions();

            Round ronde = Questions.Build(Settings, _rng);
            QuestionText = ronde.Question;

            for (int i = 0; i < 4 && i < plekken.Count; i++)
                Apples.Add(new Apple(plekken[i], ronde.Labels[i], i == ronde.CorrectIndex));
        }

        // Pick up to 4 apple spots that are easy to reach: away from the border and
        // never touching each other (so you never get a wall of apples).
        private List<Cell> PickApplePositions()
        {
            List<Cell> vrij = FreeCells();

            // Prefer inner cells; fall back to all free cells if the inside is too crowded.
            List<Cell> binnen = vrij.FindAll(c =>
                c.Col > 0 && c.Col < GridSize - 1 && c.Row > 0 && c.Row < GridSize - 1);
            List<Cell> pool = binnen.Count >= 4 ? binnen : vrij;
            Shuffle(pool);

            List<Cell> gekozen = new List<Cell>();
            // First pass: keep every apple at least one empty tile apart (also diagonally).
            foreach (Cell c in pool)
            {
                if (gekozen.Count == 4) break;
                if (!TouchesAny(c, gekozen)) gekozen.Add(c);
            }
            // Second pass: if the spacing was too strict to reach 4, top up with what's left.
            if (gekozen.Count < 4)
                foreach (Cell c in pool)
                {
                    if (gekozen.Count == 4) break;
                    if (!gekozen.Contains(c)) gekozen.Add(c);
                }
            return gekozen;
        }

        private static bool TouchesAny(Cell c, List<Cell> others)
        {
            foreach (Cell o in others)
                if (Math.Abs(c.Col - o.Col) <= 1 && Math.Abs(c.Row - o.Row) <= 1) return true;
            return false;
        }

        private List<Cell> FreeCells()
        {
            List<Cell> result = new List<Cell>();
            for (int col = 0; col < GridSize; col++)
                for (int row = 0; row < GridSize; row++)
                {
                    Cell c = new Cell(col, row);
                    if (Snake.Contains(c)) continue;
                    if (HasToken && Token.Equals(c)) continue;
                    result.Add(c);
                }
            return result;
        }

        private bool TryRandomFreeCell(out Cell cell)
        {
            List<Cell> vrij = FreeCells();
            vrij.RemoveAll(c =>
            {
                foreach (Apple a in Apples) if (a.Position.Equals(c)) return true;
                return false;
            });
            if (vrij.Count == 0) { cell = new Cell(0, 0); return false; }
            cell = vrij[_rng.Next(vrij.Count)];
            return true;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                T tijdelijk = list[i];
                list[i] = list[j];
                list[j] = tijdelijk;
            }
        }

        private static Cell Move(Cell c, Direction d)
        {
            switch (d)
            {
                case Direction.Up: return new Cell(c.Col, c.Row - 1);
                case Direction.Down: return new Cell(c.Col, c.Row + 1);
                case Direction.Left: return new Cell(c.Col - 1, c.Row);
                default: return new Cell(c.Col + 1, c.Row);
            }
        }

        private static bool IsOpposite(Direction a, Direction b)
        {
            return (a == Direction.Up && b == Direction.Down)
                || (a == Direction.Down && b == Direction.Up)
                || (a == Direction.Left && b == Direction.Right)
                || (a == Direction.Right && b == Direction.Left);
        }
    }
}  

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Python.Game;

namespace Python.Screens
{
    // The play screen. It shows what the GameEngine tracks and forwards key presses;
    // all the rules live in the engine. Two timers run: one moves the snake, one
    // counts down the clock. After each step the board is redrawn in Render().
    public partial class GameControl : UserControl
    {
        private const double BoardSize = 560.0;

        private GameEngine _engine;
        private DispatcherTimer _moveTimer;
        private DispatcherTimer _clockTimer;
        private bool _ended;
        private bool _saved;

        public GameControl() { InitializeComponent(); }

        private MainWindow Main() { return (MainWindow)Application.Current.MainWindow; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _engine = new GameEngine(Main().Settings);
            SetGridBackground(BoardSize / _engine.GridSize); // draw the grid once, not every frame

            _moveTimer = new DispatcherTimer();
            _moveTimer.Interval = TimeSpan.FromMilliseconds(_engine.CurrentMoveDelayMs());
            _moveTimer.Tick += MoveTick;
            _moveTimer.Start();

            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += ClockTick;
            _clockTimer.Start();

            Focus(); // needed to receive key presses
            Render();
            UpdateHud();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) { StopTimers(); }

        private void StopTimers()
        {
            if (_moveTimer != null) _moveTimer.Stop();
            if (_clockTimer != null) _clockTimer.Stop();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_engine == null) return;
            if (_ended) return; // once game over, leave keys alone so the name box can be typed in
            if (e.Key == Key.Up || e.Key == Key.W) _engine.SetDirection(Direction.Up);
            else if (e.Key == Key.Down || e.Key == Key.S) _engine.SetDirection(Direction.Down);
            else if (e.Key == Key.Left || e.Key == Key.A) _engine.SetDirection(Direction.Left);
            else if (e.Key == Key.Right || e.Key == Key.D) _engine.SetDirection(Direction.Right);
            else return;
            e.Handled = true; // stop the arrow keys from moving focus instead of the snake
        }

        private void MoveTick(object sender, EventArgs e)
        {
            _engine.Step();
            if (_engine.GameOver) { EndGame(); return; }

            // Speed can change (growth / power-up); only touch the timer when it actually differs.
            int delay = _engine.CurrentMoveDelayMs();
            if ((int)_moveTimer.Interval.TotalMilliseconds != delay)
                _moveTimer.Interval = TimeSpan.FromMilliseconds(delay);

            Render();
            UpdateHud();
        }

        private void ClockTick(object sender, EventArgs e)
        {
            _engine.TickSecond();
            UpdateHud();
            if (_engine.GameOver) EndGame();
        }

        private void EndGame()
        {
            if (_ended) return;
            _ended = true;
            StopTimers();
            FinalScore.Text = "Score: " + _engine.Score;
            Overlay.Visibility = Visibility.Visible;
            NameBox.Focus();
        }

        // Redraws snake, eyes, the power-up token and the apples. The grid itself is
        // the board background (see SetGridBackground), so it isn't redrawn here.
        private void Render()
        {
            Board.Children.Clear();
            double cs = BoardSize / _engine.GridSize; // size of one tile

            Brush skin = B(Main().Settings.SkinColor);
            for (int i = 0; i < _engine.Snake.Count; i++)
            {
                Cell c = _engine.Snake[i];
                Rectangle r = new Rectangle { Width = cs - 2, Height = cs - 2, RadiusX = 4, RadiusY = 4, Fill = skin };
                Canvas.SetLeft(r, c.Col * cs + 1);
                Canvas.SetTop(r, c.Row * cs + 1);
                Board.Children.Add(r);
            }
            if (_engine.Snake.Count > 0)
            {
                Cell h = _engine.Snake[0];
                double oog = cs * 0.15;
                AddDot(h.Col * cs + cs * 0.28, h.Row * cs + cs * 0.3, oog);
                AddDot(h.Col * cs + cs * 0.57, h.Row * cs + cs * 0.3, oog);
            }

            if (_engine.HasToken)
            {
                Cell p = _engine.Token;
                string kleur = _engine.TokenType == PowerUpType.Rood ? "#E5573F"
                             : _engine.TokenType == PowerUpType.Blauw ? "#4F8AD4" : "#ECC94B";
                string teken = _engine.TokenType == PowerUpType.Rood ? "SNEL"
                             : _engine.TokenType == PowerUpType.Blauw ? "x2" : "+15";
                Rectangle r = new Rectangle { Width = cs - 8, Height = cs - 8, RadiusX = 5, RadiusY = 5, Fill = B(kleur) };
                Canvas.SetLeft(r, p.Col * cs + 4);
                Canvas.SetTop(r, p.Row * cs + 4);
                Board.Children.Add(r);
                AddLabel(teken, p.Col * cs, p.Row * cs + cs * 0.32, cs, Math.Max(7, cs * 0.2), "#0A120D", true);
            }

            bool makkelijk = Main().Settings.Difficulty == Difficulty.Makkelijk;
            foreach (Apple a in _engine.Apples)
            {
                double x = a.Position.Col * cs, y = a.Position.Row * cs;

                // On Makkelijk, a dashed green ring marks the correct apple.
                if (makkelijk && a.IsCorrect)
                {
                    Ellipse ring = new Ellipse
                    {
                        Width = cs - 2,
                        Height = cs - 2,
                        Stroke = B("#39D353"),
                        StrokeThickness = 2,
                        StrokeDashArray = new DoubleCollection { 3, 2 }
                    };
                    Canvas.SetLeft(ring, x + 1);
                    Canvas.SetTop(ring, y + 1);
                    Board.Children.Add(ring);
                }

                Image appel = new Image { Width = cs - 8, Height = cs - 8, Source = AppleImage(), Stretch = Stretch.Uniform };
                Canvas.SetLeft(appel, x + 4);
                Canvas.SetTop(appel, y + 4);
                Board.Children.Add(appel);

                AddLabel(a.Label, x, y + cs * 0.33, cs, Math.Max(8, cs * 0.2), "#FFFFFF", false);
            }
        }

        private void AddDot(double x, double y, double size)
        {
            Ellipse d = new Ellipse { Width = size, Height = size, Fill = B("#0A120D") };
            Canvas.SetLeft(d, x);
            Canvas.SetTop(d, y);
            Board.Children.Add(d);
        }

        private void AddLabel(string text, double x, double top, double width, double fontSize, string hex, bool bold)
        {
            TextBlock t = new TextBlock
            {
                Text = text,
                Width = width,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = B(hex),
                FontSize = fontSize,
                FontWeight = bold ? FontWeights.Bold : FontWeights.SemiBold
            };
            Canvas.SetLeft(t, x);
            Canvas.SetTop(t, top);
            Board.Children.Add(t);
        }

        private void UpdateHud()
        {
            LivesText.Text = "Levens: " + _engine.Lives;
            ScoreText.Text = "Score: " + _engine.Score;
            int m = _engine.SecondsLeft / 60, s = _engine.SecondsLeft % 60;
            TimeText.Text = "Tijd: " + m + ":" + s.ToString("00");
            QuestionText.Text = _engine.QuestionText;

            string hint = "Besturing: pijltjes of WASD";
            if (_engine.ActivePowerUp != PowerUpType.Geen)
                hint += "   |   Power-up: " + _engine.ActivePowerUp + " (" + _engine.PowerUpSecondsLeft + "s)";
            if (Main().Settings.Difficulty == Difficulty.Makkelijk)
                hint += "   |   Makkelijk: de juiste appel licht op";
            HintText.Text = hint;
        }

        // Apple image, loaded once and frozen (it's drawn many times per frame).
        private static BitmapImage _apple;
        private static BitmapImage AppleImage()
        {
            if (_apple == null)
            {
                _apple = new BitmapImage();
                _apple.BeginInit();
                _apple.UriSource = new Uri("pack://application:,,,/Assets/apple.png");
                _apple.CacheOption = BitmapCacheOption.OnLoad;
                _apple.EndInit();
                if (_apple.CanFreeze) _apple.Freeze();
            }
            return _apple;
        }

        // Brushes are cached and frozen to cut per-frame allocation (this fixed the stutter).
        private static readonly Dictionary<string, Brush> _brushCache = new Dictionary<string, Brush>();
        private static Brush B(string hex)
        {
            Brush b;
            if (!_brushCache.TryGetValue(hex, out b))
            {
                b = (Brush)new BrushConverter().ConvertFromString(hex);
                if (b.CanFreeze) b.Freeze();
                _brushCache[hex] = b;
            }
            return b;
        }

        // Draws the grid once as a tiled, frozen background instead of rebuilding every line each frame.
        private void SetGridBackground(double cs)
        {
            Pen pen = new Pen(B("#1E6B45"), 1);
            GeometryGroup lijnen = new GeometryGroup();
            lijnen.Children.Add(new LineGeometry(new Point(0, 0), new Point(cs, 0)));
            lijnen.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, cs)));
            GeometryDrawing tekening = new GeometryDrawing(null, pen, lijnen);
            DrawingBrush brush = new DrawingBrush(tekening)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, cs, cs),
                ViewportUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None
            };
            if (brush.CanFreeze) brush.Freeze();
            Board.Background = brush;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_saved) return;
            Highscores.Add(NameBox.Text, _engine.Score);
            _saved = true;
            SavedText.Visibility = Visibility.Visible;
        }

        private void Again_Click(object sender, RoutedEventArgs e)
        {
            _engine.StartNew();
            _ended = false;
            _saved = false;
            Overlay.Visibility = Visibility.Collapsed;
            SavedText.Visibility = Visibility.Collapsed;
            _moveTimer.Interval = TimeSpan.FromMilliseconds(_engine.CurrentMoveDelayMs());
            _moveTimer.Start();
            _clockTimer.Start();
            Focus();
            Render();
            UpdateHud();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            StopTimers();
            Main().ShowMenu();
        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Windowing;
using Microsoft.UI.Dispatching;
using System;
using System.IO;
using System.Runtime.InteropServices;
using WinRT.Interop;
using Windows.Foundation;
using Windows.UI;

namespace URMS.WinUI
{
    public sealed partial class MainWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const long WS_SYSMENU = 0x00080000L;
        private const long WS_MINIMIZEBOX = 0x00020000L;
        private const long WS_MAXIMIZEBOX = 0x00010000L;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint WM_SETICON = 0x0080;
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint WM_CLOSE = 0x0010;
        private const IntPtr ICON_SMALL = 0;
        private const IntPtr ICON_BIG = (IntPtr)1;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x0010;
        private const uint LR_DEFAULTSIZE = 0x0040;
        private const int SC_CLOSE = 0xF060;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, IntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("comctl32.dll")]
        private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr SubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);

        public static MainWindow? CurrentWindow { get; private set; }

        /// <summary>Boot オーバーレイ要素を追加する</summary>
        public void AddBootOverlay(UIElement element) => BootOverlayGrid.Children.Add(element);

        /// <summary>Boot オーバーレイ要素を削除する</summary>
        public void RemoveBootOverlay(UIElement element) => BootOverlayGrid.Children.Remove(element);

        private AppWindow? _appWindow;
        private IntPtr _hwnd = IntPtr.Zero;
        private bool _uiInitialized;
        private SubclassProc? _subclassProc;

        // ── 基本タイマー ────────────────────────────
        private DispatcherQueueTimer? _clockTimer;
        private DispatcherQueueTimer? _spectrumTimer;
        private DispatcherQueueTimer? _radarTimer;
        private DispatcherQueueTimer? _statusTimer;
        private DispatcherQueueTimer? _scanlineTimer;

        // ── 背景タイマー ────────────────────────────
        private DispatcherQueueTimer? _starTimer;
        private DispatcherQueueTimer? _matrixTimer;
        private DispatcherQueueTimer? _nebulaTimer;

        // ── FPS 監視（フェーズ3） ─────────────────────
        private DateTime _lastFrameTime = DateTime.UtcNow;
        private double   _avgFrameMs    = 8.0;
        private bool     _fpsFallback   = false;

        // ── 状態 ──────────────────────────────────
        private double _radarAngle = 0;
        private double _scanlineY  = 0;
        private double _perspT     = 0;
        private double _nebulaT    = 0;
        private int    _statusPhase = 0;
        private bool   _dotVisible  = true;
        private readonly Random _rng = new();

        // ── 星空 ──────────────────────────────────
        private Ellipse[] _starEllipses = [];
        private double[]  _starA  = [];
        private double[]  _starDA = [];

        // ── パースペクティブグリッド ───────────────────
        private const int PerspCols = 22;
        private const int PerspRows = 16;
        private Line[] _perspVLines = [];
        private Line[] _perspHLines = [];

        // ── マトリクス文字 ──────────────────────────
        private TextBlock[] _matrixTb   = [];
        private double[]    _matrixDrop = [];
        private const int   MatrixFs = 11;
        private static readonly char[] MatrixChars =
            "゠01アイウエオカキクケコサシスセソ▲◆■░▒".ToCharArray();

        private static readonly string[] StatusMessages =
        [
            "SYS_NOMINAL", "ALL_CLEAR", "NET_SECURE",
            "SCAN_DONE", "MONITORING", "NODE_SYNC"
        ];

        public MainWindow()
        {
            CurrentWindow = this;
            this.InitializeComponent();
            InitializeWindowChrome();
            this.Activated += OnWindowLoaded;
            // Activated 未発火環境でも初期化を実行する
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(InitializeUiOnce);
        }

        private void InitializeWindowChrome()
        {
            try
            {
                _hwnd = WindowNative.GetWindowHandle(this);
                var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(_hwnd);
                _appWindow = AppWindow.GetFromWindowId(id);

                // 白い既定タイトルバーを透明化し、既定ボタンは不可視にする
                this.ExtendsContentIntoTitleBar = true;
                this.SetTitleBar(HeaderHost);
                var tb = _appWindow.TitleBar;
                tb.ExtendsContentIntoTitleBar = true;
                tb.PreferredHeightOption = TitleBarHeightOption.Tall;
                tb.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.InactiveBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ButtonInactiveBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ForegroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ButtonForegroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ButtonInactiveForegroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ButtonHoverBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ButtonHoverForegroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ButtonPressedBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                tb.ButtonPressedForegroundColor = Color.FromArgb(0, 0, 0, 0);
                if (_appWindow.Presenter is OverlappedPresenter presenter)
                {
                    try
                    {
                        var method = presenter.GetType().GetMethod("SetBorderAndTitleBar");
                        method?.Invoke(presenter, new object[] { false, false });
                    }
                    catch { }
                }
                ApplyTaskbarIcon(_hwnd);
                InstallSysCommandBlockHook(_hwnd);
                DisableNativeCaptionButtons(_hwnd);

                // Window状態に応じてボタン表示を切り替え
                try
                {
                    UpdateWindowStateButtons();
                }
                catch { }
            }
            catch
            {
                // API差異がある環境では既定挙動にフォールバック
            }
        }

        private void UpdateWindowStateButtons()
        {
            if (_appWindow?.Presenter == null) return;
            try
            {
                var state = ((OverlappedPresenter)_appWindow.Presenter).State;
                bool isMaximized = (state.ToString().Contains("Maximized"));
                HeaderHost.SetMaximizedState(isMaximized);
            }
            catch { }
        }

        private void InstallSysCommandBlockHook(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero || _subclassProc != null)
                return;

            _subclassProc = WindowSubclassProc;
            SetWindowSubclass(hwnd, _subclassProc, IntPtr.Zero, IntPtr.Zero);
        }

        private IntPtr WindowSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
        {
            if (uMsg == WM_SYSCOMMAND)
            {
                int cmd = (int)((long)wParam & 0xFFF0);
                if (cmd == SC_CLOSE || cmd == SC_MINIMIZE || cmd == SC_MAXIMIZE)
                    return IntPtr.Zero;
            }

            if (uMsg == WM_CLOSE)
                return IntPtr.Zero;

            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }

        private static void DisableNativeCaptionButtons(IntPtr hwnd)
        {
            var style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64();
            style &= ~(WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        private static void ApplyTaskbarIcon(IntPtr hwnd)
        {
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "images", "urms.ico");
            if (!File.Exists(iconPath))
                return;

            var hIcon = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 0, 0, LR_LOADFROMFILE | LR_DEFAULTSIZE);
            if (hIcon == IntPtr.Zero)
                return;

            SendMessage(hwnd, WM_SETICON, ICON_SMALL, hIcon);
            SendMessage(hwnd, WM_SETICON, ICON_BIG, hIcon);
        }

        private void OnKeyboardAccelerator_Q(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            MinimizeWindow();
            args.Handled = true;
        }

        private void OnKeyboardAccelerator_E(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            ToggleMaximizeWindow();
            args.Handled = true;
        }

        private void OnKeyboardAccelerator_W(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            CloseWindow();
            args.Handled = true;
        }

        public void MinimizeWindow()
        {
            if (_appWindow?.Presenter is OverlappedPresenter presenter)
                presenter.Minimize();
        }

        public void ToggleMaximizeWindow()
        {
            if (_appWindow?.Presenter is not OverlappedPresenter presenter)
                return;

            if (presenter.State == OverlappedPresenterState.Maximized)
                presenter.Restore();
            else
                presenter.Maximize();

            UpdateWindowStateButtons();
        }

        public void CloseWindow()
            => this.Close();

        private void OnWinMinClick(object sender, RoutedEventArgs e)
            => MinimizeWindow();

        private void OnWinMaxClick(object sender, RoutedEventArgs e)
            => ToggleMaximizeWindow();

        private void OnWinCloseClick(object sender, RoutedEventArgs e)
            => CloseWindow();

        private void OnHeaderMinimizeClicked(object sender, RoutedEventArgs e)
            => MinimizeWindow();

        private void OnHeaderMaximizeClicked(object sender, RoutedEventArgs e)
        {
            if (_appWindow?.Presenter is OverlappedPresenter presenter)
                presenter.Maximize();

            UpdateWindowStateButtons();
        }

        private void OnHeaderRestoreClicked(object sender, RoutedEventArgs e)
        {
            if (_appWindow?.Presenter is OverlappedPresenter presenter)
                presenter.Restore();

            UpdateWindowStateButtons();
        }

        private void OnHeaderCloseClicked(object sender, RoutedEventArgs e)
            => CloseWindow();

        private void OnHeaderDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
            => ToggleMaximizeWindow();

        private void OnWindowLoaded(object sender, WindowActivatedEventArgs e)
        {
            this.Activated -= OnWindowLoaded;

            try
            {
                this.ExtendsContentIntoTitleBar = true;
                this.SetTitleBar(HeaderHost);
                if (_appWindow?.TitleBar != null)
                {
                    _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                    _appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
                }
            }
            catch { }

            if (_hwnd != IntPtr.Zero)
                DisableNativeCaptionButtons(_hwnd);

            InitializeUiOnce();
            UpdateWindowStateButtons();
        }

        private void InitializeUiOnce()
        {
            if (_uiInitialized)
                return;
            _uiInitialized = true;

            InitSpectrumBars();
            InitPerspGrid();

            BgStarsCanvas.SizeChanged  += (_, a) => BuildStars(a.NewSize.Width, a.NewSize.Height);
            BgGridCanvas.SizeChanged   += (_, _) => DrawBgGrid();
            BgMatrixCanvas.SizeChanged += (_, a) => BuildMatrix(a.NewSize.Width, a.NewSize.Height);

            StartTimers();
            ContentFrame.Navigate(typeof(Pages.DashboardPage));
        }

        // ══════════════════════════════════════
        // スペクトラムバー (13本, ScaleYアニメ)
        // ══════════════════════════════════════
        private void InitSpectrumBars()
        {
            HeaderHost.SpectrumPanelElement.Children.Clear();
            for (int i = 0; i < 13; i++)
            {
                var st = new ScaleTransform { ScaleX = 1.0, ScaleY = 0.3 };
                var rect = new Rectangle
                {
                    Width  = 3,
                    Height = 24,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(i > 0 ? 3 : 0, 0, 0, 0),
                    Fill   = new SolidColorBrush(Color.FromArgb(128, 0, 247, 255)),
                    RenderTransformOrigin = new Point(0.5, 1.0),
                    RenderTransform = st,
                    Tag = new double[] { _rng.NextDouble() * Math.PI * 2,
                                         0.35 + _rng.NextDouble() * 0.65 }
                };
                HeaderHost.SpectrumPanelElement.Children.Add(rect);
            }
        }

        // ══════════════════════════════════════
        // 星空 (bg-stars)
        // ══════════════════════════════════════
        private void BuildStars(double w, double h)
        {
            BgStarsCanvas.Children.Clear();
            if (w <= 0) w = 1280;
            if (h <= 0) h = 720;

            int n = Math.Clamp((int)(w * h / 2600), 80, 420); // 1.46x 密度増加（フェーズ2）
            _starEllipses = new Ellipse[n];
            _starA  = new double[n];
            _starDA = new double[n];

            for (int i = 0; i < n; i++)
            {
                double r  = _rng.NextDouble() * 1.4 + 0.15;
                double a  = _rng.NextDouble();
                double da = (_rng.NextDouble() - 0.5) * 0.008;
                double x  = _rng.NextDouble() * w;
                double y  = _rng.NextDouble() * h;
                Color col = _rng.NextDouble() > 0.85
                    ? Color.FromArgb(255, 160, 220, 255)
                    : Color.FromArgb(255, 232, 244, 255);

                var el = new Ellipse
                {
                    Width  = r * 2, Height = r * 2,
                    Fill   = new SolidColorBrush(col),
                    Opacity = a, IsHitTestVisible = false
                };
                Canvas.SetLeft(el, x - r);
                Canvas.SetTop(el,  y - r);
                BgStarsCanvas.Children.Add(el);
                _starEllipses[i] = el;
                _starA[i]  = a;
                _starDA[i] = da;
            }
        }

        private void TickStars()
        {
            for (int i = 0; i < _starEllipses.Length; i++)
            {
                _starA[i] += _starDA[i];
                if (_starA[i] < 0.05) _starDA[i] =  Math.Abs(_starDA[i]);
                if (_starA[i] > 1.00) _starDA[i] = -Math.Abs(_starDA[i]);
                _starEllipses[i].Opacity = _starA[i];
            }
        }

        // ══════════════════════════════════════
        // グリッドパターン (bg-grid) — サイズ変更時一度描画
        // ══════════════════════════════════════
        private void DrawBgGrid()
        {
            BgGridCanvas.Children.Clear();
            double w = BgGridCanvas.ActualWidth;
            double h = BgGridCanvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            // 96px 大グリッド (opacity 0.025)
            var c96 = Color.FromArgb(6, 0, 247, 255);  // ~0.025*255=6
            for (double x = 0; x <= w; x += 96)
                BgGridCanvas.Children.Add(MkLine(x, 0, x, h, c96));
            for (double y = 0; y <= h; y += 96)
                BgGridCanvas.Children.Add(MkLine(0, y, w, y, c96));

            // 24px 小グリッド (opacity 0.010)
            var c24 = Color.FromArgb(3, 0, 247, 255);  // ~0.010*255=3
            for (double x = 0; x <= w; x += 24)
                BgGridCanvas.Children.Add(MkLine(x, 0, x, h, c24));
            for (double y = 0; y <= h; y += 24)
                BgGridCanvas.Children.Add(MkLine(0, y, w, y, c24));
        }

        private static Line MkLine(double x1, double y1, double x2, double y2, Color c)
            => new() { X1=x1, Y1=y1, X2=x2, Y2=y2,
                       Stroke = new SolidColorBrush(c),
                       StrokeThickness = 0.35, // 0.5px → 0.35px（フェーズ2）
                       IsHitTestVisible = false };

        // ══════════════════════════════════════
        // パースペクティブグリッド (bg-persp)
        // ══════════════════════════════════════
        private void InitPerspGrid()
        {
            BgPerspCanvas.Children.Clear();
            _perspVLines = new Line[PerspCols + 1];
            _perspHLines = new Line[PerspRows];

            for (int i = 0; i <= PerspCols; i++)
            {
                var l = new Line { StrokeThickness = 0.8, IsHitTestVisible = false };
                _perspVLines[i] = l;
                BgPerspCanvas.Children.Add(l);
            }
            for (int j = 0; j < PerspRows; j++)
            {
                var l = new Line { StrokeThickness = 0.8, IsHitTestVisible = false };
                _perspHLines[j] = l;
                BgPerspCanvas.Children.Add(l);
            }
        }

        private void TickPersp()
        {
            _perspT += 0.003;
            double W = BgPerspCanvas.ActualWidth;
            double H = BgPerspCanvas.ActualHeight;
            if (W <= 0 || H <= 0) return;

            double vx = W / 2.0;

            // 縦線
            for (int i = 0; i <= PerspCols; i++)
            {
                double bx   = (double)i / PerspCols * W;
                double dist = Math.Abs(i - PerspCols / 2.0) / PerspCols;
                double al   = Math.Max(0.008, 0.065 - dist * 0.05);
                _perspVLines[i].Stroke = new SolidColorBrush(
                    Color.FromArgb((byte)(al * 255), 0, 247, 255));
                _perspVLines[i].X1 = bx; _perspVLines[i].Y1 = H;
                _perspVLines[i].X2 = vx; _perspVLines[i].Y2 = 0;
            }

            // 横線（流れる）
            for (int j = 1; j <= PerspRows; j++)
            {
                double frac = (((double)j / PerspRows) + _perspT * 0.28) % 1.0;
                double y    = H * frac;
                double hw   = vx * frac + (W / 3.0) * (1.0 - frac);
                double al   = frac * 0.10;
                _perspHLines[j - 1].Stroke = new SolidColorBrush(
                    Color.FromArgb((byte)(al * 255), 0, 247, 255));
                _perspHLines[j - 1].X1 = vx - hw; _perspHLines[j - 1].Y1 = y;
                _perspHLines[j - 1].X2 = vx + hw; _perspHLines[j - 1].Y2 = y;
            }
        }

        // ══════════════════════════════════════
        // マトリクス文字レイン (bg-matrix)
        // ══════════════════════════════════════
        private void BuildMatrix(double w, double h)
        {
            BgMatrixCanvas.Children.Clear();
            if (w <= 0) w = 1280;
            if (h <= 0) h = 720;

            int cols = Math.Clamp((int)(w / MatrixFs), 40, 200);
            _matrixTb   = new TextBlock[cols];
            _matrixDrop = new double[cols];

            for (int i = 0; i < cols; i++)
            {
                _matrixDrop[i] = _rng.NextDouble() * -60;
                var tb = new TextBlock
                {
                    Text       = MatrixChars[_rng.Next(MatrixChars.Length)].ToString(),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize   = MatrixFs,
                    Foreground = new SolidColorBrush(
                        Color.FromArgb((byte)(128 + _rng.Next(128)), 0, 247, 255)),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(tb, i * MatrixFs);
                Canvas.SetTop(tb,  _matrixDrop[i] * MatrixFs);
                BgMatrixCanvas.Children.Add(tb);
                _matrixTb[i] = tb;
            }
        }

        private void TickMatrix()
        {
            for (int i = 0; i < _matrixTb.Length; i++)
            {
                if (_rng.NextDouble() > 0.96)
                    _matrixDrop[i] = 0;
                else
                    _matrixDrop[i] += 0.38;

                _matrixTb[i].Text = MatrixChars[_rng.Next(MatrixChars.Length)].ToString();
                Canvas.SetTop(_matrixTb[i], _matrixDrop[i] * MatrixFs);
            }
        }

        // ══════════════════════════════════════
        // タイマー起動
        // ══════════════════════════════════════
        private void StartTimers()
        {
            var q = DispatcherQueue.GetForCurrentThread();

            _clockTimer = q.CreateTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (_, _) => TickClock();
            _clockTimer.Start();
            TickClock();

            _spectrumTimer = q.CreateTimer();
            _spectrumTimer.Interval = TimeSpan.FromMilliseconds(109); // 1.1x 速く
            _spectrumTimer.Tick += (_, _) => TickSpectrum();
            _spectrumTimer.Start();

            _radarTimer = q.CreateTimer();
            _radarTimer.Interval = TimeSpan.FromMilliseconds(27); // 1.1x 速く
            _radarTimer.Tick += (_, _) => TickRadar();
            _radarTimer.Start();

            _statusTimer = q.CreateTimer();
            _statusTimer.Interval = TimeSpan.FromMilliseconds(2360); // 1.1x 速く
            _statusTimer.Tick += (_, _) => TickStatus();
            _statusTimer.Start();

            // 8ms ≈ 120fps: スキャンライン + パースペクティブグリッド（フェーズ3）
            _scanlineTimer = q.CreateTimer();
            _scanlineTimer.Interval = TimeSpan.FromMilliseconds(8);
            _scanlineTimer.Tick += (_, _) => { TickFpsMonitor(); TickScanline(); TickPersp(); };
            _scanlineTimer.Start();

            // 16ms: 星空（フェーズ3 高密度対応）
            _starTimer = q.CreateTimer();
            _starTimer.Interval = TimeSpan.FromMilliseconds(16);
            _starTimer.Tick += (_, _) => TickStars();
            _starTimer.Start();

            // 16ms: マトリクス
            _matrixTimer = q.CreateTimer();
            _matrixTimer.Interval = TimeSpan.FromMilliseconds(16);
            _matrixTimer.Tick += (_, _) => TickMatrix();
            _matrixTimer.Start();

            // 91ms: Nebulaパルス（1.1x 速く）
            _nebulaTimer = q.CreateTimer();
            _nebulaTimer.Interval = TimeSpan.FromMilliseconds(91);
            _nebulaTimer.Tick += (_, _) => TickNebula();
            _nebulaTimer.Start();
        }

        // ══════════════════════════════════════
        // UI Tick メソッド
        // ══════════════════════════════════════
        private void TickClock()
        {
            var now = DateTime.Now;
            HeaderHost.ClockTextElement.Text = now.ToString("HH:mm");
            string[] dayNames = { "日", "月", "火", "水", "木", "金", "土" };
            string dayStr = dayNames[(int)now.DayOfWeek];
            HeaderHost.DateTextElement.Text = now.ToString($"yyyy/MM/dd ({dayStr})");
        }

        private void TickSpectrum()
        {
            double t = DateTime.Now.TimeOfDay.TotalSeconds;
            foreach (UIElement child in HeaderHost.SpectrumPanelElement.Children)
            {
                if (child is Rectangle bar
                    && bar.RenderTransform is ScaleTransform st
                    && bar.Tag is double[] data)
                {
                    double sy = 0.15 + 0.85 * (0.5 + 0.5 *
                        Math.Sin(t * Math.PI * 2 / data[1] + data[0]));
                    st.ScaleY = sy;
                }
            }
        }

        private void TickRadar()
        {
            _radarAngle = (_radarAngle + 1.5) % 360;
            HeaderHost.RadarRotateElement.Angle = _radarAngle;
            double b0 = Math.Max(0, 1.0 - ((_radarAngle - 60  + 360) % 360) / 90.0);
            double b1 = Math.Max(0, 1.0 - ((_radarAngle - 200 + 360) % 360) / 90.0);
            HeaderHost.RadarBlip0Element.Opacity = b0;
            HeaderHost.RadarBlip1Element.Opacity = b1;
        }

        private void TickStatus()
        {
            _statusPhase = (_statusPhase + 1) % StatusMessages.Length;
            HeaderHost.StatusTextElement.Text = StatusMessages[_statusPhase];
            _dotVisible = !_dotVisible;
            HeaderHost.StatusDotElement.Opacity = _dotVisible ? 1.0 : 0.4;
        }

        private void TickScanline()
        {
            _scanlineY += 1.2;
            double h = BgStarsCanvas.ActualHeight; // 全画面高さ（Row0+Row1）
            if (h <= 0) h = 720;
            if (_scanlineY > h) _scanlineY = -3;
            ScanLineTranslate.Y = _scanlineY;
        }

        private void TickNebula()
        {
            // CSS nebulapulse: opacity 0.8→1.0→0.8 を 12s alternateで再現
            // Nebula Alpha +12%（フェーズ2）: 0.8→0.9 base range, max 1.0
            _nebulaT = (_nebulaT + 1.0 / 120.0) % 2.0;
            double s = Math.Sin(_nebulaT * Math.PI);
            NebulaGrid.Opacity = 0.9 + 0.1 * s; // 底上げ +10%
        }

        // ─────────────────────────────────────────────
        // FPS 監視（フェーズ3）: 高負荷時 120→60fps フォールバック
        // ─────────────────────────────────────────────
        private void TickFpsMonitor()
        {
            var now = DateTime.UtcNow;
            double ms = (now - _lastFrameTime).TotalMilliseconds;
            _lastFrameTime = now;

            // 指数移動平均でフレーム時間を平滑化
            _avgFrameMs = 0.9 * _avgFrameMs + 0.1 * ms;

            if (!_fpsFallback && _avgFrameMs > 13.0)
            {
                // 13ms 超過 → 16ms (60fps) にフォールバック
                _fpsFallback = true;
                _scanlineTimer!.Interval = TimeSpan.FromMilliseconds(16);
            }
            else if (_fpsFallback && _avgFrameMs < 9.0)
            {
                // 9ms 未満 → 120fps に復帰
                _fpsFallback = false;
                _scanlineTimer!.Interval = TimeSpan.FromMilliseconds(8);
            }
        }
    }
}

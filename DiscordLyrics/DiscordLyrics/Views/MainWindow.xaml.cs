using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DiscordLyrics.Services;
using DiscordLyrics.ViewModels;

namespace DiscordLyrics.Views;

public partial class MainWindow : Window
{
    private readonly INotificationService _notify;
    private readonly ISettingsService _settings;
    private readonly IStatusEngine _engine;
    private readonly IDiscordService _discord;

    public MainWindow(
        MainViewModel vm, INotificationService notify, ISettingsService settings,
        IStatusEngine engine, IDiscordService discord)
    {
        InitializeComponent();
        DataContext = vm;
        _notify = notify;
        _settings = settings;
        _engine = engine;
        _discord = discord;

        _notify.Shown += OnNotification;
        _engine.Updated += OnEngineUpdated;
        _discord.StateChanged += OnEngineUpdated;

        Loaded += OnLoaded;
        StateChanged += (_, _) =>
            MaxButton.Content = WindowState == WindowState.Maximized ? "" : "";
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _settings.LoadAsync();
        await _discord.ValidateAsync();

        if (_settings.Current.AutoStartEngine && _settings.HasToken)
            await _engine.StartAsync();

        UpdateFooter();
    }

    private void OnEngineUpdated() => Dispatcher.Invoke(UpdateFooter);

    private void UpdateFooter()
    {
        var live = _engine.Running;
        StatusDot.Fill = (Brush)FindResource(live ? "Brush.Success" : "Brush.Graphite");
        var account = _discord.Account?.Display;
        FooterText.Text = live
            ? (account is null ? "Live" : $"Live · {account}")
            : (_discord.Connected ? $"Idle · {account}" : "Not connected");
    }

    // ---------------- Caption buttons ----------------
    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnMaximizeRestore(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private async void OnClose(object sender, RoutedEventArgs e)
    {
        try { await _engine.StopAsync(); } catch { /* ignore */ }
        Close();
    }

    // ---------------- Toast notifications ----------------
    private void OnNotification(Notification n) => Dispatcher.Invoke(() => ShowToast(n));

    private void ShowToast(Notification n)
    {
        var accent = n.Kind switch
        {
            NotificationKind.Success => "Brush.Success",
            NotificationKind.Warning => "Brush.Warning",
            NotificationKind.Error => "Brush.Danger",
            _ => "Brush.Red",
        };

        var stripe = new Border
        {
            Width = 4,
            CornerRadius = new CornerRadius(2),
            Background = (Brush)FindResource(accent),
            Margin = new Thickness(0, 0, 12, 0),
        };

        var text = new TextBlock
        {
            Text = n.Message,
            TextWrapping = TextWrapping.Wrap,
            Foreground = (Brush)FindResource("Brush.TextPrimary"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            MaxWidth = 320,
        };

        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(stripe);
        panel.Children.Add(text);

        var card = new Border
        {
            Background = (Brush)FindResource("Brush.Card"),
            BorderBrush = (Brush)FindResource("Brush.Border"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16, 12, 18, 12),
            Margin = new Thickness(0, 8, 0, 0),
            Child = panel,
            Opacity = 0,
            RenderTransform = new TranslateTransform(0, 12),
        };

        ToastHost.Children.Add(card);

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        var slideIn = new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(220))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        card.BeginAnimation(OpacityProperty, fadeIn);
        ((TranslateTransform)card.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideIn);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.4) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (_, _) => ToastHost.Children.Remove(card);
            card.BeginAnimation(OpacityProperty, fadeOut);
        };
        timer.Start();
    }
}

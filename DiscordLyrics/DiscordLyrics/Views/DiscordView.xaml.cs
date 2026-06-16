using System.Windows;
using System.Windows.Controls;
using DiscordLyrics.ViewModels;

namespace DiscordLyrics.Views;

public partial class DiscordView : UserControl
{
    public DiscordView() => InitializeComponent();

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is DiscordViewModel vm && sender is PasswordBox box)
            vm.Token = box.Password;
    }
}

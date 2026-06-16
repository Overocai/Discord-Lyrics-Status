using CommunityToolkit.Mvvm.ComponentModel;

namespace DiscordLyrics.Infrastructure.Navigation;

/// <summary>Switches the active page inside the shell (view-model first navigation).</summary>
public interface INavigationService
{
    ObservableObject? Current { get; }
    event Action? CurrentChanged;
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
}

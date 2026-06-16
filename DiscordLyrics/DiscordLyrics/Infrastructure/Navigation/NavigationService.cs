using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordLyrics.Infrastructure.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;

    public NavigationService(IServiceProvider provider) => _provider = provider;

    public ObservableObject? Current { get; private set; }

    public event Action? CurrentChanged;

    public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
    {
        var vm = _provider.GetRequiredService<TViewModel>();
        if (ReferenceEquals(vm, Current)) return;

        Current = vm;
        (vm as IActivatable)?.OnActivated();
        CurrentChanged?.Invoke();
    }
}

/// <summary>Optional hook so a page can refresh its data when shown.</summary>
public interface IActivatable
{
    void OnActivated();
}

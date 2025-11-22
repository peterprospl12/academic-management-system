using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Base;

public abstract class BaseView : View
{
    protected readonly IServiceProvider ServiceProvider;

    protected BaseView(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        X = 0;
        Y = 0;
        Width = Dim.Fill();
        Height = Dim.Fill();
    }

    protected void ExecuteServiceAction<TService>(Action<TService> action) where TService : notnull
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        action(service);
    }

    protected TResult ExecuteServiceFunc<TService, TResult>(Func<TService, TResult> func) where TService : notnull
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return func(service);
    }
}
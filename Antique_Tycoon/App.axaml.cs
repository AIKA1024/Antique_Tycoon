using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Avalonia.Markup.Xaml;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.Views;
using Antique_Tycoon.Views.Windows;
using Avalonia.Controls;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon;

public partial class App : Application
{
  public const int DefaultPort = 13437;

  public IServiceProvider Services { get; private set; }
  public new static App Current => (App)Application.Current!;
  public string MapPath { get; } = Path.Join("..", "Maps");
  public string MapArchiveFilePath { get; } = Path.Join("..", "MapArchiveFile");

  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  public App()
  {
    Services = ConfigureServices();
    Directory.CreateDirectory(MapPath);
    Directory.CreateDirectory(MapArchiveFilePath);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
      // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
      DisableAvaloniaDataAnnotationValidation();
      desktop.MainWindow = Services.GetRequiredService<MainWindow>();
    }

    base.OnFrameworkInitializationCompleted();
  }

  private IServiceProvider ConfigureServices()
  {
    var services = new ServiceCollection();
    services.AddSingleton<MainWindow>(sp => new MainWindow
      { DataContext = sp.GetRequiredService<MainWindowViewModel>() });
    services.AddSingleton(new LibVLC("--no-video"));
    services.AddSingleton<MainWindowViewModel>();
    services.AddSingleton<NavigationService>(sp => new NavigationService(sp.GetRequiredService<MainWindowViewModel>()));
    services.AddSingleton<MapFileService>();
    services.AddSingleton<DialogService>();
    services.AddSingleton<NetClient>();
    services.AddSingleton<NetServer>();
    services.AddSingleton(new Player { IsHomeowner = true });
    services.AddSingleton(sp => TopLevel.GetTopLevel(sp.GetRequiredService<MainWindow>())!);
    return services.BuildServiceProvider();
  }

  private void DisableAvaloniaDataAnnotationValidation()
  {
    // Get an array of plugins to remove
    var dataValidationPluginsToRemove =
      BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

    // remove each entry found
    foreach (var plugin in dataValidationPluginsToRemove)
    {
      BindingPlugins.DataValidators.Remove(plugin);
    }
  }
}
using System;
using System.Globalization;
using Antique_Tycoon.Services;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Converters;

public class OwnerToBackgroundConverter : MarkupExtension, IValueConverter
{
    private readonly GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? uuid = value as string;
        if (uuid == _gameManager.LocalPlayer.Uuid)
            return (SolidColorBrush)App.Current.FindResource("MyEstateBackground");
        if (string.IsNullOrEmpty(uuid))
            return (SolidColorBrush)App.Current.FindResource("UnownedEstateBackground");
        return (SolidColorBrush)App.Current.FindResource("OpponentEstateBackground");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
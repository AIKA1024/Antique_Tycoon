using System;
using System.Globalization;
using Antique_Tycoon.Models;
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
        var player = value as Player;
        if (player == _gameManager.LocalPlayer)
            return (SolidColorBrush)App.Current.FindResource("MyEstateBrush");
        if (player == null)
            return (SolidColorBrush)App.Current.FindResource("UnownedEstateBrush");
        return (SolidColorBrush)App.Current.FindResource("OpponentEstateBrush");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
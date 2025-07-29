using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

public abstract partial class CanvasEntity : ObservableObject
{
  [ObservableProperty] private double _left;
  [ObservableProperty] private double _top;
  [ObservableProperty] private string _title;
  [ObservableProperty] private double _width = 120;
  [ObservableProperty] private double _height = 150;
  [ObservableProperty] private Bitmap _cover = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Avatars/Minecraft.png")));
  [ObservableProperty] private IBrush _background = new SolidColorBrush(Color.Parse("#eccc68"));
}
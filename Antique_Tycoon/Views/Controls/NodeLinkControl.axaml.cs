using System;
using System.Linq;
using Antique_Tycoon.Behaviors;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;


namespace Antique_Tycoon.Views.Controls;

public partial class NodeLinkControl : ContentControl
{
  private bool _isRegisted;
  private GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();
  private AnimationManager _animationManager = App.Current.Services.GetRequiredService<AnimationManager>();
  
  [GeneratedDirectProperty] public partial NodeModel NodeModel { get; set; }
  [GeneratedDirectProperty] public partial Map Map { get; set; }
  [GeneratedDirectProperty] public partial Panel LineCanvas { get; set; }

  public NodeLinkControl()
  {
    this.GetObservable(BoundsProperty).Subscribe(rect =>
    {
      if (NodeModel == null)
        return;
      NodeModel.WidthDisplayText = rect.Width.ToString();
      NodeModel.HeightDisplayText = rect.Height.ToString();
    });
  }

  protected override void OnDataContextChanged(EventArgs e)
  {
    base.OnDataContextChanged(e);

    if (DataContext is NodeModel nodeModel && !_isRegisted)
    {
      WeakReferenceMessenger.Default.Register<AntiqueChanceResponse,string>(this,nodeModel.Uuid,ReceiveAntiqueChanceResponse);
      _isRegisted = true;
    }
  }

  private async void ReceiveAntiqueChanceResponse(object sender, AntiqueChanceResponse message)
  {
    var antique = _gameManager.SelectedMap.Antiques.FirstOrDefault(a => a.Uuid == message.AntiqueUuid);
    await _animationManager.WaitAnimation(message.AnimationUuid);
    AdornerHost.SetAdornerContent(this, antique);
  }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Behaviors;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using Antique_Tycoon.Utilities;
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

  private readonly ActionQueueService _actionQueueService =
    App.Current.Services.GetRequiredService<ActionQueueService>();

  [GeneratedDirectProperty] public partial NodeModel? NodeModel { get; set; }
  [GeneratedDirectProperty] public partial Map? Map { get; set; }
  [GeneratedDirectProperty] public partial Panel? LineCanvas { get; set; }

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

    if (DataContext is NodeModel && !_isRegisted)
    {
      WeakReferenceMessenger.Default.Register<AntiqueChanceResponse>(this, ReceiveAntiqueChanceResponse);
      WeakReferenceMessenger.Default.Register<GetAntiqueResultResponse>(this, ReceiveGetAntiqueResultResponse);
      _isRegisted = true;
    }
  }

  private void ReceiveAntiqueChanceResponse(object sender, AntiqueChanceResponse message)
  {
    if (DataContext is NodeModel nodeModel && message.MineUuid != nodeModel.Uuid)
      return;

    _actionQueueService.Enqueue(() =>
    {
      var antique = _gameManager.SelectedMap.Antiques.FirstOrDefault(a => a.Uuid == message.AntiqueUuid);
      NodeAdornerHost.SetContent(this, antique);
      return Task.CompletedTask;
    });
  }

  private void ReceiveGetAntiqueResultResponse(object recipient, GetAntiqueResultResponse message)
  {
    if (DataContext is NodeModel nodeModel && message.MineUuid != nodeModel.Uuid)
      return;
    
    // _actionQueueService.Enqueue(() =>
    // {
      NodeAdornerHost.SetContent(this, null); //收起装饰器
      // return Task.CompletedTask;
    // });
  }
}
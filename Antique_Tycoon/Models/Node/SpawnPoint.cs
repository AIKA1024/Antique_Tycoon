using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Models.Node;

public partial class SpawnPoint : NodeModel
{
  public int Bonus
  {
    get;
    set => SetProperty(ref field, value);
  } = 2000;
}
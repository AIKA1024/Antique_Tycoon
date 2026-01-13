using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Entities;

public abstract class EntityBase:ObservableObject
{
  public string Uuid { get; set; } = Guid.NewGuid().ToString();
  public string Name { get; set; } = "";
}
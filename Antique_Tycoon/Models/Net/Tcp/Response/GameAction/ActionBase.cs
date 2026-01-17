using System;
using System.Text.Json.Serialization;

namespace Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BuyEstateAction), "BuyEstateAction")]
[JsonDerivedType(typeof(DrawCardAction), "DrawCardAction")]
[JsonDerivedType(typeof(RollDiceAction), "RollDiceAction")]
[JsonDerivedType(typeof(TransportAction), "TransportAction")]
[JsonDerivedType(typeof(SaleAntiqueAction), "SaleAntiqueAction")]

public abstract class ActionBase : ServiceRequest
{

}
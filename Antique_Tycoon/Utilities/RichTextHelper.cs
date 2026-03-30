using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Utilities;

public class RichTextHelper
{
  private static readonly GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();
  
  // 1. 定义附加属性 "LogSegments"
  public static readonly AttachedProperty<IEnumerable<LogSegment>> LogSegmentsProperty =
    AvaloniaProperty.RegisterAttached<RichTextHelper, TextBlock, IEnumerable<LogSegment>>(
      "LogSegments");

  public static IEnumerable<LogSegment> GetLogSegments(TextBlock element) =>
    element.GetValue(LogSegmentsProperty);

  public static void SetLogSegments(TextBlock element, IEnumerable<LogSegment> value) =>
    element.SetValue(LogSegmentsProperty, value);

  // 2. 定义附加属性 "Command"，用于点击回调
  public static readonly AttachedProperty<ICommand> CommandProperty =
    AvaloniaProperty.RegisterAttached<RichTextHelper, TextBlock, ICommand>(
      "Command");

  public static ICommand GetCommand(TextBlock element) => element.GetValue(CommandProperty);

  public static void SetCommand(TextBlock element, ICommand value) =>
    element.SetValue(CommandProperty, value);


  // 3. 属性变化时的处理逻辑
  static RichTextHelper()
  {
    LogSegmentsProperty.Changed.AddClassHandler<TextBlock>((textBlock, args) =>
    {
      RenderSegments(textBlock, args.NewValue as List<LogSegment>);
    });
  }

  private static void RenderSegments(TextBlock textBlock, List<LogSegment>? segments)
  {
    textBlock.Inlines?.Clear();

    if (segments == null || segments.Count == 0) return;

    var command = GetCommand(textBlock);

    foreach (var segment in segments)
    {
      if (segment.Type == InteractionType.None)
      {
        // 普通文本 -> Run
        textBlock.Inlines!.Add(new Run { Text = segment.Text });
      }
      else
      {
        // 交互文本 -> InlineUIContainer 包裹 HyperlinkButton
        string displayText = segment.Text;
        switch (segment.Type)
        {
          case InteractionType.PlayerName:
            displayText = _gameManager.GetPlayerByUuid(segment.Data).Name;
            break;
          case InteractionType.Location:
            break;
          case InteractionType.Antique:
            displayText = _gameManager.SelectedMap.Antiques.First(a=>a.Uuid == segment.Data).Name;
            break;
          case InteractionType.Estate:
            displayText = ((Estate)_gameManager.SelectedMap.EntitiesDict[segment.Data]).Title;
            break;
          case InteractionType.Staff:
            displayText = _gameManager.SelectedMap.Staffs.First(a => a.Uuid == segment.Data).Name;
            break;
          case InteractionType.Money:
            break;
          default:
            break;
        }

        var linkBtn = new HyperlinkButton
        {
          Content = displayText,
          Command = command,
          CommandParameter = segment,

          // 关键样式设置：去除按钮的所有默认外观
          Padding = new Thickness(0),
          Margin = new Thickness(0),
          Background = Brushes.Transparent,
          BorderThickness = new Thickness(0),
          RenderTransform = new TranslateTransform(0, 2),

          // 文字颜色
          Foreground = Brushes.SkyBlue,

          // 鼠标光标
          Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),

          // 垂直对齐，确保文字不会忽上忽下
          MinHeight = 0, 
          Height = double.NaN, // Auto
          VerticalAlignment = VerticalAlignment.Center
        };

        // 这里有个小坑：HyperlinkButton 默认可能有下划线，
        // 如果你想去掉，可以在这里设置 TextDecorations = null
        // linkBtn.Styles.Add(...); // 如果需要更复杂的样式覆盖

        // 放入容器
        var container = new InlineUIContainer
        {
          Child = linkBtn,
          // 这一步至关重要：让按钮的基线和普通文本对齐
          BaselineAlignment = BaselineAlignment.Center,
        };

        textBlock.Inlines!.Add(container);
      }
    }
  }
}
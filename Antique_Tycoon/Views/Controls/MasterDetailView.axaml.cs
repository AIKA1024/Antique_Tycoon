using System.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class MasterDetailView : TemplatedControl
{
    [GeneratedDirectProperty] public partial object? SelectedItem { get; set; }
    [GeneratedDirectProperty] public partial IList? ItemsSource { get; set; }
    [GeneratedDirectProperty] public partial IDataTemplate? ListBoxItemTemplate { get; set; }
    [GeneratedDirectProperty] public partial ControlTheme? ItemContainerTheme { get; set; }
    
    [GeneratedDirectProperty] public partial DataTemplates? DetailDataTemplates { get; set; }

    [GeneratedDirectProperty] public partial Control? DetailFoot { get; set; }
    [GeneratedDirectProperty] public partial Control? DetailContent { get; set; }
}
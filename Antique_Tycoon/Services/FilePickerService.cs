using System.Collections.Generic;
using System.Threading.Tasks;
using Antique_Tycoon.Views.Windows;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Antique_Tycoon.Services;

public class FilePickerService(MainWindow mainWindow)
{
  public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)=>TopLevel.GetTopLevel(mainWindow).StorageProvider.OpenFilePickerAsync(options);
}
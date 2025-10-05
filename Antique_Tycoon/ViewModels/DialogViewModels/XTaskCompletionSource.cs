using System.Threading.Tasks;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public class XTaskCompletionSource : TaskCompletionSource, ISupportsClearResult
{
  public void ClearResult() => SetResult();
}

public class XTaskCompletionSource<T> : TaskCompletionSource<T?>, ISupportsClearResult
{
  public void ClearResult() => SetResult(default);
}
using System.Collections.Generic;

namespace Antique_Tycoon.Models.Net.Tcp;

public interface IHistoryRecord
{
  List<LogSegment> GetLogSegments();
  IEnumerable<LogSegment> Segments => GetLogSegments();
}
using System;
using System.Collections.Generic;
using System.Linq;
using Antique_Tycoon.Models;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;

namespace Antique_Tycoon.Extensions;

public static class MapExtension
{
  /// <summary>
/// 获取从起点出发、恰好走targetStepCount步可达的所有路径（键：终点Uuid，值：路径Uuid列表）
/// </summary>
/// <param name="map">地图实例</param>
/// <param name="startNodeUuid">起点节点Uuid</param>
/// <param name="targetStepCount">目标步数（≥1）</param>
/// <returns>键=终点Uuid，值=从起点到该终点的完整路径（由节点Uuid组成）</returns>
public static Dictionary<string, List<string>> GetPathsAtExactStep(this Map map, string startNodeUuid, int targetStepCount)
{
    if (map == null) throw new ArgumentNullException(nameof(map));
    if (string.IsNullOrWhiteSpace(startNodeUuid)) throw new ArgumentNullException(nameof(startNodeUuid));
    if (targetStepCount < 1) throw new ArgumentOutOfRangeException(nameof(targetStepCount), "步数必须 >= 1");
    if (!map.EntitiesDict.TryGetValue(startNodeUuid, out var startNodeEntity)) return [];

    var startNode = (NodeModel)startNodeEntity;

    // 核心修改1：字典值类型改为 List<string>（存储节点Uuid）
    var uniqueEndPaths = new Dictionary<string, List<string>>();

    // 核心修改2：当前路径改为存储Uuid，而非NodeModel
    var currentPathUuids = new List<string> { startNode.Uuid };

    // 调用修改后的递归方法
    FindPathsRecursive(map, startNode, targetStepCount, 0, currentPathUuids, uniqueEndPaths);

    return uniqueEndPaths;
}

/// <summary>
/// 递归查找路径（修改为跟踪Uuid路径）
/// </summary>
private static void FindPathsRecursive(
    Map map,
    NodeModel currentNode,
    int targetStep,
    int currentStep,
    List<string> currentPathUuids, // 参数改为 List<string>（存储Uuid）
    Dictionary<string, List<string>> uniqueEndPaths)
{
    if (currentStep == targetStep)
    {
        // 达到目标步数时，存储终点Uuid和对应的路径Uuid列表
        if (!uniqueEndPaths.ContainsKey(currentNode.Uuid))
        {
            // 复制当前路径（避免后续递归修改影响已存储的路径）
            uniqueEndPaths[currentNode.Uuid] = new List<string>(currentPathUuids);
        }
        return;
    }

    foreach (var connector in currentNode.ConnectorModels)
    {
        foreach (var activeConn in connector.ActiveConnections)
        {
            if (string.IsNullOrWhiteSpace(activeConn.EndNodeId)) continue;
            if (!map.EntitiesDict.TryGetValue(activeConn.EndNodeId, out var targetEntity)) continue;

            var targetNode = (NodeModel)targetEntity;

            // 防环检测：检查Uuid是否已在路径中（替代原NodeModel的Contains）
            // if (currentPathUuids.Contains(targetNode.Uuid)) continue;

            // 添加目标节点的Uuid到路径
            currentPathUuids.Add(targetNode.Uuid);
            // 递归调用（步数+1）
            FindPathsRecursive(map, targetNode, targetStep, currentStep + 1, currentPathUuids, uniqueEndPaths);
            // 回溯：移除最后添加的Uuid
            currentPathUuids.RemoveAt(currentPathUuids.Count - 1);
        }
    }
}
}
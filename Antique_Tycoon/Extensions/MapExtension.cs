using System;
using System.Collections.Generic;
using System.Linq;
using Antique_Tycoon.Models;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;

namespace Antique_Tycoon.Extensions;

public static class MapExtension
{
  /// <summary>
/// 获取从起点选择路径出发、恰好走targetStepCount步可达的所有路径（键：终点Uuid，值：路径Uuid列表）
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
    if (!map.EntitiesDict.TryGetValue(startNodeUuid, out var startNodeEntity)) return new Dictionary<string, List<string>>();

    var startNode = (NodeModel)startNodeEntity;
    var uniqueEndPaths = new Dictionary<string, List<string>>();
    var currentPathUuids = new List<string> { startNode.Uuid };

    FindPathsRecursive(map, startNode, targetStepCount, 0, currentPathUuids, uniqueEndPaths);

    return uniqueEndPaths;
}

/// <summary>
/// 递归查找路径
/// </summary>
private static void FindPathsRecursive(
    Map map,
    NodeModel currentNode,
    int targetStep,
    int currentStep,
    List<string> currentPathUuids,
    Dictionary<string, List<string>> uniqueEndPaths)
{
    if (currentStep == targetStep)
    {
        if (!uniqueEndPaths.ContainsKey(currentNode.Uuid))
        {
            uniqueEndPaths[currentNode.Uuid] = new List<string>(currentPathUuids);
        }
        return;
    }

    // 1. 收集当前节点所有有效的、可通行的目标节点
    var validNextNodes = new List<NodeModel>();
    foreach (var connector in currentNode.ConnectorModels)
    {
        foreach (var activeConn in connector.ActiveConnections)
        {
            if (!string.IsNullOrWhiteSpace(activeConn.EndNodeId) && 
                map.EntitiesDict.TryGetValue(activeConn.EndNodeId, out var targetEntity))
            {
                validNextNodes.Add((NodeModel)targetEntity);
            }
        }
    }

    if (validNextNodes.Count == 0) return; // 死胡同，直接返回

    // 2. 核心修改：应用 PathPriority 筛选规则
    IEnumerable<NodeModel> nodesToExplore;
    if (currentStep == 0)
    {
        // 第 0 步：允许探索所有分支
        nodesToExplore = validNextNodes;
    }
    else
    {
        // 第 >0 步：按优先级降序排列，只取优先级最高的 1 个节点
        // （假设 PathPriority 是数值类型，值越大优先级越高）
        nodesToExplore = validNextNodes.OrderByDescending(n => n.PathPriority).Take(1);
    }

    // 3. 递归探索
    foreach (var targetNode in nodesToExplore)
    {
        // 防环检测（如果你的地图可能存在回路，强烈建议取消这行注释）
        // if (currentPathUuids.Contains(targetNode.Uuid)) continue;

        currentPathUuids.Add(targetNode.Uuid);
        FindPathsRecursive(map, targetNode, targetStep, currentStep + 1, currentPathUuids, uniqueEndPaths);
        currentPathUuids.RemoveAt(currentPathUuids.Count - 1); // 回溯
    }
}
}
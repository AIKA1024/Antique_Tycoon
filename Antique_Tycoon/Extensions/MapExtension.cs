using System;
using System.Collections.Generic;
using System.Linq;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Node;

namespace Antique_Tycoon.Extensions;

public static class MapExtension
{
    /// <summary>
/// 从起始节点出发，沿主动连接的有向路径前进指定步数，返回所有达到该步数的路径
/// </summary>
/// <param name="map">地图</param>
/// <param name="startNodeUuid">起始节点UuId</param>
/// <param name="targetStepCount">目标步数（≥1）</param>
/// <returns>路径集合，每条路径包含从起点到终点的所有节点序列</returns>
public static List<List<NodeModel>> GetPathsAtExactStep(this Map map, string startNodeUuid, int targetStepCount)
{
    if (map == null) throw new ArgumentNullException(nameof(map));
    if (string.IsNullOrWhiteSpace(startNodeUuid)) throw new ArgumentNullException(nameof(startNodeUuid));
    if (targetStepCount < 1) throw new ArgumentOutOfRangeException(nameof(targetStepCount), "步数必须 >= 1");
    if (!map.EntitiesDict.TryGetValue(startNodeUuid, out var startNodeEntity)) return [];

    var startNode = (NodeModel)startNodeEntity;
    var allPaths = new List<List<NodeModel>>();
    
    // 初始路径只包含起点
    var currentPath = new List<NodeModel> { startNode };
    
    FindPathsRecursive(map, startNode, targetStepCount, 0, currentPath, allPaths);
    
    return allPaths;
}

private static void FindPathsRecursive(
    Map map, 
    NodeModel currentNode, 
    int targetStep, 
    int currentStep, 
    List<NodeModel> currentPath, 
    List<List<NodeModel>> allPaths)
{
    // 达到目标步数，将当前路径副本加入结果集
    if (currentStep == targetStep)
    {
        allPaths.Add(new List<NodeModel>(currentPath));
        return;
    }

    // 遍历当前节点的所有连接器和主动连接
    foreach (var connector in currentNode.ConnectorModels)
    {
        foreach (var activeConn in connector.ActiveConnections)
        {
            if (string.IsNullOrWhiteSpace(activeConn.EndNodeId)) continue;
            if (!map.EntitiesDict.TryGetValue(activeConn.EndNodeId, out var targetEntity)) continue;

            var targetNode = (NodeModel)targetEntity;

            // 【防环检测】：如果在当前路径中已经走过该节点，则跳过（避免死循环）
            // 如果你的业务允许路径中出现重复节点（非简单路径），可以移除此判断
            if (currentPath.Contains(targetNode)) continue;

            // 递归前：加入路径
            currentPath.Add(targetNode);
            
            FindPathsRecursive(map, targetNode, targetStep, currentStep + 1, currentPath, allPaths);
            
            // 递归后：回溯（Backtracking），移除最后一个节点以尝试其他分支
            currentPath.RemoveAt(currentPath.Count - 1);
        }
    }
}
}
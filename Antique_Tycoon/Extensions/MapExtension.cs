using System;
using System.Collections.Generic;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Node;

namespace Antique_Tycoon.Extensions;

public static class MapExtension
{
     /// <summary>
    /// 从起始节点出发，沿主动连接的有向路径前进指定步数，仅返回恰好该步数可达的节点（去重）
    /// </summary>
    /// <param name="map">地图（不能为 null）</param>
    /// <param name="startNodeUuid">起始节点UuId（不能为 null）</param>
    /// <param name="targetStepCount">目标步数（≥1）</param>
    /// <returns>恰好目标步数可达的节点集合（空集合代表无对应节点）</returns>
    /// <exception cref="ArgumentNullException">起始节点/全局节点集合为 null 时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">目标步数小于 1 时抛出</exception>
    /// <exception cref="InvalidOperationException">全局节点集合中存在重复 UUID 时抛出</exception>
    public static IEnumerable<NodeModel> GetNodesAtExactStepViaActiveConnections(this Map map,string startNodeUuid, int targetStepCount)
    {
        var startNode = (NodeModel)map.EntitiesDict[startNodeUuid];
        if (targetStepCount < 1)
            throw new ArgumentOutOfRangeException(nameof(targetStepCount), "目标步数必须为正整数");

        // 已访问节点集合：用 Uuid 判重，避免循环和重复遍历
        var visited = new HashSet<string> { startNode.Uuid };
        // BFS 队列：存储「当前节点 + 已走步数」
        var queue = new Queue<(NodeModel Node, int CurrentStep)>();
        queue.Enqueue((startNode, 0));

        // 结果集：仅存储恰好目标步数可达的节点（自动去重）
        var result = new HashSet<NodeModel>();

        while (queue.Count > 0)
        {
            var (currentNode, currentStep) = queue.Dequeue();

            // 恰好达到目标步数，加入结果集（不继续遍历后续路径）
            if (currentStep == targetStepCount)
            {
                result.Add(currentNode);
                continue;
            }

            // 步数未达目标，继续遍历下一级主动连接
            if (currentStep > targetStepCount)
                continue;

            // 遍历当前节点的所有连接器
            foreach (var connector in currentNode.ConnectorModels)
            {

                // 遍历每个连接器的所有主动连接
                foreach (var activeConn in connector.ActiveConnections)
                {
                    // 跳过无效连接（TargetNodeUuid 为空/空白）
                    if (string.IsNullOrWhiteSpace(activeConn.EndNodeId))
                        continue;

                    // 通过 UUID 查找目标节点（核心适配逻辑）
                    if (!map.EntitiesDict.TryGetValue(activeConn.EndNodeId, out var targetNode))
                    {
                        // 可选：记录日志（如“未找到 UUID 为 XXX 的节点”），不抛出异常（避免单个无效连接影响整体）
                        Console.WriteLine($"警告：未找到目标节点 UUID={activeConn.EndNodeId}，跳过该连接");
                        continue;
                    }

                    // 跳过已访问的节点（避免循环和重复）
                    if (!visited.Add(targetNode.Uuid))
                        continue;

                    // 标记为已访问，加入队列（步数+1）
                    queue.Enqueue(((NodeModel)targetNode, currentStep + 1));
                }
            }
        }

        return result;
    }
}
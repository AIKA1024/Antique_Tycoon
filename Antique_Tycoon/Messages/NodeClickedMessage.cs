namespace Antique_Tycoon.Messages;

public class NodeClickedMessage(string nodeUuid)
{
    public string NodeUuid { get; set; } = nodeUuid;
}
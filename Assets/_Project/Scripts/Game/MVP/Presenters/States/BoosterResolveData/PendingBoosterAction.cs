using Core.Config;
using UnityEngine;

public struct PendingBoosterAction
{
    public int GroupId { get; private set; }
    public Vector2Int OriginCoord { get; private set; }
    public BoosterActionBase BoosterAction { get; private set; }
    
    public PendingBoosterAction(Vector2Int originCoord, BoosterActionBase boosterAction)
    {
        GroupId = 0;
        OriginCoord = originCoord;
        BoosterAction = boosterAction;
    }

    public PendingBoosterAction(int groupId, Vector2Int originCoord, BoosterActionBase boosterAction)
    {
        GroupId = groupId;
        OriginCoord = originCoord;
        BoosterAction = boosterAction;
    }
    
    public void SetGroupId(int groupId) => GroupId = groupId;
}
using Core.Config;
using UnityEngine;

public struct BoosterActionContext
{
    public int GroupId { get; private set; }
    public Vector2Int OriginCoord { get; private set; }
    public BoosterActionBase BoosterAction { get; private set; }
    
    public BoosterActionContext(Vector2Int originCoord, BoosterActionBase boosterAction)
    {
        GroupId = 0;
        OriginCoord = originCoord;
        BoosterAction = boosterAction;
    }

    public BoosterActionContext(int groupId, Vector2Int originCoord, BoosterActionBase boosterAction)
    {
        GroupId = groupId;
        OriginCoord = originCoord;
        BoosterAction = boosterAction;
    }
    
    public void SetGroupId(int groupId) => GroupId = groupId;
}
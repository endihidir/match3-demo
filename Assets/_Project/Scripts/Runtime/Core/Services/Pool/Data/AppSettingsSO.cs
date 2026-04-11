using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "AppSettings", menuName = "Game/App/AppSettings")]
    public sealed class AppSettingsSO : ScriptableObject
    {
        [field: SerializeField] public int TargetFrameRate { get; private set; } = 60;
        [field: SerializeField] public bool IsMultitouchEnabled {get; private set;} = false; 
        [field: SerializeField] public int TweenCapacity { get; private set; } = 2000;
        [field: SerializeField] public int SequenceCapacity { get; private set; } = 500;
    }
}
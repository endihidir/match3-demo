using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class PlayButtonUI : MonoBehaviour
    {
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public TextMeshProUGUI Label { get; private set; }
    }
}

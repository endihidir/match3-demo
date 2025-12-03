using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class ProgressBarUI : MonoBehaviour
    {
        [field: SerializeField] public Image SliderImage { get; private set; }
        [field: SerializeField] public TextMeshProUGUI SliderTxt { get; private set; }
        [field: SerializeField] public TextMeshProUGUI PercentageTxt { get; private set; }
    }
}

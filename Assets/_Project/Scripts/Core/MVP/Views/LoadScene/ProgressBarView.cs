using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Views
{
    public class ProgressBarView : MonoBehaviour
    {
        [field: SerializeField] private Image SliderImage { get; set; }
        [field: SerializeField] private TextMeshProUGUI SliderTxt { get; set; }
        [field: SerializeField] private TextMeshProUGUI PercentageTxt { get; set; }
        
        public void SetFillAmount(float value)
        {
            if(!SliderImage) return;
            
            SliderImage.fillAmount = value;
        }
        
        public void SetPercentageText(string value) => PercentageTxt?.SetText(value);
        
        public void SetLabelText(string value) => PercentageTxt?.SetText(value);
    }
}

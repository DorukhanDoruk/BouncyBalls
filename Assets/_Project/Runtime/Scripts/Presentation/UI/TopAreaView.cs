using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Presentation
{
    public sealed class TopAreaView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private Button _settingsButton;

        public Button SettingsButton => _settingsButton;

        public void SetLevelNumber(int number)
        {
            _levelLabel.SetText("Level {0}", number);
        }
    }
}

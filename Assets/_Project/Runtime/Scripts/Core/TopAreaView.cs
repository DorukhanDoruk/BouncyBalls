using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Runtime.Core
{
    public sealed class TopAreaView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private Button _settingsButton;

        public Button SettingsButton => _settingsButton;

        public void SetLevelName(string levelName)
        {
            _levelLabel.SetText(levelName);
        }
    }
}

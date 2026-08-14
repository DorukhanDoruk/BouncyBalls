using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Presentation
{
    public sealed class ResultPanelView : MonoBehaviour
    {
        [SerializeField] private Image _title;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private TextMeshProUGUI _buttonLabel;

        [Header("Title Sprites")]
        [SerializeField] private Sprite _wonSprite;
        [SerializeField] private Sprite _lostSprite;

        [Header("Button Label")]
        [SerializeField] private string _wonLabel = "CONTINUE";
        [SerializeField] private string _lostLabel = "RETRY";

        [Header("Scale")]
        [SerializeField] private float _openDuration = 0.3f;
        [SerializeField] private Ease _openEase = Ease.OutBack;
        [SerializeField] private float _closeDuration = 0.2f;
        [SerializeField] private Ease _closeEase = Ease.InQuad;

        private Tween _scale;

        public Button PlayAgainButton => _playAgainButton;

        public void Show(bool won)
        {
            _scale?.Kill();

            _title.sprite = won ? _wonSprite : _lostSprite;
            _buttonLabel.SetText(won ? _wonLabel : _lostLabel);

            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            _playAgainButton.interactable = true;

            _scale = transform.DOScale(Vector3.one, _openDuration)
                .SetEase(_openEase)
                .SetUpdate(true);
        }

        public void Hide()
        {
            _scale?.Kill();

            _playAgainButton.interactable = false;

            _scale = transform.DOScale(Vector3.zero, _closeDuration)
                .SetEase(_closeEase)
                .SetUpdate(true)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDisable()
        {
            _scale?.Kill();
            _scale = null;
        }
    }
}

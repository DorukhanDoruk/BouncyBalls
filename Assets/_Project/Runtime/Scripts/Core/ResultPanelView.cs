using Runtime.Configs;
using Runtime.Configs.Model;
using UnityEngine;
using UnityEngine.UI;
namespace Runtime.Core
{
    public sealed class ResultPanelView : MonoBehaviour
    {
        [SerializeField] private Image _title;
        [SerializeField] private Button _playAgainButton;

        [Header("Title Sprites")]
        [SerializeField] private Sprite _wonSprite;
        [SerializeField] private Sprite _lostSprite;

        [Header("Scale")]
        [SerializeField] private float _openDuration = 0.3f;
        [SerializeField] private EaseType _openEase = EaseType.OutQuad;
        [SerializeField] private float _closeDuration = 0.2f;
        [SerializeField] private EaseType _closeEase = EaseType.InQuad;

        private float _from;
        private float _to;
        private float _duration;
        private EaseType _ease;
        private float _elapsed;
        private bool _closing;

        public Button PlayAgainButton => _playAgainButton;

        private void Update()
        {
            if (_elapsed >= _duration)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            float scale = Mathf.LerpUnclamped(_from, _to, Easing.Evaluate(_ease, t));
            
            transform.localScale = new Vector3(scale, scale, 1f);

            if (_elapsed >= _duration && _closing)
            {
                _closing = false;
                gameObject.SetActive(false);
            }
        }

        public void Show(bool won)
        {
            _title.sprite = won ? _wonSprite : _lostSprite;
            _closing = false;

            Begin(0f, 1f, _openDuration, _openEase);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _closing = true;
            Begin(transform.localScale.x, 0f, _closeDuration, _closeEase);
        }

        public void HideInstant()
        {
            _closing = false;
            _elapsed = _duration;
            transform.localScale = Vector3.zero;

            gameObject.SetActive(false);
        }

        private void Begin(float from, float to, float duration, EaseType ease)
        {
            _from = from;
            _to = to;
            _duration = duration;
            _ease = ease;
            _elapsed = 0f;

            transform.localScale = new Vector3(from, from, 1f);
        }
    }
}

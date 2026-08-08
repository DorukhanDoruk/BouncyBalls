using UnityEngine;
namespace Runtime.Core
{
    public sealed class UiBackdropView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private float _shownAlpha = 0.6f;
        [SerializeField] private float _fadeDuration = 0.2f;

        private float _from;
        private float _to;
        private float _elapsed;
        private bool _hiding;

        private void Update()
        {
            if (_elapsed >= _fadeDuration)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(_from, _to, Mathf.Clamp01(_elapsed / _fadeDuration));

            if (_elapsed >= _fadeDuration && _hiding)
            {
                _hiding = false;
                gameObject.SetActive(false);
            }
        }

        public void Show()
        {
            _hiding = false;
            Begin(0f, _shownAlpha);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _hiding = true;
            Begin(_group.alpha, 0f);
        }

        public void HideInstant()
        {
            _hiding = false;
            _elapsed = _fadeDuration;
            _group.alpha = 0f;

            gameObject.SetActive(false);
        }

        private void Begin(float from, float to)
        {
            _from = from;
            _to = to;
            _elapsed = 0f;

            _group.alpha = from;
        }
    }
}

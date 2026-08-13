using DG.Tweening;
using UnityEngine;

namespace Runtime.Presentation
{
    public sealed class UiBackdropView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private float _shownAlpha = 0.6f;
        [SerializeField] private float _fadeDuration = 0.2f;

        private Tween _fade;

        public void Show()
        {
            _fade?.Kill();

            gameObject.SetActive(true);
            _group.alpha = 0f;

            // Unscaled so the backdrop still fades if the game is paused on result.
            _fade = _group.DOFade(_shownAlpha, _fadeDuration).SetUpdate(true);
        }

        public void Hide()
        {
            _fade?.Kill();

            _fade = _group.DOFade(0f, _fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDisable()
        {
            _fade?.Kill();
            _fade = null;
        }
    }
}

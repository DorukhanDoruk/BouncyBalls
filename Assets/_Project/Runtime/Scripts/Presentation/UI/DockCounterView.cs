using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Presentation
{
    public sealed class DockCounterView : MonoBehaviour
    {
        [SerializeField] private RectTransform _fillMask;
        [SerializeField] private TextMeshProUGUI _counter;
        [SerializeField] private Image _fillImage;

        [Header("Fill")]
        [SerializeField] private float _fullWidth = 150f;
        [SerializeField] private float _fillDuration = 0.25f;

        [Header("Full Warning")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _fullColor = Color.red;
        [SerializeField] private float _colorDuration = 0.25f;

        private Tween _fill;
        private Tween _color;
        private int _shownCount = -1;

        private void Awake()
        {
            SetWidth(0f);
            _fillImage.color = _normalColor;
        }

        public void SetCount(int count, int capacity)
        {
            if (count == _shownCount)
            {
                return;
            }

            _shownCount = count;
            _counter.SetText("{0}/{1}", count, capacity);

            _fill?.Kill();
            float target = _fullWidth * count / capacity;
            _fill = DOTween.To(() => _fillMask.sizeDelta.x, SetWidth, target, _fillDuration);
            
            _color?.Kill();
            _color = _fillImage.DOColor(count >= capacity ? _fullColor : _normalColor, _colorDuration);
        }

        private void SetWidth(float width)
        {
            var size = _fillMask.sizeDelta;
            size.x = width;
            _fillMask.sizeDelta = size;
        }

        private void OnDisable()
        {
            _fill?.Kill();
            _color?.Kill();
            _fill = null;
            _color = null;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Runtime.Core
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

        private float _fromWidth;
        private float _targetWidth;
        private float _elapsed;
        private int _shownCount = -1;

        private Color _fromColor;
        private Color _targetColor;
        private float _colorElapsed;

        private void Awake()
        {
            SetWidth(0f);
            _elapsed = _fillDuration;

            _fromColor = _normalColor;
            _targetColor = _normalColor;
            _colorElapsed = _colorDuration;
            _fillImage.color = _normalColor;
        }

        private void Update()
        {
            if (_elapsed < _fillDuration)
            {
                _elapsed += Time.deltaTime;
                SetWidth(Mathf.Lerp(_fromWidth, _targetWidth, Mathf.Clamp01(_elapsed / _fillDuration)));
            }

            if (_colorElapsed < _colorDuration)
            {
                _colorElapsed += Time.deltaTime;
                _fillImage.color = Color.Lerp(_fromColor, _targetColor, Mathf.Clamp01(_colorElapsed / _colorDuration));
            }
        }

        public void SetCount(int current, int max)
        {
            if (_shownCount == current)
            {
                return;
            }

            _shownCount = current;
            _counter.SetText("{0} / {1}", current, max);

            _fromWidth = _fillMask.sizeDelta.x;
            _targetWidth = _fullWidth * Mathf.Clamp01((float)current / max);
            _elapsed = 0f;

            SetWarning(current >= max);
        }

        private void SetWarning(bool full)
        {
            Color target = full ? _fullColor : _normalColor;
            if (target == _targetColor)
            {
                return;
            }

            _fromColor = _fillImage.color;
            _targetColor = target;
            _colorElapsed = 0f;
        }

        private void SetWidth(float width)
        {
            var size = _fillMask.sizeDelta;
            size.x = width;
            _fillMask.sizeDelta = size;
        }
    }
}

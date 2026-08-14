using TMPro;
using UnityEngine;

namespace Runtime.Presentation
{
    public sealed class BallLabelView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        public void SetCounter(int counter)
        {
            _text.SetText("{0}", counter);
        }

        public void SetAnchoredPosition(Vector2 position)
        {
            _rectTransform.anchoredPosition = position;
        }
    }
}

using UnityEngine;

namespace Runtime.Presentation
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class MaxWidthLayout : MonoBehaviour
    {
        [SerializeField] private float _maxWidth = 600f;
        [SerializeField] private float _sidePadding = 24f;

        private void OnEnable()
        {
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Apply();
        }
#endif

        private void Apply()
        {
            var self = (RectTransform)transform;
            
            if (transform.parent is not RectTransform parent)
            {
                return;
            }

            float available = parent.rect.width - _sidePadding * 2f;

            var size = self.sizeDelta;
            size.x = Mathf.Min(available, _maxWidth);
            self.sizeDelta = size;
        }
    }
}

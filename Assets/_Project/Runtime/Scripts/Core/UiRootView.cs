using TMPro;
using UnityEngine;
namespace Runtime.Core
{
    public sealed class UiRootView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _ballLabelPrefab;

        [Header("Ball Label Offset")]
        [SerializeField] private Vector3 _ballLabelWorldOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private Vector2 _ballLabelScreenOffset;

        public RectTransform Root => (RectTransform)transform;
        public TextMeshProUGUI BallLabelPrefab => _ballLabelPrefab;
        public Vector3 BallLabelWorldOffset => _ballLabelWorldOffset;
        public Vector2 BallLabelScreenOffset => _ballLabelScreenOffset;
    }
}

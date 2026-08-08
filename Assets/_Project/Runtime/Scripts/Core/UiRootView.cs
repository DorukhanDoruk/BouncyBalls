using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Runtime.Core
{
    public sealed class UiRootView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _ballLabelPrefab;

        [Header("Ball Label Offset")]
        [SerializeField] private Vector3 _ballLabelWorldOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private Vector2 _ballLabelScreenOffset;

        [Header("Top Area")]
        [SerializeField] private TopAreaView _topArea;

        [Header("Dock Counter")]
        [SerializeField] private DockCounterView _dockCounter;

        [Header("Result")]
        [SerializeField] private UiBackdropView _backdrop;
        [SerializeField] private ResultPanelView _resultPanel;

        public RectTransform Root => (RectTransform)transform;
        public TextMeshProUGUI BallLabelPrefab => _ballLabelPrefab;
        public Vector3 BallLabelWorldOffset => _ballLabelWorldOffset;
        public Vector2 BallLabelScreenOffset => _ballLabelScreenOffset;

        public TopAreaView TopArea => _topArea;
        public DockCounterView DockCounter => _dockCounter;
        public UiBackdropView Backdrop => _backdrop;
        public ResultPanelView ResultPanel => _resultPanel;
    }
}

using TMPro;
using UnityEngine;
namespace Runtime.Core
{
    public sealed class UiService : IService
    {
        private readonly UiRootView _rootPrefab;

        private UiRootView _root;
        private Canvas _canvas;
        private RectTransform _labelRoot;

        public UiService(UiRootView rootPrefab)
        {
            _rootPrefab = rootPrefab;
        }

        public void Initialize()
        {
            _root = Object.Instantiate(_rootPrefab);
            Object.DontDestroyOnLoad(_root.gameObject);
            _canvas = _root.GetComponent<Canvas>();

            _labelRoot = (RectTransform)new GameObject("BallLabels", typeof(RectTransform)).transform;
            _labelRoot.SetParent(_root.Root, false);

            _labelRoot.anchorMin = Vector2.zero;
            _labelRoot.anchorMax = Vector2.one;
            _labelRoot.offsetMin = Vector2.zero;
            _labelRoot.offsetMax = Vector2.zero;
        }

        // One call takes the whole UI down, labels included, so shutdown never walks
        // them one by one and cannot race the ECS world teardown.
        public void Dispose()
        {
            // On play-mode exit Unity may already have destroyed the root. Its == operator
            if (_root != null)
            {
                Object.Destroy(_root.gameObject);
            }
        }

        public Vector3 BallLabelWorldOffset => _root.BallLabelWorldOffset;

        // Scaled here so callers never have to know about the canvas.
        public Vector2 BallLabelScreenOffset => _root.BallLabelScreenOffset * _canvas.scaleFactor;

        public TextMeshProUGUI CreateBallLabel()
        {
            return Object.Instantiate(_root.BallLabelPrefab, _labelRoot, false);
        }
    }
}

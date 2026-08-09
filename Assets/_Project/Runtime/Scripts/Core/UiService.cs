using TMPro;
using UnityEngine;
namespace Runtime.Core
{
    public sealed class UiService : IService
    {
        private readonly UiRootView _rootPrefab;
        private readonly SceneLoaderService _sceneLoader;

        private UiRootView _root;
        private Canvas _canvas;
        private RectTransform _labelRoot;

        private bool _hudShown;
        private bool _resultShown;

        public UiService(UiRootView rootPrefab, SceneLoaderService sceneLoader)
        {
            _rootPrefab = rootPrefab;
            _sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            _root = Object.Instantiate(_rootPrefab);
            Object.DontDestroyOnLoad(_root.gameObject);
            _canvas = _root.GetComponent<Canvas>();

            _labelRoot = (RectTransform)new GameObject("BallLabels", typeof(RectTransform)).transform;
            _labelRoot.SetParent(_root.Root, false);
            _labelRoot.SetAsFirstSibling();

            _labelRoot.anchorMin = Vector2.zero;
            _labelRoot.anchorMax = Vector2.one;
            _labelRoot.offsetMin = Vector2.zero;
            _labelRoot.offsetMax = Vector2.zero;

            _root.TopArea.gameObject.SetActive(false);
            _root.Backdrop.HideInstant();
            _root.ResultPanel.HideInstant();
            _root.ResultPanel.PlayAgainButton.onClick.AddListener(Restart);
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

        public void ShowHud()
        {
            if (_hudShown)
            {
                return;
            }

            _hudShown = true;
            _resultShown = false;

            _root.TopArea.gameObject.SetActive(true);
            _root.DockCounter.gameObject.SetActive(true);
        }

        public void SetDockCount(int current, int max)
        {
            _root.DockCounter.SetCount(current, max);
        }

        public void ShowResult(bool won)
        {
            if (_resultShown)
            {
                return;
            }

            _resultShown = true;
            _root.Backdrop.Show();
            _root.ResultPanel.Show(won);
        }

        // The UI root survives the load, so its state is reset by hand before reloading.
        private void Restart()
        {
            if (!_hudShown)
            {
                return;
            }

            // _resultShown stays set on purpose: the level being torn down keeps reporting
            // Won/Lost for a few frames, and clearing it here reopens the panel instantly.
            _hudShown = false;

            _root.Backdrop.Hide();
            _root.ResultPanel.Hide();
            _root.TopArea.gameObject.SetActive(false);
            _root.DockCounter.gameObject.SetActive(false);

            _sceneLoader.Load(SceneNames.Gameplay);
        }
    }
}

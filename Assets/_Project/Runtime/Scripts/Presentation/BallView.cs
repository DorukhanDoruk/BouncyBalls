using Runtime.Simulation.Model;
using UnityEngine;
namespace Runtime.Presentation
{
    public class BallView : MonoBehaviour
    {
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int _outlineWidth = Shader.PropertyToID("_OutlineWidth");

        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private ParticleSystem _trail;
        [SerializeField] private float _outlineWidthValue = 0.02f;
        [SerializeField] private float _frontOutlineWidth = 0.04f;

        private Ball _ball;
        private MaterialPropertyBlock _materialPropertyBlock;

        private int _shownRow;
        private bool _trailing;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetBall(Ball ball, Color color)
        {
            _ball = ball;
            _shownRow = int.MinValue;

            transform.position = ball.Position;

            _materialPropertyBlock.SetColor(_baseColor, color);
            ApplyOutline();

            var trailMain = _trail.main;
            trailMain.startColor = color;

            // Position first, then clear, or the pooled trail streaks in from where it was.
            _trailing = false;
            _trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _trail.Clear(true);
        }

        // 5 Ball at once no need for a system update, each ball easily update themselves
        private void LateUpdate()
        {
            ApplyOutline();
            SetTrailing(_ball.State == BallState.Flying || _ball.State == BallState.ToDock);

            transform.position = _ball.Position;
        }

        private void ApplyOutline()
        {
            if (_shownRow == _ball.GridRowIndex)
            {
                return;
            }

            _shownRow = _ball.GridRowIndex;

            _materialPropertyBlock.SetFloat(_outlineWidth, _shownRow == 0 ? _frontOutlineWidth : _outlineWidthValue);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        private void SetTrailing(bool value)
        {
            if (_trailing == value)
            {
                return;
            }

            _trailing = value;

            if (value)
            {
                _trail.Play(true);
                return;
            }

            _trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnDisable()
        {
            _ball = null;
        }
    }
}

using Runtime.Config;
using DG.Tweening;
using Runtime.Core;
using Runtime.Simulation.Model;
using UnityEngine;
namespace Runtime.Presentation
{
    public class BallView : MonoBehaviour
    {
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private ParticleSystem _trail;

        private Ball _ball;
        private MaterialPropertyBlock _materialPropertyBlock;

        private BallMoveSettings _moveSettings;
        private Vector3 _lastTarget;
        private Tween _move;
        private bool _trailing;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetBall(Ball ball, Color color, BallMoveSettings moveSettings)
        {
            _ball = ball;
            _moveSettings = moveSettings;
            _lastTarget = ball.Position;

            _move?.Kill();
            transform.position = ball.Position;

            _materialPropertyBlock.SetColor(_baseColor, color);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);

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
            bool hopping = _ball.State == BallState.Flying || _ball.State == BallState.ToDock;
            SetTrailing(hopping);

            if (hopping)
            {
                _move?.Kill();
                transform.position = _ball.Position;
                _lastTarget = _ball.Position;
                return;
            }

            if (_ball.Position == _lastTarget)
            {
                return;
            }

            _lastTarget = _ball.Position;
            _move?.Kill();

            bool docked = _ball.State == BallState.AtDock;
            float duration = docked ? _moveSettings.DockInsertDuration : _moveSettings.GridShiftDuration;
            Ease ease = docked ? _moveSettings.DockInsertEase : _moveSettings.GridShiftEase;

            _move = transform.DOMove(_lastTarget, duration).SetEase(ease);
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
            _move?.Kill();
            _move = null;
            _ball = null;
        }
    }
}

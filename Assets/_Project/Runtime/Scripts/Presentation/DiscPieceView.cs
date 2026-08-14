using Runtime.Config;
using System;
using Runtime.Core;
using UnityEngine;
using static UnityEngine.Random;

namespace Runtime.Presentation
{
    // One fragment of a broken disc: flies out, falls, settles on the stick base, then dissolves.
    public class DiscPieceView : MonoBehaviour
    {
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Transform _meshHolder;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;

        private MaterialPropertyBlock _materialPropertyBlock;

        private DiscShatterSettings _settings;
        private Action<DiscPieceView> _onFinished;

        private Vector3 _baseScale;
        private Vector3 _velocity;
        private Vector3 _angularVelocity;
        private Vector3 _rotation;
        private float _groundY;
        private float _elapsed;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void Play(Mesh mesh, Color color, Vector3 discPosition, Vector3 discScale, float groundY,
            DiscShatterSettings settings, Action<DiscPieceView> onFinished)
        {
            _settings = settings;
            _onFinished = onFinished;
            _baseScale = discScale;
            _groundY = groundY;
            _elapsed = 0f;
            _rotation = Vector3.zero;

            _meshFilter.sharedMesh = mesh;

            var center = Vector3.Scale(mesh.bounds.center, discScale);
            _meshHolder.localPosition = -mesh.bounds.center;

            var outward = new Vector3(center.x, 0f, center.z);
            outward = outward.sqrMagnitude > Mathf.Epsilon ? outward.normalized : Vector3.right;

            _velocity = outward * (_settings.OutwardSpeed * Range(0.7f, 1.3f)) + Vector3.up * (_settings.UpwardSpeed * Range(0.7f, 1.3f));

            _angularVelocity = new Vector3(Range(-1f, 1f), Range(-1f, 1f), Range(-1f, 1f)) * _settings.SpinSpeed;

            transform.position = discPosition + center;
            transform.rotation = Quaternion.identity;
            transform.localScale = _baseScale;

            _materialPropertyBlock.SetColor(_baseColor, color);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            _elapsed += deltaTime;

            var position = transform.position;
            if (position.y > _groundY)
            {
                _velocity.y -= _settings.Gravity * deltaTime;
                position += _velocity * deltaTime;
                _rotation += _angularVelocity * deltaTime;

                if (position.y <= _groundY)
                {
                    position.y = _groundY;
                    _velocity = Vector3.zero;
                    _angularVelocity = Vector3.zero;
                }

                transform.position = position;
                transform.rotation = Quaternion.Euler(_rotation);
            }

            float shrink = Mathf.Lerp(1f, _settings.EndScale, Mathf.Clamp01(_elapsed / _settings.ShrinkDuration));
            float dissolve = Mathf.Clamp01((_elapsed - _settings.DissolveDelay) / _settings.DissolveDuration);
            transform.localScale = _baseScale * (shrink * (1f - dissolve));

            if (_elapsed >= _settings.Lifetime)
            {
                _onFinished(this);
            }
        }
    }
}

using System.Collections.Generic;
using DG.Tweening;
using Runtime.Simulation.Model;
using UnityEngine;
namespace Runtime.Presentation
{
    public class StickView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _topMeshRenderer;

        [Header("Body")]
        [SerializeField] private Transform _stack;
        [SerializeField] private Transform _body;
        [SerializeField] private MeshFilter _bodyMesh;

        public Transform DiscRoot => _stack;

        private readonly List<Transform> _discs = new(); // Bottom to Top
        private float BodyLength => Mathf.Max(0f, (_stick.ShownDiscCount - 1) * _discHeight);

        private Stick _stick;
        private Vector3 _stackBase;
        private float _discHeight;
        private Tween _dip;

        public void SetStick(Stick stick, float discHeight)
        {
            _stick = stick;
            _discHeight = discHeight;
            _stackBase = _stack.localPosition;
            _discs.Clear();

            ScaleBody();
            UpdateCap();
        }

        public void AddDisc(Transform disc)
        {
            _discs.Add(disc);
        }

        public void AddDiscBelow(Transform disc)
        {
            _discs.Insert(0, disc);
            disc.localPosition = new Vector3(0f, _stick.ShownDiscCount * _discHeight - _discHeight * (_discs.Count + 0.5f), 0f);
        }

        public void RemoveTopDisc()
        {
            _discs.RemoveAt(_discs.Count - 1);
            UpdateCap();
        }

        private void UpdateCap()
        {
            _topMeshRenderer.enabled = _stick.AliveCount == 0;
        }

        public void LayoutDiscs(float duration, Ease ease)
        {
            float top = _stick.ShownDiscCount * _discHeight;

            for (int i = 0; i < _discs.Count; i++)
            {
                float y = top - _discHeight * (_discs.Count - i - 0.5f);
                var target = new Vector3(0f, y, 0f);

                _discs[i].DOKill();

                if (duration <= 0f)
                {
                    _discs[i].localPosition = target;
                    continue;
                }

                _discs[i].DOLocalMove(target, duration).SetEase(ease);
            }
        }

        public void Dip(float amount, float duration)
        {
            _dip?.Kill();
            _stack.localPosition = _stackBase;

            _dip = _stack.DOPunchPosition(Vector3.down * amount, duration, 1, 0f);
        }

        private void ScaleBody()
        {
            float bodyLength = BodyLength;
            float meshHeight = _bodyMesh.sharedMesh.bounds.size.y;

            var scale = _body.localScale;
            scale.y = bodyLength / meshHeight;
            _body.localScale = scale;

            PlaceCap();
        }

        private void PlaceCap()
        {
            var cap = _topMeshRenderer.transform;
            var capBounds = _topMeshRenderer.GetComponent<MeshFilter>().sharedMesh.bounds;

            float bodyTop = _body.localPosition.y + _bodyMesh.sharedMesh.bounds.max.y * _body.localScale.y;

            var position = cap.localPosition;
            position.y = bodyTop - capBounds.min.y * cap.localScale.y;
            cap.localPosition = position;
        }


        private void OnDisable()
        {
            _dip?.Kill();
            _dip = null;
        }
    }
}

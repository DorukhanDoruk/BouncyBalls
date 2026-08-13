using Runtime.Simulation.Model;
using System;
using UnityEngine;
namespace Runtime.Presentation
{
    public class BallView : MonoBehaviour
    {
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
        
        [SerializeField] private MeshRenderer _meshRenderer;

        private Ball _ball;
        private MaterialPropertyBlock _materialPropertyBlock;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetBall(Ball ball, Color color)
        {
            _ball = ball;
            _materialPropertyBlock.SetColor(_baseColor, color);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        // 5 Ball at once no need for a system update, each ball easily update themselves
        private void LateUpdate()
        {
            transform.position = _ball.Position;
        }

        public void Dispose()
        {
            _ball = null;
        }
    }
}

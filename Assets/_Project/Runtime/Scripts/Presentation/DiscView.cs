using UnityEngine;
namespace Runtime.Presentation
{
    public class DiscView : MonoBehaviour
    {
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private MeshRenderer _meshRenderer;

        private MaterialPropertyBlock _materialPropertyBlock;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetDisc(Color color)
        {
            _materialPropertyBlock.SetColor(_baseColor, color);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}

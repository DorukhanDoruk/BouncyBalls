using Runtime.Simulation.Model;
using UnityEngine;
namespace Runtime.Presentation
{
    public class DiscView : MonoBehaviour
    {
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
        
        [SerializeField] private MeshRenderer _meshRenderer;

        private Disc _disc;
        private MaterialPropertyBlock _materialPropertyBlock;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }
        
        public void SetDisc(Disc disc, Color color)
        {
            _disc = disc;
            _materialPropertyBlock.SetColor(_baseColor, color);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        public void Dispose()
        {
            _disc = null;
        }
    }
}

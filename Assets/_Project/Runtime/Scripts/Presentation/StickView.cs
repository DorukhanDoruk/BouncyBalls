using Runtime.Simulation.Model;
using UnityEngine;
namespace Runtime.Presentation
{
    public class StickView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _topMeshRenderer;

        private Stick _stick;
        
        public void SetStick(Stick stick)
        {
            _stick = stick;
        }
    }
}

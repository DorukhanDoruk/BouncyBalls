using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Render Config", fileName = "RenderConfig")]
    public class RenderConfigSO : ScriptableObject
    {
        public const string ResourcePath = "Configuration/RenderConfig";
        public float GroundOffset = 0.01f;

        [Header("Palette")]
        public ColorPaletteSO Palette;

        [Header("Background")]
        public Mesh BackgroundMesh;
        public Material BackgroundMaterial;
        public Vector3 BackgroundPosition;
        public Vector3 BackgroundEulerAngles;
        public Vector3 BackgroundScale = Vector3.one;

        [Header("Stick Body")]
        public Mesh StickBodyMesh;
        public Material StickBodyMaterial;
        public Vector3 StickBodyScale = Vector3.one;

        [Header("Stick Base")]
        public Mesh StickBaseMesh;
        public Material StickBaseMaterial;
        public Vector3 StickBaseScale = Vector3.one;

        [Header("Hole")]
        public Mesh HoleMesh;
        public Material HoleMaterial;
        public Vector3 HoleScale = Vector3.one;

        [Header("Dock")]
        public Mesh DockMesh;
        public Material DockMaterial;
        public Vector3 DockScale = Vector3.one;

        [Header("Disc")]
        public Mesh DiscMesh;
        public Material DiscMaterial;
        public Vector3 DiscScale = Vector3.one;
        public Mesh[] DiscPieceMeshes;

        [Header("Ball")]
        public Mesh BallMesh;
        public Material BallMaterial;
        public Vector3 BallScale = Vector3.one;
    }
}

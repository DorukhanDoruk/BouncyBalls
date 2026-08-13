using Runtime.Core;
using Runtime.Simulation.Model;
using UnityEngine;

namespace Runtime.Presentation
{
    public class ViewFactory
    {
        private readonly VisualConfigSO _visualConfig;

        private readonly ObjectPool<StickView> _stickPool;
        private readonly ObjectPool<DiscView> _discPool;
        private readonly ObjectPool<BallView> _ballPool;
        private readonly ObjectPool<DockView> _dockPool;
        private readonly ObjectPool<DiscPieceView> _piecePool;

        public ViewFactory(VisualConfigSO visualConfig, Transform root)
        {
            _visualConfig = visualConfig;

            _stickPool = new ObjectPool<StickView>(visualConfig.StickPrefab.GetComponent<StickView>(), root);
            _discPool = new ObjectPool<DiscView>(visualConfig.DiscPrefab.GetComponent<DiscView>(), root);
            _ballPool = new ObjectPool<BallView>(visualConfig.BallPrefab.GetComponent<BallView>(), root);
            _dockPool = new ObjectPool<DockView>(visualConfig.DockPrefab.GetComponent<DockView>(), root);
            _piecePool = new ObjectPool<DiscPieceView>(visualConfig.DiscPiecePrefab.GetComponent<DiscPieceView>(), root);
        }

        public DockView CreateDock(Vector3 position)
        {
            return _dockPool.Get(position);
        }

        public StickView CreateStick(Stick stick, float discHeight)
        {
            var view = _stickPool.Get(stick.Position);
            view.SetStick(stick, discHeight);
            return view;
        }

        public DiscView CreateDisc(Disc disc, Vector3 position, Transform stick)
        {
            var view = _discPool.Get(position);
            view.transform.SetParent(stick, true);
            view.SetDisc(_visualConfig.GetColor(disc.Color));
            return view;
        }

        public BallView CreateBall(Ball ball)
        {
            var view = _ballPool.Get(ball.Position);
            view.SetBall(ball, _visualConfig.GetColor(ball.Color), _visualConfig.BallMove);
            return view;
        }

        public void BreakDisc(DiscView view, Disc disc, float groundY)
        {
            var position = view.transform.position;
            var scale = view.transform.lossyScale;
            var color = _visualConfig.GetColor(disc.Color);

            _discPool.Release(view);

            var meshes = _visualConfig.DiscPieceMeshes;
            for (int i = 0; i < meshes.Length; i++)
            {
                var piece = _piecePool.Get(position);
                piece.Play(meshes[i], color, position, scale, groundY, _visualConfig.DiscShatter, released => _piecePool.Release(released));
            }
        }

        public void ReleaseBall(BallView view)
        {
            _ballPool.Release(view);
        }

        public void ReleaseAll()
        {
            _stickPool.ReleaseAll();
            _discPool.ReleaseAll();
            _ballPool.ReleaseAll();
            _dockPool.ReleaseAll();
        }
    }
}

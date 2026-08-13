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

        public ViewFactory(VisualConfigSO visualConfig, Transform root)
        {
            _visualConfig = visualConfig;

            _stickPool = new ObjectPool<StickView>(visualConfig.StickPrefab.GetComponent<StickView>(), root);
            _discPool = new ObjectPool<DiscView>(visualConfig.DiscPrefab.GetComponent<DiscView>(), root);
            _ballPool = new ObjectPool<BallView>(visualConfig.BallPrefab.GetComponent<BallView>(), root);
            _dockPool = new ObjectPool<DockView>(visualConfig.DockPrefab.GetComponent<DockView>(), root);
        }

        public DockView CreateDock(Vector3 position)
        {
            return _dockPool.Get(position);
        }

        public StickView CreateStick(Stick stick)
        {
            var view = _stickPool.Get(stick.Position);
            view.SetStick(stick);
            return view;
        }

        public DiscView CreateDisc(Disc disc, Vector3 position)
        {
            var view = _discPool.Get(position);
            view.SetDisc(disc, _visualConfig.GetColor(disc.Color));
            return view;
        }

        public BallView CreateBall(Ball ball)
        {
            var view = _ballPool.Get(ball.Position);
            view.SetBall(ball, _visualConfig.GetColor(ball.Color));
            return view;
        }

        public void ReleaseStick(StickView view)
        {
            _stickPool.Release(view);
        }

        public void ReleaseDisc(DiscView view)
        {
            _discPool.Release(view);
            view.Dispose();
        }

        public void ReleaseBall(BallView view)
        {
            _ballPool.Release(view);
            view.Dispose();
        }
    }
}

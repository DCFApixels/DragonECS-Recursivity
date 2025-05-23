﻿#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Core.Unchecked;
using System;

namespace DCFApixels.DragonECS.Recursivity.Internal
{
    using static EcsConsts;

    [MetaName("On")]
    [MetaGroup(EcsRecursivityConsts.PACK_GROUP, SYSTEMS_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MetaColor(MetaColor.BlueViolet)]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_BCFB761A93019EF3EB1EFDEC770B9238")]
    public class RunOnSystem<TComponent, TWorld> : IEcsInit, IRecursiveRun, IRecursiveStart, IEcsPipelineMember, IEcsInject<TWorld>
        where TComponent : struct, IEcsComponent
        where TWorld : EcsWorld
    {
        class Aspect : EcsAspect
        {
            public EcsPool<TComponent> values = Inc;
        }
        public EcsPipeline Pipeline { get; set; }
        private TWorld[] _worlds = new TWorld[4];
        private int _worldsCount;

        private int _maxLoops;
        private int _currentRunLoops;

        private IOnRunner<TComponent> _onRunner;

        private int[] _filteredEntities = new int[64];
        private int _filteredEntitiesCount;

        public RunOnSystem(int maxLoops = -1)
        {
            _maxLoops = maxLoops;
        }

        public void Init()
        {
            _onRunner = Pipeline.GetRunnerInstance<IOnRunner<TComponent>>();
        }

        public void RecursiveStart()
        {
            //reset
            _currentRunLoops = 0;
        }
        public bool RecursiveRun(int runVersion)
        {
            if (_maxLoops >= 0)
            {
                if (_currentRunLoops >= _maxLoops)
                {
                    return false;
                }
                _currentRunLoops++;
            }

            bool result = false;

            for (int i = 0; i < _worldsCount; i++)
            {
                var world = _worlds[i];
                if (world.IsDestroyed)
                {
                    world = _worlds[--_worldsCount];
                    _worlds[i] = world;
                }

                if (world.IsComponentTypeDeclared<TComponent>())
                {
                    Aspect a = world.GetAspect<Aspect>();
                    _filteredEntitiesCount = a.Mask.GetIterator().IterateTo(world.Entities, ref _filteredEntities);
                    EcsSpan events = UncheckedUtility.CreateSpan(world.ID, _filteredEntities, _filteredEntitiesCount);

                    if (events.Count > 0)
                    {
                        result = true;
                        using (world.DisableAutoReleaseDelEntBuffer())
                        {
                            _onRunner.ToRun(events);
                            foreach (var e in events)
                            {
                                a.values.TryDel(e);
                            }
                        }
                        world.ReleaseDelEntityBufferAll();
                    }
                }
            }

            return result;
        }

        void IEcsInject<TWorld>.Inject(TWorld obj)
        {
            if (obj == null) { return; }
            if (_worlds.Length <= _worldsCount)
            {
                Array.Resize(ref _worlds, _worlds.Length << 1);
            }
            _worlds[_worldsCount++] = obj;
        }
    }
}
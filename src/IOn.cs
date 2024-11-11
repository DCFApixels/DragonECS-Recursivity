using DCFApixels.DragonECS.Recursivity.Internal;
using DCFApixels.DragonECS.RunnersCore;
using System;

namespace DCFApixels.DragonECS
{
    [MetaName("On")]
    [MetaColor(MetaColor.BlueViolet)]
    public interface IOn<T> : IEcsProcess
    {
        void ToRun(EcsSpan targetEntities);
    }

    public static class RecursivityExt
    {
        public static EcsPipeline.Builder AddRecursiveSystem(this EcsPipeline.Builder b)
        {
            return b.AddUnique(new RunRecursiveSystem());
        }
    }
    public static class RecursivityOnExt
    {
        public static EcsPipeline.Builder AddOn<T, TWorld>(this EcsPipeline.Builder b, int maxLoops = -1)
            where T : struct, IEcsComponent
            where TWorld : EcsWorld
        {
            return b.AddOn<T, TWorld>(maxLoops, null);
        }
        public static EcsPipeline.Builder AddOn<T, TWorld>(this EcsPipeline.Builder b, string layer)
            where T : struct, IEcsComponent
            where TWorld : EcsWorld
        {
            return b.AddOn<T, TWorld>(-1, layer);
        }
        public static EcsPipeline.Builder AddOn<T, TWorld>(this EcsPipeline.Builder b, int maxLoops, string layer)
            where T : struct, IEcsComponent
            where TWorld : EcsWorld
        {
            b.AddUnique(new RunRecursiveSystem());
            return b.AddUnique(new RunOnSystem<T, TWorld>(maxLoops), layer);
        }
    }
    public static class RecursivityTagOnExt
    {
        public static EcsPipeline.Builder AddOn<T, TWorld>(this EcsPipeline.Builder b, int maxLoops = -1)
            where T : struct, IEcsTagComponent
            where TWorld : EcsWorld
        {
            return b.AddOn<T, TWorld>(maxLoops, null);
        }
        public static EcsPipeline.Builder AddOn<T, TWorld>(this EcsPipeline.Builder b, string layer)
            where T : struct, IEcsTagComponent
            where TWorld : EcsWorld
        {
            return b.AddOn<T, TWorld>(-1, layer);
        }
        public static EcsPipeline.Builder AddOn<T, TWorld>(this EcsPipeline.Builder b, int maxLoops, string layer)
            where T : struct, IEcsTagComponent
            where TWorld : EcsWorld
        {
            b.AddUnique(new RunRecursiveSystem());
            return b.AddUnique(new RunOnTagSystem<T, TWorld>(maxLoops), layer);
        }
    }
}

namespace DCFApixels.DragonECS.Recursivity.Internal
{
    internal class IOnRunner<T> : EcsRunner<IOn<T>>, IOn<T>
    {
#if DEBUG && !DISABLE_DEBUG
        private EcsProfilerMarker[] _markers;
        protected override void OnSetup()
        {
            _markers = new EcsProfilerMarker[Process.Length];
            for (int i = 0; i < Process.Length; i++)
            {
                _markers[i] = new EcsProfilerMarker($"{Process[i].GetMeta().Name}.{nameof(IOn<T>.ToRun)}");
            }
        }
#endif
        public void ToRun(EcsSpan targetEntities)
        {
#if DEBUG && !DISABLE_DEBUG
            for (int i = 0, n = Process.Length < _markers.Length ? Process.Length : _markers.Length; i < n; i++)
            {
                _markers[i].Begin();
                try
                {
                    Process[i].ToRun(targetEntities);
                }
                catch (Exception e)
                {
#if DISABLE_CATH_EXCEPTIONS
                    throw;
#endif
                    EcsDebug.PrintError(e);
                }
                _markers[i].End();
            }
#else
            foreach (var item in Process)
            {
                try { item.ToRun(targetEntities); }
                catch (Exception e)
                {
#if DISABLE_CATH_EXCEPTIONS
                    throw;
#endif
                    EcsDebug.PrintError(e);
                }
            }
#endif
        }
    }
}
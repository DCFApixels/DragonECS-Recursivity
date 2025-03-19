#if DISABLE_DEBUG
#undef DEBUG
#endif

namespace DCFApixels.DragonECS.Recursivity.Internal
{
    using static EcsConsts;
    using static EcsRecursivityConsts;

    [MetaGroup(EcsRecursivityConsts.PACK_GROUP, SYSTEMS_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MetaColor(MetaColor.BlueViolet)]
    [MetaID("DragonECS_E326771A9301F751DF0272F4E5EBCCCF")]
    internal class RunRecursiveSystem : IEcsInit, IEcsRun, IEcsPipelineMember, IEcsModule, IEcsDefaultAddParams
    {
        AddParams IEcsDefaultAddParams.AddParams => RECURSIVE_LAYER;
        public EcsPipeline Pipeline { get; set; }


        EcsProcess<IRecursiveStart> _processStart;
        EcsProcess<IRecursiveRun> _processRun;
        EcsProcess<IRecursiveEnd> _processEnd;

        private int _iteration = 0;
        public void Init()
        {
            _processStart = Pipeline.GetProcess<IRecursiveStart>();
            _processRun = Pipeline.GetProcess<IRecursiveRun>();
            _processEnd = Pipeline.GetProcess<IRecursiveEnd>();
        }
        public void Run()
        {
            foreach (var system in _processStart)
            {
                system.RecursiveStart();
            }

            RunRecursiveRuns();
        }

        private void RunRecursiveRuns()
        {
            bool isLoop = false;
            foreach (var system in _processRun)
            {
                isLoop |= system.RecursiveRun(_iteration);
            }
            _iteration++;
            if (isLoop)
            {
                if (_iteration >= CRITICAL_RECURSIVE_COUNT)
                {
                    EcsDebug.PrintWarning("The cycle limit was exceeded, the recursive system was stopped to avoid infinite looping.");
                }
                else
                {
                    RunRecursiveRuns();
                }
            }
            else
            {
                foreach (var system in _processEnd)
                {
                    system.RecursiveEnd(_iteration);
                }
                _iteration = 0;
            }

        }
        public void Import(EcsPipeline.Builder b)
        {
            b.Layers.InsertAfter(BASIC_LAYER, RECURSIVE_LAYER);
            b.Add(this);
        }
    }


    public interface IRecursiveStart : IEcsProcess
    {
        void RecursiveStart();
    }
    public interface IRecursiveRun : IEcsProcess
    {
        bool RecursiveRun(int runVersion);
    }
    public interface IRecursiveEnd : IEcsProcess
    {
        void RecursiveEnd(int runVersion);
    }
}

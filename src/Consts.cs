namespace DCFApixels.DragonECS.Recursivity
{
    public static class EcsRecursivityConsts
    {
        public const string PACK_GROUP = "_" + EcsConsts.FRAMEWORK_NAME + "/Recursivity";

        public const int CRITICAL_RECURSIVE_COUNT = 100000;
        public const string RECURSIVE_LAYER = nameof(RECURSIVE_LAYER);
        public const string POST_RECURSIVE_LAYER = nameof(POST_RECURSIVE_LAYER);
    }
}

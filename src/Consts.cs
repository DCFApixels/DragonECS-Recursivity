namespace DCFApixels.DragonECS.Recursivity
{
    using static EcsConsts;
    public static class EcsRecursivityConsts
    {
        public const string NAME_SPACE = AUTHOR + "." + FRAMEWORK_NAME + ".Recursivity.";
        public const string PACK_GROUP = "_" + FRAMEWORK_NAME + "/Recursivity";

        public const int CRITICAL_RECURSIVE_COUNT = 100000;
        public const string RECURSIVE_LAYER = NAME_SPACE + nameof(RECURSIVE_LAYER);
    }
}

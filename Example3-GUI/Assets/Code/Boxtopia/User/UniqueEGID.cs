using Svelto.ECS;

namespace User
{
    static class UniqueEGID
    {
        static readonly ExclusiveGroup Initialized = new ExclusiveGroup();
        static readonly ExclusiveGroup ToValidate = new ExclusiveGroup();
        static readonly ExclusiveGroup Register = new ExclusiveGroup();
        static readonly ExclusiveGroup Validated = new ExclusiveGroup();

        public static readonly EGID UserInitialized = new EGID(0, Initialized);
        public static readonly EGID UserToValidate = new EGID(0, ToValidate);
        public static readonly EGID UserToRegister = new EGID(0, Register);
        public static readonly EGID UserReady = new EGID(0, Validated);
    }
}
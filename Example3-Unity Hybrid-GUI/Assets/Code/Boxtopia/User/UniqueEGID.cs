using Svelto.ECS;

namespace User
{
    static class UniqueEGID
    {
        static readonly ExclusiveGroup ToValidate = new ExclusiveGroup();
        static readonly ExclusiveGroup Register = new ExclusiveGroup();

        public static readonly EGID UserToValidate = new EGID(0, ToValidate);
        public static readonly EGID UserToRegister = new EGID(0, Register);
    }
}
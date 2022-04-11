namespace Svelto.Common.DataStructures
{
    public class SharedDictonary
    {
        public static void Init()
        {
            test.Data.test = SharedDictionaryStruct.Create();
        }

        static readonly SharedStaticWrapper<SharedDictionaryStruct, SharedDictonary> test;
    }
}
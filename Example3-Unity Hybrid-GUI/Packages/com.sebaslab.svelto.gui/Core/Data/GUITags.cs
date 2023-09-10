namespace Svelto.ECS.GUI
{
    public static class GUITags
    {
        internal static bool ExtractTag(string value, out char tag, out string target)
        {
            if (value != null && value.Length == 2 && value[0] == '@')
            {
                tag = value[1];
                target = "";
                return true;
            }

            if (value != null && value.Length > 3 && value[0] == '@' && value[2] == ':')
            {
                tag = value[1];
                target = value.Substring(3);
                return true;
            }

            tag = (char)0;
            target = value;
            return false;
        }

        internal static void ParseFullname(string target, out string root, out string name)
        {
            var nameParts = target.Split('.');
            if (nameParts.Length == 1)
            {
                root = nameParts[0];
                name = nameParts[0];
            }
            else if (nameParts.Length == 2)
            {
                root = nameParts[0];
                name = nameParts[1];
            }
            else
            {
                root = "";
                name = "";
                DBC.Check.Require(false);
            }
        }
    }
}
using System.Text;
using System.Threading;

public static class FastConcatUtility
{
    static readonly ThreadLocal<StringBuilder> _stringBuilder =
        new ThreadLocal<StringBuilder>(() => new StringBuilder(256));

    public static string FastConcat(this string str1, string value)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1).Append(value);

            return _stringBuilder.Value.ToString();
        }
    }
    public static string FastConcat(this string str1, int value)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1).Append(value);

            return _stringBuilder.Value.ToString();
        }
    }
    
    public static string FastConcat(this string str1, uint value)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1).Append(value);

            return _stringBuilder.Value.ToString();
        }
    }
    
    public static string FastConcat(this string str1, long value)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1).Append(value);

            return _stringBuilder.Value.ToString();
        }
    }    
    
    public static string FastConcat(this string str1, float value)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1).Append(value);

            return _stringBuilder.Value.ToString();
        }
    }
    
    public static string FastConcat(this string str1, double value)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1).Append(value);

            return _stringBuilder.Value.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1);
            _stringBuilder.Value.Append(str2);
            _stringBuilder.Value.Append(str3);

            return _stringBuilder.Value.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3, string str4)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1);
            _stringBuilder.Value.Append(str2);
            _stringBuilder.Value.Append(str3);
            _stringBuilder.Value.Append(str4);


            return _stringBuilder.Value.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3, string str4, string str5)
    {
        {
            _stringBuilder.Value.Clear();

            _stringBuilder.Value.Append(str1);
            _stringBuilder.Value.Append(str2);
            _stringBuilder.Value.Append(str3);
            _stringBuilder.Value.Append(str4);
            _stringBuilder.Value.Append(str5);

            return _stringBuilder.Value.ToString();
        }
    }
}
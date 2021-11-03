using System.Text;
using System.Threading;

public static class FastConcatUtility
{
    static readonly ThreadLocal<StringBuilder> stringBuilder =
        new ThreadLocal<StringBuilder>(() => new StringBuilder(256));

    public static string FastConcat(this string str1, string value)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }
    public static string FastConcat(this string str1, int value)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, uint value)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, long value)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, float value)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, double value)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1);
            _stringBuilder.Append(str2);
            _stringBuilder.Append(str3);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3, string str4)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1);
            _stringBuilder.Append(str2);
            _stringBuilder.Append(str3);
            _stringBuilder.Append(str4);


            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3, string str4, string str5)
    {
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1);
            _stringBuilder.Append(str2);
            _stringBuilder.Append(str3);
            _stringBuilder.Append(str4);
            _stringBuilder.Append(str5);

            return _stringBuilder.ToString();
        }
    }
    
    
    static StringBuilder _stringBuilder
    {
        get
        {
            try
            {
                return stringBuilder.Value;
            }
            catch
            {
                return new StringBuilder(); //this is just to handle finalizer that could be called after the _threadSafeStrings is finalized. So pretty rare
            }

        }
    }

}
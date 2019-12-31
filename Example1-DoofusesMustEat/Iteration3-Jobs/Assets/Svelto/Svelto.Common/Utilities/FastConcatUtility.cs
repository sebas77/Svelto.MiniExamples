using System.Text;

public static class FastConcatUtility
{
#if DEBUG && !PROFILE    
    static readonly StringBuilder _stringBuilder = new StringBuilder(256, 1024*1024);
#else    
    static readonly StringBuilder _stringBuilder = new StringBuilder(256);
#endif

    public static string FastConcat(this string str1, string value)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }
    public static string FastConcat(this string str1, int value)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }
    
    public static string FastConcat(this string str1, uint value)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }
    
    public static string FastConcat(this string str1, long value)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }    
    
    public static string FastConcat(this string str1, float value)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }
    
    public static string FastConcat(this string str1, double value)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(str1).Append(value);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3)
    {
        lock (_stringBuilder)
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
        lock (_stringBuilder)
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
        lock (_stringBuilder)
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
}
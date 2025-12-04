namespace IODataPlatform.Utilities;

public static partial class Extensions {

    /// <summary>将字符串转为指定类型，仅支持有限的类型</summary>
    public static object To(this string text, Type type) {
        var result = type switch {
            var t when t == typeof(string) => (object)text,
            var t when t == typeof(int) => text.ToInt32(),
            var t when t == typeof(int?) => text.ToNullableInt32(),
            var t when t == typeof(float) => text.ToSingle(),
            var t when t == typeof(float?) => text.ToNullableSingle(),
            var t when t == typeof(double) => text.ToDouble(),
            var t when t == typeof(double?) => text.ToNullableDouble(),
            var t when t == typeof(DateTime) => text.ToDateTime(),
            var t when t == typeof(DateTime?) => text.ToNullableDateTime(),
            _ => throw new("不支持的类型转换"),
        };

        return result!;
    }

    /// <summary>将字符串转为指定类型，仅支持有限的类型</summary>
    public static T To<T>(this string text) {
        return (T)text.To(typeof(T));
    }

    /// <summary>将字符串转为int，无法识别的转为0</summary>
    public static int ToInt32(this string text) {
        try { return int.Parse(text); } catch { return 0; }
    }

    /// <summary>将字符串转为int?，无法识别的转为null</summary>
    public static int? ToNullableInt32(this string text) {
        try { return int.Parse(text); } catch { return null; }
    }

    /// <summary>将字符串转为float，无法识别的转为0</summary>
    public static float ToSingle(this string text) {
        try { return float.Parse(text); } catch { return 0; }
    }

    /// <summary>将字符串转为float?，无法识别的转为null</summary>
    public static float? ToNullableSingle(this string text) {
        try { return float.Parse(text); } catch { return null; }
    }

    /// <summary>将字符串转为double，无法识别的转为0</summary>
    public static double ToDouble(this string text) {
        try { return double.Parse(text); } catch { return 0; }
    }

    /// <summary>将字符串转为double?，无法识别的转为null</summary>
    public static double? ToNullableDouble(this string text) {
        try { return double.Parse(text); } catch { return null; }
    }

    /// <summary>将字符串转为DateTime，无法识别的转为default</summary>
    public static DateTime ToDateTime(this string text) {
        try { return DateTime.Parse(text); } catch { return default; }
    }

    /// <summary>将字符串转为DateTime?，无法识别的转为null</summary>
    public static DateTime? ToNullableDateTime(this string text) {
        try { return DateTime.Parse(text); } catch { return null; }
    }

}
using System.Buffers;
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.12.47513")]
internal static class _003CRegexGenerator_g_003EF762D5FD329DBBD70E19FA14A99B374DF9975376AED0BDD1C4EEE6BE27FD4145C__Utilities
{
	internal static readonly TimeSpan s_defaultTimeout = ((AppContext.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") is TimeSpan timeSpan) ? timeSpan : Regex.InfiniteMatchTimeout);

	internal static readonly bool s_hasTimeout = s_defaultTimeout != Regex.InfiniteMatchTimeout;

	internal static readonly SearchValues<char> s_ascii_FFFFFFFFFF7FFFFFFFFFFFFF7FFFFFFF = SearchValues.Create("\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\t\n\v\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&'()*+,-.0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefhijklmnopqrstuvwxyz{|}~\u007f".AsSpan());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int IndexOfNonAsciiOrAny_6E2142753B21384F014A69E1BD4B8E45FD36B20CC7E41048B2A3D9F034B23F37(this ReadOnlySpan<char> span)
	{
		int num = span.IndexOfAnyExcept(s_ascii_FFFFFFFFFF7FFFFFFFFFFFFF7FFFFFFF);
		if ((uint)num < (uint)span.Length)
		{
			if (char.IsAscii(span[num]))
			{
				return num;
			}
			do
			{
				char c;
				if (((c = span[num]) < '\u0080') ? ((byte)("\0\0耀\0\0\0\u0080\0"[(int)c >> 4] & (1 << (c & 0xF))) != 0) : RegexRunner.CharInClass(c, "\0\u0006\0/0gh一龦"))
				{
					return num;
				}
				num++;
			}
			while ((uint)num < (uint)span.Length);
		}
		return -1;
	}
}

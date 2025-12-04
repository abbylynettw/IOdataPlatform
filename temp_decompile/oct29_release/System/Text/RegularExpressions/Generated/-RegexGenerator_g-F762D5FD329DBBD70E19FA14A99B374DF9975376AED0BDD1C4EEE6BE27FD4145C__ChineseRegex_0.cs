using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.12.47513")]
internal sealed class _003CRegexGenerator_g_003EF762D5FD329DBBD70E19FA14A99B374DF9975376AED0BDD1C4EEE6BE27FD4145C__ChineseRegex_0 : Regex
{
	private sealed class RunnerFactory : RegexRunnerFactory
	{
		private sealed class Runner : RegexRunner
		{
			protected override void Scan(ReadOnlySpan<char> inputSpan)
			{
				if (TryFindNextPossibleStartingPosition(inputSpan))
				{
					int num = runtextpos;
					Capture(0, num, runtextpos = num + 1);
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if ((uint)num < (uint)inputSpan.Length)
				{
					int num2 = inputSpan.Slice(num).IndexOfNonAsciiOrAny_6E2142753B21384F014A69E1BD4B8E45FD36B20CC7E41048B2A3D9F034B23F37();
					if (num2 >= 0)
					{
						runtextpos = num + num2;
						return true;
					}
				}
				runtextpos = inputSpan.Length;
				return false;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF762D5FD329DBBD70E19FA14A99B374DF9975376AED0BDD1C4EEE6BE27FD4145C__ChineseRegex_0 Instance = new _003CRegexGenerator_g_003EF762D5FD329DBBD70E19FA14A99B374DF9975376AED0BDD1C4EEE6BE27FD4145C__ChineseRegex_0();

	private _003CRegexGenerator_g_003EF762D5FD329DBBD70E19FA14A99B374DF9975376AED0BDD1C4EEE6BE27FD4145C__ChineseRegex_0()
	{
		pattern = "[\\u4e00-\\u9fa5/g]";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF762D5FD329DBBD70E19FA14A99B374DF9975376AED0BDD1C4EEE6BE27FD4145C__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF762D5FD329DBBD70E19FA14A99B374DF9975376AED0BDD1C4EEE6BE27FD4145C__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

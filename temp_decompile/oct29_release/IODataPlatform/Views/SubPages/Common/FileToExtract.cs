using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
public class FileToExtract(string fullname) : ObservableObject()
{
	[ObservableProperty]
	private string result = string.Empty;

	public string FullName { get; } = fullname;

	public string FileName { get; } = Path.GetFileNameWithoutExtension(fullname);

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.FileToExtract.result" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Result
	{
		get
		{
			return result;
		}
		[MemberNotNull("result")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(result, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Result);
				result = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Result);
			}
		}
	}
}

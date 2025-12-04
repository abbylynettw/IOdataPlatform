using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Models;

/// <summary>
/// 鏂囨。澶х翰椤癸紙鏍囬锛?
/// </summary>
/// <inheritdoc />
public partial class OutlineItem : ObservableObject
{
	/// <summary>
	/// 閿ょ偣ID锛堢敤浜庤烦杞級
	/// </summary>
	[ObservableProperty]
	private string id = string.Empty;

	/// <summary>
	/// 鏍囬鏂囨湰
	/// </summary>
	[ObservableProperty]
	private string text = string.Empty;

	/// <summary>
	/// 鏍囬灞傜骇锛?-6锛?
	/// </summary>
	[ObservableProperty]
	private int level = 1;

	/// <summary>
	/// 瀛椾綋澶у皬锛堟牴鎹眰绾у姩鎬佽绠楋級
	/// </summary>
	public double FontSize => Level switch
	{
		1 => 13.0, 
		2 => 12.5, 
		3 => 12.0, 
		_ => 11.5, 
	};

	/// <summary>
	/// 缂╄繘锛堟牴鎹眰绾у姩鎬佽绠楋級
	/// </summary>
	public Thickness Indent => new Thickness((Level - 1) * 12, 2.0, 2.0, 2.0);

	/// <inheritdoc cref="F:IODataPlatform.Models.OutlineItem.id" />
}

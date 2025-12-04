using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Aspose.Cells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Utilities;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public partial class GenericDataComparisonViewModel(GlobalModel model, IMessageService message, IPickerService picker, ExcelService excel) : ObservableObject()
{
	[ObservableProperty]
	private string oldDataSource = "璇烽€夋嫨鏃х増鏁版嵁鏂囦欢";

	[ObservableProperty]
	private string newDataSource = "璇烽€夋嫨鏂扮増鏁版嵁鏂囦欢";

	[ObservableProperty]
	private int addedCount;

	[ObservableProperty]
	private int deletedCount;

	[ObservableProperty]
	private int modifiedCount;

	[ObservableProperty]
	private bool hasComparisonResults;

	[ObservableProperty]
	private bool hasDataLoaded;

	[ObservableProperty]
	private bool showAdded = true;

	[ObservableProperty]
	private bool showDeleted = true;

	[ObservableProperty]
	private bool showModified = true;

	[ObservableProperty]
	private bool showUnchanged;

	[ObservableProperty]
	private ObservableCollection<string> availableFields = new ObservableCollection<string>();

	[ObservableProperty]
	private string selectedField = string.Empty;

	private ICollectionView? filteredResultsView;

	private DataTable oldDataTable;

	private DataTable newDataTable;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.SelectOldFileCommand" />.</summary>
}

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public partial class NetListComparisonViewModel : ObservableObject
{
	[ObservableProperty]
	private string oldDataSource;

	[ObservableProperty]
	private string newDataSource;

	[ObservableProperty]
	private int addedCount;

	[ObservableProperty]
	private int deletedCount;

	[ObservableProperty]
	private int modifiedCount;

	[ObservableProperty]
	private bool hasComparisonResults;

	[ObservableProperty]
	private bool showAdded;

	[ObservableProperty]
	private bool showDeleted;

	[ObservableProperty]
	private bool showModified;

	[ObservableProperty]
	private bool showUnchanged;

	private ICollectionView? filteredResultsView;

	private List<CableData>? oldDataList;

	private List<CableData>? newDataList;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.SelectOldFileCommand" />.</summary>
}

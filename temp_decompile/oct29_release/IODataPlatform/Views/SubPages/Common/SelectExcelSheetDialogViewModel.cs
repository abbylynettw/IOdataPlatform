using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
public class SelectExcelSheetDialogViewModel : ObservableObject
{
	[ObservableProperty]
	private string selectFilePath;

	[ObservableProperty]
	private List<string>? sheetNames;

	[ObservableProperty]
	private string selectedSheetName;

	[ObservableProperty]
	private string currentSystemInfo;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.SelectExcelSheetDialogViewModel.SelectFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? selectFileCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.SelectExcelSheetDialogViewModel.selectFilePath" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SelectFilePath
	{
		get
		{
			return selectFilePath;
		}
		[MemberNotNull("selectFilePath")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(selectFilePath, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SelectFilePath);
				selectFilePath = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SelectFilePath);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.SelectExcelSheetDialogViewModel.sheetNames" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public List<string>? SheetNames
	{
		get
		{
			return sheetNames;
		}
		set
		{
			if (!EqualityComparer<List<string>>.Default.Equals(sheetNames, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SheetNames);
				sheetNames = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SheetNames);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.SelectExcelSheetDialogViewModel.selectedSheetName" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SelectedSheetName
	{
		get
		{
			return selectedSheetName;
		}
		[MemberNotNull("selectedSheetName")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(selectedSheetName, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SelectedSheetName);
				selectedSheetName = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SelectedSheetName);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.SelectExcelSheetDialogViewModel.currentSystemInfo" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string CurrentSystemInfo
	{
		get
		{
			return currentSystemInfo;
		}
		[MemberNotNull("currentSystemInfo")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(currentSystemInfo, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.CurrentSystemInfo);
				currentSystemInfo = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.CurrentSystemInfo);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.SelectExcelSheetDialogViewModel.SelectFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand SelectFileCommand => selectFileCommand ?? (selectFileCommand = new RelayCommand(SelectFile));

	public SelectExcelSheetDialogViewModel(SqlSugarContext context, GlobalModel model, IMessageService message, IPickerService picker, ExcelService excel)
	{
		_003Cpicker_003EP = picker;
		_003Cexcel_003EP = excel;
		selectFilePath = "请选择Excel文件";
		currentSystemInfo = "当前系统：未选择项目";
		base._002Ector();
	}

	/// <summary>
	/// 设置当前系统信息
	/// </summary>
	/// <param name="projectName">项目名称</param>
	/// <param name="controlSystem">控制系统</param>
	public void SetCurrentSystemInfo(ControlSystem controlSystem)
	{
		CurrentSystemInfo = $"当前系统: {controlSystem}，请按照数据资产中心的配置表中当前系统相应的字段来导入";
	}

	[RelayCommand]
	private async void SelectFile()
	{
		string text = _003Cpicker_003EP.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx;*.xlsm");
		if (text != null)
		{
			SelectFilePath = text;
		}
		SheetNames = await _003Cexcel_003EP.GetSheetNames(SelectFilePath);
	}
}

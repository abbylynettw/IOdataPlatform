using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class CabinetAllocatedViewModel(SqlSugarContext context, GlobalModel model, IMessageService message, DepXT2ViewModel xt2, DepAQJViewModel aqj, NavigationParameterService parameterService) : ObservableObject(), INavigationAware
{
	private ControlSystem controlSystem;

	/// <summary>全部机柜</summary>
	[ObservableProperty]
	private List<StdCabinet>? cabinets;

	[ObservableProperty]
	private StdCabinet? cabinet;

	/// <summary>查看的板</summary>
	[ObservableProperty]
	private Board? viewBoard;

	/// <summary>数据库中全部板卡信息</summary>
	[ObservableProperty]
	private List<config_card_type_judge>? boardOptions;

	/// <summary> 冗余率 </summary>
	[ObservableProperty]
	private int redundancyRate = 20;

	[ObservableProperty]
	private ObservableCollection<string> localBoxNumberOptions = new ObservableCollection<string>();

	[ObservableProperty]
	private ObservableCollection<string> powerTypeOptions = new ObservableCollection<string>();

	[ObservableProperty]
	private string localBoxNumber1 = string.Empty;

	[ObservableProperty]
	private string powerType1 = string.Empty;

	[ObservableProperty]
	private string localBoxNumber2 = string.Empty;

	[ObservableProperty]
	private string powerType2 = string.Empty;

	[ObservableProperty]
	private string localBoxNumber3 = string.Empty;

	[ObservableProperty]
	private string powerType3 = string.Empty;

	[ObservableProperty]
	private ObservableCollection<IoFullData>? displayBoardPoints;

	[ObservableProperty]
	private ObservableCollection<IoFullData>? displayUnsetPoints;

	[ObservableProperty]
	private int unsetPointCount;

	[ObservableProperty]
	private int boardPointCount;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.SaveCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? saveCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.AddBoardToSlotCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<SlotInfo>? addBoardToSlotCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.AddBoardToUnsetCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? addBoardToUnsetCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.ViewBoardPointsCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<Board>? viewBoardPointsCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.AddTagCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<TagType>? addTagCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.DeleteTagCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<TagType>? deleteTagCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.AllocateIOCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? allocateIOCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.RecalcCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? recalcCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.cabinets" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public List<StdCabinet>? Cabinets
	{
		get
		{
			return cabinets;
		}
		set
		{
			if (!EqualityComparer<List<StdCabinet>>.Default.Equals(cabinets, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Cabinets);
				cabinets = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Cabinets);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.cabinet" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public StdCabinet? Cabinet
	{
		get
		{
			return cabinet;
		}
		set
		{
			if (!EqualityComparer<StdCabinet>.Default.Equals(cabinet, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Cabinet);
				cabinet = value;
				OnCabinetChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Cabinet);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.viewBoard" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public Board? ViewBoard
	{
		get
		{
			return viewBoard;
		}
		set
		{
			if (!EqualityComparer<Board>.Default.Equals(viewBoard, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ViewBoard);
				viewBoard = value;
				OnViewBoardChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ViewBoard);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.boardOptions" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public List<config_card_type_judge>? BoardOptions
	{
		get
		{
			return boardOptions;
		}
		set
		{
			if (!EqualityComparer<List<config_card_type_judge>>.Default.Equals(boardOptions, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.BoardOptions);
				boardOptions = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.BoardOptions);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.redundancyRate" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int RedundancyRate
	{
		get
		{
			return redundancyRate;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(redundancyRate, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.RedundancyRate);
				redundancyRate = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.RedundancyRate);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.localBoxNumberOptions" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> LocalBoxNumberOptions
	{
		get
		{
			return localBoxNumberOptions;
		}
		[MemberNotNull("localBoxNumberOptions")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(localBoxNumberOptions, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.LocalBoxNumberOptions);
				localBoxNumberOptions = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.LocalBoxNumberOptions);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.powerTypeOptions" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> PowerTypeOptions
	{
		get
		{
			return powerTypeOptions;
		}
		[MemberNotNull("powerTypeOptions")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(powerTypeOptions, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PowerTypeOptions);
				powerTypeOptions = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PowerTypeOptions);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.localBoxNumber1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string LocalBoxNumber1
	{
		get
		{
			return localBoxNumber1;
		}
		[MemberNotNull("localBoxNumber1")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(localBoxNumber1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.LocalBoxNumber1);
				localBoxNumber1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.LocalBoxNumber1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.powerType1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string PowerType1
	{
		get
		{
			return powerType1;
		}
		[MemberNotNull("powerType1")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(powerType1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PowerType1);
				powerType1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PowerType1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.localBoxNumber2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string LocalBoxNumber2
	{
		get
		{
			return localBoxNumber2;
		}
		[MemberNotNull("localBoxNumber2")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(localBoxNumber2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.LocalBoxNumber2);
				localBoxNumber2 = value;
				OnLocalBoxNumber2Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.LocalBoxNumber2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.powerType2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string PowerType2
	{
		get
		{
			return powerType2;
		}
		[MemberNotNull("powerType2")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(powerType2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PowerType2);
				powerType2 = value;
				OnPowerType2Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PowerType2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.localBoxNumber3" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string LocalBoxNumber3
	{
		get
		{
			return localBoxNumber3;
		}
		[MemberNotNull("localBoxNumber3")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(localBoxNumber3, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.LocalBoxNumber3);
				localBoxNumber3 = value;
				OnLocalBoxNumber3Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.LocalBoxNumber3);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.powerType3" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string PowerType3
	{
		get
		{
			return powerType3;
		}
		[MemberNotNull("powerType3")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(powerType3, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PowerType3);
				powerType3 = value;
				OnPowerType3Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PowerType3);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.displayBoardPoints" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<IoFullData>? DisplayBoardPoints
	{
		get
		{
			return displayBoardPoints;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<IoFullData>>.Default.Equals(displayBoardPoints, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DisplayBoardPoints);
				displayBoardPoints = value;
				OnDisplayBoardPointsChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayBoardPoints);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.displayUnsetPoints" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<IoFullData>? DisplayUnsetPoints
	{
		get
		{
			return displayUnsetPoints;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<IoFullData>>.Default.Equals(displayUnsetPoints, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DisplayUnsetPoints);
				displayUnsetPoints = value;
				OnDisplayUnsetPointsChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayUnsetPoints);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.unsetPointCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int UnsetPointCount
	{
		get
		{
			return unsetPointCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(unsetPointCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.UnsetPointCount);
				unsetPointCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.UnsetPointCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.boardPointCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int BoardPointCount
	{
		get
		{
			return boardPointCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(boardPointCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.BoardPointCount);
				boardPointCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.BoardPointCount);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.Save" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand SaveCommand => saveCommand ?? (saveCommand = new AsyncRelayCommand(Save));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.AddBoardToSlot(IODataPlatform.Models.ExcelModels.SlotInfo)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<SlotInfo> AddBoardToSlotCommand => addBoardToSlotCommand ?? (addBoardToSlotCommand = new RelayCommand<SlotInfo>(AddBoardToSlot));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.AddBoardToUnset" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand AddBoardToUnsetCommand => addBoardToUnsetCommand ?? (addBoardToUnsetCommand = new RelayCommand(AddBoardToUnset));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.ViewBoardPoints(IODataPlatform.Models.ExcelModels.Board)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<Board> ViewBoardPointsCommand => viewBoardPointsCommand ?? (viewBoardPointsCommand = new RelayCommand<Board>(ViewBoardPoints));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.AddTag(IODataPlatform.Models.ExcelModels.TagType)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<TagType> AddTagCommand => addTagCommand ?? (addTagCommand = new AsyncRelayCommand<TagType>(AddTag));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.DeleteTag(IODataPlatform.Models.ExcelModels.TagType)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<TagType> DeleteTagCommand => deleteTagCommand ?? (deleteTagCommand = new AsyncRelayCommand<TagType>(DeleteTag));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.AllocateIO" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AllocateIOCommand => allocateIOCommand ?? (allocateIOCommand = new AsyncRelayCommand(AllocateIO));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.Recalc" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RecalcCommand => recalcCommand ?? (recalcCommand = new AsyncRelayCommand(Recalc));

	public async void OnNavigatedTo()
	{
		ControlSystem parameter = parameterService.GetParameter<ControlSystem>("controlSystem");
		controlSystem = parameter;
		BoardOptions = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
		switch (controlSystem)
		{
		case ControlSystem.龙鳍:
			if (xt2.AllData == null)
			{
				throw new Exception("没数据");
			}
			Cabinets = xt2.AllData.ToList().BuildCabinetStructureOther(BoardOptions);
			break;
		case ControlSystem.中控:
			if (xt2.AllData == null)
			{
				throw new Exception("没数据");
			}
			Cabinets = xt2.AllData.ToList().BuildCabinetStructureOther(BoardOptions);
			break;
		case ControlSystem.龙核:
			if (aqj.AllData == null)
			{
				throw new Exception("没数据");
			}
			Cabinets = aqj.AllData.ToList().BuildCabinetStructureLH(BoardOptions);
			break;
		}
		await Task.Delay(200);
		Messengers.FullScreen.OnNext(value: true);
	}

	public async void OnNavigatedFrom()
	{
		if (await message.ConfirmAsync("确认操作\r\n保存IO分配的结果?"))
		{
			await Save();
		}
		else
		{
			model.Status.Busy("正在重置数据……");
			switch (controlSystem)
			{
			case ControlSystem.龙鳍:
				await xt2.ReloadAllData();
				break;
			case ControlSystem.龙核:
				await aqj.ReloadAllData();
				break;
			}
			model.Status.Reset();
		}
		await Task.Delay(700);
		Messengers.FullScreen.OnNext(value: false);
	}

	[RelayCommand]
	private async Task Save()
	{
		if (Cabinets == null)
		{
			throw new Exception("无数据可保存");
		}
		model.Status.Busy("正在保存……");
		switch (controlSystem)
		{
		case ControlSystem.龙鳍:
		{
			DepXT2ViewModel depXT2ViewModel = xt2;
			ObservableCollection<IoFullData> observableCollection2 = new ObservableCollection<IoFullData>();
			foreach (IoFullData item in new _003C_003Ez__ReadOnlyArray<StdCabinet>(Cabinets.ToArray()).ToPoint())
			{
				observableCollection2.Add(item);
			}
			depXT2ViewModel.AllData = observableCollection2;
			await xt2.SaveAndUploadFileAsync();
			await xt2.ReloadAllData();
			break;
		}
		case ControlSystem.中控:
		{
			DepXT2ViewModel depXT2ViewModel2 = xt2;
			ObservableCollection<IoFullData> observableCollection3 = new ObservableCollection<IoFullData>();
			foreach (IoFullData item2 in new _003C_003Ez__ReadOnlyArray<StdCabinet>(Cabinets.ToArray()).ToPoint())
			{
				observableCollection3.Add(item2);
			}
			depXT2ViewModel2.AllData = observableCollection3;
			await xt2.SaveAndUploadFileAsync();
			await xt2.ReloadAllData();
			break;
		}
		case ControlSystem.龙核:
		{
			DepAQJViewModel depAQJViewModel = aqj;
			ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
			foreach (IoFullData item3 in new _003C_003Ez__ReadOnlyArray<StdCabinet>(Cabinets.ToArray()).ToPoint())
			{
				observableCollection.Add(item3);
			}
			depAQJViewModel.AllData = observableCollection;
			await aqj.SaveAndUploadFileAsync();
			await aqj.ReloadAllData();
			break;
		}
		}
		model.Status.Reset();
	}

	[RelayCommand]
	private void AddBoardToSlot(SlotInfo slot)
	{
		Xt2BoardEditObj xt2BoardEditObj = new Xt2BoardEditObj();
		if (Edit(xt2BoardEditObj, "在插槽中添加端子板"))
		{
			slot.Board = Board.Create(xt2BoardEditObj.Type);
		}
	}

	[RelayCommand]
	private void AddBoardToUnset()
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		Xt2BoardEditObj xt2BoardEditObj = new Xt2BoardEditObj();
		if (Edit(xt2BoardEditObj, "添加未分配端子板"))
		{
			Cabinet.AddBoardToVirtualSlot(Board.Create(xt2BoardEditObj.Type));
		}
	}

	private bool Edit(Xt2BoardEditObj board, string title)
	{
		EditorOptionBuilder<Xt2BoardEditObj> editorOptionBuilder = board.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithEditorHeight(250.0).WithValidator((Xt2BoardEditObj x) => (x.Type == null) ? "请选择类型" : string.Empty);
		editorOptionBuilder.AddProperty<config_card_type_judge>("Type").WithHeader("类型").EditAsCombo<config_card_type_judge>()
			.WithOptions(BoardOptions.Select((config_card_type_judge x) => (IoCardType: x.IoCardType, x: x)).ToArray());
		return editorOptionBuilder.Build().EditWithWpfUI();
	}

	public void RemoveFromAllParents(IoFullData point)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		Cabinet.UnsetPoints.Remove(point);
		DisplayBoardPoints?.Remove(point);
		Xt2Channel xt2Channel = Cabinet.VirtualSlots.SelectMany(delegate(SlotInfo vs)
		{
			IEnumerable<Xt2Channel> enumerable = vs.Board?.Channels;
			return enumerable ?? Enumerable.Empty<Xt2Channel>();
		}).SingleOrDefault((Xt2Channel x) => x.Point == point);
		if (xt2Channel != null)
		{
			xt2Channel.Point = null;
		}
		Xt2Channel xt2Channel2 = (from x in Cabinet.Cages.SelectMany((ChassisInfo x) => x.Slots)
			select x.Board into x
			where x != null
			select x).SelectMany((Board x) => x.Channels).SingleOrDefault((Xt2Channel x) => x.Point == point);
		if (xt2Channel2 != null)
		{
			xt2Channel2.Point = null;
		}
	}

	public void RemoveFromAllParents(Board board)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		SlotInfo slotInfo = Cabinet.VirtualSlots.FirstOrDefault((SlotInfo vs) => vs.Board == board);
		if (slotInfo != null)
		{
			Cabinet.VirtualSlots.Remove(slotInfo);
		}
		SlotInfo slotInfo2 = Cabinet.Cages.SelectMany((ChassisInfo x) => x.Slots).SingleOrDefault((SlotInfo x) => x.Board == board);
		if (slotInfo2 != null)
		{
			slotInfo2.Board = null;
		}
	}

	private async Task ResetPoints(bool isRecalc)
	{
		if (Cabinet == null)
		{
			return;
		}
		switch (controlSystem)
		{
		case ControlSystem.龙鳍:
			if (isRecalc)
			{
				await xt2.Recalc(Cabinet.Name);
			}
			break;
		}
		int? num = Cabinets?.FindIndex((StdCabinet c) => c.Name == Cabinet.Name);
		if (num.HasValue && num != -1)
		{
			Cabinets[num.Value] = Cabinet;
		}
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in DisplayBoardPoints ?? new ObservableCollection<IoFullData>())
		{
			observableCollection.Add(item);
		}
		DisplayBoardPoints = observableCollection;
		StdCabinet stdCabinet = Cabinet;
		ObservableCollection<IoFullData> observableCollection2 = new ObservableCollection<IoFullData>();
		foreach (IoFullData item2 in Cabinet?.UnsetPoints)
		{
			observableCollection2.Add(item2);
		}
		stdCabinet.UnsetPoints = observableCollection2;
	}

	public async void Move(Board board, SlotInfo slot)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		Board board2 = slot.Board;
		if (board2 != null)
		{
			Cabinet.AddBoardToVirtualSlot(board2);
		}
		RemoveFromAllParents(board);
		slot.Board = board;
		await ResetPoints(isRecalc: true);
		List<SlotInfo> list = Cabinet.VirtualSlots.OrderBy((SlotInfo vs) => vs.Board?.Channels.Count ?? 0).ToList();
		Cabinet.VirtualSlots.Clear();
		foreach (SlotInfo item in list)
		{
			Cabinet.VirtualSlots.Add(item);
		}
	}

	public async void Move(IoFullData point, Xt2Channel channel)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		Board parent = GetParent(channel);
		if (point.CardType != parent.Type)
		{
			throw new Exception("点类型和卡件类型不一致");
		}
		IoFullData point2 = channel.Point;
		if (point2 != null)
		{
			Cabinet.UnsetPoints.Add(point2);
		}
		RemoveFromAllParents(point);
		channel.Point = point;
		await ResetPoints(isRecalc: true);
		Filter();
	}

	public async void Move(List<IoFullData> points, Xt2Channel channel)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		Board parent = GetParent(channel);
		foreach (IoFullData point in points)
		{
			if (point.CardType != parent.Type)
			{
				throw new Exception("点类型和卡件类型不一致");
			}
			Xt2Channel xt2Channel = parent.Channels.SkipWhile((Xt2Channel x) => x.Index < channel.Index).FirstOrDefault((Xt2Channel x) => x.Point == null) ?? throw new Exception("没有空白通道");
			RemoveFromAllParents(point);
			xt2Channel.Point = point;
		}
		await ResetPoints(isRecalc: true);
		Filter();
	}

	public async void Unset(Board board)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		RemoveFromAllParents(board);
		Cabinet.AddBoardToVirtualSlot(board);
		List<SlotInfo> list = Cabinet.VirtualSlots.OrderBy((SlotInfo vs) => vs.Board?.Channels.Count ?? 0).ToList();
		Cabinet.VirtualSlots.Clear();
		foreach (SlotInfo item in list)
		{
			Cabinet.VirtualSlots.Add(item);
		}
		await ResetPoints(isRecalc: true);
	}

	public async void Unset(List<IoFullData> points)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		foreach (IoFullData point in points)
		{
			RemoveFromAllParents(point);
			Cabinet.UnsetPoints.Add(point);
		}
		await ResetPoints(isRecalc: true);
		Filter();
	}

	public async void Unset(IoFullData point)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		RemoveFromAllParents(point);
		Cabinet.UnsetPoints.Add(point);
		await ResetPoints(isRecalc: true);
		Filter();
	}

	public void Delete(Board board)
	{
		if (board.Channels.Any((Xt2Channel x) => x.Point != null))
		{
			throw new Exception("无法删除，卡件上还有点");
		}
		RemoveFromAllParents(board);
	}

	public void View(Board board)
	{
		ViewBoardPoints(board);
	}

	[RelayCommand]
	private void ViewBoardPoints(Board board)
	{
		ViewBoard = board;
	}

	public Board GetParent(Xt2Channel channel)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		IEnumerable<Board> first = from x in Cabinet.Cages.SelectMany((ChassisInfo x) => x.Slots)
			select x.Board into x
			where x != null
			select x;
		IEnumerable<Board> second = from vs in Cabinet.VirtualSlots
			where vs.Board != null
			select vs.Board;
		IEnumerable<Board> source = first.Concat(second);
		return source.Single((Board x) => x.Channels.Contains(channel));
	}

	[RelayCommand]
	private async Task AddTag(TagType type)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		switch (type)
		{
		case TagType.Alarm:
		{
			List<config_card_type_judge> source = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
			config_card_type_judge config_card_type_judge = source.FirstOrDefault((config_card_type_judge c) => c.IoCardType == "DI211");
			config_card_type_judge config_card_type_judge2 = source.FirstOrDefault((config_card_type_judge c) => c.IoCardType == "DO211");
			if (config_card_type_judge == null)
			{
				throw new Exception("未找到卡件DI211的数量");
			}
			if (config_card_type_judge2 == null)
			{
				throw new Exception("未找到卡件DO211的数量");
			}
			model.Status.Busy("正在添加报警点……");
			if (!cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots).SelectMany(delegate(SlotInfo slot)
			{
				IEnumerable<Xt2Channel> enumerable = slot.Board?.Channels;
				return enumerable ?? Enumerable.Empty<Xt2Channel>();
			}).Any(delegate(Xt2Channel channel)
			{
				IoFullData? point = channel.Point;
				return point != null && point.PointType == TagType.Alarm;
			}))
			{
				AddAlarmPoints(cabinet, config_card_type_judge);
				AddControlAlarmPoint(cabinet, config_card_type_judge2);
			}
			model.Status.Busy("添加报警点完毕……");
			break;
		}
		case TagType.BackUp:
			model.Status.Busy("正在添加备用点……");
			foreach (ChassisInfo cage in cabinet.Cages)
			{
				foreach (SlotInfo slot in cage.Slots)
				{
					string text = slot.Board?.Type;
					if (text == null || text.Contains("DP") || text.Contains("FF"))
					{
						continue;
					}
					for (int i = 0; i < slot.Board.Channels.Count; i++)
					{
						if (slot.Board.Channels[i].Point == null)
						{
							IoFullData ioFullData = slot.Board.Channels.FirstOrDefault((Xt2Channel c) => c.Point != null)?.Point;
							if (ioFullData == null)
							{
								throw new Exception($"机柜{cabinet.Name}机笼{cage.Index}插槽{slot.Index.ToString("00")}卡件{text.Substring(0, 2)}没有通道，无法添加备用点！");
							}
							slot.Board.Channels[i].Point = new IoFullData
							{
								CabinetNumber = cabinet.Name,
								SignalPositionNumber = $"{cabinet.Name}{cage.Index}{slot.Index.ToString("00")}{text.Substring(0, 2)}CH{i.ToString("00")}",
								SystemCode = "BEIYONG",
								Cage = cage.Index,
								Slot = slot.Index,
								CardType = text,
								Description = "备用",
								Channel = i,
								IoType = ioFullData.IoType,
								PowerType = ioFullData.PowerType,
								ElectricalCharacteristics = ioFullData.ElectricalCharacteristics,
								SignalEffectiveMode = ioFullData.SignalEffectiveMode,
								PointType = TagType.BackUp,
								Version = "A",
								ModificationDate = DateTime.Now
							};
						}
					}
				}
			}
			model.Status.Busy("添加备用点完毕……");
			break;
		}
		await ResetPoints(isRecalc: true);
		Filter();
		model.Status.Reset();
		static void AddAlarmPoints(StdCabinet cabinet, config_card_type_judge config)
		{
			string[] array = new string[6] { "电源A故障报警", "电源B故障报警", "机柜门开", "温度高报警", "风扇故障", "网络故障" };
			string[] array2 = new string[6] { "PWFA", "PWFB", "DROP", "TEPH", "FAN", "SWF" };
			for (int j = 0; j < 6; j++)
			{
				IoFullData item = new IoFullData
				{
					CabinetNumber = cabinet.Name,
					PointType = TagType.Alarm,
					SignalPositionNumber = cabinet.Name,
					Cage = 0,
					Slot = 0,
					Channel = 0,
					IoType = "DI",
					PowerType = "DI1",
					ElectricalCharacteristics = "无源常开",
					SignalEffectiveMode = "NO",
					SystemCode = "JIGUIBAOJING",
					ExtensionCode = array2[j],
					Description = "控制柜" + cabinet.Name + "机柜" + array[j]
				};
				cabinet.UnsetPoints.Add(item);
			}
		}
		static void AddControlAlarmPoint(StdCabinet cabinet, config_card_type_judge config)
		{
			IoFullData item = new IoFullData
			{
				CabinetNumber = cabinet.Name,
				PointType = TagType.Alarm,
				Cage = 0,
				Slot = 0,
				Channel = 0,
				SignalPositionNumber = cabinet.Name,
				SystemCode = "JIGUIBAOJING",
				ExtensionCode = "ALM",
				Description = "控制柜" + cabinet.Name + "机柜报警灯",
				IoType = "DO",
				PowerType = "DO2",
				ElectricalCharacteristics = "有源常闭",
				SignalEffectiveMode = "NO"
			};
			cabinet.UnsetPoints.Add(item);
		}
	}

	[RelayCommand]
	private async Task DeleteTag(TagType type)
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		model.Status.Busy("正在删除点……");
		Cabinet.RemovePoints(type);
		model.Status.Reset();
	}

	[RelayCommand]
	private async Task AllocateIO()
	{
		if (Cabinet == null)
		{
			throw new Exception();
		}
		model.Status.Busy("正在分配……");
		FormularHelper formularHelper = new FormularHelper();
		List<config_card_type_judge> configs = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
		if (controlSystem == ControlSystem.龙鳍)
		{
			Cabinet = formularHelper.AutoAllocateIO(Cabinet, configs, (double)RedundancyRate / 100.0);
		}
		if (controlSystem == ControlSystem.中控)
		{
			Cabinet = formularHelper.AutoAllocateIO(Cabinet, configs, (double)RedundancyRate / 100.0);
		}
		else if (controlSystem == ControlSystem.龙核)
		{
			Cabinet = formularHelper.AutoAllocateLongHeIOSingle(Cabinet, configs, (double)RedundancyRate / 100.0);
		}
		await ResetPoints(isRecalc: true);
		Filter();
		model.Status.Success("分配完毕！");
	}

	[RelayCommand]
	private async Task Recalc()
	{
		await ResetPoints(isRecalc: true);
		Filter();
	}

	private void Filter()
	{
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in (from x in ViewBoard?.Channels
			where x.Point != null
			select x.Point).WhereIf((IoFullData x) => x.LocalBoxNumber == LocalBoxNumber2, LocalBoxNumber2 != "全部").WhereIf((IoFullData x) => x.PowerType == PowerType2, PowerType2 != "全部") ?? Array.Empty<IoFullData>())
		{
			observableCollection.Add(item);
		}
		DisplayBoardPoints = observableCollection;
		ObservableCollection<IoFullData> observableCollection2 = new ObservableCollection<IoFullData>();
		foreach (IoFullData item2 in Cabinet?.UnsetPoints?.WhereIf((IoFullData x) => x.LocalBoxNumber == LocalBoxNumber3, LocalBoxNumber3 != "全部").WhereIf((IoFullData x) => x.PowerType == PowerType3, PowerType3 != "全部") ?? Array.Empty<IoFullData>())
		{
			observableCollection2.Add(item2);
		}
		DisplayUnsetPoints = observableCollection2;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.Cabinet" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.Cabinet" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnCabinetChanged(StdCabinet? value)
	{
		ViewBoard = null;
		DisplayBoardPoints = null;
		DisplayUnsetPoints = null;
		if (Cabinet == null)
		{
			return;
		}
		List<IoFullData> source = CabinetCalc.CabinetStructureToPoint(new _003C_003Ez__ReadOnlySingleElementList<StdCabinet>(Cabinet));
		ObservableCollection<string> observableCollection = new ObservableCollection<string>();
		observableCollection.Add("全部");
		foreach (string item in source.Select((IoFullData x) => x.LocalBoxNumber).Distinct())
		{
			observableCollection.Add(item);
		}
		LocalBoxNumberOptions = observableCollection;
		ObservableCollection<string> observableCollection2 = new ObservableCollection<string>();
		observableCollection2.Add("全部");
		foreach (string item2 in source.Select((IoFullData x) => x.PowerType).Distinct())
		{
			observableCollection2.Add(item2);
		}
		PowerTypeOptions = observableCollection2;
		LocalBoxNumber1 = "全部";
		LocalBoxNumber2 = "全部";
		LocalBoxNumber3 = "全部";
		PowerType1 = "全部";
		PowerType2 = "全部";
		PowerType3 = "全部";
		Filter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.ViewBoard" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.ViewBoard" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnViewBoardChanged(Board? value)
	{
		await ResetPoints(isRecalc: false);
		Filter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.LocalBoxNumber2" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.LocalBoxNumber2" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnLocalBoxNumber2Changed(string value)
	{
		Filter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.PowerType2" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.PowerType2" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnPowerType2Changed(string value)
	{
		Filter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.LocalBoxNumber3" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.LocalBoxNumber3" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnLocalBoxNumber3Changed(string value)
	{
		Filter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.PowerType3" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.PowerType3" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnPowerType3Changed(string value)
	{
		Filter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.DisplayBoardPoints" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.DisplayBoardPoints" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnDisplayBoardPointsChanged(ObservableCollection<IoFullData>? value)
	{
		BoardPointCount = DisplayBoardPoints?.Count ?? 0;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.DisplayUnsetPoints" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.CabinetAllocatedViewModel.DisplayUnsetPoints" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnDisplayUnsetPointsChanged(ObservableCollection<IoFullData>? value)
	{
		UnsetPointCount = DisplayUnsetPoints?.Count ?? 0;
	}
}

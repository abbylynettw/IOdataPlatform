using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
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
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Cable;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class MatchViewModel(INavigationService navigation, SqlSugarContext context, StorageService storage, ExcelService excel, GlobalModel model, CableViewModel cable) : ObservableObject(), INavigationAware
{
	[ObservableProperty]
	private MatchCableDataResult? result;

	[ObservableProperty]
	private ObservableCollection<MatchCableDataFail>? failist1;

	[ObservableProperty]
	private ObservableCollection<MatchCableDataFail>? failist2;

	[ObservableProperty]
	private ObservableCollection<CableData>? successList;

	[ObservableProperty]
	private int successDataCount;

	[ObservableProperty]
	private config_project_subProject? subProject1;

	[ObservableProperty]
	private publish_termination? publish1;

	[ObservableProperty]
	private string header1 = string.Empty;

	[ObservableProperty]
	private ObservableCollection<publish_termination>? publishes1;

	[ObservableProperty]
	private config_project_subProject? subProject2;

	[ObservableProperty]
	private publish_termination? publish2;

	[ObservableProperty]
	private string header2 = string.Empty;

	[ObservableProperty]
	private ObservableCollection<publish_termination>? publishes2;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.SaveCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? saveCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.LoadCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? loadCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.OverwriteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? overwriteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.AppendCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? appendCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.UnMatchCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<CableData>? unMatchCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.MatchCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? matchCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.MatchSelectedCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? matchSelectedCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.result" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public MatchCableDataResult? Result
	{
		get
		{
			return result;
		}
		set
		{
			if (!EqualityComparer<MatchCableDataResult>.Default.Equals(result, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Result);
				result = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Result);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.failist1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<MatchCableDataFail>? Failist1
	{
		get
		{
			return failist1;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<MatchCableDataFail>>.Default.Equals(failist1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Failist1);
				failist1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Failist1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.failist2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<MatchCableDataFail>? Failist2
	{
		get
		{
			return failist2;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<MatchCableDataFail>>.Default.Equals(failist2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Failist2);
				failist2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Failist2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.successList" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<CableData>? SuccessList
	{
		get
		{
			return successList;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<CableData>>.Default.Equals(successList, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SuccessList);
				successList = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SuccessList);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.successDataCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int SuccessDataCount
	{
		get
		{
			return successDataCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(successDataCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SuccessDataCount);
				successDataCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SuccessDataCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.subProject1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_subProject? SubProject1
	{
		get
		{
			return subProject1;
		}
		set
		{
			if (!EqualityComparer<config_project_subProject>.Default.Equals(subProject1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProject1);
				subProject1 = value;
				OnSubProject1Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProject1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.publish1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public publish_termination? Publish1
	{
		get
		{
			return publish1;
		}
		set
		{
			if (!EqualityComparer<publish_termination>.Default.Equals(publish1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Publish1);
				publish1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Publish1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.header1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Header1
	{
		get
		{
			return header1;
		}
		[MemberNotNull("header1")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(header1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Header1);
				header1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Header1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.publishes1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<publish_termination>? Publishes1
	{
		get
		{
			return publishes1;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<publish_termination>>.Default.Equals(publishes1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Publishes1);
				publishes1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Publishes1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.subProject2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_subProject? SubProject2
	{
		get
		{
			return subProject2;
		}
		set
		{
			if (!EqualityComparer<config_project_subProject>.Default.Equals(subProject2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProject2);
				subProject2 = value;
				OnSubProject2Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProject2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.publish2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public publish_termination? Publish2
	{
		get
		{
			return publish2;
		}
		set
		{
			if (!EqualityComparer<publish_termination>.Default.Equals(publish2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Publish2);
				publish2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Publish2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.header2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Header2
	{
		get
		{
			return header2;
		}
		[MemberNotNull("header2")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(header2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Header2);
				header2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Header2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.MatchViewModel.publishes2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<publish_termination>? Publishes2
	{
		get
		{
			return publishes2;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<publish_termination>>.Default.Equals(publishes2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Publishes2);
				publishes2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Publishes2);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.MatchViewModel.Save" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand SaveCommand => saveCommand ?? (saveCommand = new AsyncRelayCommand(Save));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.MatchViewModel.Load" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand LoadCommand => loadCommand ?? (loadCommand = new AsyncRelayCommand(Load));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.MatchViewModel.Overwrite" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand OverwriteCommand => overwriteCommand ?? (overwriteCommand = new AsyncRelayCommand(Overwrite));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.MatchViewModel.Append" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand AppendCommand => appendCommand ?? (appendCommand = new RelayCommand(Append));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.MatchViewModel.UnMatch(IODataPlatform.Models.ExcelModels.CableData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<CableData> UnMatchCommand => unMatchCommand ?? (unMatchCommand = new AsyncRelayCommand<CableData>(UnMatch));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.MatchViewModel.Match" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand MatchCommand => matchCommand ?? (matchCommand = new AsyncRelayCommand(Match));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.MatchViewModel.MatchSelected" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand MatchSelectedCommand => matchSelectedCommand ?? (matchSelectedCommand = new RelayCommand(MatchSelected));

	public void OnNavigatedFrom()
	{
	}

	public void OnNavigatedTo()
	{
		SubProject1 = cable.SubProject1 ?? throw new Exception();
		SubProject2 = cable.SubProject2 ?? throw new Exception();
		Header1 = cable.Major1?.CableSystem + " - " + SubProject1.Name;
		Header2 = cable.Major2?.CableSystem + " - " + SubProject2.Name;
	}

	[RelayCommand]
	private async Task Save()
	{
		model.Status.Busy("正在保存数据……");
		string localAbsolutePath = storage.GetLocalAbsolutePath(storage.GetCableTempFileRelativePath());
		await File.WriteAllTextAsync(localAbsolutePath, JsonSerializer.Serialize(Result ?? throw new Exception("没有数据需要保存")));
		model.Status.Reset();
	}

	[RelayCommand]
	private async Task Load()
	{
		model.Status.Busy("正在加载数据……");
		string localAbsolutePath = storage.GetLocalAbsolutePath(storage.GetCableTempFileRelativePath());
		try
		{
			Result = JsonSerializer.Deserialize<MatchCableDataResult>(await File.ReadAllTextAsync(localAbsolutePath));
			model.Status.Reset();
		}
		catch
		{
			throw new Exception("没有数据可以加载");
		}
	}

	[RelayCommand]
	private async Task Overwrite()
	{
		CableViewModel cableViewModel = cable;
		ObservableCollection<CableData> observableCollection = new ObservableCollection<CableData>();
		foreach (CableData success in SuccessList)
		{
			observableCollection.Add(success);
		}
		cableViewModel.AllData = observableCollection;
		await cable.SaveAndUploadRealtimeFileAsync();
		navigation.GoBack();
	}

	[RelayCommand]
	private void Append()
	{
		throw new NotImplementedException();
	}

	[RelayCommand]
	private async Task UnMatch(CableData cable)
	{
		Tuple<TerminationData, TerminationData> tuple = await DataConverter.UnMatchCableData(cable);
		Failist1.Add(new MatchCableDataFail
		{
			Data = tuple.Item1,
			Reason = "拆分"
		});
		Failist2.Add(new MatchCableDataFail
		{
			Data = tuple.Item2,
			Reason = "拆分"
		});
		SuccessList.Remove(cable);
		SuccessDataCount = SuccessList.Count;
	}

	[RelayCommand]
	private async Task Match()
	{
		config_project_major major1 = cable.Major1 ?? throw new Exception("请选择端接数据");
		config_project_major major2 = cable.Major2 ?? throw new Exception("请选择端接数据");
		int id = (SubProject1 ?? throw new Exception("请选择端接数据")).Id;
		int subProjectId2 = (SubProject2 ?? throw new Exception("请选择端接数据")).Id;
		int id2 = (Publish1 ?? throw new Exception("请选择端接数据")).Id;
		int publishId2 = (Publish2 ?? throw new Exception("请选择端接数据")).Id;
		model.Status.Busy("正在获取端接数据……");
		string file1 = await storage.DownloadPublishTerminationFileAsync(id, id2);
		string file2 = await storage.DownloadPublishTerminationFileAsync(subProjectId2, publishId2);
		IEnumerable<TerminationData> data1 = await Task.Run(async () => (await excel.GetDataTableAsStringAsync(file1, hasHeader: true)).StringTableToIEnumerableByDiplay<TerminationData>());
		IEnumerable<TerminationData> duanjieList = await Task.Run(async () => (await excel.GetDataTableAsStringAsync(file2, hasHeader: true)).StringTableToIEnumerableByDiplay<TerminationData>());
		model.Status.Busy("正在匹配端接数据……");
		Result = await DataConverter.MatchCableData(data1, duanjieList, major1.CableSystem, major2.CableSystem);
		ObservableCollection<MatchCableDataFail> observableCollection = new ObservableCollection<MatchCableDataFail>();
		foreach (MatchCableDataFail item in Result.FailList1)
		{
			observableCollection.Add(item);
		}
		Failist1 = observableCollection;
		ObservableCollection<MatchCableDataFail> observableCollection2 = new ObservableCollection<MatchCableDataFail>();
		foreach (MatchCableDataFail item2 in Result.FailList2)
		{
			observableCollection2.Add(item2);
		}
		Failist2 = observableCollection2;
		ObservableCollection<CableData> observableCollection3 = new ObservableCollection<CableData>();
		foreach (CableData success in Result.SuccessList)
		{
			observableCollection3.Add(success);
		}
		SuccessList = observableCollection3;
		SuccessDataCount = Result.SuccessList.Count;
		model.Status.Reset();
	}

	[RelayCommand]
	private void MatchSelected()
	{
		if (Failist1 == null)
		{
			throw new Exception("没有需要匹配的数据");
		}
		if (cable.Major1 == null)
		{
			throw new Exception("没有需要匹配的数据");
		}
		if (cable.Major2 == null)
		{
			throw new Exception("没有需要匹配的数据");
		}
		MatchCableDataFail matchCableDataFail = Failist1.SingleOrDefault((MatchCableDataFail x) => x.IsChecked) ?? throw new Exception("请在两个列表中分别选择一个要匹配的数据");
		MatchCableDataFail matchCableDataFail2 = Failist2.SingleOrDefault((MatchCableDataFail x) => x.IsChecked) ?? throw new Exception("请在两个列表中分别选择一个要匹配的数据");
		model.Status.Busy("正在生成电缆数据……");
		CableData item = DataConverter.MatchCableData(matchCableDataFail.Data, matchCableDataFail2.Data);
		SuccessList.Add(item);
		Failist1.Remove(matchCableDataFail);
		Failist2.Remove(matchCableDataFail2);
		SuccessDataCount = SuccessList.Count;
		model.Status.Reset();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.SubProject1" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.SubProject1" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProject1Changed(config_project_subProject? value)
	{
		Publishes1 = null;
		if (SubProject1 != null)
		{
			Publishes1 = new ObservableCollection<publish_termination>(await (from x in context.Db.Queryable<publish_termination>()
				where x.SubProjectId == SubProject1.Id
				select x).ToListAsync());
			if (Publishes1.Count == 1)
			{
				Publish1 = Publishes1[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.SubProject2" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Cable.MatchViewModel.SubProject2" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProject2Changed(config_project_subProject? value)
	{
		Publishes2 = null;
		if (SubProject2 != null)
		{
			Publishes2 = new ObservableCollection<publish_termination>(await (from x in context.Db.Queryable<publish_termination>()
				where x.SubProjectId == SubProject2.Id
				select x).ToListAsync());
			if (Publishes2.Count == 1)
			{
				Publish2 = Publishes2[0];
			}
		}
	}
}

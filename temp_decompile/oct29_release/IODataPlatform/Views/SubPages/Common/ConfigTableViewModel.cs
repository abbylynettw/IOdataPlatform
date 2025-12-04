using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ConfigTableViewModel(SqlSugarContext context, IMessageService message, INavigationService navigation, GlobalModel model) : ObservableObject(), INavigationAware
{
	[ObservableProperty]
	private string title = string.Empty;

	[ObservableProperty]
	private string description = string.Empty;

	[ObservableProperty]
	private IList? data;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.ConvertBackCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? convertBackCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<object>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<object>? deleteCommand;

	public Type? DataType { get; set; }

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.title" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Title
	{
		get
		{
			return title;
		}
		[MemberNotNull("title")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(title, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Title);
				title = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Title);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.description" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Description
	{
		get
		{
			return description;
		}
		[MemberNotNull("description")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(description, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Description);
				description = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Description);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.data" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IList? Data
	{
		get
		{
			return data;
		}
		set
		{
			if (!EqualityComparer<IList>.Default.Equals(data, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Data);
				data = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Data);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.ConvertBack" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ConvertBackCommand => convertBackCommand ?? (convertBackCommand = new RelayCommand(ConvertBack));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.Edit(System.Object)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<object> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<object>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ConfigTableViewModel.Delete(System.Object)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<object> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<object>(Delete));

	public void OnNavigatedFrom()
	{
	}

	public async void OnNavigatedTo()
	{
		model.Status.Busy("正在获取配置表数据……");
		await Refresh();
		model.Status.Reset();
	}

	[RelayCommand]
	private void ConvertBack()
	{
		navigation.GoBack();
	}

	public async Task Refresh()
	{
		if (DataType == typeof(config_card_type_judge))
		{
			Data = new ObservableCollection<config_card_type_judge>(await context.Db.Queryable<config_card_type_judge>().ToListAsync());
		}
		else if (DataType == typeof(config_terminalboard_type_judge))
		{
			Data = new ObservableCollection<config_terminalboard_type_judge>(await context.Db.Queryable<config_terminalboard_type_judge>().ToListAsync());
		}
		else if (DataType == typeof(config_connection_points))
		{
			Data = new ObservableCollection<config_connection_points>(await context.Db.Queryable<config_connection_points>().ToListAsync());
		}
		else if (DataType == typeof(config_power_supply_method))
		{
			Data = new ObservableCollection<config_power_supply_method>(await context.Db.Queryable<config_power_supply_method>().ToListAsync());
		}
		else if (DataType == typeof(config_output_format_values))
		{
			Data = new ObservableCollection<config_output_format_values>(await context.Db.Queryable<config_output_format_values>().ToListAsync());
		}
		else if (DataType == typeof(config_cable_categoryAndColor))
		{
			Data = new ObservableCollection<config_cable_categoryAndColor>(await context.Db.Queryable<config_cable_categoryAndColor>().ToListAsync());
		}
		else if (DataType == typeof(config_cable_function))
		{
			Data = new ObservableCollection<config_cable_function>(await context.Db.Queryable<config_cable_function>().ToListAsync());
		}
		else if (DataType == typeof(config_cable_spec))
		{
			Data = new ObservableCollection<config_cable_spec>(await context.Db.Queryable<config_cable_spec>().ToListAsync());
		}
		else if (DataType == typeof(config_cable_startNumber))
		{
			Data = new ObservableCollection<config_cable_startNumber>(await context.Db.Queryable<config_cable_startNumber>().ToListAsync());
		}
		else if (DataType == typeof(config_cable_systemNumber))
		{
			Data = new ObservableCollection<config_cable_systemNumber>(await context.Db.Queryable<config_cable_systemNumber>().ToListAsync());
		}
		else if (DataType == typeof(config_termination_yjs))
		{
			Data = new ObservableCollection<config_termination_yjs>(await context.Db.Queryable<config_termination_yjs>().ToListAsync());
		}
		else if (DataType == typeof(config_aqj_analog))
		{
			Data = new ObservableCollection<config_aqj_analog>(await context.Db.Queryable<config_aqj_analog>().ToListAsync());
		}
		else if (DataType == typeof(config_aqj_control))
		{
			Data = new ObservableCollection<config_aqj_control>(await context.Db.Queryable<config_aqj_control>().ToListAsync());
		}
		else if (DataType == typeof(config_aqj_stationReplace))
		{
			Data = new ObservableCollection<config_aqj_stationReplace>(await context.Db.Queryable<config_aqj_stationReplace>().ToListAsync());
		}
		else if (DataType == typeof(config_aqj_tagReplace))
		{
			Data = new ObservableCollection<config_aqj_tagReplace>(await context.Db.Queryable<config_aqj_tagReplace>().ToListAsync());
		}
		else if (DataType == typeof(config_aqj_stationCabinet))
		{
			Data = new ObservableCollection<config_aqj_stationCabinet>(await context.Db.Queryable<config_aqj_stationCabinet>().ToListAsync());
		}
		else if (DataType == typeof(config_controlSystem_mapping))
		{
			Data = new ObservableCollection<config_controlSystem_mapping>(await context.Db.Queryable<config_controlSystem_mapping>().ToListAsync());
		}
	}

	[RelayCommand]
	private async Task Add()
	{
		Type type = DataType ?? throw new Exception("出现问题，请返回上级页面");
		object obj = Activator.CreateInstance(type) ?? throw new Exception("无法创建类型实例");
		if (!CreateBuilder(obj, "添加").EditWithWpfUI())
		{
			return;
		}
		if (DataType == typeof(config_card_type_judge))
		{
			config_card_type_judge config_card_type_judge = (config_card_type_judge)obj;
			if (string.IsNullOrEmpty(config_card_type_judge.IoCardType))
			{
				model.Status.Error("IO卡型号不能为空。");
				return;
			}
			if (data != null)
			{
				foreach (config_card_type_judge datum in data)
				{
					if (datum.IoCardType.Equals(config_card_type_judge.IoCardType))
					{
						model.Status.Error("IO卡型号已存在，不能重复添加。");
						return;
					}
				}
			}
			await context.Db.Insertable<config_card_type_judge>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_connection_points))
		{
			await context.Db.Insertable<config_connection_points>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_output_format_values))
		{
			await context.Db.Insertable<config_output_format_values>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_power_supply_method))
		{
			await context.Db.Insertable<config_power_supply_method>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_terminalboard_type_judge))
		{
			await context.Db.Insertable<config_terminalboard_type_judge>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_categoryAndColor))
		{
			await context.Db.Insertable<config_cable_categoryAndColor>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_function))
		{
			await context.Db.Insertable<config_cable_function>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_spec))
		{
			await context.Db.Insertable<config_cable_spec>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_startNumber))
		{
			await context.Db.Insertable<config_cable_startNumber>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_systemNumber))
		{
			await context.Db.Insertable<config_cable_systemNumber>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_termination_yjs))
		{
			await context.Db.Insertable<config_termination_yjs>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_analog))
		{
			await context.Db.Insertable<config_aqj_analog>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_control))
		{
			await context.Db.Insertable<config_aqj_control>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_stationReplace))
		{
			await context.Db.Insertable<config_aqj_stationReplace>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_tagReplace))
		{
			await context.Db.Insertable<config_aqj_tagReplace>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_stationCabinet))
		{
			await context.Db.Insertable<config_aqj_stationCabinet>(obj).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_controlSystem_mapping))
		{
			await context.Db.Insertable<config_controlSystem_mapping>(obj).ExecuteCommandAsync();
		}
		await Refresh();
		model.Status.Success("已添加");
	}

	[RelayCommand]
	private async Task Edit(object obj)
	{
		if (DataType == typeof(config_card_type_judge))
		{
			config_card_type_judge config_card_type_judge = obj.AsTrue<config_card_type_judge>().JsonClone();
			if (!CreateBuilder(config_card_type_judge, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_card_type_judge).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_connection_points))
		{
			config_connection_points config_connection_points = obj.AsTrue<config_connection_points>().JsonClone();
			if (!CreateBuilder(config_connection_points, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_connection_points).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_output_format_values))
		{
			config_output_format_values config_output_format_values = obj.AsTrue<config_output_format_values>().JsonClone();
			if (!CreateBuilder(config_output_format_values, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_output_format_values).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_power_supply_method))
		{
			config_power_supply_method config_power_supply_method = obj.AsTrue<config_power_supply_method>().JsonClone();
			if (!CreateBuilder(config_power_supply_method, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_power_supply_method).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_terminalboard_type_judge))
		{
			config_terminalboard_type_judge config_terminalboard_type_judge = obj.AsTrue<config_terminalboard_type_judge>().JsonClone();
			if (!CreateBuilder(config_terminalboard_type_judge, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_terminalboard_type_judge).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_systemNumber))
		{
			config_cable_systemNumber config_cable_systemNumber = obj.AsTrue<config_cable_systemNumber>().JsonClone();
			if (!CreateBuilder(config_cable_systemNumber, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_cable_systemNumber).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_categoryAndColor))
		{
			config_cable_categoryAndColor config_cable_categoryAndColor = obj.AsTrue<config_cable_categoryAndColor>().JsonClone();
			if (!CreateBuilder(config_cable_categoryAndColor, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_cable_categoryAndColor).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_function))
		{
			config_cable_function config_cable_function = obj.AsTrue<config_cable_function>().JsonClone();
			if (!CreateBuilder(config_cable_function, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_cable_function).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_spec))
		{
			config_cable_spec config_cable_spec = obj.AsTrue<config_cable_spec>().JsonClone();
			if (!CreateBuilder(config_cable_spec, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_cable_spec).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_cable_startNumber))
		{
			config_cable_startNumber config_cable_startNumber = obj.AsTrue<config_cable_startNumber>().JsonClone();
			if (!CreateBuilder(config_cable_startNumber, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_cable_startNumber).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_termination_yjs))
		{
			config_termination_yjs config_termination_yjs = obj.AsTrue<config_termination_yjs>().JsonClone();
			if (!CreateBuilder(config_termination_yjs, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_termination_yjs).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_analog))
		{
			config_aqj_analog config_aqj_analog = obj.AsTrue<config_aqj_analog>().JsonClone();
			if (!CreateBuilder(config_aqj_analog, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_aqj_analog).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_control))
		{
			config_aqj_control config_aqj_control = obj.AsTrue<config_aqj_control>().JsonClone();
			if (!CreateBuilder(config_aqj_control, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_aqj_control).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_stationReplace))
		{
			config_aqj_stationReplace config_aqj_stationReplace = obj.AsTrue<config_aqj_stationReplace>().JsonClone();
			if (!CreateBuilder(config_aqj_stationReplace, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_aqj_stationReplace).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_tagReplace))
		{
			config_aqj_tagReplace config_aqj_tagReplace = obj.AsTrue<config_aqj_tagReplace>().JsonClone();
			if (!CreateBuilder(config_aqj_tagReplace, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_aqj_tagReplace).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_aqj_stationCabinet))
		{
			config_aqj_stationCabinet config_aqj_stationCabinet = obj.AsTrue<config_aqj_stationCabinet>().JsonClone();
			if (!CreateBuilder(config_aqj_stationCabinet, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_aqj_stationCabinet).ExecuteCommandAsync();
		}
		else if (DataType == typeof(config_controlSystem_mapping))
		{
			config_controlSystem_mapping config_controlSystem_mapping = obj.AsTrue<config_controlSystem_mapping>().JsonClone();
			if (!CreateBuilder(config_controlSystem_mapping, "编辑").EditWithWpfUI())
			{
				return;
			}
			await context.Db.Updateable(config_controlSystem_mapping).ExecuteCommandAsync();
		}
		await Refresh();
		model.Status.Success("已修改");
	}

	[RelayCommand]
	private async Task Delete(object obj)
	{
		if (await message.ConfirmAsync("确认删除"))
		{
			if (obj is config_card_type_judge config_card_type_judge)
			{
				await context.Db.Deleteable<config_card_type_judge>().In(config_card_type_judge.Id).ExecuteCommandAsync();
			}
			else if (obj is config_connection_points config_connection_points)
			{
				await context.Db.Deleteable<config_connection_points>().In(config_connection_points.Id).ExecuteCommandAsync();
			}
			else if (obj is config_output_format_values config_output_format_values)
			{
				await context.Db.Deleteable<config_output_format_values>().In(config_output_format_values.Id).ExecuteCommandAsync();
			}
			else if (obj is config_power_supply_method config_power_supply_method)
			{
				await context.Db.Deleteable<config_power_supply_method>().In(config_power_supply_method.Id).ExecuteCommandAsync();
			}
			else if (obj is config_terminalboard_type_judge config_terminalboard_type_judge)
			{
				await context.Db.Deleteable<config_terminalboard_type_judge>().In(config_terminalboard_type_judge.Id).ExecuteCommandAsync();
			}
			else if (obj is config_cable_categoryAndColor config_cable_categoryAndColor)
			{
				await context.Db.Deleteable<config_cable_categoryAndColor>().In(config_cable_categoryAndColor.Id).ExecuteCommandAsync();
			}
			else if (obj is config_cable_function config_cable_function)
			{
				await context.Db.Deleteable<config_cable_function>().In(config_cable_function.Id).ExecuteCommandAsync();
			}
			else if (obj is config_cable_spec config_cable_spec)
			{
				await context.Db.Deleteable<config_cable_spec>().In(config_cable_spec.Id).ExecuteCommandAsync();
			}
			else if (obj is config_cable_startNumber config_cable_startNumber)
			{
				await context.Db.Deleteable<config_cable_startNumber>().In(config_cable_startNumber.Id).ExecuteCommandAsync();
			}
			else if (obj is config_cable_systemNumber config_cable_systemNumber)
			{
				await context.Db.Deleteable<config_cable_systemNumber>().In(config_cable_systemNumber.Id).ExecuteCommandAsync();
			}
			else if (obj is config_termination_yjs config_termination_yjs)
			{
				await context.Db.Deleteable<config_termination_yjs>().In(config_termination_yjs.Id).ExecuteCommandAsync();
			}
			else if (obj is config_aqj_analog config_aqj_analog)
			{
				await context.Db.Deleteable<config_aqj_analog>().In(config_aqj_analog.Id).ExecuteCommandAsync();
			}
			else if (obj is config_aqj_control config_aqj_control)
			{
				await context.Db.Deleteable<config_aqj_control>().In(config_aqj_control.Id).ExecuteCommandAsync();
			}
			else if (obj is config_aqj_stationReplace config_aqj_stationReplace)
			{
				await context.Db.Deleteable<config_aqj_stationReplace>().In(config_aqj_stationReplace.Id).ExecuteCommandAsync();
			}
			else if (obj is config_aqj_tagReplace config_aqj_tagReplace)
			{
				await context.Db.Deleteable<config_aqj_tagReplace>().In(config_aqj_tagReplace.Id).ExecuteCommandAsync();
			}
			else if (obj is config_aqj_stationCabinet config_aqj_stationCabinet)
			{
				await context.Db.Deleteable<config_aqj_stationCabinet>().In(config_aqj_stationCabinet.Id).ExecuteCommandAsync();
			}
			else if (obj is config_controlSystem_mapping config_controlSystem_mapping)
			{
				await context.Db.Deleteable<config_controlSystem_mapping>().In(config_controlSystem_mapping.Id).ExecuteCommandAsync();
			}
			await Refresh();
			model.Status.Success("已删除");
		}
	}

	private EditorOptions CreateBuilder(object obj, string title)
	{
		if (!(obj is config_card_type_judge obj2))
		{
			if (!(obj is config_connection_points obj3))
			{
				if (!(obj is config_output_format_values obj4))
				{
					if (!(obj is config_power_supply_method obj5))
					{
						if (!(obj is config_terminalboard_type_judge obj6))
						{
							if (!(obj is config_cable_categoryAndColor obj7))
							{
								if (!(obj is config_cable_function obj8))
								{
									if (!(obj is config_cable_spec obj9))
									{
										if (!(obj is config_cable_startNumber obj10))
										{
											if (!(obj is config_cable_systemNumber obj11))
											{
												if (!(obj is config_termination_yjs obj12))
												{
													if (!(obj is config_aqj_analog obj13))
													{
														if (!(obj is config_aqj_control obj14))
														{
															if (!(obj is config_aqj_stationReplace obj15))
															{
																if (!(obj is config_aqj_tagReplace obj16))
																{
																	if (!(obj is config_aqj_stationCabinet obj17))
																	{
																		if (obj is config_controlSystem_mapping obj18)
																		{
																			return BuildEditorOptions(obj18.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600.0));
																		}
																		throw new NotImplementedException("开发者注意：此类型未实现");
																	}
																	return BuildEditorOptions(obj17.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600.0));
																}
																return BuildEditorOptions(obj16.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600.0));
															}
															return BuildEditorOptions(obj15.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600.0));
														}
														return BuildEditorOptions(obj14.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600.0));
													}
													return BuildEditorOptions(obj13.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600.0));
												}
												return BuildEditorOptions(obj12.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600.0));
											}
											return BuildEditorOptions(obj11.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
										}
										return BuildEditorOptions(obj10.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
									}
									return BuildEditorOptions(obj9.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
								}
								return BuildEditorOptions(obj8.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
							}
							return BuildEditorOptions(obj7.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
						}
						return BuildEditorOptions(obj6.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
					}
					return BuildEditorOptions(obj5.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
				}
				return BuildEditorOptions(obj4.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
			}
			return BuildEditorOptions(obj3.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600.0));
		}
		return BuildEditorOptions(obj2.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380.0));
	}

	private EditorOptions BuildEditorOptions<T>(EditorOptionBuilder<T> builder)
	{
		PropertyInfo[] properties = typeof(T).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			DisplayAttribute customAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
			if (customAttribute != null && !(customAttribute.GetAutoGenerateField() ?? true))
			{
				continue;
			}
			string name = propertyInfo.Name;
			string text = customAttribute?.Name ?? name;
			if (typeof(T) == typeof(config_controlSystem_mapping) && name == "StdField")
			{
				continue;
			}
			if (propertyInfo.PropertyType == typeof(string))
			{
				builder.AddProperty<string>(name).WithHeader(text).WithPlaceHoplder("请输入" + text)
					.EditAsText();
				continue;
			}
			if (propertyInfo.PropertyType == typeof(double))
			{
				builder.AddProperty<double>(name).WithHeader(text).WithPlaceHoplder("请输入" + text)
					.EditAsDouble();
				continue;
			}
			if (propertyInfo.PropertyType == typeof(int))
			{
				builder.AddProperty<int>(name).WithHeader(text).WithPlaceHoplder("请输入" + text)
					.EditAsInt();
				continue;
			}
			if (propertyInfo.PropertyType == typeof(Department))
			{
				builder.AddProperty<Department>(name).WithHeader(text).WithPlaceHoplder("请输入" + text)
					.EditAsInt();
				continue;
			}
			if (propertyInfo.PropertyType == typeof(bool))
			{
				builder.AddProperty<bool>(name).WithHeader(text).WithPlaceHoplder("请输入" + text)
					.EditAsBoolean()
					.ConvertFromProperty((bool x) => x)
					.ConvertToProperty((bool x) => x);
				continue;
			}
			throw new NotImplementedException("开发者注意：此类型未实现");
		}
		return builder.Build();
	}
}

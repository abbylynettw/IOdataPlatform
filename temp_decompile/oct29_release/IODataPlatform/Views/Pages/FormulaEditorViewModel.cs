using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 公式编辑器页面视图模型类
/// 提供复杂的公式编辑和管理功能，支持多控制系统的字段映射和条件匹配
/// 实现基于表达式树的复杂逻辑判断，支持子条件的层次化组织
/// 广泛用于不同控制系统间的数据映射和自动化数据处理
/// </summary>
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class FormulaEditorViewModel(SqlSugarContext context, IMessageService message) : ObservableObject()
{
	/// <summary>当前选中的目标控制系统类型</summary>
	[ObservableProperty]
	private FormulaEditorType? targetType;

	/// <summary>当前可用的所有字段集合，根据选中的目标类型动态生成</summary>
	[ObservableProperty]
	private ObservableCollection<FormulaEditorField>? fields;

	/// <summary>当前选中的字段对象，用于公式编辑</summary>
	[ObservableProperty]
	private FormulaEditorField? field;

	/// <summary>当前公式的所有索引项集合，按索引顺序排列</summary>
	[ObservableProperty]
	private ObservableCollection<formular_Index>? indecis;

	/// <summary>当前选中的索引项，用于管理具体的条件集合</summary>
	[ObservableProperty]
	private formular_Index? index;

	/// <summary>当前索引项下的所有条件集合，按条件顺序排列</summary>
	[ObservableProperty]
	private ObservableCollection<formular_index_condition>? conditions;

	/// <summary>当前正在编辑的公式对象</summary>
	[ObservableProperty]
	private formular? formula;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.AddIndexCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addIndexCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.EditIndexCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<formular_Index>? editIndexCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.DeleteIndexCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<formular_Index>? deleteIndexCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.ViewIndexCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<formular_Index>? viewIndexCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.MoveUpIndexCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<formular_Index>? moveUpIndexCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.MoveDownIndexCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<formular_Index>? moveDownIndexCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.AddConditionCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addConditionCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.EditConditionCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<formular_index_condition>? editConditionCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.DeleteConditionCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<formular_index_condition>? deleteConditionCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.MoveUpConditionCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<formular_index_condition>? moveUpConditionCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.FormulaEditorViewModel.MoveDownConditionCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<formular_index_condition>? moveDownConditionCommand;

	/// <summary>
	/// 所有可用的目标控制系统类型集合
	/// 包含龙鳞、中控、龙核、一室四种主要控制系统
	/// 每种系统都对应IoFullData类型的数据结构
	/// </summary>
	public ObservableCollection<FormulaEditorType> TargetTypes { get; } = new ObservableCollection<FormulaEditorType>
	{
		new FormulaEditorType(ControlSystem.龙鳍, typeof(IoFullData)),
		new FormulaEditorType(ControlSystem.中控, typeof(IoFullData)),
		new FormulaEditorType(ControlSystem.龙核, typeof(IoFullData)),
		new FormulaEditorType(ControlSystem.一室, typeof(IoFullData))
	};

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.FormulaEditorViewModel.targetType" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public FormulaEditorType? TargetType
	{
		get
		{
			return targetType;
		}
		set
		{
			if (!EqualityComparer<FormulaEditorType>.Default.Equals(targetType, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.TargetType);
				targetType = value;
				OnTargetTypeChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.TargetType);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.FormulaEditorViewModel.fields" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<FormulaEditorField>? Fields
	{
		get
		{
			return fields;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<FormulaEditorField>>.Default.Equals(fields, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Fields);
				fields = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Fields);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.FormulaEditorViewModel.field" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public FormulaEditorField? Field
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<FormulaEditorField>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Field);
				field = value;
				OnFieldChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Field);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.FormulaEditorViewModel.indecis" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<formular_Index>? Indecis
	{
		get
		{
			return indecis;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<formular_Index>>.Default.Equals(indecis, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Indecis);
				indecis = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Indecis);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.FormulaEditorViewModel.index" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public formular_Index? Index
	{
		get
		{
			return index;
		}
		set
		{
			if (!EqualityComparer<formular_Index>.Default.Equals(index, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Index);
				index = value;
				OnIndexChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Index);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.FormulaEditorViewModel.conditions" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<formular_index_condition>? Conditions
	{
		get
		{
			return conditions;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<formular_index_condition>>.Default.Equals(conditions, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Conditions);
				conditions = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Conditions);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.FormulaEditorViewModel.formula" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public formular? Formula
	{
		get
		{
			return formula;
		}
		set
		{
			if (!EqualityComparer<formular>.Default.Equals(formula, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Formula);
				formula = value;
				OnFormulaChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Formula);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.AddIndex" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddIndexCommand => addIndexCommand ?? (addIndexCommand = new AsyncRelayCommand(AddIndex));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.EditIndex(IODataPlatform.Models.DBModels.formular_Index)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<formular_Index> EditIndexCommand => editIndexCommand ?? (editIndexCommand = new AsyncRelayCommand<formular_Index>(EditIndex));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.DeleteIndex(IODataPlatform.Models.DBModels.formular_Index)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<formular_Index> DeleteIndexCommand => deleteIndexCommand ?? (deleteIndexCommand = new AsyncRelayCommand<formular_Index>(DeleteIndex));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.ViewIndex(IODataPlatform.Models.DBModels.formular_Index)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<formular_Index> ViewIndexCommand => viewIndexCommand ?? (viewIndexCommand = new RelayCommand<formular_Index>(ViewIndex));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.MoveUpIndex(IODataPlatform.Models.DBModels.formular_Index)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<formular_Index> MoveUpIndexCommand => moveUpIndexCommand ?? (moveUpIndexCommand = new AsyncRelayCommand<formular_Index>(MoveUpIndex));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.MoveDownIndex(IODataPlatform.Models.DBModels.formular_Index)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<formular_Index> MoveDownIndexCommand => moveDownIndexCommand ?? (moveDownIndexCommand = new AsyncRelayCommand<formular_Index>(MoveDownIndex));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.AddCondition" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddConditionCommand => addConditionCommand ?? (addConditionCommand = new AsyncRelayCommand(AddCondition));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.EditCondition(IODataPlatform.Models.DBModels.formular_index_condition)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<formular_index_condition> EditConditionCommand => editConditionCommand ?? (editConditionCommand = new AsyncRelayCommand<formular_index_condition>(EditCondition));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.DeleteCondition(IODataPlatform.Models.DBModels.formular_index_condition)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<formular_index_condition> DeleteConditionCommand => deleteConditionCommand ?? (deleteConditionCommand = new AsyncRelayCommand<formular_index_condition>(DeleteCondition));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.MoveUpCondition(IODataPlatform.Models.DBModels.formular_index_condition)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<formular_index_condition> MoveUpConditionCommand => moveUpConditionCommand ?? (moveUpConditionCommand = new AsyncRelayCommand<formular_index_condition>(MoveUpCondition));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.FormulaEditorViewModel.MoveDownCondition(IODataPlatform.Models.DBModels.formular_index_condition)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<formular_index_condition> MoveDownConditionCommand => moveDownConditionCommand ?? (moveDownConditionCommand = new AsyncRelayCommand<formular_index_condition>(MoveDownCondition));

	/// <summary>
	/// 添加索引项命令
	/// 创建新的索引项并通过编辑器对话框供用户编辑
	/// 自动计算新索引的顺序号，确保索引的连续性
	/// </summary>
	/// <returns>异步任务，表示添加操作的完成</returns>
	[RelayCommand]
	private async Task AddIndex()
	{
		if (Formula == null)
		{
			throw new Exception("开发人员注意");
		}
		if (Indecis == null)
		{
			throw new Exception("开发人员注意");
		}
		formular_Index formular_Index = new formular_Index
		{
			FormulaId = Formula.Id
		};
		if (CreateEidtorBuilder(formular_Index, "添加").EditWithWpfUI())
		{
			formular_Index.Index = ((!Indecis.Any()) ? 1 : (Indecis.Max((formular_Index x) => x.Index) + 1));
			await context.Db.Insertable(formular_Index).ExecuteCommandAsync();
			OnFormulaChanged(null);
		}
	}

	/// <summary>
	/// 编辑索引项命令
	/// 创建索引项的副本并通过编辑器对话框供用户修改
	/// 更新数据库后自动刷新界面显示
	/// </summary>
	/// <param name="data">要编辑的索引项对象</param>
	/// <returns>异步任务，表示编辑操作的完成</returns>
	[RelayCommand]
	private async Task EditIndex(formular_Index data)
	{
		if (Formula == null)
		{
			throw new Exception("开发人员注意");
		}
		if (Indecis == null)
		{
			throw new Exception("开发人员注意");
		}
		formular_Index updateObj = new formular_Index().CopyPropertiesFrom(data);
		if (CreateEidtorBuilder(updateObj, "编辑").EditWithWpfUI())
		{
			await context.Db.Updateable(updateObj).ExecuteCommandAsync();
			OnFormulaChanged(null);
		}
	}

	/// <summary>
	/// 删除索引项命令
	/// 先删除该索引下的所有条件，再删除索引本身
	/// 删除后自动重新排列剩余索引的顺序号，保持连续性
	/// </summary>
	/// <param name="data">要删除的索引项对象</param>
	/// <returns>异步任务，表示删除操作的完成</returns>
	[RelayCommand]
	private async Task DeleteIndex(formular_Index data)
	{
		if (await message.ConfirmAsync("确认删除"))
		{
			if (Indecis == null)
			{
				throw new Exception("开发人员注意");
			}
			await (from x in context.Db.Deleteable<formular_index_condition>()
				where x.IndexId == data.Id
				select x).ExecuteCommandAsync();
			await context.Db.Deleteable(data).ExecuteCommandAsync();
			for (int num = 0; num < Indecis.Count; num++)
			{
				Indecis[num].Index = num + 1;
			}
			await context.Db.Updateable(Indecis.ToList()).ExecuteCommandAsync();
			OnFormulaChanged(null);
		}
	}

	/// <summary>
	/// 查看索引项命令
	/// 设置当前选中的索引项，触发条件列表的加载
	/// 用于在界面上显示该索引项的详细条件
	/// </summary>
	/// <param name="data">要查看的索引项对象</param>
	[RelayCommand]
	private void ViewIndex(formular_Index data)
	{
		Index = data;
	}

	/// <summary>
	/// 上移索引项命令
	/// 将指定的索引项在列表中上移一位，重新排列所有索引项的顺序号
	/// 如果已经在最顶部则不执行任何操作
	/// </summary>
	/// <param name="data">要上移的索引项对象</param>
	/// <returns>异步任务，表示移动操作的完成</returns>
	[RelayCommand]
	private async Task MoveUpIndex(formular_Index data)
	{
		if (Indecis == null)
		{
			throw new Exception("开发人员注意");
		}
		int num = Indecis.IndexOf(data);
		if (num != 0)
		{
			Indecis.Move(num - 1, num);
			for (int i = 0; i < Indecis.Count; i++)
			{
				Indecis[i].Index = i + 1;
			}
			await context.Db.Updateable(Indecis.ToList()).ExecuteCommandAsync();
			OnFormulaChanged(null);
		}
	}

	/// <summary>
	/// 下移索引项命令
	/// 将指定的索引项在列表中下移一位，重新排列所有索引项的顺序号
	/// 如果已经在最底部则不执行任何操作
	/// </summary>
	/// <param name="data">要下移的索引项对象</param>
	/// <returns>异步任务，表示移动操作的完成</returns>
	[RelayCommand]
	private async Task MoveDownIndex(formular_Index data)
	{
		if (Indecis == null)
		{
			throw new Exception("开发人员注意");
		}
		int num = Indecis.IndexOf(data);
		if (num != Indecis.Count - 1)
		{
			Indecis.Move(num, num + 1);
			for (int i = 0; i < Indecis.Count; i++)
			{
				Indecis[i].Index = i + 1;
			}
			await context.Db.Updateable(Indecis.ToList()).ExecuteCommandAsync();
			OnFormulaChanged(null);
		}
	}

	/// <summary>
	/// 添加条件命令
	/// 为当前选中的索引项添加新的条件
	/// 自动计算新条件的顺序号，确保条件的连续性
	/// </summary>
	/// <returns>异步任务，表示添加操作的完成</returns>
	/// <exception cref="T:System.Exception">当未选中索引项时抛出异常</exception>
	[RelayCommand]
	private async Task AddCondition()
	{
		if (Index == null)
		{
			throw new Exception("请点击顺序表中的查看按钮");
		}
		if (Conditions == null)
		{
			throw new Exception("开发人员注意");
		}
		formular_index_condition formular_index_condition = new formular_index_condition
		{
			IndexId = Index.Id
		};
		if (CreateEidtorBuilder(formular_index_condition, "添加").EditWithWpfUI())
		{
			formular_index_condition.Index = ((!Conditions.Any()) ? 1 : (Conditions.Max((formular_index_condition x) => x.Index) + 1));
			await context.Db.Insertable(formular_index_condition).ExecuteCommandAsync();
			OnIndexChanged(null);
		}
	}

	/// <summary>
	/// 编辑条件命令
	/// 创建条件的副本并通过编辑器对话框供用户修改
	/// 更新数据库后自动刷新界面显示
	/// </summary>
	/// <param name="data">要编辑的条件对象</param>
	/// <returns>异步任务，表示编辑操作的完成</returns>
	[RelayCommand]
	private async Task EditCondition(formular_index_condition data)
	{
		formular_index_condition formular_index_condition = new formular_index_condition().CopyPropertiesFrom(data);
		if (CreateEidtorBuilder(formular_index_condition, "编辑").EditWithWpfUI())
		{
			await context.Db.Updateable(formular_index_condition).ExecuteCommandAsync();
			OnIndexChanged(null);
		}
	}

	/// <summary>
	/// 删除条件命令
	/// 删除指定的条件后自动重新排列剩余条件的顺序号，保持连续性
	/// 需要用户确认后才执行删除操作
	/// </summary>
	/// <param name="data">要删除的条件对象</param>
	/// <returns>异步任务，表示删除操作的完成</returns>
	[RelayCommand]
	private async Task DeleteCondition(formular_index_condition data)
	{
		if (Conditions == null)
		{
			throw new Exception("开发人员注意");
		}
		if (await message.ConfirmAsync("确认删除"))
		{
			await context.Db.Deleteable(data).ExecuteCommandAsync();
			for (int i = 0; i < Conditions.Count; i++)
			{
				Conditions[i].Index = i + 1;
			}
			await context.Db.Updateable(Conditions.ToList()).ExecuteCommandAsync();
			OnIndexChanged(null);
		}
	}

	/// <summary>
	/// 上移条件命令
	/// 将指定的条件在列表中上移一位，重新排列所有条件的顺序号
	/// 如果已经在最顶部则不执行任何操作
	/// </summary>
	/// <param name="data">要上移的条件对象</param>
	/// <returns>异步任务，表示移动操作的完成</returns>
	[RelayCommand]
	private async Task MoveUpCondition(formular_index_condition data)
	{
		if (Conditions == null)
		{
			throw new Exception("开发人员注意");
		}
		int num = Conditions.IndexOf(data);
		if (num != 0)
		{
			Conditions.Move(num - 1, num);
			for (int i = 0; i < Conditions.Count; i++)
			{
				Conditions[i].Index = i + 1;
			}
			await context.Db.Updateable(Conditions.ToList()).ExecuteCommandAsync();
			OnIndexChanged(null);
		}
	}

	/// <summary>
	/// 下移条件命令
	/// 将指定的条件在列表中下移一位，重新排列所有条件的顺序号
	/// 注意：这里的逻辑有问题，实际上在执行下移操作
	/// </summary>
	/// <param name="data">要下移的条件对象</param>
	/// <returns>异步任务，表示移动操作的完成</returns>
	[RelayCommand]
	private async Task MoveDownCondition(formular_index_condition data)
	{
		if (Conditions == null)
		{
			throw new Exception("开发人员注意");
		}
		int num = Conditions.IndexOf(data);
		if (num != Conditions.Count - 1)
		{
			Conditions.Move(num, num + 1);
			for (int i = 0; i < Conditions.Count; i++)
			{
				Conditions[i].Index = i + 1;
			}
			await context.Db.Updateable(Conditions.ToList()).ExecuteCommandAsync();
			OnIndexChanged(null);
		}
	}

	/// <summary>
	/// 基于表达式树的公式匹配算法
	/// 使用动态表达式编译和执行技术，实现复杂条件的高性能匹配
	/// 支持多种字段运算符（等于、不等于、包含等）和条件连接符（并且、或）
	/// 按照索引顺序优先级进行匹配，返回第一个满足条件的结果值
	/// </summary>
	/// <typeparam name="T">目标对象类型，必须是引用类型</typeparam>
	/// <param name="targetTypeInstance">要匹配的目标对象实例</param>
	/// <returns>匹配成功的返回值，没有匹配时返回"Err"</returns>
	/// <exception cref="T:System.Exception">当找不到对应字段的公式时抛出异常</exception>
	/// <exception cref="T:System.NotSupportedException">当遇到不支持的运算符时抛出异常</exception>
	public string FindMatchingFormulaReturnValueAsyncByExpression<T>(T targetTypeInstance) where T : class
	{
		int? fieldIdToGet = (from q in context.Db.Queryable<formular>()
			where q.TargetType == TargetType.Name.ToString() && q.FieldName == Field.OwnDisplayName
			select q).ToList().FirstOrDefault()?.Id;
		if (!fieldIdToGet.HasValue)
		{
			throw new Exception("未找到该字段的公式");
		}
		List<formular_Index> list = (from x in context.Db.Queryable<formular_Index>()
			where (int?)x.FormulaId == fieldIdToGet
			orderby x.Index
			select x).ToList();
		foreach (formular_Index value in list)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "target");
			Expression expression = null;
			List<formular_index_condition> list2 = (from c in context.Db.Queryable<formular_index_condition>()
				where c.IndexId == value.Id
				orderby c.Index
				select c).ToList();
			foreach (formular_index_condition condition in list2)
			{
				PropertyInfo propertyInfo = typeof(T).GetProperties().FirstOrDefault((PropertyInfo a) => a.Name == condition.PropertyName);
				if (propertyInfo == null)
				{
					continue;
				}
				object value2 = propertyInfo.GetValue(targetTypeInstance) ?? "";
				ConstantExpression constantExpression = Expression.Constant(value2);
				ConstantExpression constantExpression2 = Expression.Constant(Convert.ChangeType(condition.FieldValue, propertyInfo.PropertyType), propertyInfo.PropertyType);
				Expression expression2;
				switch (condition.FieldOperator)
				{
				case FieldOperator.等于:
					expression2 = Expression.Equal(constantExpression, constantExpression2);
					break;
				case FieldOperator.不等于:
					expression2 = Expression.NotEqual(constantExpression, constantExpression2);
					break;
				case FieldOperator.包含:
					if (propertyInfo.PropertyType == typeof(string))
					{
						expression2 = Expression.Call(constantExpression, "Contains", null, constantExpression2);
						break;
					}
					goto default;
				case FieldOperator.不包含:
					if (propertyInfo.PropertyType == typeof(string))
					{
						expression2 = Expression.Not(Expression.Call(constantExpression, "Contains", null, constantExpression2));
						break;
					}
					goto default;
				case FieldOperator.起始于:
					if (propertyInfo.PropertyType == typeof(string))
					{
						expression2 = Expression.Call(constantExpression, "StartsWith", null, constantExpression2);
						break;
					}
					goto default;
				case FieldOperator.终止于:
					if (propertyInfo.PropertyType == typeof(string))
					{
						expression2 = Expression.Call(constantExpression, "EndsWith", null, constantExpression2);
						break;
					}
					goto default;
				default:
					throw new NotSupportedException($"不支持的运算符: {condition.FieldOperator}");
				}
				Expression expression3 = expression2;
				Expression expression4 = ((expression != null) ? (condition.ConditionOperator switch
				{
					ConditionOperator.并且 => Expression.AndAlso(expression, expression3), 
					ConditionOperator.或 => Expression.OrElse(expression, expression3), 
					ConditionOperator.无 => expression, 
					_ => throw new NotSupportedException($"不支持的连接运算符: {condition.ConditionOperator}"), 
				}) : expression3);
				expression = expression4;
			}
			if (expression != null)
			{
				Func<T, bool> func = Expression.Lambda<Func<T, bool>>(expression, new ParameterExpression[1] { parameterExpression }).Compile();
				if (func(targetTypeInstance))
				{
					return value.ReturnValue;
				}
			}
		}
		return "Err";
	}

	/// <summary>
	/// 创建索引项编辑器的静态辅助方法
	/// 配置索引项编辑器的布局和属性设置
	/// 主要用于编辑索引项的返回值字段
	/// </summary>
	/// <param name="index">要编辑的索引项对象</param>
	/// <param name="title">编辑器对话框的标题</param>
	/// <returns>配置好的编辑器选项</returns>
	private static EditorOptions CreateEidtorBuilder(formular_Index index, string title)
	{
		EditorOptionBuilder<formular_Index> editorOptionBuilder = index.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithEditorHeight(150.0);
		editorOptionBuilder.AddProperty<string>("ReturnValue").WithHeader("返回值").EditAsText();
		return editorOptionBuilder.Build();
	}

	/// <summary>
	/// 创建条件编辑器的实例方法
	/// 配置条件编辑器的复杂布局和验证逻辑
	/// 包含条件运算符、字段名、字段运算符和字段值的编辑
	/// 自动根据当前目标类型生成可用字段列表
	/// </summary>
	/// <param name="condition">要编辑的条件对象</param>
	/// <param name="title">编辑器对话框的标题</param>
	/// <returns>配置好的编辑器选项</returns>
	/// <exception cref="T:System.Exception">当目标类型为空时抛出异常</exception>
	private EditorOptions CreateEidtorBuilder(formular_index_condition condition, string title)
	{
		if (TargetType == null)
		{
			throw new Exception("开发人员注意");
		}
		Dictionary<string, string> propsDic = TargetType.Type.GetProperties().ToDictionary((PropertyInfo x) => x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name, (PropertyInfo x) => x.Name);
		EditorOptionBuilder<formular_index_condition> editorOptionBuilder = condition.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithEditorHeight(400.0).WithValidator(delegate(formular_index_condition x)
		{
			if (string.IsNullOrEmpty(x.FieldName))
			{
				return "请选择字段名";
			}
			if (string.IsNullOrEmpty(x.FieldValue))
			{
				return "请输入字段值";
			}
			x.PropertyName = propsDic[x.FieldName];
			return "";
		});
		editorOptionBuilder.AddProperty<ConditionOperator>("ConditionOperator").WithHeader("条件运算符").EditAsCombo<ConditionOperator>()
			.WithOptions<ConditionOperator>();
		editorOptionBuilder.AddProperty<string>("FieldName").WithHeader("字段名").EditAsCombo<string>()
			.WithOptions(propsDic.Keys.Select((string x) => (x, x)).ToArray());
		editorOptionBuilder.AddProperty<FieldOperator>("FieldOperator").WithHeader("字段运算符").EditAsCombo<FieldOperator>()
			.WithOptions<FieldOperator>();
		editorOptionBuilder.AddProperty<string>("FieldValue").WithHeader("字段值").EditAsText();
		return editorOptionBuilder.Build();
	}

	/// <summary>
	/// 目标类型改变时的部分方法
	/// 根据选中的控制系统类型动态生成可用字段列表
	/// 通过数据库映射表获取不同系统间的字段对应关系
	/// 并筛选出具有有效映射关系的字段
	/// </summary>
	/// <param name="value">新选中的目标类型</param>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnTargetTypeChanged(FormulaEditorType? value)
	{
		Fields = null;
		if (TargetType == null)
		{
			return;
		}
		List<config_controlSystem_mapping> mappings = (from it in context.Db.Queryable<config_controlSystem_mapping>()
			where it.StdField != null
			select it).ToList();
		PropertyInfo[] properties = typeof(IoFullData).GetProperties();
		Func<config_controlSystem_mapping, string?> getDisplayName = TargetType.Name switch
		{
			ControlSystem.龙鳍 => (config_controlSystem_mapping m) => m.LqOld, 
			ControlSystem.中控 => (config_controlSystem_mapping m) => m.ZkOld, 
			ControlSystem.龙核 => (config_controlSystem_mapping m) => m.LhOld, 
			ControlSystem.一室 => (config_controlSystem_mapping m) => m.Xt1Old, 
			_ => (config_controlSystem_mapping m) => m.StdField, 
		};
		ObservableCollection<FormulaEditorField> observableCollection = new ObservableCollection<FormulaEditorField>();
		foreach (FormulaEditorField item in properties.Select((PropertyInfo p) => (Property: p, DisplayName: p.GetCustomAttribute<DisplayAttribute>()?.Name)).Where<(PropertyInfo, string)>(delegate((PropertyInfo Property, string DisplayName) t)
		{
			config_controlSystem_mapping config_controlSystem_mapping = mappings.FirstOrDefault((config_controlSystem_mapping m) => m.StdField == t.DisplayName);
			return config_controlSystem_mapping != null && getDisplayName(config_controlSystem_mapping) != null;
		}).Select<(PropertyInfo, string), FormulaEditorField>(delegate((PropertyInfo Property, string DisplayName) t)
		{
			config_controlSystem_mapping arg = mappings.First((config_controlSystem_mapping m) => m.StdField == t.DisplayName);
			return new FormulaEditorField(getDisplayName(arg) ?? t.Property.Name, t.Property);
		})
			.ToList())
		{
			observableCollection.Add(item);
		}
		Fields = observableCollection;
	}

	/// <summary>
	/// 字段改变时的部分方法
	/// 根据选中的字段自动查找或创建对应的公式记录
	/// 如果数据库中不存在该字段的公式，则自动创建新的公式记录
	/// </summary>
	/// <param name="value">新选中的字段对象</param>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnFieldChanged(FormulaEditorField? value)
	{
		Formula = null;
		if (TargetType != null && Field != null)
		{
			List<formular> list = await (from x in context.Db.Queryable<formular>()
				where x.TargetType == TargetType.Name.ToString()
				where x.FieldName == Field.OwnDisplayName
				select x).ToListAsync();
			if (list.Count == 0)
			{
				formular formula = new formular
				{
					FieldName = Field.OwnDisplayName,
					TargetType = TargetType.Name.ToString()
				};
				await context.Db.Insertable(formula).ExecuteCommandIdentityIntoEntityAsync();
				Formula = formula;
			}
			else
			{
				Formula = list[0];
			}
		}
	}

	/// <summary>
	/// 索引改变时的部分方法
	/// 清空条件集合，重新加载当前索引项的所有条件
	/// 条件按照索引顺序升序排列
	/// </summary>
	/// <param name="value">新的索引对象</param>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnIndexChanged(formular_Index? value)
	{
		Conditions = null;
		if (Index == null)
		{
			return;
		}
		ObservableCollection<formular_index_condition> observableCollection = new ObservableCollection<formular_index_condition>();
		foreach (formular_index_condition item in await (from x in context.Db.Queryable<formular_index_condition>()
			where x.IndexId == Index.Id
			orderby x.Index
			select x).ToListAsync())
		{
			observableCollection.Add(item);
		}
		Conditions = observableCollection;
	}

	/// <summary>
	/// 公式改变时的部分方法
	/// 清空索引和条件集合，重新加载当前公式的所有索引项
	/// 索引项按照索引顺序升序排列
	/// </summary>
	/// <param name="value">新的公式对象</param>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnFormulaChanged(formular? value)
	{
		Indecis = null;
		Conditions = null;
		if (Formula == null)
		{
			return;
		}
		ObservableCollection<formular_Index> observableCollection = new ObservableCollection<formular_Index>();
		foreach (formular_Index item in await (from x in context.Db.Queryable<formular_Index>()
			where x.FormulaId == Formula.Id
			orderby x.Index
			select x).ToListAsync())
		{
			observableCollection.Add(item);
		}
		Indecis = observableCollection;
	}
}

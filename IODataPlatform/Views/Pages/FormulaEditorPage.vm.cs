﻿﻿﻿﻿﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.DataContracts;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using SqlSugar;
using Expression = System.Linq.Expressions.Expression;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 公式编辑器页面视图模型类
/// 提供复杂的公式编辑和管理功能，支持多控制系统的字段映射和条件匹配
/// 实现基于表达式树的复杂逻辑判断，支持子条件的层次化组织
/// 广泛用于不同控制系统间的数据映射和自动化数据处理
/// </summary>
public partial class FormulaEditorViewModel(SqlSugarContext context, IMessageService message) : ObservableObject
{



    /// <summary>
    /// 所有可用的目标控制系统类型集合
    /// 包含龙鳞、中控、龙核、一室四种主要控制系统
    /// 每种系统都对应IoFullData类型的数据结构
    /// </summary>
    public ObservableCollection<FormulaEditorType> TargetTypes { get; } = new ObservableCollection<FormulaEditorType> {
                        new(ControlSystem.龙鳍, typeof(IoFullData)), new(ControlSystem.中控, typeof(IoFullData)),
                        new(ControlSystem.龙核, typeof(IoFullData)), new(ControlSystem.一室, typeof(IoFullData))};
 
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



    /// <summary>
    /// 目标类型改变时的部分方法
    /// 根据选中的控制系统类型动态生成可用字段列表
    /// 通过数据库映射表获取不同系统间的字段对应关系
    /// 并筛选出具有有效映射关系的字段
    /// </summary>
    /// <param name="value">新选中的目标类型</param>
    partial void OnTargetTypeChanged(FormulaEditorType? value)
    {
        Fields = null;
        if (TargetType is null) return;

        // 获取映射表，确保 StdField 不为空
        var mappings = context.Db.Queryable<config_controlSystem_mapping>()
            .Where(it => it.StdField != null).ToList();

        // 获取 IoFullData 类型的所有属性
        var properties = typeof(IoFullData).GetProperties();

        // 根据 TargetType.Name 确定映射字段
        Func<config_controlSystem_mapping, string?> getDisplayName = TargetType.Name switch
        {
            ControlSystem.龙鳍 => m => m.LqOld,
            ControlSystem.中控 => m => m.ZkOld,
            ControlSystem.龙核 => m => m.LhOld,
            ControlSystem.一室 => m => m.Xt1Old,
            _ => m => m.StdField
        };

        // 构造 Fields 列表，仅包含符合条件的属性
        Fields = [..properties
            .Select(p => (Property: p, DisplayName: p.GetCustomAttribute<DisplayAttribute>()?.Name))
            .Where(t =>
            {
                var mapping = mappings.FirstOrDefault(m => m.StdField == t.DisplayName);
                return mapping != null && getDisplayName(mapping) != null; // 确保 StdField 和 当前系统字段不为空
            })
            .Select(t =>
            {
                var mapping = mappings.First(m => m.StdField == t.DisplayName);
                return new FormulaEditorField(getDisplayName(mapping) ?? t.Property.Name, t.Property);
            })
            .ToList()];
    }



    /// <summary>
    /// 字段改变时的部分方法
    /// 根据选中的字段自动查找或创建对应的公式记录
    /// 如果数据库中不存在该字段的公式，则自动创建新的公式记录
    /// </summary>
    /// <param name="value">新选中的字段对象</param>
    async partial void OnFieldChanged(FormulaEditorField? value) {
        Formula = null;
        if (TargetType is null) { return; }
        if (Field is null) { return; }
        
        // 查找已存在的公式记录
        var formulas = await context.Db.Queryable<formular>()
            .Where(x => x.TargetType == TargetType.Name.ToString())
            .Where(x => x.FieldName == Field.OwnDisplayName)
            .ToListAsync();

        // 如果不存在则创建新的公式记录
        if (formulas.Count == 0) {
            var formula = new formular() { FieldName = Field.OwnDisplayName, TargetType = TargetType.Name.ToString() };
            await context.Db.Insertable(formula).ExecuteCommandIdentityIntoEntityAsync();
            Formula = formula;
        } else {
            Formula = formulas[0];
        }
    }

    /// <summary>
    /// 公式改变时的部分方法
    /// 清空索引和条件集合，重新加载当前公式的所有索引项
    /// 索引项按照索引顺序升序排列
    /// </summary>
    /// <param name="value">新的公式对象</param>
    async partial void OnFormulaChanged(formular? value) {
        Indecis = null;
        Conditions = null;
        if (Formula is null) { return; }
        Indecis = [.. await context.Db.Queryable<formular_Index>().Where(x => x.FormulaId == Formula.Id).OrderBy(x => x.Index).ToListAsync()];
    }

    /// <summary>
    /// 索引改变时的部分方法
    /// 清空条件集合，重新加载当前索引项的所有条件
    /// 条件按照索引顺序升序排列
    /// </summary>
    /// <param name="value">新的索引对象</param>
    async partial void OnIndexChanged(formular_Index? value) {
        Conditions = null;
        if (Index is null) { return; }
        Conditions = [.. await context.Db.Queryable<formular_index_condition>().Where(x => x.IndexId == Index.Id).OrderBy(x => x.Index).ToListAsync()];
    }

    /// <summary>
    /// 添加索引项命令
    /// 创建新的索引项并通过编辑器对话框供用户编辑
    /// 自动计算新索引的顺序号，确保索引的连续性
    /// </summary>
    /// <returns>异步任务，表示添加操作的完成</returns>
    [RelayCommand]
    private async Task AddIndex() {
        if (Formula is null) { throw new("开发人员注意"); }
        if (Indecis is null) { throw new("开发人员注意"); }
        var data = new formular_Index() { FormulaId = Formula.Id };
        if (!CreateEidtorBuilder(data, "添加").EditWithWpfUI()) { return; }
        
        // 计算新索引的顺序号
        data.Index = Indecis.Any() ? Indecis.Max(x => x.Index) + 1 : 1;

        await context.Db.Insertable(data).ExecuteCommandAsync();
        OnFormulaChanged(null);
    }

    /// <summary>
    /// 编辑索引项命令
    /// 创建索引项的副本并通过编辑器对话框供用户修改
    /// 更新数据库后自动刷新界面显示
    /// </summary>
    /// <param name="data">要编辑的索引项对象</param>
    /// <returns>异步任务，表示编辑操作的完成</returns>
    [RelayCommand]
    private async Task EditIndex(formular_Index data) {
        if (Formula is null) { throw new("开发人员注意"); }
        if (Indecis is null) { throw new("开发人员注意"); }
        
        // 创建副本以避免直接修改原对象
        var objToEdit = new formular_Index().CopyPropertiesFrom(data);
        if (!CreateEidtorBuilder(objToEdit, "编辑").EditWithWpfUI()) { return; }

        await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        OnFormulaChanged(null);
    }

    /// <summary>
    /// 删除索引项命令
    /// 先删除该索引下的所有条件，再删除索引本身
    /// 删除后自动重新排列剩余索引的顺序号，保持连续性
    /// </summary>
    /// <param name="data">要删除的索引项对象</param>
    /// <returns>异步任务，表示删除操作的完成</returns>
    [RelayCommand]
    private async Task DeleteIndex(formular_Index data) {
        if (!await message.ConfirmAsync("确认删除")) { return; }
        if (Indecis is null) { throw new("开发人员注意"); }
        
        // 先删除该索引下的所有条件
        await context.Db.Deleteable<formular_index_condition>().Where(x => x.IndexId == data.Id).ExecuteCommandAsync();
        await context.Db.Deleteable(data).ExecuteCommandAsync();

        // 重新排列剩余索引的顺序号
        for (int i = 0; i < Indecis.Count; i++) {
            Indecis[i].Index = i + 1;
        }
        await context.Db.Updateable(Indecis.ToList()).ExecuteCommandAsync();
        OnFormulaChanged(null);
    }

    /// <summary>
    /// 查看索引项命令
    /// 设置当前选中的索引项，触发条件列表的加载
    /// 用于在界面上显示该索引项的详细条件
    /// </summary>
    /// <param name="data">要查看的索引项对象</param>
    [RelayCommand]
    private void ViewIndex(formular_Index data) {
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
    private async Task MoveUpIndex(formular_Index data) {
        if (Indecis is null) { throw new("开发人员注意"); }
        var index = Indecis.IndexOf(data);
        if (index == 0) { return; }
        
        // 在集合中移动位置
        Indecis.Move(index - 1, index);
        
        // 重新计算所有索引的顺序号
        for (int i = 0; i < Indecis.Count; i++) {
            Indecis[i].Index = i + 1;
        }

        await context.Db.Updateable(Indecis.ToList()).ExecuteCommandAsync();
        OnFormulaChanged(null);
    }

    /// <summary>
    /// 下移索引项命令
    /// 将指定的索引项在列表中下移一位，重新排列所有索引项的顺序号
    /// 如果已经在最底部则不执行任何操作
    /// </summary>
    /// <param name="data">要下移的索引项对象</param>
    /// <returns>异步任务，表示移动操作的完成</returns>
    [RelayCommand]
    private async Task MoveDownIndex(formular_Index data) {
        if (Indecis is null) { throw new("开发人员注意"); }
        var index = Indecis.IndexOf(data);
        if (index == Indecis.Count - 1) { return; }
        
        // 在集合中移动位置
        Indecis.Move(index, index + 1);
        
        // 重新计算所有索引的顺序号
        for (int i = 0; i < Indecis.Count; i++) {
            Indecis[i].Index = i + 1;
        }

        await context.Db.Updateable(Indecis.ToList()).ExecuteCommandAsync();
        OnFormulaChanged(null);
    }

    /// <summary>
    /// 添加条件命令
    /// 为当前选中的索引项添加新的条件
    /// 自动计算新条件的顺序号，确保条件的连续性
    /// </summary>
    /// <returns>异步任务，表示添加操作的完成</returns>
    /// <exception cref="Exception">当未选中索引项时抛出异常</exception>
    [RelayCommand]
    private async Task AddCondition() {
        if (Index is null) { throw new("请点击顺序表中的查看按钮"); }
        if (Conditions is null) { throw new("开发人员注意"); }
        var data = new formular_index_condition() { IndexId = Index.Id };
        if (!CreateEidtorBuilder(data, "添加").EditWithWpfUI()) { return; }
        
        // 计算新条件的顺序号
        data.Index = Conditions.Any() ? Conditions.Max(x => x.Index) + 1 : 1;

        await context.Db.Insertable(data).ExecuteCommandAsync();
        OnIndexChanged(null);
    }

    /// <summary>
    /// 编辑条件命令
    /// 创建条件的副本并通过编辑器对话框供用户修改
    /// 更新数据库后自动刷新界面显示
    /// </summary>
    /// <param name="data">要编辑的条件对象</param>
    /// <returns>异步任务，表示编辑操作的完成</returns>
    [RelayCommand]
    private async Task EditCondition(formular_index_condition data) {
        // 创建副本以避免直接修改原对象
        var objToEdit = new formular_index_condition().CopyPropertiesFrom(data);
        if (!CreateEidtorBuilder(objToEdit, "编辑").EditWithWpfUI()) { return; }

        await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        OnIndexChanged(null);
    }

    /// <summary>
    /// 删除条件命令
    /// 删除指定的条件后自动重新排列剩余条件的顺序号，保持连续性
    /// 需要用户确认后才执行删除操作
    /// </summary>
    /// <param name="data">要删除的条件对象</param>
    /// <returns>异步任务，表示删除操作的完成</returns>
    [RelayCommand]
    private async Task DeleteCondition(formular_index_condition data) {
        if (Conditions is null) { throw new("开发人员注意"); }
        if (!await message.ConfirmAsync("确认删除")) { return; }
        await context.Db.Deleteable(data).ExecuteCommandAsync();

        // 重新排列剩余条件的顺序号
        for (int i = 0; i < Conditions.Count; i++) {
            Conditions[i].Index = i + 1;
        }
        await context.Db.Updateable(Conditions.ToList()).ExecuteCommandAsync();
        OnIndexChanged(null);
    }

    /// <summary>
    /// 上移条件命令
    /// 将指定的条件在列表中上移一位，重新排列所有条件的顺序号
    /// 如果已经在最顶部则不执行任何操作
    /// </summary>
    /// <param name="data">要上移的条件对象</param>
    /// <returns>异步任务，表示移动操作的完成</returns>
    [RelayCommand]
    private async Task MoveUpCondition(formular_index_condition data) {
        if (Conditions is null) { throw new("开发人员注意"); }
        var index = Conditions.IndexOf(data);
        if (index == 0) { return; }
        
        // 在集合中移动位置
        Conditions.Move(index - 1, index);

        // 重新计算所有条件的顺序号
        for (int i = 0; i < Conditions.Count; i++) {
            Conditions[i].Index = i + 1;
        }
        await context.Db.Updateable(Conditions.ToList()).ExecuteCommandAsync();
        OnIndexChanged(null);
    }

    /// <summary>
    /// 下移条件命令
    /// 将指定的条件在列表中下移一位，重新排列所有条件的顺序号
    /// 注意：这里的逻辑有问题，实际上在执行下移操作
    /// </summary>
    /// <param name="data">要下移的条件对象</param>
    /// <returns>异步任务，表示移动操作的完成</returns>
    [RelayCommand]
    private async Task MoveDownCondition(formular_index_condition data) {
        if (Conditions is null) { throw new("开发人员注意"); }
        var index = Conditions.IndexOf(data);
        if (index == Conditions.Count - 1) { return; }
        
        // 在集合中移动位置
        Conditions.Move(index, index + 1);

        // 重新计算所有条件的顺序号
        for (int i = 0; i < Conditions.Count; i++) {
            Conditions[i].Index = i + 1;
        }
        await context.Db.Updateable(Conditions.ToList()).ExecuteCommandAsync();
        OnIndexChanged(null);
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
    /// <exception cref="Exception">当找不到对应字段的公式时抛出异常</exception>
    /// <exception cref="NotSupportedException">当遇到不支持的运算符时抛出异常</exception>
    public string FindMatchingFormulaReturnValueAsyncByExpression<T>(T targetTypeInstance) where T : class
    {
        // 第一阶段：查找对应的公式 ID
        var fieldIdToGet = context.Db.Queryable<formular>()
            .Where(q => q.TargetType == TargetType.Name.ToString() && q.FieldName == Field.OwnDisplayName)
            .ToList().FirstOrDefault()?.Id;
        if (fieldIdToGet == null) throw new Exception("未找到该字段的公式");
        
        // 第二阶段：获取所有索引项，按索引顺序排列
        var values = context.Db.Queryable<formular_Index>().Where(f => f.FormulaId == fieldIdToGet).OrderBy(x => x.Index).ToList();
        
        // 第三阶段：逐个匹配索引项的条件
        foreach (var value in values)
        {
            ParameterExpression paramTarget = Expression.Parameter(typeof(T), "target");
            Expression finalExpression = null;

            // 获取当前索引项的所有条件
            var conditions = context.Db.Queryable<formular_index_condition>()
                .Where(f => f.IndexId == value.Id)
                .OrderBy(c => c.Index)
                .ToList();
                
            // 逐个构建条件表达式
            foreach (var condition in conditions)
            {
                var propertyInfo = typeof(T).GetProperties().FirstOrDefault(a => a.Name == condition.PropertyName);

                if (propertyInfo == null)
                {
                    continue; // 属性不存在时跳过
                }
                
                // 获取实际属性值和期望值
                var propertyValue = propertyInfo.GetValue(targetTypeInstance) ?? "";
                ConstantExpression propertyConstantValue = Expression.Constant(propertyValue);
                ConstantExpression constantValue = Expression.Constant(Convert.ChangeType(condition.FieldValue, propertyInfo.PropertyType), propertyInfo.PropertyType);

                // 根据运算符构建条件表达式
                Expression conditionExpression = condition.FieldOperator switch
                {
                    FieldOperator.等于 => Expression.Equal(propertyConstantValue, constantValue),
                    FieldOperator.不等于 => Expression.NotEqual(propertyConstantValue, constantValue),
                    FieldOperator.包含 when propertyInfo.PropertyType == typeof(string) =>
                        Expression.Call(propertyConstantValue, "Contains", null, constantValue),
                    FieldOperator.不包含 when propertyInfo.PropertyType == typeof(string) =>
                        Expression.Not(Expression.Call(propertyConstantValue, "Contains", null, constantValue)),
                    FieldOperator.起始于 when propertyInfo.PropertyType == typeof(string) =>
                        Expression.Call(propertyConstantValue, "StartsWith", null, constantValue),
                    FieldOperator.终止于 when propertyInfo.PropertyType == typeof(string) =>
                        Expression.Call(propertyConstantValue, "EndsWith", null, constantValue),
                    _ => throw new NotSupportedException($"不支持的运算符: {condition.FieldOperator}")
                };

                // 使用条件连接符组合表达式
                finalExpression = finalExpression == null
                    ? conditionExpression
                    : condition.ConditionOperator switch
                    {
                        ConditionOperator.并且 => Expression.AndAlso(finalExpression, conditionExpression),
                        ConditionOperator.或 => Expression.OrElse(finalExpression, conditionExpression),
                        ConditionOperator.无 => finalExpression,
                        _ => throw new NotSupportedException($"不支持的连接运算符: {condition.ConditionOperator}")
                    };
            }

            // 编译并执行表达式，如果匹配成功则返回结果
            if (finalExpression != null)
            {
                var conditionDelegate = Expression.Lambda<Func<T, bool>>(finalExpression, paramTarget).Compile();
                if (conditionDelegate(targetTypeInstance))
                {
                    return value.ReturnValue;
                }
            }
        }
        
        // 所有条件都不匹配时返回错误标识
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
    private static EditorOptions CreateEidtorBuilder(formular_Index index, string title) {
        var builder = index.CreateEditorBuilder();
        builder.WithTitle(title).WithEditorHeight(150);
        builder.AddProperty<string>(nameof(formular_Index.ReturnValue)).WithHeader("返回值").EditAsText();
        return builder.Build();
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
    /// <exception cref="Exception">当目标类型为空时抛出异常</exception>
    private EditorOptions CreateEidtorBuilder(formular_index_condition condition, string title) {
        if (TargetType is null) { throw new("开发人员注意"); }
        
        // 构建字段名到属性名的映射字典
        var propsDic = TargetType.Type.GetProperties().ToDictionary(x => x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name, x => x.Name);
        var builder = condition.CreateEditorBuilder();
        
        // 配置编辑器基本属性和验证器
        builder.WithTitle(title).WithEditorHeight(400).WithValidator(x => {
            if (string.IsNullOrEmpty(x.FieldName)) { return "请选择字段名"; }
            if (string.IsNullOrEmpty(x.FieldValue)) { return "请输入字段值"; }
            x.PropertyName = propsDic[x.FieldName];
            return "";
        });
        
        // 添加各种编辑属性
        builder.AddProperty<ConditionOperator>(nameof(formular_index_condition.ConditionOperator)).WithHeader("条件运算符").EditAsCombo<ConditionOperator>().WithOptions<ConditionOperator>();
        builder.AddProperty<string>(nameof(formular_index_condition.FieldName)).WithHeader("字段名").EditAsCombo<string>().WithOptions(propsDic.Keys.Select(x => (x, x)).ToArray());
        builder.AddProperty<FieldOperator>(nameof(formular_index_condition.FieldOperator)).WithHeader("字段运算符").EditAsCombo<FieldOperator>().WithOptions<FieldOperator>();
        builder.AddProperty<string>(nameof(formular_index_condition.FieldValue)).WithHeader("字段值").EditAsText();
        return builder.Build();
    }

}

/// <summary>
/// 公式编辑器的目标类型封装类
/// 将控制系统枚举和对应的.NET类型绑定在一起
/// 用于在界面上显示可选的控制系统并支持后续的类型操作
/// </summary>
/// <param name="system">控制系统枚举值</param>
/// <param name="type">对应的.NET数据类型</param>
public class FormulaEditorType(ControlSystem system, Type type)
{
    /// <summary>控制系统名称</summary>
    public ControlSystem Name { get; } = system;
    
    /// <summary>对应的数据类型</summary>
    public Type Type { get; } = type;
}

/// <summary>
/// 公式编辑器的字段封装类
/// 将字段的显示名称和对应的属性信息绑定在一起
/// 用于在界面上显示可编辑的字段并支持后续的反射操作
/// </summary>
/// <param name="Name">字段的显示名称</param>
/// <param name="property">对应的属性信息</param>
public class FormulaEditorField(string Name,PropertyInfo property) {
    /// <summary>字段的自定义显示名称</summary>
    public string OwnDisplayName { get; } = Name;
    
    /// <summary>对应的属性名称</summary>
    public string PropertyName { get; } = property.Name;
    
    /// <summary>对应的属性信息对象</summary>
    public PropertyInfo Property { get; } = property;
}
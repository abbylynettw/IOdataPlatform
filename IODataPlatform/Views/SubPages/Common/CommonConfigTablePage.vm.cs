using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Utilities;

using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using SqlSugar;
using Wpf.Ui;

namespace IODataPlatform.Views.SubPages.Common;

public partial class ConfigTableViewModel(SqlSugarContext context, IMessageService message, INavigationService navigation, GlobalModel model, ExcelService excel, IContentDialogService dialogService) : ObservableObject, INavigationAware {

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private IList? data;

    public Type? DataType { get; set; }

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo() {
        model.Status.Busy("正在获取配置表数据……");
        await Refresh();
        model.Status.Reset();
    }

    [RelayCommand]
    private void ConvertBack() {
        navigation.GoBack();
    }

    [RelayCommand]
    private async Task BatchImport()
    {
        if (DataType == null)
        {
            model.Status.Warn("配置表类型未指定");
            return;
        }

        try
        {
            // 1. 使用 SelectExcelSheetDialog 选择 Excel 文件和工作表
            var sheetDialog = App.GetService<SelectExcelSheetDialogViewModel>();
            var selectSheetDialog = new SelectExcelSheetDialog(sheetDialog, dialogService.GetContentPresenter());

            if (await selectSheetDialog.ShowAsync() != ContentDialogResult.Primary)
                return;

            if (string.IsNullOrEmpty(sheetDialog.SelectFilePath) || string.IsNullOrEmpty(sheetDialog.SelectedSheetName))
            {
                model.Status.Warn("未选择文件或工作表");
                return;
            }

            model.Status.Busy("正在读取Excel数据...");

            // 2. 读取 Excel 数据
            var dataTable = await excel.GetDataTableAsStringAsync(sheetDialog.SelectFilePath, sheetDialog.SelectedSheetName, hasHeader: true);
            
            // 3. 校验列是否匹配
            var requiredColumns = DataType.GetProperties()
                .Select(p => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name)
                .Where(name => name != "Id" && name != "CreateTime" && name != "SubProjectId") // 排除自动生成的字段
                .ToArray();

            var existingColumns = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            var missingColumns = requiredColumns.Except(existingColumns).ToArray();

            if (missingColumns.Any())
            {
                model.Status.Reset();
                await message.AlertAsync($"数据校验失败！\n\n缺少必需列：\n{string.Join("\n", missingColumns.Select(c => $"  • {c}"))}");
                return;
            }

            // 4. 使用反射动态转换为实体对象
            var convertedData = ConvertDataTableToList(dataTable, DataType);

            if (convertedData == null || !convertedData.Any())
            {
                model.Status.Reset();
                await message.AlertAsync("数据为空，没有可导入的数据！");
                return;
            }

            model.Status.Reset();

            // 5. 确认导入
            var confirm = await message.ConfirmAsync(
                $"✅ 数据校验通过\n\n共读取 {convertedData.Count} 条数据\n包含所有必需字段\n\n确认要导入这些数据吗？\n\n⚠️ 此操作将清空现有数据！",
                "确认导入");

            if (!confirm)
            {
                return;
            }

            model.Status.Busy("正在导入数据...");

            // 6. 使用反射调用删除旧数据
            var deleteMethod = typeof(ISqlSugarClient)
                .GetMethods()
                .Where(m => m.Name == "Deleteable" && m.IsGenericMethod && m.GetParameters().Length == 0)
                .FirstOrDefault()
                ?.MakeGenericMethod(DataType);
            
            if (deleteMethod != null)
            {
                var deleteable = deleteMethod.Invoke(context.Db, null);
                // 获取无参数的 ExecuteCommandAsync 方法
                var executeMethod = deleteable?.GetType().GetMethod("ExecuteCommandAsync", Type.EmptyTypes);
                await (executeMethod?.Invoke(deleteable, null) as Task ?? Task.CompletedTask);
            }

            // 7. 设置默认值（CreateTime 和 SubProjectId）
            foreach (var item in convertedData)
            {
                var createTimeProp = DataType.GetProperty("CreateTime");
                if (createTimeProp != null && createTimeProp.PropertyType == typeof(DateTime))
                {
                    createTimeProp.SetValue(item, DateTime.Now);
                }

                var subProjectIdProp = DataType.GetProperty("SubProjectId");
                if (subProjectIdProp != null && subProjectIdProp.PropertyType == typeof(int))
                {
                    subProjectIdProp.SetValue(item, 0); // 默认为全局配置
                }
            }

            // 8. 批量插入新数据（使用强类型转换）
            var array = Array.CreateInstance(DataType, convertedData.Count);
            for (int i = 0; i < convertedData.Count; i++)
            {
                array.SetValue(convertedData[i], i);
            }

            var insertMethod = typeof(ISqlSugarClient)
                .GetMethods()
                .Where(m => m.Name == "Insertable" && m.IsGenericMethod)
                .Where(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.IsArray)
                .FirstOrDefault()
                ?.MakeGenericMethod(DataType);

            if (insertMethod != null)
            {
                var insertable = insertMethod.Invoke(context.Db, new object[] { array });
                // 获取无参数的 ExecuteCommandAsync 方法
                var executeMethod = insertable?.GetType().GetMethod("ExecuteCommandAsync", Type.EmptyTypes);
                await (executeMethod?.Invoke(insertable, null) as Task ?? Task.CompletedTask);
            }

            // 9. 刷新界面
            await Refresh();
            model.Status.Success($"导入成功！共导入 {convertedData.Count} 条数据。");
        }
        catch (Exception ex)
        {
            model.Status.Error($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 通用方法：将 DataTable 转换为指定类型的 List
    /// </summary>
    private List<object>? ConvertDataTableToList(DataTable dataTable, Type targetType)
    {
        try
        {
            // 通过反射调用 StringTableToIEnumerableByDiplay<T> 扩展方法
            var method = typeof(IODataPlatform.Utilities.Extensions)
                .GetMethod(nameof(IODataPlatform.Utilities.Extensions.StringTableToIEnumerableByDiplay))
                ?.MakeGenericMethod(targetType);

            if (method == null)
                return null;

            // 调用扩展方法，传入 dataTable 实例
            var enumerable = method.Invoke(null, new object[] { dataTable }) as IEnumerable;
            return enumerable?.Cast<object>().ToList();
        }
        catch
        {
            return null;
        }
    }

    public async Task Refresh() {
        if (DataType == typeof(config_card_type_judge)) {
            var result = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            Data = new ObservableCollection<config_card_type_judge>(result);
        } else if (DataType == typeof(config_terminalboard_type_judge)) {
            var result = await context.Db.Queryable<config_terminalboard_type_judge>().ToListAsync();
            Data = new ObservableCollection<config_terminalboard_type_judge>(result);
        } else if (DataType == typeof(config_connection_points)) {
            var result = await context.Db.Queryable<config_connection_points>().ToListAsync();
            Data = new ObservableCollection<config_connection_points>(result);
        } else if (DataType == typeof(config_power_supply_method)) {
            var result = await context.Db.Queryable<config_power_supply_method>().ToListAsync();
            Data = new ObservableCollection<config_power_supply_method>(result);
        } else if (DataType == typeof(config_output_format_values)) {
            var result = await context.Db.Queryable<config_output_format_values>().ToListAsync();
            Data = new ObservableCollection<config_output_format_values>(result);
        }
        else if (DataType == typeof(config_cable_categoryAndColor)) {
            var result = await context.Db.Queryable<config_cable_categoryAndColor>().ToListAsync();
            Data = new ObservableCollection<config_cable_categoryAndColor>(result);
        } else if (DataType == typeof(config_cable_function)) {
            var result = await context.Db.Queryable<config_cable_function>().ToListAsync();
            Data = new ObservableCollection<config_cable_function>(result);
        } else if (DataType == typeof(config_cable_spec)) {
            var result = await context.Db.Queryable<config_cable_spec>().ToListAsync();
            Data = new ObservableCollection<config_cable_spec>(result);
        } else if (DataType == typeof(config_cable_startNumber)) {
            var result = await context.Db.Queryable<config_cable_startNumber>().ToListAsync();
            Data = new ObservableCollection<config_cable_startNumber>(result);
        } else if (DataType == typeof(config_cable_systemNumber)) {
            var result = await context.Db.Queryable<config_cable_systemNumber>().ToListAsync();
            Data = new ObservableCollection<config_cable_systemNumber>(result);
        } else if (DataType == typeof(config_termination_yjs)){
            var result = await context.Db.Queryable<config_termination_yjs>().ToListAsync();
            Data = new ObservableCollection<config_termination_yjs>(result);
        }else if (DataType == typeof(config_aqj_analog)){
            var result = await context.Db.Queryable<config_aqj_analog>().ToListAsync();
            Data = new ObservableCollection<config_aqj_analog>(result);
        }else if (DataType == typeof(config_aqj_control)){
            var result = await context.Db.Queryable<config_aqj_control>().ToListAsync();
            Data = new ObservableCollection<config_aqj_control>(result);
        }else if (DataType == typeof(config_aqj_stationReplace)){
            var result = await context.Db.Queryable<config_aqj_stationReplace>().ToListAsync();
            Data = new ObservableCollection<config_aqj_stationReplace>(result);
        }else if (DataType == typeof(config_aqj_tagReplace)){
            var result = await context.Db.Queryable<config_aqj_tagReplace>().ToListAsync();
            Data = new ObservableCollection<config_aqj_tagReplace>(result);
        }
        else if (DataType == typeof(config_aqj_stationCabinet))
        {
            var result = await context.Db.Queryable<config_aqj_stationCabinet>().ToListAsync();
            Data = new ObservableCollection<config_aqj_stationCabinet>(result);
        }   
        else if (DataType == typeof(config_controlSystem_mapping))
        {
            var result = await context.Db.Queryable<config_controlSystem_mapping>().ToListAsync();
            Data = new ObservableCollection<config_controlSystem_mapping>(result);
        }
        else if (DataType == typeof(config_xt2_cabinet_alarm))
        {
            // XT2机柜报警配置表：不过滤子项目，显示全部
            var result = await context.Db.Queryable<config_xt2_cabinet_alarm>().ToListAsync();
            Data = new ObservableCollection<config_xt2_cabinet_alarm>(result);
        }
        //todo 多类型判断
    }

    [RelayCommand]
    private async Task Add() {
        var type = DataType ?? throw new("出现问题，请返回上级页面");
        var instance = Activator.CreateInstance(type) ?? throw new("无法创建类型实例");

        if (!CreateBuilder(instance, "添加").EditWithWpfUI()) { return; }


        if (DataType == typeof(config_card_type_judge)) {
            //田：在这追加对IO卡型号重复的逻辑
            config_card_type_judge tmpInstance = (config_card_type_judge)instance;
            if(string.IsNullOrEmpty(tmpInstance.IoCardType))
            {
                model.Status.Error("IO卡型号不能为空。");
                return;
            }
            if (data != null)
            {
                foreach (config_card_type_judge item in data)
                {
                    if (item.IoCardType.Equals(tmpInstance.IoCardType))
                    {
                        model.Status.Error("IO卡型号已存在，不能重复添加。");
                        return;
                    }
                }
            }
            //-------------
            await context.Db.Insertable<config_card_type_judge>(instance).ExecuteCommandAsync();
        } else if (DataType == typeof(config_connection_points)) {
            await context.Db.Insertable<config_connection_points>(instance).ExecuteCommandAsync();
        } else if (DataType == typeof(config_output_format_values)) {
            await context.Db.Insertable<config_output_format_values>(instance).ExecuteCommandAsync();
        } else if (DataType == typeof(config_power_supply_method)) {
            await context.Db.Insertable<config_power_supply_method>(instance).ExecuteCommandAsync();
        } else if (DataType == typeof(config_terminalboard_type_judge)) {
            await context.Db.Insertable<config_terminalboard_type_judge>(instance).ExecuteCommandAsync();
        }else if (DataType == typeof(config_cable_categoryAndColor)){
            await context.Db.Insertable<config_cable_categoryAndColor>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_function))
        {
            await context.Db.Insertable<config_cable_function>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_spec))
        {
            await context.Db.Insertable<config_cable_spec>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_startNumber))
        {
            await context.Db.Insertable<config_cable_startNumber>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_systemNumber))
        {
            await context.Db.Insertable<config_cable_systemNumber>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_termination_yjs))
        {
            await context.Db.Insertable<config_termination_yjs>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_analog))
        {
            await context.Db.Insertable<config_aqj_analog>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_control))
        {
            await context.Db.Insertable<config_aqj_control>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_stationReplace))
        {
            await context.Db.Insertable<config_aqj_stationReplace>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_tagReplace))
        {
            await context.Db.Insertable<config_aqj_tagReplace>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_stationCabinet))
        {
            await context.Db.Insertable<config_aqj_stationCabinet>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_controlSystem_mapping))
        {
            await context.Db.Insertable<config_controlSystem_mapping>(instance).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_xt2_cabinet_alarm))
        {
            await context.Db.Insertable<config_xt2_cabinet_alarm>((config_xt2_cabinet_alarm)instance).ExecuteCommandAsync();
        }
        await Refresh();
        model.Status.Success("已添加");
    }

    [RelayCommand]
    private async Task Edit(object obj) {

        if (DataType == typeof(config_card_type_judge)) {
            var objToEdit = obj.AsTrue<config_card_type_judge>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        } else if (DataType == typeof(config_connection_points)) {
            var objToEdit = obj.AsTrue<config_connection_points>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        } else if (DataType == typeof(config_output_format_values)) {
            var objToEdit = obj.AsTrue<config_output_format_values>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        } else if (DataType == typeof(config_power_supply_method)) {
            var objToEdit = obj.AsTrue<config_power_supply_method>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        } else if (DataType == typeof(config_terminalboard_type_judge)) {
            var objToEdit = obj.AsTrue<config_terminalboard_type_judge>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_systemNumber))
        {
            var objToEdit = obj.AsTrue<config_cable_systemNumber>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_categoryAndColor))
        {
            var objToEdit = obj.AsTrue<config_cable_categoryAndColor>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_function))
        {
            var objToEdit = obj.AsTrue<config_cable_function>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_spec))
        {
            var objToEdit = obj.AsTrue<config_cable_spec>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_cable_startNumber))
        {
            var objToEdit = obj.AsTrue<config_cable_startNumber>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_termination_yjs))
        {
            var objToEdit = obj.AsTrue<config_termination_yjs>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_analog))
        {
            var objToEdit = obj.AsTrue<config_aqj_analog>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_control))
        {
            var objToEdit = obj.AsTrue<config_aqj_control>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_stationReplace))
        {
            var objToEdit = obj.AsTrue<config_aqj_stationReplace>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_tagReplace))
        {
            var objToEdit = obj.AsTrue<config_aqj_tagReplace>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_aqj_stationCabinet))
        {
            var objToEdit = obj.AsTrue<config_aqj_stationCabinet>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_controlSystem_mapping))
        {
            var objToEdit = obj.AsTrue<config_controlSystem_mapping>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        else if (DataType == typeof(config_xt2_cabinet_alarm))
        {
            var objToEdit = obj.AsTrue<config_xt2_cabinet_alarm>().JsonClone();
            if (!CreateBuilder(objToEdit!, "编辑").EditWithWpfUI()) { return; }
            await context.Db.Updateable(objToEdit).ExecuteCommandAsync();
        }
        // todo 多类型判断

        await Refresh();
        model.Status.Success("已修改");
    }

    [RelayCommand]
    private async Task Delete(object obj) {
        if (!await message.ConfirmAsync("确认删除")) { return; }
        if (obj is config_card_type_judge obj1) {
            await context.Db.Deleteable<config_card_type_judge>().In(obj1.Id).ExecuteCommandAsync();
        } else if (obj is config_connection_points obj2) {
            await context.Db.Deleteable<config_connection_points>().In(obj2.Id).ExecuteCommandAsync();
        } else if (obj is config_output_format_values obj3) {
            await context.Db.Deleteable<config_output_format_values>().In(obj3.Id).ExecuteCommandAsync();
        } else if (obj is config_power_supply_method obj4) {
            await context.Db.Deleteable<config_power_supply_method>().In(obj4.Id).ExecuteCommandAsync();
        } else if (obj is config_terminalboard_type_judge obj5) {
            await context.Db.Deleteable<config_terminalboard_type_judge>().In(obj5.Id).ExecuteCommandAsync();
        }
        else if (obj is config_cable_categoryAndColor obj6)
        {
            await context.Db.Deleteable<config_cable_categoryAndColor>().In(obj6.Id).ExecuteCommandAsync();
        }
        else if (obj is config_cable_function obj7)
        {
            await context.Db.Deleteable<config_cable_function>().In(obj7.Id).ExecuteCommandAsync();
        }
        else if (obj is config_cable_spec obj8)
        {
            await context.Db.Deleteable<config_cable_spec>().In(obj8.Id).ExecuteCommandAsync();
        }
        else if (obj is config_cable_startNumber obj9)
        {
            await context.Db.Deleteable<config_cable_startNumber>().In(obj9.Id).ExecuteCommandAsync();
        }
        else if (obj is config_cable_systemNumber obj10)
        {
            await context.Db.Deleteable<config_cable_systemNumber>().In(obj10.Id).ExecuteCommandAsync();
        }
        else if (obj is config_termination_yjs obj11)
        {
            await context.Db.Deleteable<config_termination_yjs>().In(obj11.Id).ExecuteCommandAsync();
        }
        else if (obj is config_aqj_analog obj12)
        {
            await context.Db.Deleteable<config_aqj_analog>().In(obj12.Id).ExecuteCommandAsync();
        }
        else if (obj is config_aqj_control obj13)
        {
            await context.Db.Deleteable<config_aqj_control>().In(obj13.Id).ExecuteCommandAsync();
        }
        else if (obj is config_aqj_stationReplace obj14)
        {
            await context.Db.Deleteable<config_aqj_stationReplace>().In(obj14.Id).ExecuteCommandAsync();
        }
        else if (obj is config_aqj_tagReplace obj15)
        {
            await context.Db.Deleteable<config_aqj_tagReplace>().In(obj15.Id).ExecuteCommandAsync();
        }
        else if (obj is config_aqj_stationCabinet obj16)
        {
            await context.Db.Deleteable<config_aqj_stationCabinet>().In(obj16.Id).ExecuteCommandAsync();
        }
        else if (obj is config_controlSystem_mapping obj17)
        {
            await context.Db.Deleteable<config_controlSystem_mapping>().In(obj17.Id).ExecuteCommandAsync();
        }
        else if (obj is config_xt2_cabinet_alarm obj18)
        {
            await context.Db.Deleteable<config_xt2_cabinet_alarm>().In(obj18.Id).ExecuteCommandAsync();
        }
        // todo 多类型判断
        await Refresh();
        model.Status.Success("已删除");
    }

    private EditorOptions CreateBuilder(object obj, string title) {

        return obj switch {
            config_card_type_judge o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_connection_points o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            config_output_format_values o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_power_supply_method o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_terminalboard_type_judge o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_cable_categoryAndColor o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_cable_function o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_cable_spec o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_cable_startNumber o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_cable_systemNumber o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(380)),
            config_termination_yjs o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            config_aqj_analog o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            config_aqj_control o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            config_aqj_stationReplace o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            config_aqj_tagReplace o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            config_aqj_stationCabinet o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            config_controlSystem_mapping o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            config_xt2_cabinet_alarm o => BuildEditorOptions(o.CreateEditorBuilder().WithTitle(title).WithEditorHeight(600)),
            _ => throw new NotImplementedException("开发者注意：此类型未实现"),
        };

    }

    private EditorOptions BuildEditorOptions<T>(EditorOptionBuilder<T> builder) {
        foreach (var prop in typeof(T).GetProperties()) {
            var display = prop.GetCustomAttribute<DisplayAttribute>();
            if (display is not null && !display.GetAutoGenerateField().GetValueOrDefault(true)) { continue; }
            var propName = prop.Name;
            var displayName = display?.Name ?? propName;
            //标准化字段不能修改
            if (typeof(T) == typeof(config_controlSystem_mapping) && propName == "StdField") continue;
          
           
            if (prop.PropertyType == typeof(string)) {
                builder.AddProperty<string>(propName).WithHeader(displayName).WithPlaceHoplder($"请输入{displayName}").EditAsText();
            } else if (prop.PropertyType == typeof(double)) {
                builder.AddProperty<double>(propName).WithHeader(displayName).WithPlaceHoplder($"请输入{displayName}").EditAsDouble();
            } else if (prop.PropertyType == typeof(int)) {
                builder.AddProperty<int>(propName).WithHeader(displayName).WithPlaceHoplder($"请输入{displayName}").EditAsInt();
            }else if (prop.PropertyType == typeof(Department)){
                builder.AddProperty<Department>(propName).WithHeader(displayName).WithPlaceHoplder($"请输入{displayName}").EditAsInt();
            }else if (prop.PropertyType == typeof(bool)){
                builder.AddProperty<bool>(propName).WithHeader(displayName).WithPlaceHoplder($"请输入{displayName}").EditAsBoolean().ConvertFromProperty(x => (bool)x).ConvertToProperty(x => (bool)x); ;
            } else {
                throw new NotImplementedException("开发者注意：此类型未实现");
            }
        }

        return builder.Build();
    }

}

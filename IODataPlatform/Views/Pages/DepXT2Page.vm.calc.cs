﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using Aspose.Pdf;
using Aspose.Pdf.Operators;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using System.Text;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Channels;

using Expression = System.Linq.Expressions.Expression;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// DepXT2视图模型的计算功能部分类
/// 负责龙鳍控制系统(XT2)的IO数据计算和处理，包括板卡类型识别、信号映射、端子板配置等核心业务逻辑
/// 支持基于公式引擎的动态计算和硬编码计算两种模式，适用于工业自动化系统的IO配置管理
/// 主要业务场景：工程设计阶段的IO点配置、
/// 、运维阶段的配置变更
/// </summary>
partial class DepXT2ViewModel
{
    /// <summary>
    /// 是否使用公式模式进行计算
    /// 当设置为true时，使用硬编码的计算方法；设置为false时，使用数据库中的公式配置
    /// 用于在调试阶段快速验证计算逻辑，或在公式配置不完整时提供备用计算方案
    /// </summary>
    [ObservableProperty]
    private bool useFormula = true;

    /// <summary>
    /// UseFormula属性变更后的处理
    /// 仅切换模式，不自动触发计算
    /// 用户需要手动点击"重新计算"按钮来执行计算
    /// </summary>
    partial void OnUseFormulaChanged(bool value)
    {
        // 仅切换模式，不进行任何计算操作
        // 用户需手动点击"重新计算"按钮
    }

    /// <summary>
    /// 重新计算命令方法
    /// 触发龙鳍控制系统的IO数据重新计算，支持全量计算或指定机柜的部分计算
    /// 适用于数据导入后的批量处理、配置变更后的重新生成、问题排查时的单点验证
    /// </summary>
    /// <param name="cabinetName">指定要计算的机柜名称，为null时计算所有机柜的数据</param>
    /// <returns>异步任务，表示计算操作的完成</returns>
    /// <exception cref="Exception">当没有可计算的数据时抛出异常</exception>
    [RelayCommand]
    public async Task Recalc(string cabinetName = null)
    {
        if (SubProject is null)
        { throw new Exception("子项目为空，找不到控制系统"); }

        // 显示将要计算的列并让用户确认
        var modeText = UseFormula ? "固定算法" : "公式编辑器";
        var calculatedFields = GetCalculatedFieldsList();
        var scopeText = cabinetName == null ? "所有数据" : $"机柜 [{cabinetName}]";
        
        var confirmMessage = $"当前使用{modeText}模式\n计算范围：{scopeText}\n\n将计算以下列：\n{calculatedFields}\n\n是否继续？";
        
        var result = await message.ConfirmAsync(confirmMessage);
        if (!result)
        {
            return;
        }

        var controsystem = context.Db.Queryable<config_project_major>()
                                  .Where(it => it.Id == SubProject.MajorId).First().ControlSystem;
        await RecalcMethod(controsystem, cabinetName);
    }

    /// <summary>
    /// 获取将要计算的字段列表
    /// </summary>
    /// <returns>返回格式化的字段列表字符串</returns>
    private string GetCalculatedFieldsList()
    {
        var fields = new List<string>
        {
            "• OF显示格式",
            "• 点名 (TagName)",
            "• IO卡型号",
            "• 供电方式",
            "• 端子板型号",
            "• 柜内板卡编号",
            "• 板卡编号",
            "• 端子板编号",
            "• 板卡地址",
            "• 信号正极",
            "• 信号负极",
            "• RTD补偿C端",
            "• RTD补偿E端",
            "• 站号"
        };

        return string.Join("\n", fields);
    }

    /// <summary>
    /// 核心重新计算方法（公开接口，由Recalc命令调用）
    /// 执行指定控制系统的IO数据重新计算，包括板卡类型识别、端子板配置、信号映射、供电方式判断等完整的IO配置计算流程
    /// 支持基于公式引擎和硬编码两种计算模式，能够处理模拟量、数字量、现场总线等多种信号类型
    /// 计算完成后自动保存结果并上传至服务器，同时更新界面显示状态
    /// 适用场景：工程设计阶段的IO配置生成、设备变更后的重新计算、调试阶段的配置验证
    /// </summary>
    /// <param name="controlSystem">目标控制系统类型，决定使用的计算规则和配置参数</param>
    /// <param name="cabinetName">指定要计算的机柜名称，为null时计算所有机柜数据，用于批量处理或单机柜调试</param>
    /// <returns>异步任务，表示计算操作的完成状态</returns>
    /// <exception cref="Exception">当AllData为null（无数据可计算）时抛出异常</exception>
    /// <exception cref="InvalidOperationException">当公式计算过程中发生错误时抛出异常</exception>
    /// <exception cref="DatabaseException">当数据库查询失败或数据不一致时抛出异常</exception>
    public async Task RecalcMethod(ControlSystem controlSystem,string cabinetName=null)
    {
        await RecalcMethodInternal(controlSystem, cabinetName, showStatus: true);
    }

    /// <summary>
    /// 内部重新计算方法（无状态提示，供内部调用）
    /// 执行指定控制系统的IO数据重新计算，不显示状态提示信息
    /// 用于自动化场景，如IO分配后、编辑板卡后的自动计算
    /// </summary>
    /// <param name="controlSystem">目标控制系统类型</param>
    /// <param name="cabinetName">指定要计算的机柜名称，为null时计算所有机柜数据</param>
    /// <param name="showStatus">是否显示状态提示（默认true）</param>
    public async Task RecalcMethodInternal(ControlSystem controlSystem, string cabinetName = null, bool showStatus = true)
    {
        _ = AllData ?? throw new Exception("无数据可计算");
        if (showStatus)
        {
            model.Status.Busy($"正在重新计算……");
        }
        List<formular> formulars = UseFormula ? [] : context.Db.Queryable<formular>().ToList();
        List<formular_Index> formularIndexes = UseFormula ? [] : context.Db.Queryable<formular_Index>().ToList();
        List<formular_index_condition> conditions = UseFormula ? [] : context.Db.Queryable<formular_index_condition>().ToList();
        var fm = new FormulaBuilder();//公式编辑器
        DisplayInfo displayInfo = DisplayAttributeHelper.GetDisplayInfo<IoFullData>();
        var p = new IoFullData();
        string cardType = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.CardType)]);
        string powerSupplyMethod = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.PowerSupplyMethod)]);
        string terminalBoardModel = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.TerminalBoardModel)]);
        string signalPlus = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.SignalPlus)]);
        string signalMinus = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.SignalMinus)]);
        string rTDCompensationC = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.RTDCompensationC)]);
        string rTDCompensationE = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.RTDCompensationE)]);
        string signalEffectiveMode = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.SignalEffectiveMode)]);
        
        string fFSlaveModuleID = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.FFSlaveModuleID)]);
        string fFSlaveModuleSignalPositive = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.FFSlaveModuleSignalPositive)]);
        string fFSlaveModuleSignalNegative = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames[nameof(p.FFSlaveModuleSignalNegative)]);
        // 过滤处理的点数据
        var pointsToProcess = cabinetName == null
            ? AllData.Where(point => point.PointType != TagType.CommunicationReserved) // 跳过预留板卡
            : AllData.Where(point => point.CabinetNumber == cabinetName && point.PointType != TagType.CommunicationReserved); // 跳过预留板卡
        _ = pointsToProcess ?? throw new Exception();

        foreach (var point in pointsToProcess)
        {
            // 🔑 跳过所有预留信号，预留信号不参与任何计算
            if (point.PointType == TagType.CommunicationReserved || point.PointType == TagType.AlarmReserved)
            {
                continue;
            }
            
            // 🔑 跳过所有报警点，报警点不参与固定算法计算
            if (point.PointType == TagType.Alarm || point.PointType == TagType.CommunicationAlarm)
            {
                continue;
            }
            
            if(!UseFormula)
                point.SignalEffectiveMode = fm.FindMatchingFormulaReturnValueAsyncByExpressionNew(point, controlSystem, signalEffectiveMode, formulars, formularIndexes, conditions);

            // Process each point here
            point.OFDisplayFormat = GetOfDisplayFormat(point.RangeUpperLimit, point.RangeLowerLimit);
            point.TagName = GetTagName(point.SignalPositionNumber, point.ExtensionCode);
            point.CardType = UseFormula ? GetIoCardTypeFormula(point) : fm.FindMatchingFormulaReturnValueAsyncByExpression(point, controlSystem, cardType, formulars, formularIndexes, conditions);
            point.PowerSupplyMethod = UseFormula ? GetPowerSupplyMethodFormula(point) : fm.FindMatchingFormulaReturnValueAsyncByExpression(point, controlSystem, powerSupplyMethod, formulars, formularIndexes, conditions);
            point.TerminalBoardModel = UseFormula ? GetTerminalBoxTypeFormula(point.CardType, point.PowerSupplyMethod, point.ElectricalCharacteristics) : fm.FindMatchingFormulaReturnValueAsyncByExpression(point, ControlSystem.龙鳍, terminalBoardModel, formulars, formularIndexes, conditions);

            point.CardNumberInCabinet = GetCardInCabinetNumber(point.Cage, point.Slot, point.CardType);
            point.CardNumber = GetCardNumber(point.CabinetNumber, point.CardNumberInCabinet, point.CardType);
            point.TerminalBoardNumber = GetTerminalBoardNumber(point.TerminalBoardModel, point.CardType, point.Cage, point.Slot, point.Channel);
            point.CardAddress = GetCardAddress(point.Cage, point.Slot, point.CardType);

            point.SignalPlus = UseFormula ? GetSingalPlusFormula(point) : fm.FindMatchingFormulaReturnValueAsyncByExpressionNew(point, controlSystem, signalPlus, formulars, formularIndexes, conditions);
            point.SignalMinus = UseFormula ? GetSingalMinusFormula(point) : fm.FindMatchingFormulaReturnValueAsyncByExpressionNew(point, controlSystem, signalMinus, formulars, formularIndexes, conditions);
            point.RTDCompensationC = UseFormula ? GetRTDCompensationEndC(point) : fm.FindMatchingFormulaReturnValueAsyncByExpressionNew(point, controlSystem, rTDCompensationC, formulars, formularIndexes, conditions);
            point.RTDCompensationE = UseFormula ? GetRTDCompensationEndD(point.PowerSupplyMethod, point.Channel) : fm.FindMatchingFormulaReturnValueAsyncByExpressionNew(point, controlSystem, rTDCompensationE, formulars, formularIndexes, conditions);

        } 
        //设置站号
        SetSTationNumber();
        await SaveAndUploadFileAsync();
        AllData = [.. AllData];
        if (showStatus)
        {
            model.Status.Success("重新计算完毕");
        }
    }
    /// <summary>
    /// 设置站号方法
    /// 为每个机柜分配唯一的站号，用于现场总线通信的设备地址识别
    /// 站号从2开始递增，每个机柜中的所有IO点使用相同的站号
    /// 适用于PROFIBUS-DP、FF等现场总线系统的设备地址配置
    /// 业务场景：系统集成阶段的站号规划、现场调试时的地址对照、运维阶段的设备管理
    /// </summary>
    /// <remarks>
    /// 站号分配规则：
    /// - 起始地址：2（保留地址0和1为系统使用）
    /// - 分配方式：按机柜编号顺序递增
    /// - 地址范围：2-125（PROFIBUS协议限制）
    /// </remarks>
    void SetSTationNumber()
    {
        //添加站号
        var startAddress = 2;
        var cabinets = AllData.GroupBy(a => a.CabinetNumber);
        foreach (var cabinet in cabinets)
        {
            foreach (var point in cabinet.ToList())
            {
                point.StationNumber = (startAddress).ToString();
            }
            startAddress++;
        }
       
    }
       
    /// <summary>
    /// 获取字段显示名称的反射工具方法
    /// 通过反射机制获取指定字段的DisplayAttribute显示名称，用于动态构建界面显示和数据映射
    /// 适用于多语言支持、动态表单生成、配置化界面等场景
    /// </summary>
    /// <typeparam name="T">目标类型，通常为数据模型类</typeparam>
    /// <param name="fieldName">字段名称，必须与类中的属性名一致</param>
    /// <returns>返回DisplayAttribute中定义的显示名称，如果没有定义则返回字段名本身</returns>
    /// <exception cref="ArgumentException">当字段名不存在时可能引发反射异常</exception>
    public static string GetFieldDisplayName<T>(string fieldName)
    {
        Type type = typeof(T);
        var propertyInfo = type.GetProperty(fieldName);
        if (propertyInfo != null)
        {
            var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                return displayAttribute.Name;
            }
        }
        return fieldName; // 如果没有 Display Attribute，则返回字段名
    }

    /// <summary>
    /// 龙鳍系统供电方式计算方法
    /// 根据新的复杂公式计算供电方式，支持多种协议检测和类型判断
    /// 支持AI、AO、DI、DO、PI、FF、DP等多种信号类型的供电判断，精确区分DCS供电和现场供电
    /// 适用于工程设计阶段的供电方案选型、现场施工阶段的接线指导、调试阶段的问题排查
    /// </summary>
    /// <param name="x">包含供电相关信息的IO数据实体，包括描述、供电类型等关键字段</param>
    /// <returns>返回格式化的供电方式字符串，如"DCS"、"USER提供24VDC"等，"--"表示不需要供电，"Err"表示计算错误</returns>
    /// <exception cref="Exception">当供电类型参数不正确或无法识别时抛出异常</exception>
    /// <remarks>
    /// 新公式逻辑：
    /// =IF(OR(IFERROR(SEARCH("TCP",AG4),-1)>0,IFERROR(SEARCH("RTU",AG4),-1)>0),"--",
    ///   IF(LEFT(AI4,2)="AI",CHOOSE(MID(AI4,3,IF(IFERROR(SEARCH("NH",AI4),-1)>0,"1",LEN(AI4)-1)),"DCS","DCS(提供24VDC)","DCS(提供220VAC)","USER","USER","DCS(提供220VAC)","DCS","DCS","DCS","DCS"),
    ///     IF(LEFT(AI4,2)="AO","DCS",
    ///       IF(LEFT(AI4,1)="P",CHOOSE(MID(AI4,2,LEN(AI4)-1),"DCS(提供24VDC)","DCS(提供220VAC)","USER"),
    ///         IF(LEFT(AI4,2)="DI",CHOOSE(MID(AI4,3,LEN(AI4)-1),"DCS","DCS(提供24VDC)","DCS(提供220VAC)","USER","USER","DCS(提供24VDC)"),
    ///           IF(LEFT(AI4,2)="DO",CHOOSE(MID(AI4,3,LEN(AI4)-1),"DCS","DCS(提供24VDC)","DCS(提供220VAC)","USER","USER"),
    ///             IF(LEFT(AI4,2)="FF",CHOOSE(MID(AI4,3,LEN(AI4)-1),"DCS","DCS(提供24VDC)","DCS(提供220VAC)","USER","USER","DCS","DCS","DCS"),
    ///               IF(LEFT(AI4,2)="DP","DCS(提供220VAC)",
    ///                 IF(IFERROR(SEARCH("DP",AG4),-1)>0,"--",
    ///                   IF(IFERROR(SEARCH("OPC",AG4),-1)>0,"--","Err")))))))))
    /// 供电方式编码规则：
    /// - TCP/RTU协议：根据描述中的关键字返回"--"
    /// - AI类型：根据编号选择不同供电方式，特殊处理NH格式
    /// - AO类型：统一返回"DCS"
    /// - P类型：根据编号选择供电方式
    /// - DI/DO类型：根据编号区分供电方案
    /// - FF类型：现场总线供电处理
    /// - DP类型：统一使用220VAC供电
    /// - 其他协议：DP/OPC根据描述判断
    /// </remarks>
    public string GetPowerSupplyMethodFormula(IoFullData x)
    {
        try
        {
            string description = x.Description ?? "";
            string powerType = x.PowerType ?? "";

            // 第一层判断：检查描述中是否包含TCP或RTU
            if (description.Contains("TCP") || description.Contains("RTU"))
            {
                return "--";
            }

            // 判断供电类型的前缀
            if (powerType.Length >= 2)
            {
                string prefix2 = powerType.Substring(0, 2);
                string prefix1 = powerType.Substring(0, 1);

                // AI类型处理
                if (prefix2 == "AI")
                {
                    // 计算MID的长度：如果包含NH则为1，否则为总长度-1
                    int midLength = powerType.Contains("NH") ? 1 : powerType.Length - 2;
                    if (midLength > 0 && powerType.Length >= 3)
                    {
                        string numberPart = powerType.Substring(2, Math.Min(midLength, powerType.Length - 2));
                        return numberPart switch
                        {
                            "1" => "DCS",
                            "2" => "DCS(提供24VDC)",
                            "3" => "DCS(提供220VAC)",
                            "4" => "USER",
                            "5" => "USER",
                            "6" => "DCS(提供220VAC)",
                            "7" => "DCS",
                            "8" => "DCS",
                            "9" => "DCS",
                            _ => "DCS"
                        };
                    }
                    return "DCS";
                }

                // AO类型处理
                if (prefix2 == "AO")
                {
                    return "DCS";
                }

                // P类型处理
                if (prefix1 == "P" && powerType.Length >= 2)
                {
                    string numberPart = powerType.Substring(1);
                    return numberPart switch
                    {
                        "1" => "DCS(提供24VDC)",
                        "2" => "DCS(提供220VAC)",
                        "3" => "USER",
                        _ => "DCS(提供24VDC)"
                    };
                }

                // DI类型处理
                if (prefix2 == "DI" && powerType.Length >= 3)
                {
                    string numberPart = powerType.Substring(2);
                    return numberPart switch
                    {
                        "1" => "DCS",
                        "2" => "DCS(提供24VDC)",
                        "3" => "DCS(提供220VAC)",
                        "4" => "USER",
                        "5" => "USER",
                        "6" => "DCS(提供24VDC)",
                        _ => "DCS"
                    };
                }

                // DO类型处理
                if (prefix2 == "DO" && powerType.Length >= 3)
                {
                    string numberPart = powerType.Substring(2);
                    return numberPart switch
                    {
                        "1" => "DCS",
                        "2" => "DCS(提供24VDC)",
                        "3" => "DCS(提供220VAC)",
                        "4" => "USER",
                        "5" => "USER",
                        _ => "DCS"
                    };
                }

                // FF类型处理
                if (prefix2 == "FF" && powerType.Length >= 3)
                {
                    string numberPart = powerType.Substring(2);
                    return numberPart switch
                    {
                        "1" => "DCS",
                        "2" => "DCS(提供24VDC)",
                        "3" => "DCS(提供220VAC)",
                        "4" => "USER",
                        "5" => "USER",
                        "6" => "DCS",
                        "7" => "DCS",
                        "8" => "DCS",
                        _ => "DCS"
                    };
                }

                // DP类型处理
                if (prefix2 == "DP")
                {
                    return "DCS(提供220VAC)";
                }
            }

            // 检查描述中的其他协议
            if (description.Contains("DP"))
            {
                return "--";
            }

            if (description.Contains("OPC"))
            {
                return "--";
            }

            // 默认情况
            return "Err";
        }
        catch (Exception ex)
        {
            throw new Exception($"供电类型计算错误：{ex.Message}");
        }
    }

    /// <summary>
    /// 端子板类型计算方法（新版Excel公式实现）
    /// 根据板卡类型、供电方式和电气特性等参数，自动选择合适的端子板型号
    /// 支持龙鳍系统所有板卡类型的端子板选型，确保信号、供电和安全隔离的匹配性
    /// 适用于工程设计阶段的端子板选型、采购阶段的材料统计、现场安装阶段的型号核对
    /// </summary>
    /// <param name="boardType">板卡类型，如"AI216"、"DI211"、"FF211"等，决定基本的端子板选型方向</param>
    /// <param name="powerSupply">供电方式，如"DCS"、"USER"等，影响端子板的隔离设计</param>
    /// <param name="electricalCharacteristics">电气特性，如"4~20mA"、"PT100"等，决定信号调理方式</param>
    /// <returns>返回端子板型号，如"TB241"、"TB271"等，"--"表示不需要端子板，"ERR"表示选型失败</returns>
    /// <remarks>
    /// 新公式逻辑：
    /// =IF(I3="DI211",IF(LEFT(AI3,4)="USER","TB221","TB222"),
    ///   IF(I3="DO211",IF(LEFT(AI3,4)="USER","TB231","TB233"),
    ///     IF(OR(I3="AI216",I3="AI212"),"TB241",
    ///       IF(I3="AI221","TB242",
    ///         IF(I3="AI232","TB246",
    ///           IF(I3="AO211","TB251",
    ///             IF(OR(I3="PI211",I3="AO215",I3="MD211"),"TB244",
    ///               IF(I3="MD216","--",
    ///                 IF(I3="FF211","TB271",
    ///                   IF(I3="DP211","TB272",
    ///                     IF(I3="--","--","ERR"))))))))))
    /// 
    /// 端子板选型规则：
    /// - DI211：根据供电方式选择TB221(USER)或TB222(DCS)
    /// - DO211：根据供电方式选择TB231(USER)或TB233(DCS)
    /// - AI216/AI212：使用TB241（4-20mA信号调理）
    /// - AI221：使用TB242（热电偶信号调理）
    /// - AI232：使用TB246（热电阻信号调理）
    /// - AO211：使用TB251（模拟量输出）
    /// - PI211/AO215/MD211：使用TB244（多功能端子板）
    /// - MD216：不需要端子板，返回"--"
    /// - FF211：使用TB271（FF总线端子板）
    /// - DP211：使用TB272（DP总线端子板）
    /// - "--"：无板卡，返回"--"
    /// - 其他：返回"ERR"表示选型错误
    /// </remarks>
    public string GetTerminalBoxTypeFormula(string boardType, string powerSupply, string electricalCharacteristics)
    {
        // 第1层：DI211板卡判断
        if (boardType == "DI211")
        {
            // 根据供电方式前4位判断
            if (powerSupply.Length >= 4 && powerSupply.Substring(0, 4) == "USER")
            {
                return "TB221"; // 用户供电
            }
            return "TB222"; // DCS供电或其他
        }

        // 第2层：DO211板卡判断
        if (boardType == "DO211")
        {
            // 根据供电方式前4位判断
            if (powerSupply.Length >= 4 && powerSupply.Substring(0, 4) == "USER")
            {
                return "TB231"; // 用户供电
            }
            return "TB233"; // DCS供电或其他
        }

        // 第3层：AI216或AI212板卡
        if (boardType == "AI216" || boardType == "AI212")
        {
            return "TB241";
        }

        // 第4层：AI221板卡（热电偶）
        if (boardType == "AI221")
        {
            return "TB242";
        }

        // 第5层：AI232板卡（热电阻）
        if (boardType == "AI232")
        {
            return "TB246";
        }

        // 第6层：AO211板卡
        if (boardType == "AO211")
        {
            return "TB251";
        }

        // 第7层：PI211、AO215或MD211板卡
        if (boardType == "PI211" || boardType == "AO215" || boardType == "MD211")
        {
            return "TB244";
        }

        // 第8层：MD216板卡（MODBUS TCP不需要端子板）
        if (boardType == "MD216")
        {
            return "--";
        }

        // 第9层：FF211板卡（FF总线）
        if (boardType == "FF211")
        {
            return "TB271";
        }

        // 第10层：DP211板卡（PROFIBUS DP）
        if (boardType == "DP211")
        {
            return "TB272";
        }

        // 第11层：无板卡
        if (boardType == "--")
        {
            return "--";
        }

        // 第12层：未知板卡类型
        return "ERR";
    }

    /// <summary>
    /// IO板卡类型计算方法（新版Excel公式实现）
    /// 根据新的Excel公式逻辑进行板卡类型识别，优先检测描述字段中的协议关键字
    /// 支持龙鳍系统全系列板卡的自动识别，包括模拟量、数字量、现场总线、串口通信等类型
    /// 适用于工程设计阶段的板卡配置、采购阶段的材料统计、现场安装阶段的板卡核对
    /// </summary>
    /// <param name="data">包含板卡选型所需全部信息的IO数据实体，包括描述、IO类型、供电类型、抗震类别等</param>
    /// <returns>返回板卡型号，如"AI216"、"DI211"、"FF211"等，"--"表示OPC协议，"未知板卡"表示无法识别</returns>
    /// <remarks>
    /// 新公式逻辑（按优先级从高到低）：
    /// 1. TCP协议检测：描述中包含"TCP" → MD216
    /// 2. RTU协议检测：描述中包含"RTU" → MD211
    /// 3. DP/PROFIBUS检测：描述中包含"DP"或"PROFIBUS" → DP211
    /// 4. FF检测：供电类型以FF开头 或 描述中包含"FF" → FF211
    /// 5. PI检测：供电类型以P开头 或 IO类型为PI 或 抗震类别为P → PI211
    /// 6. AI类型细分：
    ///    - AI+供电类型含"NH" → AI212（计量泵等）
    ///    - AI+描述含"4*20mA" → AI216（标准4-20mA）
    ///    - AI+描述含"TC" → AI221（热电偶）
    ///    - AI+描述含"PT" → AI232（热电阻）
    /// 7. AO类型细分：
    ///    - AO+供电类型为AO1或AO2 → AO211
    ///    - AO+供电类型为AOH → AO215
    /// 8. DI类型：IO类型为DI → DI211
    /// 9. DO类型：IO类型为DO → DO211
    /// 10. OPC协议：描述中包含"OPC" → "--"
    /// 11. 其他情况 → "未知板卡"
    /// </remarks>
    public string GetIoCardTypeFormula(IoFullData data)
    {
        string lectricalCharacteristics = data.ElectricalCharacteristics ?? "";
        string ioType = data.IoType?.Trim() ?? "";
        string powerType = data.PowerType?.Trim() ?? "";
        string seismicCategory = data.SeismicCategory?.Trim() ?? "";

        // 第1层：TCP协议检测（描述中包含"TCP"）
        if (lectricalCharacteristics.IndexOf("TCP", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "MD216";
        }

        // 第2层：RTU协议检测（描述中包含"RTU"）
        if (lectricalCharacteristics.IndexOf("RTU", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "MD211";
        }

        // 第3层：DP或PROFIBUS协议检测
        if (lectricalCharacteristics.IndexOf("DP", StringComparison.OrdinalIgnoreCase) >= 0 ||
            lectricalCharacteristics.IndexOf("PROFIBUS", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "DP211";
        }

        // 第4层：FF协议检测
        // 修改为：供电类型以FF开头 或 描述中包含"FF"
        if (powerType.StartsWith("FF", StringComparison.OrdinalIgnoreCase) || 
            lectricalCharacteristics.IndexOf("FF", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "FF211";
        }

        // 第5层：PI类型判断
        // 修改为：供电类型以P开头 或 IO类型为PI 或 抗震类别为P
        if (powerType.StartsWith("P", StringComparison.OrdinalIgnoreCase) || 
            ioType == "PI" || 
            seismicCategory == "P")
        {
            return "PI211";
        }

        // 第6层：AI类型细分判断
        if (ioType == "AI")
        {
            // AI + 供电类型包含"NH" → AI212
            if (powerType.IndexOf("NH", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "AI212";
            }

            // AI + 描述包含"4*20mA" → AI216（*代表任意字符）
            if (System.Text.RegularExpressions.Regex.IsMatch(lectricalCharacteristics, @"4.20mA", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return "AI216";
            }

            // AI + 描述包含"TC" → AI221
            if (lectricalCharacteristics.IndexOf("TC", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "AI221";
            }

            // AI + 描述包含"PT" → AI232
            if (lectricalCharacteristics.IndexOf("PT", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "AI232";
            }

            // 如果是AI但没有匹配到具体类型，返回未知AI板卡
            return "未知AI板卡";
        }

        // 第7层：AO类型细分判断
        if (ioType == "AO")
        {
            // AO + 供电类型为AO1或AO2 → AO211
            if (powerType == "AO1" || powerType == "AO2")
            {
                return "AO211";
            }

            // AO + 供电类型为AOH → AO215
            if (powerType == "AOH")
            {
                return "AO215";
            }

            // 默认AO返回未知
            return "未知AO板卡";
        }

        // 第8层：DI类型判断
        if (ioType == "DI")
        {
            return "DI211";
        }

        // 第9层：DO类型判断
        if (ioType == "DO")
        {
            return "DO211";
        }

        // 第10层：OPC协议检测（描述中包含"OPC"）
        if (lectricalCharacteristics.IndexOf("OPC", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "--";
        }

        // 第11层：无法识别的板卡类型
        return "未知板卡";
    }

    /// <summary>
    /// 计算输出显示格式的小数位数（新版Excel公式实现）
    /// 根据测量范围的上下限值和工程单位计算合适的小数位数，确保数据显示的精度和可读性平衡
    /// 采用智能算法根据量程范围大小自动调整显示精度，为操作员提供最优的数据呈现
    /// 适用于监控画面的数值显示、报表生成、数据归档等场景
    /// </summary>
    /// <param name="upperLimitStr">测量范围上限值字符串，需要能够转换为数值类型</param>
    /// <param name="lowerLimitStr">测量范围下限值字符串，需要能够转换为数值类型</param>
    /// <param name="engineeringUnit">工程单位，用于特殊单位处理（如r/min）</param>
    /// <returns>返回小数位数字符串，如"3"、"2"、"1"、"0"，空字符串表示上限为空或转换失败</returns>
    /// <remarks>
    /// 新公式逻辑：=IF(AM3="","",IF(AK3="r/min",0,IF(ABS(AM3-AL3)<=10,3,IF(AND((10<ABS(AM3-AL3)),(ABS(AM3-AL3)<=100)),2,IF(100<ABS(AM3-AL3),1,"")))))
    /// 
    /// 显示精度分级规则：
    /// - 上限为空：返回空字符串
    /// - 工程单位为"r/min"：返回"0"（不显示小数）
    /// - 范围 ≤ 10：显示3位小数（高精度测量）
    /// - 10 < 范围 ≤ 100：显示2位小数（中精度测量）
    /// - 范围 > 100：显示1位小数（低精度测量）
    /// - 其他情况：返回空字符串
    /// 
    /// 与旧版本差异：
    /// 1. 新增上限为空检查
    /// 2. 新增工程单位"r/min"特殊处理
    /// 3. 简化逻辑，移除科学计数法处理
    /// 4. 简化范围>100的处理，统一返回"1"
    /// </remarks>
    public string GetOfDisplayFormat(string upperLimitStr, string lowerLimitStr, string engineeringUnit = "")
    {
        // 公式第1层：IF(AM3="","",...)
        if (string.IsNullOrEmpty(upperLimitStr))
        {
            return "";
        }

        // 公式第2层：IF(AK3="r/min",0,...)
        if (engineeringUnit == "r/min")
        {
            return "0";
        }

        // 尝试转换上下限值
        if (double.TryParse(upperLimitStr, out double upperLimit) && 
            double.TryParse(lowerLimitStr, out double lowerLimit))
        {
            // 计算范围的绝对值
            double range = Math.Abs(upperLimit - lowerLimit);

            // 公式第3层：IF(ABS(AM3-AL3)<=10,3,...)
            if (range <= 10)
            {
                return "3";
            }

            // 公式第4层：IF(AND((10<ABS(AM3-AL3)),(ABS(AM3-AL3)<=100)),2,...)
            if (range > 10 && range <= 100)
            {
                return "2";
            }

            // 公式第5层：IF(100<ABS(AM3-AL3),1,"")
            if (range > 100)
            {
                return "1";
            }

            // 其他情况返回空字符串
            return "";
        }
        else
        {
            // 转换失败返回空字符串
            return "";
        }
    }
    /// <summary>
    /// 基于配置表的输出显示格式计算方法（重载方法）
    /// 根据数据库配置表和IO数据的工程单位、测量范围进行精确的显示格式匹配
    /// 提供比基础方法更精确的格式计算，支持不同工程单位的个性化显示需求
    /// 适用于复杂工况下的精确显示控制、标准化显示格式管理、多单位制转换显示
    /// </summary>
    /// <param name="query">输出格式配置值列表，包含不同范围和单位的显示格式定义</param>
    /// <param name="data">IO数据实体，包含测量范围和工程单位信息</param>
    /// <returns>返回匹配的小数位数，如果找到唯一匹配返回配置值，否则返回空字符串</returns>
    /// <remarks>
    /// 匹配优先级：
    /// 1. 工程单位匹配（精确匹配或空值兼容）
    /// 2. 下限值匹配（配置下限 < 数据下限）
    /// 3. 上限值匹配（配置上限 >= 数据上限）
    /// 4. 唯一性验证（必须只有一条匹配记录）
    /// </remarks>
    public string GetOfDisplayFormat(List<config_output_format_values> query, IoFullData data)
    {

        // 使用TryParse，如果转换失败，则low和high默认为0
        _ = int.TryParse(data.RangeLowerLimit, out int low);
        _ = int.TryParse(data.RangeUpperLimit, out int high);


        // 如果EngineeringUnit为空，筛选RangeUnit为空的记录
        if (string.IsNullOrEmpty(data.EngineeringUnit))
        {
            query = query.Where(r => r.RangeUnit == "").ToList();
        }
        else
        {
            query = query.Where(r => r.RangeUnit == data.EngineeringUnit).ToList();
        }

        // 根据low和high的值进一步筛选
        if (query != null)
        {
            query = query.Where(f => f.RangeLow < low).ToList();
        }
        if (query != null)
        {
            query = query.Where(f => f.RangeHigh >= high).ToList();
        }
        if (query != null && query.Count == 1)
        {
            // 执行查询并获取结果
            var formatValue = query.ToList().FirstOrDefault();
            // 如果找到匹配的记录，返回DecimalPlaces，否则返回空字符串
            return formatValue?.DecimalPlaces ?? "";
        }
        return "";
    }

    /// <summary>
    /// 基于配置表的IO板卡类型判断方法
    /// 通过数据库配置表进行IO板卡类型的智能匹配，支持多条件组合判断
    /// 提供比硬编码方法更灵活的配置化板卡选型，便于后期维护和扩展
    /// 适用于标准化的板卡选型规则管理、产品线扩展的兼容支持
    /// </summary>
    /// <param name="list">板卡类型判断配置列表，包含IO类型、信号规格、供电类型等判断条件</param>
    /// <param name="data">IO数据实体，包含板卡选型所需的所有判断参数</param>
    /// <returns>返回匹配的IO板卡类型，如无匹配返回"未知板卡"</returns>
    /// <remarks>
    /// 匹配条件（所有条件必须同时满足）：
    /// - IO类型匹配或配置为空（兼容所有类型）
    /// - 信号规格匹配或配置为空（支持分号分隔的多值匹配）
    /// - 供电类型精确匹配或配置为空
    /// </remarks>
    public string GetIoCardType(List<config_card_type_judge> list, IoFullData data)
    {
        return list.Where(x => string.IsNullOrEmpty(x.IoType) || x.IoType == data.IoType)
            .Where(x => string.IsNullOrEmpty(x.SignalSpec) || x.SignalSpec.Split(";").Contains(data.ElectricalCharacteristics))
            .Where(x => string.IsNullOrEmpty(x.PowerType) || x.PowerType == data.PowerType)
            .FirstOrDefault()?.IoCardType ?? "未知板卡";
    }

    /// <summary>
    /// 基于配置表的端子板类型判断方法
    /// 通过数据库配置表进行端子板类型的智能匹配，支持板卡类型、信号类型、信号规格的组合判断
    /// 提供灵活的配置化端子板选型，确保与板卡和信号特性的完美匹配
    /// 适用于标准化端子板选型规则、产品系列化管理、现场安装指导
    /// </summary>
    /// <param name="list">端子板类型判断配置列表，包含板卡类型、信号类型、信号规格等匹配条件</param>
    /// <param name="data">IO数据实体，包含端子板选型所需的板卡类型、供电方式、电气特性等信息</param>
    /// <returns>返回匹配的端子板型号，如无匹配返回"ERR"</returns>
    /// <remarks>
    /// 匹配逻辑：
    /// - 板卡类型：精确匹配或配置为空
    /// - 信号类型：供电方式前缀匹配或配置为空  
    /// - 信号规格：电气特性精确匹配或配置为空
    /// 所有条件必须同时满足才能匹配成功
    /// </remarks>
    public string GetTerminalBoxType(List<config_terminalboard_type_judge> list, IoFullData data)
    {
        return list.Where(x => string.IsNullOrEmpty(x.CardType) || x.CardType == data.CardType)
            .Where(x => string.IsNullOrEmpty(x.SignalType) || data.PowerSupplyMethod.StartsWith(x.SignalType))
            .Where(x => string.IsNullOrEmpty(x.SignalSpec) || data.ElectricalCharacteristics == x.SignalSpec)
            .FirstOrDefault()?.TerminalBlock ?? "ERR";
    }

    /// <summary>
    /// 基于配置表的供电方式判断方法
    /// 通过数据库配置表进行供电方式的智能匹配，支持供电类型、板卡类型、传感器类型的组合判断
    /// 提供标准化的供电方式选择逻辑，确保供电配置的准确性和一致性
    /// 适用于供电方案的标准化管理、安全供电设计、设备兼容性验证
    /// </summary>
    /// <param name="list">供电方式配置列表，包含供电类型、板卡类型、传感器类型等匹配条件</param>
    /// <param name="data">IO数据实体，包含供电方式判断所需的供电方式、板卡类型、传感器类型等信息</param>
    /// <returns>返回匹配的供电模式，如无匹配返回"ERR"</returns>
    /// <remarks>
    /// 匹配条件：
    /// - 供电类型：与数据供电方式精确匹配或配置为空
    /// - 板卡类型：与数据板卡类型精确匹配或配置为空
    /// - 传感器类型：与数据传感器类型精确匹配或配置为空
    /// 注意：代码中第二个板卡类型判断可能存在逻辑错误（data.CardType == data.CardType）
    /// </remarks>
    public string GetPowerSupplyMethod(List<config_power_supply_method> list, IoFullData data)
    {
        return list.Where(x => string.IsNullOrEmpty(x.SupplyType) || x.SupplyType == data.PowerSupplyMethod)
            .Where(x => string.IsNullOrEmpty(x.CardType) || data.CardType == data.CardType)
            .Where(x => string.IsNullOrEmpty(x.SensorType) || data.SensorType == x.SensorType)
            .FirstOrDefault()?.SupplyModel ?? "ERR";
    }

    /// <summary>
    /// 获取标准化信号位号
    /// 对原始信号位号进行标准化处理，移除连字符以生成规范的位号格式
    /// 适用于位号标准化管理、图纸自动生成、数据库索引优化
    /// 业务场景：P&ID图纸位号核对、仪表台账管理、现场设备标识
    /// </summary>
    /// <param name="SignalPositionNumber">原始信号位号，可能包含连字符等特殊字符</param>
    /// <returns>返回处理后的标准化信号位号，如果输入为空则返回空字符串</returns>
    /// <remarks>
    /// 处理规则：
    /// - 移除所有连字符("-")
    /// - 保持其他字符不变
    /// - 空值安全处理
    /// </remarks>
    public string GetSignalPositionNumber(string SignalPositionNumber)
    {
        if (string.IsNullOrEmpty(SignalPositionNumber))
        {
            return "";
        }
        return SignalPositionNumber.Replace("-", "");
    }

    /// <summary>
    /// 生成标准化IO点名（TagName）
    /// 根据原始变量名和扩展码生成符合龙鳍系统规范的IO点名格式
    /// 采用"R"+变量名+"_"+扩展码的命名规则，确保点名的唯一性和可识别性
    /// 适用于DCS组态、监控画面开发、数据库建表、报警配置等场景
    /// </summary>
    /// <param name="oldVarName">原始变量名，通常为信号位号或设备标识</param>
    /// <param name="oldExtCode">扩展码，用于区分同一设备的不同信号或参数</param>
    /// <returns>返回格式化的IO点名，格式为"R{变量名}[_{扩展码}]"</returns>
    /// <remarks>
    /// 命名规则：
    /// - 前缀：固定添加"R"标识符
    /// - 变量名处理：去除空格和连字符
    /// - 扩展码：非空时添加下划线分隔符
    /// - 示例：R010001_PV, R010002（无扩展码）
    /// </remarks>
    public string GetTagName(string oldVarName, string oldExtCode)
    {
        string processedText = "R" + oldVarName.Trim().Replace("-", "");
        if (!string.IsNullOrEmpty(oldExtCode))
        {
            processedText += "_" + oldExtCode;
        }
        return processedText;
    }

    /// <summary>
    /// 生成IO板卡柜内编号（新版Excel公式实现）
    /// 根据笼号、槽位号和板卡类型生成机柜内唯一的板卡编号
    /// 采用标准化编号规则确保现场安装和维护时的准确识别
    /// 适用于机柜布置图生成、设备标识制作、维护手册编制、备件管理
    /// </summary>
    /// <param name="cage">笼号，标识机柜内的笼架位置</param>
    /// <param name="slot">槽位号，标识笼架内的具体槽位（自动补零到2位）</param>
    /// <param name="IOCardType">IO板卡类型，如"AI216"、"DI211"等</param>
    /// <returns>返回柜内编号，格式为"{笼号}{槽位:00}{类型标识}"，板卡类型为"--"或空时返回"--"或空字符串</returns>
    /// <remarks>
    /// 新公式逻辑：=IF(I3="--","--",L3&TEXT(M3,"00")&IF(I3="AI232","RTD",IF(I3="AI221","TC",LEFT(I3,2))))
    /// 编号规则：
    /// - 板卡类型为"--" → 返回"--"
    /// - 笼号：直接使用原值
    /// - 槽位：格式化为两位数字（如01、02、10）
    /// - 类型标识：
    ///   * AI232 → "RTD"（热电阻）
    ///   * AI221 → "TC"（热电偶）
    ///   * 其他类型 → 取前两位字符
    /// - 示例：
    ///   * 105AI（笼1槽05的AI216板卡）
    ///   * 210RTD（笼2槽10的AI232热电阻板卡）
    ///   * 103TC（笼1槽03的AI221热电偶板卡）
    ///   * 208DI（笼2槽08的DI211板卡）
    /// </remarks>
    public string GetCardInCabinetNumber(int cage, int slot, string IOCardType)
    {
        // 公式第1层：IF(I3="--","--",...)
        if (IOCardType == "--")
        {
            return "--";
        }

        // 空值处理
        if (string.IsNullOrEmpty(IOCardType))
        {
            return "";
        }

        // 公式第2层：L3&TEXT(M3,"00")&...
        // 槽位格式化为两位数字
        string slotFormatted = $"{slot:00}";

        // 公式第3层：IF(I3="AI232","RTD",IF(I3="AI221","TC",LEFT(I3,2)))
        string typeIdentifier;
        if (IOCardType == "AI232")
        {
            typeIdentifier = "RTD"; // 热电阻
        }
        else if (IOCardType == "AI221")
        {
            typeIdentifier = "TC";  // 热电偶 (ThermoCouple)
        }
        else
        {
            // 取板卡类型前2位
            typeIdentifier = IOCardType.Length >= 2 ? IOCardType.Substring(0, 2) : IOCardType;
        }

        // 拼接结果：机笼 + 槽位(2位) + 类型标识
        return $"{cage}{slotFormatted}{typeIdentifier}";
    }

    /// <summary>
    /// 生成完整板卡编号（新版Excel公式实现）
    /// 根据机柜号和板卡柜内编号生成完整的系统级板卡编号
    /// 采用标准化编号规则确保全系统范围内的板卡唯一性
    /// 适用于系统级设备管理、跨机柜资源统计、全局设备索引
    /// </summary>
    /// <param name="cabinetNumber">机柜编号，如"C"、"D"等</param>
    /// <param name="cardNumberInCabinet">板卡柜内编号，如"105AI"、"210RTD"等</param>
    /// <param name="cardType">板卡类型，用于判断是否为特殊值"--"</param>
    /// <returns>返回完整板卡编号，格式为"{机柜号}{柜内编号}"，板卡类型为"--"时返回"--"</returns>
    /// <remarks>
    /// 新公式逻辑：=IF(I3="--","--",H3&J3)
    /// 编号规则：
    /// - 板卡类型为"--" → 返回"--"
    /// - 正常情况 → 机柜号 + 板卡柜内编号
    /// - 示例：
    ///   * C105AI（机柜C的105号AI板卡）
    ///   * D210RTD（机柜D的210号热电阻板卡）
    ///   * --（无板卡）
    /// 业务场景：
    /// - 用于生成系统级唯一的板卡标识
    /// - 便于跨机柜的设备管理和统计
    /// - 支持设备台账和备件库管理
    /// </remarks>
    public string GetCardNumber(string cabinetNumber, string cardNumberInCabinet, string cardType)
    {
        // 公式：IF(I3="--","--",H3&J3)
        if (cardType == "--")
        {
            return "--";
        }

        // 空值安全处理
        if (string.IsNullOrEmpty(cabinetNumber) || string.IsNullOrEmpty(cardNumberInCabinet))
        {
            return "";
        }

        // 拼接机柜号和柜内编号
        return $"{cabinetNumber}{cardNumberInCabinet}";
    }

    /// <summary>
    /// 生成端子板编号
    /// 根据端子板型号、板卡类型、安装位置和网络类型生成唯一的端子板编号
    /// 支持不同类型端子板的编号规则，特别处理FF现场总线的网络标识
    /// 适用于端子板标识制作、接线图生成、现场安装指导、维护手册编制
    /// </summary>
    /// <param name="terminalBoardModel">端子板型号，如"TB271"、"TB241"等（仅作为传入参数，未使用）</param>
    /// <param name="cardType">板卡类型，用于确定端子板类别和编号规则</param>
    /// <param name="cage">笼号，标识机柜内的笼架位置</param>
    /// <param name="slot">槽位号，标识笼架内的具体槽位</param>
    /// <param name="channel">通道号，对FF211板卡表示网段号，直接使用N列值</param>
    /// <returns>返回端子板编号，格式根据板卡类型而定</returns>
    /// <remarks>
    /// 新公式逻辑：=IF(I3="FF211",L3&TEXT(M3,"00")&"_"&N3&"FI",IF(OR(I3="MD216",I3="--"),"--",L3&TEXT(M3,"00")&"BN"))
    /// 编号规则：
    /// - FF211板卡：{笼号}{槽位:00}_{通道号}FI（通道号直接使用N列值）
    /// - MD216或"--"：返回"--"
    /// - 其他板卡：{笼号}{槽位:00}BN
    /// - 示例：101_1FI（FF板卡通道1）、205BN（普通板卡）
    /// </remarks>
    public string GetTerminalBoardNumber(string terminalBoardModel, string cardType, int cage, int slot, int channel)
    {
        // 公式第1层：IF(I3="FF211",L3&TEXT(M3,"00")&"_"&N3&"FI",...)
        if (cardType == "FF211")
        {
            return $"{cage}{slot:00}_{channel}FI";
        }

        // 公式第2层：IF(OR(I3="MD216",I3="--"),"--",...)
        if (cardType == "MD216" || cardType == "--")
        {
            return "--";
        }

        // 公式第3层：默认返回 L3&TEXT(M3,"00")&"BN"
        return $"{cage}{slot:00}BN";
    }

    /// <summary>
    /// 计算板卡系统地址（新版Excel公式实现）
    /// 根据笼号和槽位号计算板卡在龙鳍系统中的唯一地址编号
    /// 采用标准地址映射算法确保地址空间的合理分配和系统扩展性
    /// 适用于系统组态、通信配置、故障诊断、地址规划
    /// </summary>
    /// <param name="cage">笼号，从1开始编号</param>
    /// <param name="slot">槽位号，从1开始编号</param>
    /// <param name="cardType">板卡类型，用于判断是否为特殊值"--"</param>
    /// <returns>返回计算得到的板卡系统地址，板卡类型为"--"时返回"--"字符串</returns>
    /// <remarks>
    /// 新公式逻辑：=IF(I3="--","--",(L3-1)*10+M3)
    /// 地址计算公式：(笼号-1) × 10 + 槽位号
    /// 地址分配示例：
    /// - 笼1槽1 → 地址1
    /// - 笼1槽10 → 地址10  
    /// - 笼2槽1 → 地址11
    /// - 笼3槽5 → 地址25
    /// - 板卡类型"--" → "--"
    /// 每个笼架支持最多10个槽位（1-10）
    /// </remarks>
    public string GetCardAddress(int cage, int slot, string cardType)
    {
        // 公式第1层：IF(I3="--","--",...)
        if (cardType == "--")
        {
            return "--";
        }

        // 公式第2层：(L3-1)*10+M3
        // 地址计算公式：(笼号-1) × 10 + 槽位号
        return ((cage - 1) * 10 + slot).ToString();
    }

    /// <summary>
    /// 获取信号正极连接点编号
    /// 根据新的复杂公式计算信号在端子板上的正极连接点位置
    /// 支持更复杂的逻辑判断：端子板型号、板卡类型、信号有效方式、供电方式的组合
    /// 适用于现场接线图生成、施工图纸编制、调试阶段的线号核对
    /// </summary>
    /// <param name="data">包含连接点计算所需信息的IO数据实体，包括端子板型号、通道号、板卡类型、信号有效方式、供电方式等</param>
    /// <returns>返回正极连接点编号，如"1A"、"FA+"等，"--"表示不需要连接，"Err"表示计算错误</returns>
    /// <remarks>
    /// 新公式逻辑：
    /// =IF(OR(T3="TB221",T3="TB222",T3="TB242",T3="TB243",T3="TB246"),N3&"A",
    ///   IF(AND(OR(T3="TB231",T3="TB232",T3="TB233"),OR(AN3="",AN3="NO")),N3&"B",
    ///     IF(AND(OR(T3="TB231",T3="TB232",T3="TB233"),AN3="NC"),N3&"A",
    ///       IF(AND(T3="TB241",OR(I3="AI212",I3="AI216")),N3&CHOOSE(MID(AI3,3,1),"B","A","A","A","A","B"),
    ///         IF(AND(T3="TB244",I3="AO215"),N3*2+22,
    ///           IF(AND(T3="TB244",I3="PI211"),(N3-1)*4+2,
    ///             IF(AND(T3="TB244",I3="MD211"),CHOOSE(N3,"8","28"),
    ///               IF(T3="TB271","a",
    ///                 IF(OR(T3="TB272",T3="TB271"),"--",
    ///                   IF(T3="TB251",N3&"B",
    ///                     IF(T3="--","--","Err")))))))))))
    /// </remarks>
    public string GetSingalPlusFormula(IoFullData data)
    {
        string terminalBoardModel = data.TerminalBoardModel ?? "";
        int channel = data.Channel;
        string cardType = data.CardType ?? "";
        string signalEffectiveMode = data.SignalEffectiveMode ?? "";
        string powerSupplyMethod = data.PowerSupplyMethod ?? "";

        // 第一层判断：TB221, TB222, TB242, TB243, TB246 => N3&"A"
        if (terminalBoardModel == "TB221" || terminalBoardModel == "TB222" || 
            terminalBoardModel == "TB242" || terminalBoardModel == "TB243" || terminalBoardModel == "TB246")
        {
            return $"{channel}A";
        }

        // 第二层判断：TB231, TB232, TB233 + (信号有效方式为空或NO) => N3&"B"
        if ((terminalBoardModel == "TB231" || terminalBoardModel == "TB232" || terminalBoardModel == "TB233") &&
            (string.IsNullOrEmpty(signalEffectiveMode) || signalEffectiveMode == "NO"))
        {
            return $"{channel}B";
        }

        // 第三层判断：TB231, TB232, TB233 + 信号有效方式为NC => N3&"A"
        if ((terminalBoardModel == "TB231" || terminalBoardModel == "TB232" || terminalBoardModel == "TB233") &&
            signalEffectiveMode == "NC")
        {
            return $"{channel}A";
        }

        // 第四层判断：TB241 + (AI212或AI216) => N3&CHOOSE(MID(AI3,3,1),"B","A","A","A","A","B")
        if (terminalBoardModel == "TB241" && (cardType == "AI212" || cardType == "AI216"))
        {
            // 获取供电方式的第3个字符
            if (powerSupplyMethod.Length >= 3)
            {
                char thirdChar = powerSupplyMethod[2];
                // CHOOSE函数：第1个=B, 第2-5个=A, 第6个=B
                string suffix = thirdChar switch
                {
                    '1' => "B",
                    '2' => "A",
                    '3' => "A",
                    '4' => "A",
                    '5' => "A",
                    '6' => "B",
                    _ => "A" // 默认值
                };
                return $"{channel}{suffix}";
            }
            return $"{channel}A"; // 默认值
        }

        // 第五层判断：TB244 + AO215 => N3*2+22
        if (terminalBoardModel == "TB244" && cardType == "AO215")
        {
            return (channel * 2 + 22).ToString();
        }

        // 第六层判断：TB244 + PI211 => (N3-1)*4+2
        if (terminalBoardModel == "TB244" && cardType == "PI211")
        {
            return ((channel - 1) * 4 + 2).ToString();
        }

        // 第七层判断：TB244 + MD211 => CHOOSE(N3,"8","28")
        if (terminalBoardModel == "TB244" && cardType == "MD211")
        {
            return channel switch
            {
                1 => "8",
                2 => "28",
                _ => "8" // 默认值
            };
        }

        // 第八层判断：TB271 => "a"
        if (terminalBoardModel == "TB271")
        {
            return "a";
        }

        // 第九层判断：TB272或TB271 => "--"
        if (terminalBoardModel == "TB272" || terminalBoardModel == "TB271")
        {
            return "--";
        }

        // 第十层判断：TB251 => N3&"B"
        if (terminalBoardModel == "TB251")
        {
            return $"{channel}B";
        }

        // 第十一层判断："--" => "--"
        if (terminalBoardModel == "--")
        {
            return "--";
        }

        // 默认情况："Err"
        return "Err";
    }

    /// <summary>
    /// 获取RTD温度传感器C端补偿连接点
    /// 根据新的复杂公式计算补偿连接点，支持多种端子板类型和供电方式
    /// 主要用于现场总线设备、模拟量信号的补偿连接，确保测量精度
    /// 适用于温度测量回路设计、仪表接线图生成、现场调试指导
    /// </summary>
    /// <param name="data">包含补偿连接点计算所需信息的IO数据实体，包括端子板型号、供电方式、通道号、板卡类型等</param>
    /// <returns>如果需要补偿连接返回相应的连接点编号，否则返回"--"表示不需要补偿</returns>
    /// <remarks>
    /// 新公式逻辑：
    /// =IF(T3="TB271","IE-BUS",
    ///   IF(OR(AI3="AI7",AI3="AI8",AI3="AI9"),N3&"C",
    ///     IF(AND(T3="TB244",I3="MD211"),"GND:"&CHOOSE(N3,"10","30"),
    ///       "--")))
    /// 补偿连接规则：
    /// - TB271端子板：返回"IE-BUS"（现场总线系统）
    /// - 供电方式为AI7/AI8/AI9：返回通道+"C"
    /// - TB244+MD211：返回"GND:"+选择值（通道1→"10"，通道2→"30"）
    /// - 其他情况：返回"--"
    /// </remarks>
    public string GetRTDCompensationEndC(IoFullData data)
    {
        string terminalBoardModel = data.TerminalBoardModel ?? "";
        string powerSupplyMethod = data.PowerSupplyMethod ?? "";
        int channel = data.Channel;
        string cardType = data.CardType ?? "";

        // 第一层判断：TB271 => "IE-BUS"
        if (terminalBoardModel == "TB271")
        {
            return "IE-BUS";
        }

        // 第二层判断：供电方式为AI7, AI8, AI9 => N3&"C"
        if (powerSupplyMethod == "AI7" || powerSupplyMethod == "AI8" || powerSupplyMethod == "AI9")
        {
            return $"{channel}C";
        }

        // 第三层判断：TB244 + MD211 => "GND:"&CHOOSE(N3,"10","30")
        if (terminalBoardModel == "TB244" && cardType == "MD211")
        {
            return channel switch
            {
                1 => "GND:10",
                2 => "GND:30",
                _ => "GND:10" // 默认值
            };
        }

        // 默认情况："--"（按照Excel公式逻辑）
        return "--";
    }

    /// <summary>
    /// 获取信号负极连接点编号
    /// 根据新的复杂公式计算信号在端子板上的负极连接点位置
    /// 与正极连接点配对使用，确保信号回路的完整性和正确性
    /// 适用于现场接线图生成、施工图纸编制、调试阶段的线号核对、故障排查
    /// </summary>
    /// <param name="data">包含连接点计算所需信息的IO数据实体，包括端子板型号、通道号、供电方式等</param>
    /// <returns>返回负极连接点编号，如"1C"、"FA-"等，"--"表示不需要连接，"Err"表示计算错误</returns>
    /// <remarks>
    /// 新公式逻辑：
    /// =IF(OR(T3="TB221",T3="TB222",T3="TB242",T3="TB246"),N3&"B",
    ///   IF(OR(T3="TB231",T3="TB233",T3="TB251"),N3&"C",
    ///     IF(AND(T3="TB241",OR(I3="AI212",I3="AI216")),N3&CHOOSE(MID(AI3,3,1),"A","C","C","C","C","A"),
    ///       IF(AND(T3="TB244",I3="AO215"),N3*2+19,
    ///         IF(AND(T3="TB244",I3="PI211"),N3*4,
    ///           IF(AND(T3="TB244",I3="MD211"),CHOOSE(N3,"12","32"),
    ///             IF(T3="TB271","b",
    ///               IF(OR(T3="TB272",T3="TB271"),"--",
    ///                 IF(T3="--","--","Err")))))))))
    /// </remarks>
    public string GetSingalMinusFormula(IoFullData data)
    {
        string terminalBoardModel = data.TerminalBoardModel ?? "";
        int channel = data.Channel;
        string cardType = data.CardType ?? "";
        string powerSupplyMethod = data.PowerSupplyMethod ?? "";

        // 第一层判断：TB221, TB222, TB242, TB246 => N3&"B"
        if (terminalBoardModel == "TB221" || terminalBoardModel == "TB222" || 
            terminalBoardModel == "TB242" || terminalBoardModel == "TB246")
        {
            return $"{channel}B";
        }

        // 第二层判断：TB231, TB233, TB251 => N3&"C"
        if (terminalBoardModel == "TB231" || terminalBoardModel == "TB233" || terminalBoardModel == "TB251")
        {
            return $"{channel}C";
        }

        // 第三层判断：TB241 + (AI212或AI216) => N3&CHOOSE(MID(AI3,3,1),"A","C","C","C","C","A")
        if (terminalBoardModel == "TB241" && (cardType == "AI212" || cardType == "AI216"))
        {
            // 获取供电方式的第3个字符
            if (powerSupplyMethod.Length >= 3)
            {
                char thirdChar = powerSupplyMethod[2];
                // CHOOSE函数：第1个=A, 第2-5个=C, 第6个=A
                string suffix = thirdChar switch
                {
                    '1' => "A",
                    '2' => "C",
                    '3' => "C",
                    '4' => "C",
                    '5' => "C",
                    '6' => "A",
                    _ => "C" // 默认值
                };
                return $"{channel}{suffix}";
            }
            return $"{channel}C"; // 默认值
        }

        // 第四层判断：TB244 + AO215 => N3*2+19
        if (terminalBoardModel == "TB244" && cardType == "AO215")
        {
            return (channel * 2 + 19).ToString();
        }

        // 第五层判断：TB244 + PI211 => N3*4
        if (terminalBoardModel == "TB244" && cardType == "PI211")
        {
            return (channel * 4).ToString();
        }

        // 第六层判断：TB244 + MD211 => CHOOSE(N3,"12","32")
        if (terminalBoardModel == "TB244" && cardType == "MD211")
        {
            return channel switch
            {
                1 => "12",
                2 => "32",
                _ => "12" // 默认值
            };
        }

        // 第七层判断：TB271 => "b"
        if (terminalBoardModel == "TB271")
        {
            return "b";
        }

        // 第八层判断：TB272或TB271 => "--"
        if (terminalBoardModel == "TB272" || terminalBoardModel == "TB271")
        {
            return "--";
        }

        // 第九层判断："--" => "--"
        if (terminalBoardModel == "--")
        {
            return "--";
        }

        // 默认情况："Err"
        return "Err";
    }

    /// <summary>
    /// 异步获取信号正极连接点编号（基于配置表）
    /// 通过数据库配置表和C#脚本动态计算信号正极连接点，支持复杂的计算逻辑
    /// 提供比硬编码方法更灵活的配置化连接点计算，便于规则变更和系统扩展
    /// 适用于标准化连接点规则管理、复杂计算逻辑实现、产品化配置
    /// </summary>
    /// <param name="configs">连接点配置列表，包含端子板型号、IO类型、有效模式等匹配条件和计算公式</param>
    /// <param name="data">IO数据实体，包含连接点计算所需的全部参数信息</param>
    /// <returns>返回计算后的正极连接点编号，计算失败返回"ERR"</returns>
    /// <exception cref="CompilationErrorException">当C#脚本编译失败时抛出异常</exception>
    /// <exception cref="RuntimeException">当脚本运行时发生错误时抛出异常</exception>
    /// <remarks>
    /// 匹配条件（所有非空条件必须同时满足）：
    /// - 端子板型号匹配
    /// - IO类型匹配
    /// - 端子板编号后缀匹配
    /// - 信号有效模式匹配
    /// - 供电方式前缀匹配
    /// 计算公式中"CH"将被替换为实际通道号
    /// </remarks>
    public async Task<string> GetSingalPlusAsync(List<config_connection_points> configs, IoFullData data)
    {
        var config = configs
            .Where(x => string.IsNullOrEmpty(x.TerminalBoardModel) || x.TerminalBoardModel == data.TerminalBoardModel)
            .Where(x => string.IsNullOrEmpty(x.IoType) || x.IoType == data.IoType)
            .Where(x => string.IsNullOrEmpty(x.TerminalBoardNumber) || data.TerminalBoardNumber.EndsWith(x.TerminalBoardNumber))
            .Where(x => $"{x.SignalEffectiveMode}" == data.SignalEffectiveMode)
            .Where(x => string.IsNullOrEmpty(x.PowerSupply) || data.TerminalBoardNumber.StartsWith(x.PowerSupply))
            .SingleOrDefault()?.SignalPlus ?? "ERR";
        if (config == "ERR")
        {
            return "ERR";
        }
        var result = await CSharpScript.EvaluateAsync(config.Replace("CH", $"{data.Channel}"));

        return $"{result}";
    }

    /// <summary>
    /// 异步获取信号负极连接点编号（基于配置表）
    /// 通过数据库配置表和C#脚本动态计算信号负极连接点，支持复杂的计算逻辑
    /// 与正极连接点计算方法配对使用，确保信号回路连接的完整性和准确性
    /// 适用于标准化连接点规则管理、复杂计算逻辑实现、产品化配置
    /// </summary>
    /// <param name="configs">连接点配置列表，包含端子板型号、IO类型、有效模式等匹配条件和计算公式</param>
    /// <param name="data">IO数据实体，包含连接点计算所需的全部参数信息</param>
    /// <returns>返回计算后的负极连接点编号，计算失败返回"ERR"</returns>
    /// <exception cref="CompilationErrorException">当C#脚本编译失败时抛出异常</exception>
    /// <exception cref="RuntimeException">当脚本运行时发生错误时抛出异常</exception>
    /// <remarks>
    /// 匹配逻辑与正极连接点相同：
    /// - 端子板型号、IO类型、端子板编号、有效模式、供电方式的组合匹配
    /// - 使用SignalMinus字段的计算公式
    /// - 通道号替换："CH" → 实际通道号
    /// - 单一匹配原则：必须找到唯一匹配的配置记录
    /// </remarks>
    public async Task<string> GetSignalMinusAsync(List<config_connection_points> configs, IoFullData data)
    {
        var config = configs
            .Where(x => string.IsNullOrEmpty(x.TerminalBoardModel) || x.TerminalBoardModel == data.TerminalBoardModel)
            .Where(x => string.IsNullOrEmpty(x.IoType) || x.IoType == data.IoType)
            .Where(x => string.IsNullOrEmpty(x.TerminalBoardNumber) || data.TerminalBoardNumber.EndsWith(x.TerminalBoardNumber))
            .Where(x => $"{x.SignalEffectiveMode}" == data.SignalEffectiveMode)
            .Where(x => string.IsNullOrEmpty(x.PowerSupply) || data.TerminalBoardNumber.StartsWith(x.PowerSupply))
            .SingleOrDefault()?.SignalMinus ?? "ERR";
        if (config == "ERR")
        {
            return "ERR";
        }
        var result = await CSharpScript.EvaluateAsync(config.Replace("CH", $"{data.Channel}"));

        return $"{result}";
    }


    /// <summary>
    /// 获取RTD温度传感器C端补偿连接点
    /// 判断传感器类型是否需要C端补偿，并生成相应的连接点编号
    /// 主要用于PT100热电阻的温度补偿连接，确保测量精度
    /// 适用于温度测量回路设计、仪表接线图生成、现场调试指导
    /// </summary>
    /// <param name="sensorType">传感器类型描述，用于判断是否为PT100类型</param>
    /// <param name="channel">通道号，用于生成具体的连接点编号</param>
    /// <returns>如果是PT100传感器返回"{通道号}C"，否则返回"--"表示不需要补偿</returns>
    /// <remarks>
    /// 补偿连接规则：
    /// - PT100传感器：需要C端补偿连接，返回通道+"C"
    /// - 其他传感器：不需要补偿，返回"--"
    /// - 空值安全：传感器类型为null时返回"--"
    /// - 用途：三线制PT100的温度补偿
    /// </remarks>
    public string GetRTDCompensationEndC(string sensorType, int channel)
    {
        return sensorType != null && sensorType.Contains("PT100") ? $"{channel}C" : "--";
    }

    /// <summary>
    /// 获取RTD温度传感器D端补偿连接点（新版Excel公式实现）
    /// 根据供电方式判断是否需要D端补偿连接，仅AI9类型需要D端连接
    /// 专门用于特定供电方式的第四根导线连接，确保信号完整性
    /// 适用于高精度温度测量、标准温度检测、精密仪表接线
    /// </summary>
    /// <param name="powerSupplyMethod">供电方式，用于判断是否为AI9类型</param>
    /// <param name="channel">通道号，用于生成具体的连接点编号</param>
    /// <returns>如果供电方式为AI9返回"{通道号}D"，否则返回"--"表示不需要D端连接</returns>
    /// <remarks>
    /// 新公式逻辑：=IF(AI3="AI9",N3&"D","--")
    /// 连接规则：
    /// - 供电方式为AI9：需要D端连接，返回通道+"D"
    /// - 其他供电方式：不需要D端，返回"--"
    /// - 空值安全：供电方式为null或空时返回"--"
    /// 
    /// 与旧版本差异：
    /// 旧版：基于传感器类型判断（四线制PT100）
    /// 新版：基于供电方式判断（AI9）
    /// </remarks>
    public string GetRTDCompensationEndD(string powerSupplyMethod, int channel)
    {
        // 公式：IF(AI3="AI9",N3&"D","--")
        if (powerSupplyMethod == "AI9")
        {
            return $"{channel}D";
        }
        return "--";
    }

    /// <summary>
    /// 生成FF从站模块编号
    /// 根据就地箱号、FF/DP从站号和FF从站模块型号生成完整的FF从站模块编号
    /// 仅当FF从站模块型号包含"FS"时才生成编号，否则返回"--"
    /// 适用于FF现场总线从站设备的标识管理、设备配置文档生成、现场设备标签制作
    /// </summary>
    /// <param name="localBoxNumber">就地箱号，标识现场设备所在的就地控制箱编号</param>
    /// <param name="ffSlaveStationNumber">FF/DP从站号，用于现场总线通信的设备地址标识</param>
    /// <param name="ffSlaveModuleModel">FF从站模块型号，如果包含"FS"则表示是FF从站模块</param>
    /// <returns>如果模块型号包含"FS"则返回格式为"{就地箱号}_{从站号}_{模块型号}"的编号，否则返回"--"</returns>
    /// <exception cref="ArgumentNullException">当任何输入参数为null时可能引发异常</exception>
    /// <remarks>
    /// 生成规则：
    /// - 检查FF从站模块型号是否包含"FS"字符串
    /// - 包含"FS"：生成格式为"{就地箱号}_{从站号}_{模块型号}"
    /// - 不包含"FS"：返回"--"表示不是FF从站模块
    /// - 空值安全：输入参数为null或空时返回"--"
    /// 业务场景：FF现场总线系统的从站设备管理、设备配置表生成、现场设备维护
    /// 示例：就地箱"JD001"，从站号"01"，模块"FS100" → "JD001_01_FS100"
    /// </remarks>
    public string GetFFSlaveModuleNumber(string localBoxNumber, string ffSlaveStationNumber, string ffSlaveModuleModel)
    {
        // 空值检查
        if (string.IsNullOrEmpty(localBoxNumber) ||
            string.IsNullOrEmpty(ffSlaveStationNumber) ||
            string.IsNullOrEmpty(ffSlaveModuleModel))
        {
            return "--";
        }

        try
        {
            // 检查FF从站模块型号是否包含"FS"
            if (ffSlaveModuleModel.Contains("FS"))
            {
                // 生成FF从站模块编号：{就地箱号}_{从站号}_{模块型号}
                return $"{localBoxNumber}_{ffSlaveStationNumber}_{ffSlaveModuleModel}";
            }
            else
            {
                // 不是FF从站模块，返回"--"
                return "--";
            }
        }
        catch (Exception)
        {
            // 异常情况返回错误标识
            return "--";
        }
    }

    /// <summary>
    /// 生成FF从站模块信号正极连接点
    /// 根据FF从站模块型号、IO类型、传感器类型和信号有效模式等参数，计算FF从站模块的正极连接点
    /// 仅当FF从站模块型号包含"FS"时才进行计算，否则返回"--"
    /// 适用于FF现场总线从站设备的信号连接设计、接线图生成、现场调试指导
    /// </summary>
    /// <param name="data">包含FF从站模块连接点计算所需信息的IO数据实体</param>
    /// <returns>返回正极连接点编号，如"01A"、"02B"等，"--"表示不需要连接或不是FF从站模块</returns>
    /// <exception cref="ArgumentNullException">当输入参数为null时可能引发异常</exception>
    /// <remarks>
    /// 计算规则（按优先级排序）：
    /// 1. 检查FF从站模块型号是否包含"FS"，不包含则返回"--"
    /// 2. A类型+四线制：{FF从站通道}A
    /// 3. AI类型：{FF从站通道}B
    /// 4. AO类型：{FF从站通道}C
    /// 5. DI类型：{FF从站通道}A
    /// 6. DO类型+NC模式：{FF从站通道}A
    /// 7. DO类型+其他模式：{FF从站通道}B
    /// 8. 其他情况："--"
    /// 业务场景：FF现场总线系统中从站设备的信号连接设计和施工指导
    /// </remarks>
    public string GetFFSlaveModuleSignalPlus(IoFullData data)
    {
        // 空值检查
        if (data == null ||
            string.IsNullOrEmpty(data.FFSlaveModuleModel) ||
            string.IsNullOrEmpty(data.IoType) ||
            !data.FFTerminalChannel.HasValue)
        {
            return "--";
        }

        try
        {
            // 检查是否为FF从站模块
            if (!data.FFSlaveModuleModel.Contains("FS"))
            {
                return "--";
            }

            int channel = data.FFTerminalChannel.Value;
            string ioType = data.IoType?.Trim() ?? "";
            string sensorType = data.SensorType ?? "";
            string signalEffectiveMode = data.SignalEffectiveMode ?? "";

            // 按优先级判断连接点
            // A类型+四线制
            if (ioType.Contains("A") && sensorType.Contains("四"))
            {
                return $"{channel}A";
            }

            // AI类型
            if (ioType.Contains("AI"))
            {
                return $"{channel}B";
            }

            // AO类型
            if (ioType.Contains("AO"))
            {
                return $"{channel}C";
            }

            // DI类型
            if (ioType.Contains("DI"))
            {
                return $"{channel}A";
            }

            // DO类型+NC模式
            if (ioType.Contains("DO") && signalEffectiveMode.Contains("NC"))
            {
                return $"{channel}A";
            }

            // DO类型+其他模式
            if (ioType.Contains("DO"))
            {
                return $"{channel}B";
            }

            // 其他情况
            return "--";
        }
        catch (Exception)
        {
            // 异常情况返回错误标识
            return "--";
        }
    }

    /// <summary>
    /// 生成FF从站模块信号负极连接点
    /// 根据FF从站模块型号、IO类型、传感器类型等参数，计算FF从站模块的负极连接点
    /// 与正极连接点配对使用，确保信号回路连接的完整性和准确性
    /// 适用于FF现场总线从站设备的信号连接设计、接线图生成、现场调试指导
    /// </summary>
    /// <param name="data">包含FF从站模块连接点计算所需信息的IO数据实体</param>
    /// <returns>返回负极连接点编号，如"01C"、"02A"等，"--"表示不需要连接或不是FF从站模块</returns>
    /// <exception cref="ArgumentNullException">当输入参数为null时可能引发异常</exception>
    /// <remarks>
    /// 计算规则（按优先级排序）：
    /// 1. 检查FF从站模块型号是否包含"FS"，不包含则返回"--"
    /// 2. A类型+四线制：{FF从站通道}C
    /// 3. AI类型：{FF从站通道}A
    /// 4. AO类型：{FF从站通道}B
    /// 5. DI类型：{FF从站通道}B
    /// 6. DO类型+无源模式：{FF从站通道}C
    /// 7. DO类型+其他模式：{FF从站通道}D
    /// 8. 其他情况："--"
    /// 业务场景：FF现场总线系统中从站设备的信号连接设计和施工指导
    /// </remarks>
    public string GetFFSlaveModuleSignalMinus(IoFullData data)
    {
        // 空值检查
        if (data == null ||
            string.IsNullOrEmpty(data.FFSlaveModuleModel) ||
            string.IsNullOrEmpty(data.IoType) ||
            !data.FFTerminalChannel.HasValue)
        {
            return "--";
        }

        try
        {
            // 检查是否为FF从站模块
            if (!data.FFSlaveModuleModel.Contains("FS"))
            {
                return "--";
            }

            int channel = data.FFTerminalChannel.Value;
            string ioType = data.IoType?.Trim() ?? "";
            string sensorType = data.SensorType ?? "";

            // 按优先级判断连接点
            // A类型+四线制
            if (ioType.Contains("A") && sensorType.Contains("四"))
            {
                return $"{channel}C";
            }

            // AI类型
            if (ioType.Contains("AI"))
            {
                return $"{channel}A";
            }

            // AO类型
            if (ioType.Contains("AO"))
            {
                return $"{channel}B";
            }

            // DI类型
            if (ioType.Contains("DI"))
            {
                return $"{channel}B";
            }

            // DO类型+无源模式
            if (ioType.Contains("DO") && sensorType.Contains("无源"))
            {
                return $"{channel}C";
            }

            // DO类型+其他模式
            if (ioType.Contains("DO"))
            {
                return $"{channel}D";
            }

            // 其他情况
            return "--";
        }
        catch (Exception)
        {
            // 异常情况返回错误标识
            return "--";
        }
    }

    #region FF从站模块自动分配逻辑

    /// <summary>
    /// 设备组的实现类，用于适配 IGrouping 接口
    /// </summary>
    private class DeviceGroup : IGrouping<string, IoFullData>
    {
        public string Key { get; }
        private readonly List<IoFullData> _signals;

        public DeviceGroup(string key, List<IoFullData> signals)
        {
            Key = key;
            _signals = signals ?? new List<IoFullData>();
        }

        public IEnumerator<IoFullData> GetEnumerator() => _signals.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// 执行FF模块自动分配
    /// 【修改】根据新的输入格式进行所有FF模块分配和通道计算，不再区分FF7/FF8和其他FF类型
    /// </summary>
    /// <returns>分配结果报告（包含成功或失败信息）</returns>
    public async Task<string> PerformFFSlaveModuleAllocation()
    {
        var report = new StringBuilder();
        report.AppendLine("FF从站模块分配报告");
        report.AppendLine($"分配时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine(new string('=', 50));

        try
        {
            if (AllData == null || AllData.Count == 0)
            {
                report.AppendLine();
                report.AppendLine("【分配失败】");
                report.AppendLine("❌ 无IO数据可分配，请先导入数据");
                return report.ToString();
            }

            // 获取所有FF从站箱的信号
            // 判断逻辑：1.IO类型包含FF 2.FF从站模块型号不为空（有模块配置的才是从站箱）
            var ffSlaveSignals = AllData.Where(signal => 
                !string.IsNullOrEmpty(signal.IoType) &&
                signal.IoType.Contains("FF", StringComparison.OrdinalIgnoreCase) && // IO类型包含FF
                !string.IsNullOrEmpty(signal.FFSlaveModuleModel) // 有模块配置的是从站箱
            ).ToList();
            
            if (!ffSlaveSignals.Any())
            {
                report.AppendLine();
                report.AppendLine("【分配终止】");
                report.AppendLine("未找到有效的FF从站模块信号");
                report.AppendLine();
                report.AppendLine("提示：从站箱需要同时满足：");
                report.AppendLine("  1. IO类型包含FF（如FFAI、FFAO、FFDI、FFDO）");
                report.AppendLine("  2. 填写了从站模块型号（FFSlaveModuleModel）");
                return report.ToString();
            }

            report.AppendLine($"找到{ffSlaveSignals.Count}个FF从站信号");
            report.AppendLine();

            // ✅ 数据完整性验证
            report.AppendLine("【数据完整性检查】");
            var validationErrors = new List<string>();
            
            // 检查网段信息
            var signalsWithoutNetType = ffSlaveSignals.Where(s => string.IsNullOrEmpty(s.NetType)).ToList();
            if (signalsWithoutNetType.Any())
            {
                validationErrors.Add($"❌ {signalsWithoutNetType.Count}个信号缺少网段信息（NetType为空）");
                report.AppendLine($"  缺少网段信息的信号：");
                foreach (var signal in signalsWithoutNetType.Take(10))
                {
                    report.AppendLine($"    - {signal.SignalPositionNumber} (机柜{signal.CabinetNumber}, 机笼{signal.Cage}, 插槽{signal.Slot})");
                }
                if (signalsWithoutNetType.Count > 10)
                {
                    report.AppendLine($"    ...还有{signalsWithoutNetType.Count - 10}个信号");
                }
            }

            // 检查机笼、插槽信息
            var signalsWithoutCageSlot = ffSlaveSignals.Where(s => s.Cage <= 0 || s.Slot <= 0).ToList();
            if (signalsWithoutCageSlot.Any())
            {
                validationErrors.Add($"❌ {signalsWithoutCageSlot.Count}个信号缺少机笼或插槽信息");
                report.AppendLine($"  缺少机笼/插槽信息的信号：");
                foreach (var signal in signalsWithoutCageSlot.Take(10))
                {
                    report.AppendLine($"    - {signal.SignalPositionNumber} (机柜{signal.CabinetNumber}, 机笼{signal.Cage}, 插槽{signal.Slot})");
                }
                if (signalsWithoutCageSlot.Count > 10)
                {
                    report.AppendLine($"    ...还有{signalsWithoutCageSlot.Count - 10}个信号");
                }
            }

            // 如果有验证错误，停止分配
            if (validationErrors.Any())
            {
                report.AppendLine();
                report.AppendLine(new string('=', 50));
                report.AppendLine("【分配失败】");
                report.AppendLine("检测到以下数据问题，无法继续分配：");
                foreach (var error in validationErrors)
                {
                    report.AppendLine($"  {error}");
                }
                report.AppendLine();
                report.AppendLine("请确保：");
                report.AppendLine("  1. 所有FF从站信号都已分配网段（NetType，如Net1、Net2）");
                report.AppendLine("  2. 所有信号都有正确的机笼和插槽信息");
                report.AppendLine();
                report.AppendLine("提示：可以先执行IO自动分配，系统会自动填充网段等信息");
                
                return report.ToString();
            }

            report.AppendLine("  ✅ 所有信号数据完整，可以进行分配");
            report.AppendLine();

            // **重要：清空之前的FF从站模块分配结果**
            ClearFFSlaveAllocationResults();
            report.AppendLine("已清空之前的FF从站模块分配结果");
            report.AppendLine();

            // 按机柜分组处理
            var cabinetGroups = ffSlaveSignals.GroupBy(s => s.CabinetNumber);
            int totalProcessedSignals = 0;
            int totalModulesAllocated = 0;

            foreach (var cabinetGroup in cabinetGroups)
            {
                var cabinetResult = await ProcessCabinetFFSlaveModuleAllocation(cabinetGroup.Key, cabinetGroup.ToList());
                report.AppendLine(cabinetResult.Report);
                totalProcessedSignals += cabinetResult.ProcessedSignals;
                totalModulesAllocated += cabinetResult.ModulesAllocated;
            }

            report.AppendLine(new string('=', 50));
            report.AppendLine("【分配成功】");
            report.AppendLine($"处理信号数量：{totalProcessedSignals}");
            report.AppendLine($"分配模块数量：{totalModulesAllocated}");
            report.AppendLine("分配完成！");

            return report.ToString();
        }
        catch (Exception ex)
        {
            // 捕获所有异常，记录到报告中
            report.AppendLine();
            report.AppendLine(new string('=', 50));
            report.AppendLine("【分配失败】");
            report.AppendLine($"❌ 分配过程中发生错误：{ex.Message}");
            report.AppendLine();
            report.AppendLine("错误详情：");
            report.AppendLine(ex.ToString());
            
            return report.ToString();
        }
    }

    /// <summary>
    /// 清空FF从站模块分配结果
    /// 在重新分配之前清空所有FF从站模块相关字段，确保数据一致性
    /// </summary>
    /// <remarks>
    /// 清空字段包括：
    /// - FFDPStaionNumber: FF从站站号
    /// - FFTerminalChannel: FF从站通道
    /// - FFSlaveModuleID: FF从站模块编号
    /// - FFSlaveModuleSignalPositive: FF从站模块信号正极
    /// - FFSlaveModuleSignalNegative: FF从站模块信号负极
    /// 注意：FFSlaveModuleModel不清空，因为它作为输入配置保持
    /// </remarks>
    private void ClearFFSlaveAllocationResults()
    {
        foreach (var signal in AllData)
        {
            // 只清空从站箱的信号（IO类型包含FF且有模块配置）
            if (!string.IsNullOrEmpty(signal.IoType) &&
                signal.IoType.Contains("FF", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(signal.FFSlaveModuleModel))
            {
                // 清空分配结果字段
                signal.FFDPStaionNumber = null;
                signal.FFTerminalChannel = null;
                signal.FFSlaveModuleID = null;
                signal.FFSlaveModuleSignalPositive = null;
                signal.FFSlaveModuleSignalNegative = null;
                
                // ❗重要：不清空FFSlaveModuleModel，因为它是输入的模块配置信息，分配算法需要用它
                // signal.FFSlaveModuleModel 保持不变
            }
        }
    }

    /// <summary>
    /// 判断供电类型是否为FF从站类型（FF7或FF8）
    /// </summary>
    /// <param name="powerType">供电类型字符串</param>
    /// <returns>如果是FF7或FF8返回true，否则返回false</returns>
    /// <remarks>
    /// 业务功能：
    /// - 精确匹配FF7和FF8供电类型
    /// - 防止匹配到FF17、FF18等非预期类型
    /// 
    /// 使用场景：
    /// - FF从站模块分配前的信号筛选
    /// - FF从站箱类型判断
    /// 
    /// 异常处理：
    /// - 对空值或null返回false
    /// </remarks>
    private bool IsFFSlavePowerType(string powerType)
    {
        if (string.IsNullOrEmpty(powerType))
            return false;
            
        var trimmed = powerType.Trim().ToUpper();
        
        // 精确匹配FF7或FF8，防止匹配到FF17、FF18等
        return trimmed == "FF7" || trimmed == "FF8";
    }

    /// <summary>
    /// 按设备对信号进行分组
    /// 根据信号类型组合采用不同的分组规则
    /// </summary>
    /// <param name="signals">需要分组的信号列表</param>
    /// <returns>按设备分组的信号组</returns>
    /// <exception cref="ArgumentException">当信号数据不完整时抛出异常</exception>
    /// <remarks>
    /// 分组规则：
    /// - A信号 + A信号：短横线之后相同
    /// - D信号 + D信号：整个信号位号相同
    /// - A信号 + D信号：短横线之后相同
    /// </remarks>
    private IEnumerable<IGrouping<string, IoFullData>> GroupSignalsByDevice(List<IoFullData> signals)
    {
        try
        {
            // 预处理：验证数据完整性
            var validSignals = signals.Where(s =>
                !string.IsNullOrEmpty(s.IoType) &&
                !string.IsNullOrEmpty(s.SignalPositionNumber))
                .ToList();

            if (validSignals.Count != signals.Count)
            {
                throw new ArgumentException("存在无效的信号数据（IoType或SignalPositionNumber为空）");
            }

            // 使用自定义分组逻辑，基于信号类型的分组规则
            var deviceGroups = new Dictionary<string, List<IoFullData>>();

            foreach (var signal in validSignals)
            {
                string deviceKey = null;
                bool foundGroup = false;

                // 尝试找到匹配的设备组
                foreach (var existingGroup in deviceGroups)
                {
                    if (existingGroup.Value.Any(existingSignal => IsSameDeviceGroup(signal, existingSignal)))
                    {
                        deviceKey = existingGroup.Key;
                        foundGroup = true;
                        break;
                    }
                }

                // 如果没有找到匹配的组，创建新组
                if (!foundGroup)
                {
                    deviceKey = GetDeviceIdentifier(signal);
                    deviceGroups[deviceKey] = new List<IoFullData>();
                }

                deviceGroups[deviceKey].Add(signal);
            }

            // 转换为 IGrouping 格式
            return deviceGroups.Select(kvp => new DeviceGroup(kvp.Key, kvp.Value));
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"设备分组失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 判断两个信号是否属于同一设备组
    /// </summary>
    private bool IsSameDeviceGroup(IoFullData signal1, IoFullData signal2)
    {
        if (signal1 == null || signal2 == null) return false;

        string type1 = signal1.IoType;
        string type2 = signal2.IoType;
        
        // 获取设备标识
        string GetKey(IoFullData signal, bool useFullNumber = false)
        {
            if (useFullNumber) return signal.SignalPositionNumber ?? "";
            var parts = signal.SignalPositionNumber?.Split('-');
            return parts?.Length > 1 ? parts.Last() : signal.SignalPositionNumber ?? "";
        }

        if (type1.Contains("A") && type2.Contains("A"))  // A和A：短横线后部分
            return GetKey(signal1) == GetKey(signal2);
        
        if (type1.Contains("D") && type2.Contains("D"))  // D和D：整个信号位号
            return GetKey(signal1, true) == GetKey(signal2, true);
        
        if ((type1.Contains("A") && type2.Contains("D")) || (type1.Contains("D") && type2.Contains("A")))  // A和D：短横线后部分
            return GetKey(signal1) == GetKey(signal2);

        return false;
    }

    /// <summary>
    /// 获取设备标识符
    /// </summary>
    private string GetDeviceIdentifier(IoFullData signal)
    {
        if (signal?.IoType?.Contains("D") == true)
        {
            return signal.SignalPositionNumber ?? "";
        }
        // A信号或其他：使用短横线后部分
        var parts = signal?.SignalPositionNumber?.Split('-');
        return parts?.Length > 1 ? parts.Last() : signal?.SignalPositionNumber ?? "";
    }

    /// <summary>
    /// 处理单个机柜的FF从站模块分配
    /// 按照物理结构：机柜 → 机笼 → 插槽 → 板卡 → 网段 进行分组处理
    /// 每个网段内的信号为一个处理单元
    /// </summary>
    /// <param name="cabinetName">机柜名称</param>
    /// <param name="cabinetSignals">机柜内的所有信号</param>
    /// <returns>分配结果</returns>
    private async Task<(string Report, int ProcessedSignals, int ModulesAllocated)> ProcessCabinetFFSlaveModuleAllocation(
        string cabinetName, List<IoFullData> cabinetSignals)
    {
        var report = new StringBuilder();
        report.AppendLine($"\n机柜 {cabinetName}:");
        
        int processedSignals = 0;
        int modulesAllocated = 0;

        try
        {
            // 按物理结构分组：机笼 → 插槽 → 板卡 → 网段
            // 每个网段内的信号为一个处理单元
            var networkUnits = cabinetSignals
                .Where(s => s.Cage > 0 && s.Slot > 0 && !string.IsNullOrEmpty(s.NetType))
                .GroupBy(s => new { s.Cage, s.Slot, s.NetType })
                .OrderBy(g => g.Key.Cage)
                .ThenBy(g => g.Key.Slot)
                .ThenBy(g => g.Key.NetType)
                .ToList();

            if (!networkUnits.Any())
            {
                report.AppendLine("  未找到有效的网段单元（机笼-插槽-网段）");
                return (report.ToString(), 0, 0);
            }

            report.AppendLine($"  找到 {networkUnits.Count} 个网段处理单元");

            // 处理每个网段单元
            foreach (var networkUnit in networkUnits)
            {
                var unitResult = ProcessBoardUnitFFSlaveModules(
                    cabinetName, 
                    networkUnit.Key.Cage, 
                    networkUnit.Key.Slot, 
                    networkUnit.Key.NetType, 
                    networkUnit.ToList());
                    
                report.AppendLine(unitResult.Report);
                processedSignals += unitResult.ProcessedSignals;
                modulesAllocated += unitResult.ModulesAllocated;
            }
        }
        catch (Exception ex)
        {
            report.AppendLine($"  错误：{ex.Message}");
        }

        return (report.ToString(), processedSignals, modulesAllocated);
    }

    /// <summary>
    /// 处理板卡单元的FF从站模块分配
    /// 板卡单元：机笼 + 插槽 + 网段，这是最小的处理单元
    /// 先按设备分组，再按模块类型分配
    /// </summary>
    /// <param name="cabinetName">机柜名称</param>
    /// <param name="cage">机笼号</param>
    /// <param name="slot">插槽号</param>
    /// <param name="networkType">网段类型</param>
    /// <param name="unitSignals">板卡单元内的信号</param>
    /// <returns>处理结果</returns>
    private (string Report, int ProcessedSignals, int ModulesAllocated) ProcessBoardUnitFFSlaveModules(
        string cabinetName, int cage, int slot, string networkType, List<IoFullData> unitSignals)
    {
        var report = new StringBuilder();
        report.AppendLine($"  板卡单元 {cage}-{slot:00}-{networkType}:");
        
        int processedSignals = 0;
        int modulesAllocated = 0;
        int currentStationNumber = 1; // 从01开始统一分配站号

        try
        {
            // 按设备分组（相同设备的信号应该放到一起）
            var deviceGroups = GroupSignalsByDevice(unitSignals);
            
            if (!deviceGroups.Any())
            {
                report.AppendLine("    未找到有效的设备分组");
                return (report.ToString(), 0, 0);
            }

            report.AppendLine($"    找到 {deviceGroups.Count()} 个设备组");

            // 一次性处理整个网段的所有设备组（共享模块资源）
            var result = ProcessAllDeviceGroupsInNetwork(cabinetName, cage, slot, networkType, unitSignals, currentStationNumber, report);
            processedSignals = result.ProcessedSignals;
            modulesAllocated = result.ModulesAllocated;
        }
        catch (Exception ex)
        {
            report.AppendLine($"    错误：{ex.Message}");
        }

        return (report.ToString(), processedSignals, modulesAllocated);
    }
   

    /// <summary>
    /// 处理网段内所有设备组的FF从站模块分配（共享模块资源）
    /// </summary>
    private (int ProcessedSignals, int ModulesAllocated) ProcessAllDeviceGroupsInNetwork(
        string cabinetName, int cage, int slot, string networkType, 
        List<IoFullData> allSignals, int baseStationNumber, StringBuilder report)
    {
        int processedSignals = 0;
        int modulesAllocated = 0;

        try
        {
            if (!allSignals.Any())
            {
                report.AppendLine("    网段信号为空");
                return (0, 0);
            }

            // ❗调试输出：查看收集前的模块配置
            report.AppendLine($"    开始收集模块配置，网段信号数：{allSignals.Count}");
            var debugConfigs = allSignals
                .Select((s, index) => new { Index = index + 1, Config = s.FFSlaveModuleModel, SignalPos = s.SignalPositionNumber })
                .ToList();
            
            foreach (var debug in debugConfigs.Take(5)) // 只显示前5个
            {
                report.AppendLine($"      信号{debug.Index}({debug.SignalPos}): FFSlaveModuleModel = '{debug.Config ?? "<null>"}'");
            }
            if (debugConfigs.Count > 5)
            {
                report.AppendLine($"      ...还有{debugConfigs.Count - 5}个信号");
            }

            // 收集网段内所有不同的模块配置并去重合并
            var allModuleConfigs = allSignals
                .Select(s => s.FFSlaveModuleModel)
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct()
                .ToList();
            
            report.AppendLine($"    收集到{allModuleConfigs.Count}个不同的模块配置：{string.Join(", ", allModuleConfigs.Select(c => $"'{c}'"))}");
            
            if (!allModuleConfigs.Any())
            {
                report.AppendLine("    网段无有效模块配置");
                return (0, 0);
            }
            
            // 解析并合并所有模块配置
            var networkModules = ParseAndMergeNetworkModules(allModuleConfigs);
            report.AppendLine($"    网段模块配置: {string.Join(", ", networkModules.Select(m => $"{m.Count}个{m.ModuleType}"))}");

            // 将所有设备信号分配到共享的模块
            var result = AllocateDeviceGroupsToNetworkModules(allSignals, networkModules, baseStationNumber, report);
            processedSignals = result.ProcessedSignals;
            modulesAllocated = result.ModulesUsed;
            
            report.AppendLine($"    网段分配完成：使用{modulesAllocated}个模块，处理{processedSignals}个信号");
        }
        catch (Exception ex)
        {
            report.AppendLine($"    错误：{ex.Message}");
        }

        return (processedSignals, modulesAllocated);
    }

    /// <summary>
    /// 解析网段模块配置（如"1FS201 1FS202"表示1个FS201和1个FS202）
    /// </summary>
    private List<(int Count, string ModuleType)> ParseNetworkModules(string moduleInput)
    {
        var modules = new List<(int Count, string ModuleType)>();
        
        if (string.IsNullOrEmpty(moduleInput))
            return modules;
            
        // 按空格分割处理多个模块
        var parts = moduleInput.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in parts)
        {
            var parsed = ParseFFSlaveModuleModel(part.Trim());
            if (!string.IsNullOrEmpty(parsed.ModuleType))
            {
                modules.Add(parsed);
            }
        }
        
        return modules;
    }
    
    /// <summary>
    /// 解析并合并多个模块配置字符串，去重并统计数量
    /// 例如：["1FS201 1FS202", "2FS201"] → [(3, "FS201"), (1, "FS202")]
    /// </summary>
    /// <param name="moduleConfigs">多个模块配置字符串</param>
    /// <returns>合并后的模块列表</returns>
    private List<(int Count, string ModuleType)> ParseAndMergeNetworkModules(List<string> moduleConfigs)
    {
        var moduleTypeCount = new Dictionary<string, int>();
        
        foreach (var config in moduleConfigs)
        {
            if (string.IsNullOrEmpty(config))
                continue;
                
            var modules = ParseNetworkModules(config);
            foreach (var module in modules)
            {
                if (moduleTypeCount.ContainsKey(module.ModuleType))
                {
                    // 对于同一种模块类型，取最大数量（避免重复累加）
                    moduleTypeCount[module.ModuleType] = Math.Max(moduleTypeCount[module.ModuleType], module.Count);
                }
                else
                {
                    moduleTypeCount[module.ModuleType] = module.Count;
                }
            }
        }
        
        return moduleTypeCount.Select(kvp => (kvp.Value, kvp.Key)).ToList();
    }
    
    /// <summary>
    /// 将设备组平均分配到网段的模块中
    /// 先分配信号到模块，再根据模块位置确定站号
    /// </summary>
    private (int ProcessedSignals, int ModulesUsed) AllocateDeviceGroupsToNetworkModules(
        List<IoFullData> signals, List<(int Count, string ModuleType)> networkModules, 
        int baseStationNumber, StringBuilder report)
    {
        int processedSignals = 0;
        
        // 创建模块定义列表（不包含站号，站号后续根据分配结果确定）
        var moduleDefinitions = new List<(string ModuleType, Dictionary<string, int> UsedChannels, List<IoFullData> AssignedSignals)>();
        foreach (var module in networkModules)
        {
            for (int i = 0; i < module.Count; i++)
            {
                var usedChannels = new Dictionary<string, int> { ["AI"] = 0, ["AO"] = 0, ["DI"] = 0, ["DO"] = 0 };
                var assignedSignals = new List<IoFullData>();
                moduleDefinitions.Add((module.ModuleType, usedChannels, assignedSignals));
            }
        }
        
        report.AppendLine($"        网段共有{moduleDefinitions.Count}个模块:");
        for (int i = 0; i < moduleDefinitions.Count; i++)
        {
            var module = moduleDefinitions[i];
            var capacityInfo = GetModuleCapacityInfo(module.ModuleType);
            report.AppendLine($"          模块{i + 1}: {module.ModuleType} - {capacityInfo}");
        }
        
        // 按设备分组
        var deviceGroups = GroupSignalsByDevice(signals);
        report.AppendLine($"        设备分组数量: {deviceGroups.Count()}");
        
        // 为每个设备组逐个分配，并在每个设备后显示模块剩余情况
        int deviceIndex = 0;
        foreach (var deviceGroup in deviceGroups)
        {
            var deviceSignals = deviceGroup.ToList();
            deviceIndex++;
            
            report.AppendLine($"        第{deviceIndex}个设备组 {deviceGroup.Key} ({deviceSignals.Count}个信号):");
            
            // 选择负载最低的兼容模块来放置整个设备组
            int selectedModuleIndex = SelectBestModuleForDeviceGroup(deviceSignals, moduleDefinitions, report);
            
            if (selectedModuleIndex == -1)
            {
                report.AppendLine($"          警告: 没有模块能支持设备组 {deviceGroup.Key} 的所有信号或容量不足");
                continue;
            }
            
            report.AppendLine($"          选择模块{selectedModuleIndex + 1}({moduleDefinitions[selectedModuleIndex].ModuleType})进行分配");
            
            // 将设备组的所有信号分配到选定模块
            var result = AssignDeviceGroupToSelectedModule(deviceSignals, selectedModuleIndex, moduleDefinitions, report);
            processedSignals += result.ProcessedSignals;
            
            // 在每个设备分配后显示模块剩余情况
            report.AppendLine($"        设备组{deviceIndex}分配后，模块剩余情况:");
            for (int i = 0; i < moduleDefinitions.Count; i++)
            {
                var module = moduleDefinitions[i];
                var remainingInfo = GetDetailedRemainingCapacity(module.ModuleType, module.UsedChannels);
                var loadInfo = $"已分配{module.AssignedSignals.Count}个信号";
                report.AppendLine($"          模块{i + 1}({module.ModuleType}): {remainingInfo} ({loadInfo})");
            }
            report.AppendLine(""); // 空行分隔
        }
        
        // 现在根据分配结果确定站号
        for (int i = 0; i < moduleDefinitions.Count; i++)
        {
            var module = moduleDefinitions[i];
            int stationNumber = baseStationNumber + i;
            
            // 为该模块的所有信号设置站号
            foreach (var signal in module.AssignedSignals)
            {
                signal.FFDPStaionNumber = stationNumber.ToString("D2");
                
                // 保持原始FFSlaveModuleModel中的数量信息，只更新模块型号部分
                signal.FFSlaveModuleModel = UpdateModuleTypeInOriginalFormat(signal.FFSlaveModuleModel, module.ModuleType);
                
                // 更新其他FF从站相关字段
                signal.FFSlaveModuleID = GetFFSlaveModuleNumber(signal.LocalBoxNumber, signal.FFDPStaionNumber, module.ModuleType);
                signal.FFSlaveModuleSignalPositive = GetFFSlaveModuleSignalPlus(signal);
                signal.FFSlaveModuleSignalNegative = GetFFSlaveModuleSignalMinus(signal);
            }
            
            if (module.AssignedSignals.Any())
            {
                var remainingCapacity = GetDetailedRemainingCapacity(module.ModuleType, module.UsedChannels);
                report.AppendLine($"        模块{i + 1}({module.ModuleType})分配完成: 站号{stationNumber:D2}, 分配{module.AssignedSignals.Count}个信号, {remainingCapacity}");
                report.AppendLine($"          已更新{module.AssignedSignals.Count}个信号的FF模块型号：实际分配到{module.ModuleType}(保持原始数量信息)");
            }
        }
        
        return (processedSignals, moduleDefinitions.Count);
    }
    
    /// <summary>
    /// 为设备组选择最佳模块（兼容性+负载均衡）
    /// </summary>
    private int SelectBestModuleForDeviceGroup(
        List<IoFullData> deviceSignals,
        List<(string ModuleType, Dictionary<string, int> UsedChannels, List<IoFullData> AssignedSignals)> moduleDefinitions,
        StringBuilder report)
    {
        // 获取设备的所有IO类型
        var deviceIOTypes = deviceSignals.Select(s => GetIOTypePrefix(s.IoType)).Distinct().ToList();
        report.AppendLine($"          设备IO类型: {string.Join(", ", deviceIOTypes)}");
        
        var candidateModules = new List<(int Index, int LoadScore)>();
        
        for (int i = 0; i < moduleDefinitions.Count; i++)
        {
            var module = moduleDefinitions[i];
            
            // 检查是否支持所有IO类型
            bool supportsAllTypes = deviceIOTypes.All(ioType => GetChannelRange(module.ModuleType, ioType) != null);
            if (!supportsAllTypes)
            {
                report.AppendLine($"          模块{i + 1}({module.ModuleType}): 不支持所有IO类型");
                continue;
            }
            
            // 检查容量是否充足
            bool hasEnoughCapacity = true;
            var capacityInfo = new List<string>();
            
            foreach (var ioType in deviceIOTypes)
            {
                var channelRange = GetChannelRange(module.ModuleType, ioType).Value;
                var signalsOfType = deviceSignals.Count(s => GetIOTypePrefix(s.IoType) == ioType);
                var availableChannels = channelRange.Count - module.UsedChannels[ioType];
                
                capacityInfo.Add($"{ioType}需要{signalsOfType}/剩余{availableChannels}");
                
                if (signalsOfType > availableChannels)
                {
                    hasEnoughCapacity = false;
                }
            }
            
            report.AppendLine($"          模块{i + 1}({module.ModuleType}): {string.Join(", ", capacityInfo)} - {(hasEnoughCapacity ? "容量充足" : "容量不足")}");
            
            if (hasEnoughCapacity)
            {
                // 计算负载分数（已分配信号数）
                int loadScore = module.AssignedSignals.Count;
                candidateModules.Add((i, loadScore));
            }
        }
        
        if (!candidateModules.Any())
        {
            return -1;
        }
        
        // 选择负载最低的模块
        var selected = candidateModules.OrderBy(c => c.LoadScore).First();
        report.AppendLine($"          选择结果: 模块{selected.Index + 1}(负载{selected.LoadScore}个信号)");
        
        return selected.Index;
    }
    
    /// <summary>
    /// 将设备组的所有信号分配到指定模块
    /// </summary>
    private (int ProcessedSignals, bool Success) AssignDeviceGroupToSelectedModule(
        List<IoFullData> deviceSignals, int moduleIndex,
        List<(string ModuleType, Dictionary<string, int> UsedChannels, List<IoFullData> AssignedSignals)> moduleDefinitions,
        StringBuilder report)
    {
        int processedSignals = 0;
        var module = moduleDefinitions[moduleIndex];
        
        // 按IO类型分组设备信号
        var ioTypeGroups = deviceSignals.GroupBy(s => GetIOTypePrefix(s.IoType)).ToList();
        
        foreach (var ioGroup in ioTypeGroups)
        {
            string ioType = ioGroup.Key;
            var ioSignals = ioGroup.OrderBy(s => s.SignalPositionNumber).ToList();
            
            var channelRange = GetChannelRange(module.ModuleType, ioType).Value;
            int startChannel = channelRange.Start + module.UsedChannels[ioType];
            
            report.AppendLine($"          分配{ioType}信号({ioSignals.Count}个)到模块{moduleIndex + 1}:");
            
            for (int i = 0; i < ioSignals.Count; i++)
            {
                var signal = ioSignals[i];
                
                signal.FFTerminalChannel = startChannel + i;
                module.AssignedSignals.Add(signal);
                
                report.AppendLine($"            {signal.SignalPositionNumber} -> 通道{signal.FFTerminalChannel}");
                
                processedSignals++;
            }
            
            // 更新已使用通道数
            module.UsedChannels[ioType] += ioSignals.Count;
        }
        
        return (processedSignals, true);
    }
    
    /// <summary>
    /// 获取模块容量信息
    /// </summary>
    private string GetModuleCapacityInfo(string moduleType)
    {
        return moduleType?.Trim() switch
        {
            "FS201" => "AI:2, AO:2, DI:6, DO:4",
            "FS202" => "DI:10, DO:6",  // DO通道修正为6个（11-16）
            _ => "未知模块类型"
        };
    }
    
    /// <summary>
    /// 从原始网段配置中找到匹配的模块配置，保持原始格式
    /// 例如：网段配置"1FS201 2FS202"，实际分配到FS202 → 返回"2FS202"
    /// </summary>
    /// <param name="originalFormat">原始网段配置，如"1FS201 2FS202"</param>
    /// <param name="actualModuleType">实际分配到的模块型号，如"FS202"</param>
    /// <returns>匹配的原始模块配置字符串</returns>
    private string UpdateModuleTypeInOriginalFormat(string originalFormat, string actualModuleType)
    {
        if (string.IsNullOrEmpty(originalFormat) || string.IsNullOrEmpty(actualModuleType))
        {
            return actualModuleType ?? "";
        }

        try
        {
            // 按空格和制表符分割原始网段配置
            var parts = originalFormat.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            
            // 优先查找完全匹配的模块型号配置
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                // 检查是否包含目标模块型号（大小写不敏感）
                if (trimmedPart.IndexOf(actualModuleType, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // 验证这确实是一个模块配置（包含数字+FS+数字的格式）
                    if (System.Text.RegularExpressions.Regex.IsMatch(trimmedPart, @"^\d*FS\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        // 找到匹配的原始配置，直接返回完整的原始值
                        return trimmedPart;
                    }
                }
            }
            
            // 如果没有找到完全匹配的，尝试找到任何FS模块配置并使用其数量
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (System.Text.RegularExpressions.Regex.IsMatch(trimmedPart, @"^\d*FS\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    var parsed = ParseFFSlaveModuleModel(trimmedPart);
                    // 使用找到的数量和新的模块型号组合
                    return parsed.Count > 1 ? $"{parsed.Count}{actualModuleType}" : actualModuleType;
                }
            }
            
            // 最后的回退方案：直接返回模块型号
            return actualModuleType;
        }
        catch (Exception)
        {
            // 解析失败时返回实际模块型号
            return actualModuleType;
        }
    }
    private string GetDetailedRemainingCapacity(string moduleType, Dictionary<string, int> usedChannels)
    {
        var capacities = new List<string>();
        
        if (moduleType == "FS201")
        {
            int aiRemaining = 2 - usedChannels["AI"];
            int aoRemaining = 2 - usedChannels["AO"];
            int diRemaining = 6 - usedChannels["DI"];
            int doRemaining = 4 - usedChannels["DO"];
            
            capacities.Add($"AI剩余{aiRemaining}通道");
            capacities.Add($"AO剩余{aoRemaining}通道");
            capacities.Add($"DI剩余{diRemaining}通道");
            capacities.Add($"DO剩余{doRemaining}通道");
        }
        else if (moduleType == "FS202")
        {
            int diRemaining = 10 - usedChannels["DI"];
            int doRemaining = 2 - usedChannels["DO"];
            
            capacities.Add($"DI剩余{diRemaining}通道");
            capacities.Add($"DO剩余{doRemaining}通道");
        }
        
        return string.Join(", ", capacities);
    }

    /// <summary>
    /// 获取IO类型前缀
    /// </summary>
    /// <param name="ioType">IO类型</param>
    /// <returns>IO类型前缀</returns>
    private string GetIOTypePrefix(string ioType)
    {
        if (string.IsNullOrEmpty(ioType))
            return "";
            
        string upperType = ioType.Trim().ToUpper();
        
        if (upperType.Contains("AI")) return "AI";
        if (upperType.Contains("AO")) return "AO";
        if (upperType.Contains("DI")) return "DI";
        if (upperType.Contains("DO")) return "DO";
        
        return "";
    }  



    /// <summary>
    /// 解析FF从站模块型号，提取数量和模块类型
    /// 根据项目规范，输入格式为'数量+型号'，如'2FS202'表示2个FS202模块
    /// </summary>
    /// <param name="moduleInput">模块输入字符串，如"2FS202"、"1FS201"、"FS201"等</param>
    /// <returns>返回元组(数量, 模块型号)，解析失败时返回(1, 原字符串)</returns>
    /// <remarks>
    /// 解析规则：
    /// - "2FS202" → (2, "FS202")
    /// - "1FS201" → (1, "FS201")
    /// - "FS201" → (1, "FS201") // 没有数量前缀时默认为1个
    /// </remarks>
    private (int Count, string ModuleType) ParseFFSlaveModuleModel(string moduleInput)
    {
        if (string.IsNullOrEmpty(moduleInput))
            return (1, moduleInput ?? "");

        var trimmed = moduleInput.Trim();
        
        // 查找FS开始的位置
        var fsIndex = trimmed.IndexOf("FS", StringComparison.OrdinalIgnoreCase);
        if (fsIndex == -1)
        {
            // 如果没有找到FS，返回原字符串
            return (1, trimmed);
        }
        
        if (fsIndex == 0)
        {
            // 直接以FS开头，如"FS201"，数量默认为1
            return (1, trimmed);
        }
        
        // 提取FS前面的数量部分
        var countPart = trimmed.Substring(0, fsIndex).Trim();
        var moduleTypePart = trimmed.Substring(fsIndex).Trim();
        
        if (int.TryParse(countPart, out int count) && count > 0)
        {
            return (count, moduleTypePart);
        }
        
        // 解析失败，返回默认值
        return (1, trimmed);
    }
    /// </summary>
    /// <param name="moduleType">模块类型</param>
    /// <param name="ioTypePrefix">IO类型前缀</param>
    /// <returns>通道范围，如果不支持返回null</returns>
    /// <remarks>
    /// FS201模块通道分配：
    /// - AI: 1-2
    /// - AO: 1-2
    /// - DI: 1-6
    /// - DO: 1-4
    ///
    /// FS202模块通道分配：
    /// - DI: 1-10
    /// - DO: 1-2
    /// - AI/AO: 不支持
    /// </remarks>
    private (int Start, int Count)? GetChannelRange(string moduleType, string ioTypePrefix)
    {
        return moduleType?.Trim() switch
        {
            "FS201" => ioTypePrefix switch
            {
                "AI" => (1, 2),  // AI: 1-2
                "AO" => (3, 2),  // AO: 3-4
                "DI" => (5, 6),  // DI: 5-10
                "DO" => (11, 4), // DO: 11-14
                _ => null
            },
            "FS202" => ioTypePrefix switch
            {
                "DI" => (1, 10), // DI: 1-10
                "DO" => (11, 6), // DO: 11-16 (起始通道11，共6个通道)
                "AI" => null,    // 不支持
                "AO" => null,    // 不支持
                _ => null
            },
            _ => null
        };
    }

    #endregion

}

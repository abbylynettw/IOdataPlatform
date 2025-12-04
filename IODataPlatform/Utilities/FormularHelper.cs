#pragma warning disable CA1822 // 将成员标记为 static
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using Aspose.Pdf.Forms;
using System.Windows.Xps.Packaging;
using Aspose.Pdf.Operators;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Models; // 添加引用以支持 CabinetReservedSlotConfig

using SqlSugar;
using Wpf.Ui.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace IODataPlatform.Utilities
{
    /// <summary>
    /// 公式帮助类
    /// 提供IO数据处理中的各种公式计算和数据转换功能
    /// 包含标签名解析、电源分组、卡件编号计算等核心业务逻辑
    /// 支持多种数据格式转换和复杂的字符串处理操作
    /// </summary>
    public class FormularHelper()
    {
        /// <summary>当前机笼索引，用于跟踪下一个插入板卡的机笼位置</summary>
        private int currentCageIndex = 0;
        
        /// <summary>IO分配报告（记录分配过程和决策）</summary>
        private System.Text.StringBuilder allocationReport = new System.Text.StringBuilder();

        /// <summary>
        /// 电源供给方式分组字典
        /// 定义不同IO类型的电源分组策略，用于优化板卡布局和电源管理
        /// 支持DI、DO、AI、AO等多种信号类型的智能分组
        /// </summary>
        private Dictionary<string, string> powerSupplyGrouping = new Dictionary<string, string>()
        {
            // DI 分组
            {"DI1", "Group1"}, {"DI6", "Group1"},
            {"DI2", "Group2"}, {"DI3", "Group2"}, {"DI4", "Group2"}, {"DI5", "Group2"},
            // DO 分组
            {"DO1", "Group3"}, {"DO2", "Group3"},
            {"DO3", "Group4"}, // DO3 并且 DO5 如果板卡不够可以放在一起
            {"DO4", "Group5"},
            {"DO5", "Group6"}, // 单独为 DO5 分配一个组，如果需要与 DO3 放在一起，则可以调整为 "Group4"
            // AI 分组
            {"AI1", "Group7"}, {"AI6", "Group7"},
            {"AI2", "Group8"}, {"AI3", "Group8"}, {"AI4", "Group8"}, {"AI5", "Group8"},
            {"AI7", "Group9"}, {"AI8", "Group9"}, {"AI9", "Group9"},
            {"P1","Group10"}, {"P2","Group10"}, {"P3","Group10"},
            {"AO1","Group11"},
            {"AOH","Group12"},
            {"AO2","Group13"},
        };

        /// <summary>
        /// 计算IO卡件编号和板卡后缀
        /// 根据IO模块编号和指定后缀，计算生成最终的卡件编号
        /// 支持特殊的编号规则和格式化要求
        /// </summary>
        /// <param name="ioModule">IO模块编号字符串</param>
        /// <param name="lastfix">要添加的后缀字符串</param>
        /// <returns>返回计算后的完整卡件编号</returns>
        public string CalculateIoCardNumberAndBN(string ioModule, string lastfix)
        {
            if (string.IsNullOrEmpty(ioModule) || ioModule.Length < 7)
            {
                // 处理 IO_Module 为空或长度不足 7 的情况
                return ioModule + lastfix;
            }

            string rightThreeDigits = ioModule.Substring(ioModule.Length - 3, 3);
            if (ioModule[6] == '1')
            {
                int number = int.Parse(rightThreeDigits) + 2;
                return number.ToString("D3") + lastfix; // 使用 "D3" 确保数字是三位数
            }
            else
            {
                return rightThreeDigits + lastfix;
            }
        }
        public string GetTagNameSection(string tagName, int index)
        {
            if (string.IsNullOrEmpty(tagName))
                return "";
            if (GetEx(tagName) != "")
            {
                tagName = tagName.Remove(0, 2);
            }
            var tagArr = tagName.Split('_');
            if (tagArr != null && tagArr.Length > 0 && tagArr.Length <= 4)
            {
                var middle = -1;
                for (int i = 0; i < tagArr.Length; i++)
                {
                    if (tagArr[i] == "H" || tagArr[i] == "HO" || tagArr[i] == "HC" || tagArr[i] == "SA" || tagArr[i] == "DH")
                    {
                        middle = i;
                    }
                }
                switch (index)
                {
                    case 0:
                        if (middle == -1)
                        {
                            if (tagArr.Length > 2)
                            {
                                return "";
                            }
                            else
                            {
                                return tagArr[0];
                            }
                        }
                        return tagArr[0];
                    case 1:
                        string str = "";
                        if (middle == -1)
                        {
                            if (tagArr.Length == 2)
                            {
                                return tagArr[1];
                            }
                            return str;
                        }
                        //去掉第一部分 和middle之后的部分
                        for (int i = 1; i < middle; i++)
                        {
                            str += i == middle - 1 ? tagArr[i] : tagArr[i] + "_";
                        }
                        return str;

                    case 2:
                        if (middle == -1)
                        {
                            return "";
                        }
                        return tagArr[middle];
                    case 3:
                        string str1 = "";
                        if (middle == -1)
                        {
                            return str1;
                        }
                        //去掉第一部分 和middle之后的部分
                        for (int i = middle + 1; i < tagArr.Length; i++)
                        {
                            str1 += i == tagArr.Length - 1 ? tagArr[i] : tagArr[i] + "_";
                        }
                        return str1;

                    default:
                        break;
                }
            }
            return "";
        }

        public string GetEx(string tagName)
        {
            if (string.IsNullOrEmpty(tagName) || tagName.Length < 2)
            {
                return "";
            }
            var firstChar = tagName[0];
            var secondChar = tagName[1];
            if (RegexDao.IsEnglisCh(firstChar.ToString()) && RegexDao.IsNumber(secondChar.ToString()))
            {
                return tagName.Substring(0, 2);
            }
            return "";
        }
        public int CountCharacterOccurrences(string str, char character)
        {
            int count = 0;
            foreach (char c in str)
            {
                if (c == character)
                {
                    count++;
                }
            }
            return count;
        }

        public bool JudgeIsSame(string 信号位号, string 仪表功能号)
        {
            try
            {
                if (信号位号[..2] != 仪表功能号[..2])
                { return false; }
                return 信号位号.Split("-").Last() == 仪表功能号.Split("-").Last();
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static float GetDigitsAsFloat(string str)
        {
            var sb = new StringBuilder();
            bool dotEncountered = false;

            foreach (char c in str)
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                }
                else if (c == '.' && !dotEncountered)
                {
                    // Append the dot only if it's the first one encountered
                    sb.Append(c);
                    dotEncountered = true;
                }
            }

            return float.Parse(sb.ToString(), CultureInfo.InvariantCulture);
        }

        public List<xtes_AVI> ConvertToAviList(IEnumerable<IoFullData> substations)
        {
            var aviList = substations.Select(s =>
            {
                // 尝试将 RangeUpperLimit 和 RangeLowerLimit 转换为数值类型
                double upperLimit, lowerLimit;
                bool isUpperValid = double.TryParse(s.RangeUpperLimit, out upperLimit);
                bool isLowerValid = double.TryParse(s.RangeLowerLimit, out lowerLimit);

                // 如果是异常情况，MU 和 MD 赋值为 0，否则按正常赋值
                double muValue = (!isUpperValid) ? 100 : upperLimit;
                double mdValue = (!isLowerValid) ? 0 : lowerLimit;
                // 使用一个表达式进行赋值
                string ofValue = (muValue < mdValue) ? "Err" :
                                 (upperLimit - lowerLimit) <= 10 ? "3" :
                                 (upperLimit - lowerLimit) <= 100 ? "2" : "1";
                string tpValue = System.Text.RegularExpressions.Regex.IsMatch(s.ElectricalCharacteristics, @"4.20mA", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? "6" :
                         s.ElectricalCharacteristics.Contains("PT100") ? "7" :
                         s.ElectricalCharacteristics.Contains("TC") ? "13" : "Err";
                return new xtes_AVI
                {
                    CHN = s.Channel.ToString(),
                    PN = s.TagName,
                    DESC = s.Description,
                    UNIT = s.EngineeringUnit,
                    MU = muValue,
                    MD = mdValue,
                    TRAIN = "NULL",
                    IH = "1",
                    SYS = s.SystemCode,
                    SUBNET = s.SubNet,
                    SN = s.StationNumber,
                    CLN = ((s.Cage - 1) == 0 ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
                    MON = s.CardType,
                    OF = ofValue,
                    RG = s.RGRelatedScreen,
                    // 其他字段根据需要填充
                    TP = tpValue,
                    SQ = "0",
                    QFM = "1",
                    QFIA = "0",
                    LCV = "0",
                    SD = "1.0",
                    OLQ = "1",
                    OEL = "10",
                    OLT = "3",
                    ALLOCATION = "0",
                    ACUT = "1",
                    INHIBIT = "0", // 
                    DSEL = "0",
                    DI = 1,
                    H4AP = (!string.IsNullOrEmpty(s.High4LimitAlarmLevel) &&
                            !string.IsNullOrEmpty(s.High4AlarmDescription) &&
                            !string.IsNullOrEmpty(s.High4LimitAlarmTag))
                            ? 1 : 0,
                    H4 = s.High4LimitAlarmValue,
                    H4LEVEL = s.High4LimitAlarmLevel,
                    H4_DESC = s.High4AlarmDescription,
                    H4KA = s.High4LimitAlarmTag,
                    H4SI = "0",
                    H4DL = "0",

                    H3AP = (!string.IsNullOrEmpty(s.High3LimitAlarmLevel) &&
                            !string.IsNullOrEmpty(s.High3AlarmDescription) &&
                            !string.IsNullOrEmpty(s.High3LimitAlarmTag))
                            ? 1 : 0,
                    H3 = s.High3LimitAlarmValue,
                    H3LEVEL = s.High3LimitAlarmLevel,
                    H3_DESC = s.High3AlarmDescription,
                    H3KA = s.High3LimitAlarmTag,
                    H3SI = "0",
                    H3DL = "0",

                    H2AP = (!string.IsNullOrEmpty(s.High2LimitAlarmLevel) &&
                            !string.IsNullOrEmpty(s.High2AlarmDescription) &&
                            !string.IsNullOrEmpty(s.High2LimitAlarmTag))
                            ? 1 : 0,
                    H2 = s.High2LimitAlarmValue,
                    H2LEVEL = s.High2LimitAlarmLevel,
                    H2_DESC = s.High2AlarmDescription,
                    H2KA = s.High2LimitAlarmTag,
                    H2SI = "0",
                    H2DL = "0",

                    H1AP = (!string.IsNullOrEmpty(s.High1LimitAlarmLevel) &&
                            !string.IsNullOrEmpty(s.High1AlarmDescription) &&
                            !string.IsNullOrEmpty(s.High1LimitAlarmTag))
                            ? 1 : 0,
                    H1 = s.High1LimitAlarmValue,
                    H1LEVEL = s.High1LimitAlarmLevel,
                    H1_DESC = s.High1AlarmDescription,
                    H1KA = s.High1LimitAlarmTag,
                    H1SI = "0",
                    H1DL = "0",

                    L1AP = (!string.IsNullOrEmpty(s.Low1LimitAlarmLevel) &&
                            !string.IsNullOrEmpty(s.Low1AlarmDescription) &&
                            !string.IsNullOrEmpty(s.Low1LimitAlarmTag))
                            ? 1 : 0,
                    L1 = s.Low1LimitAlarmValue,
                    L1LEVEL = s.Low1LimitAlarmLevel,
                    L1_DESC = s.Low1AlarmDescription,
                    L1KA = s.Low1LimitAlarmTag,
                    L1SI = "0",
                    L1DL = "0",

                    L2AP = (!string.IsNullOrEmpty(s.Low2LimitAlarmLevel) &&
                            !string.IsNullOrEmpty(s.Low2AlarmDescription) &&
                            !string.IsNullOrEmpty(s.Low2LimitAlarmTag))
                            ? 1 : 0,
                    L2 = s.Low2LimitAlarmValue,
                    L2LEVEL = s.Low2LimitAlarmLevel,
                    L2_DESC = s.Low2AlarmDescription,
                    L2KA = s.Low2LimitAlarmTag,
                    L2SI = "0",
                    L2DL = "0",

                    L3AP = (!string.IsNullOrEmpty(s.Low3LimitAlarmLevel) &&
                            !string.IsNullOrEmpty(s.Low3AlarmDescription) &&
                            !string.IsNullOrEmpty(s.Low3LimitAlarmTag))
                            ? 1 : 0,
                    L3 = s.Low3LimitAlarmValue,
                    L3LEVEL = s.Low3LimitAlarmLevel,
                    L3_DESC = s.Low3AlarmDescription,
                    L3KA = s.Low3LimitAlarmTag,
                    L3SI = "0",
                    L3DL = "0",

                    L4AP = (!string.IsNullOrEmpty(s.Low4LimitAlarmLevel) &&
                            !string.IsNullOrEmpty(s.Low4AlarmDescription) &&
                            !string.IsNullOrEmpty(s.Low4LimitAlarmTag))
                            ? 1 : 0,
                    L4 = s.Low4LimitAlarmValue,
                    L4LEVEL = s.Low4LimitAlarmLevel,
                    L4_DESC = s.Low4AlarmDescription,
                    L4KA = s.Low4LimitAlarmTag,
                    L4SI = "0",
                    L4DL = "0",
                    RALM = "0",
                    FILTER_TIME = "0"
                };
            }).ToList();

            return aviList;
        }
        public List<xtes_PVI> ConvertToPviList(IEnumerable<IoFullData> substations)
        {
            var pviList = substations.Select(s =>
            {
                // 尝试将 RangeUpperLimit 和 RangeLowerLimit 转换为数值类型
                double upperLimit, lowerLimit;
                bool isUpperValid = double.TryParse(s.RangeUpperLimit, out upperLimit);
                bool isLowerValid = double.TryParse(s.RangeLowerLimit, out lowerLimit);

                // 如果是异常情况，MU 和 MD 赋值为 0，否则按正常赋值
                double muValue = (!isUpperValid) ? 100 : upperLimit;
                double mdValue = (!isLowerValid) ? 0 : lowerLimit;
                // 使用一个表达式进行赋值
                string ofValue = (muValue < mdValue) ? "Err" :
                                 (upperLimit - lowerLimit) <= 10 ? "3" :
                                 (upperLimit - lowerLimit) <= 100 ? "2" : "1";
                string tpValue = System.Text.RegularExpressions.Regex.IsMatch(s.ElectricalCharacteristics, @"4.20mA", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? "6" :
                         s.ElectricalCharacteristics.Contains("PT100") ? "7" :
                         s.ElectricalCharacteristics.Contains("TC") ? "13" : "Err";

                return new xtes_PVI
                {
                    CHN = s.Channel.ToString(),
                    PN = s.TagName,
                    DESC = s.Description,
                    UNIT = s.EngineeringUnit,
                    MU = muValue,
                    MD = mdValue,
                    TRAIN = "NULL",
                    IH = "1",
                    SYS = s.SystemCode,
                    SUBNET = s.SubNet,
                    SN = s.StationNumber,
                    CLN = ((s.Cage - 1) == 0 ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
                    MON = s.CardType,
                    OF = ofValue,
                    RG = s.RGRelatedScreen,
                    // 其他字段根据需要填充                  
                    QFM = "1",
                    QFIA = "0",
                    SD = "1.0",
                    OLQ = "1",
                    OEL = "10",
                    OLT = "3",
                    PG = "1",
                    ALLOCATION = "0",
                    ACUT = "1",
                    INHIBIT = "0", // 
                    DSEL = "0",
                    DI = 1,
                    H4AP = "0",
                    H4 = "98",
                    H4LEVEL = "0",
                    H4DEC = "0",
                    H4SI = "0",
                    H4DL = "0",
                    H3AP = "0",
                    H3 = "95",
                    H3LEVEL = "0",
                    H3DEC = "0",
                    H3SI = "0",
                    H3DL = "0",
                    H2AP = "0",
                    H2 = "90",
                    H2LEVEL = "0",
                    H2DEC = "0",
                    H2SI = "0",
                    H2DL = "0",
                    H1AP = "0",
                    H1 = "80",
                    H1LEVEL = "0",
                    H1DEC = "0",
                    H1SI = "0",
                    H1DL = "0",

                    L1AP = "0",
                    L1 = "20",
                    L1LEVEL = "0",
                    L1DEC = "0",
                    L1SI = "0",
                    L1DL = "0",

                    L2AP = "0",
                    L2 = "10",
                    L2LEVEL = "0",
                    L2DEC = "0",
                    L2SI = "0",
                    L2DL = "0",

                    L3AP = "0",
                    L3 = "5",
                    L3LEVEL = "0",
                    L3DEC = "0",
                    L3SI = "0",
                    L3DL = "0",

                    L4AP = "0",
                    L4 = "3",
                    L4LEVEL = "0",
                    L4DEC = "0",
                    L4SI = "0",
                    L4DL = "0",
                    FREQUENCY = "33",
                    MAXPW = "0",
                    MINPW = "0",
                    PFT = "0"

                };
            }).ToList();

            return pviList;
        }
        public List<xtes_AVO> ConvertToAvoList(IEnumerable<IoFullData> substations)
        {
            var avoList = substations.Select(s =>
            {
                // 尝试将 RangeUpperLimit 和 RangeLowerLimit 转换为数值类型
                double upperLimit, lowerLimit;
                bool isUpperValid = double.TryParse(s.RangeUpperLimit, out upperLimit);
                bool isLowerValid = double.TryParse(s.RangeLowerLimit, out lowerLimit);

                // 如果是异常情况，MU 和 MD 赋值为 0，否则按正常赋值
                double muValue = (!isUpperValid) ? 100 : upperLimit;
                double mdValue = (!isLowerValid) ? 0 : lowerLimit;
                // 使用一个表达式进行赋值
                string ofValue = (muValue < mdValue) ? "Err" :
                                 (upperLimit - lowerLimit) <= 10 ? "3" :
                                 (upperLimit - lowerLimit) <= 100 ? "2" : "1";
                string tpValue = System.Text.RegularExpressions.Regex.IsMatch(s.ElectricalCharacteristics, @"4.20mA", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? "6" :
                         s.ElectricalCharacteristics.Contains("PT100") ? "7" :
                         s.ElectricalCharacteristics.Contains("TC") ? "13" : "Err";
                return new xtes_AVO
                {
                    CHN = s.Channel.ToString(),
                    PN = s.TagName,
                    DESC = s.Description,
                    UNIT = s.EngineeringUnit,
                    MU = muValue,
                    MD = mdValue,
                    TRAIN = "NULL",
                    IH = "1",
                    SYS = s.SystemCode,
                    SUBNET = s.SubNet,
                    CLN = ((s.Cage - 1) == 0 ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
                    MON = s.CardType,
                    SN = s.StationNumber,
                    OF = ofValue,
                    TP = "1",
                    FAVTYPE = "0",
                    FAV = "0",
                    ISOF = "17",
                    RG = s.RGRelatedScreen,
                    GROUPS = "1"  // 添加GROUPS字段
                };
            }).ToList();

            return avoList;
        }

        public List<xtes_DVI> ConvertToDviList(IEnumerable<IoFullData> substations)
        {
            var dviList = substations.Select(s =>
            {
                return new xtes_DVI
                {
                    CHN = s.Channel.ToString(),
                    PN = s.TagName,
                    DESC = s.Description,
                    TRAIN = "NULL",
                    IH = "1",
                    SYS = s.SystemCode,
                    INLOG = "0",  // 修正：应为0而非1
                    SUBNET = s.SubNet,
                    CLN = ((s.Cage - 1) == 0 ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
                    MON = s.CardType,
                    SN = s.StationNumber,
                    RG = s.RGRelatedScreen,
                    TOC = "1",  // 修正：应为1而非60
                    TVA = "60",
                    TOCT = "10",
                    BCT = "1",  // 修正：应为1而非0
                    DBT = "100",  // 修正：应为100而非5
                    QFM = "1",
                    QFID = "0",


                    SOE = "0",
                    QUICK = "0",
                    IC = "1",
                    ALLOCATION = "0",
                    ACUT = "1",
                    AP = (!string.IsNullOrEmpty(s.AlarmLevel) && // 检查 AlarmLevel 是否不为空
                          !string.IsNullOrEmpty(s.SwitchQuantityAlarmTag) && // 检查 SwitchQuantityAlarmTag 是否不为空
                          !string.IsNullOrEmpty(s.AlarmDescription))
                         ? 1 : 0,
                    ALMLEVEL = s.AlarmLevel,
                    KA = s.SwitchQuantityAlarmTag,
                    AL_DESC = s.AlarmDescription,
                    AF = s.AlarmAttribute,
                    DEC = "0",
                    SI = "0",
                    TBTYPE = "1",
                    ROUT = "0",
                    E1 = "1",
                    E0 = "0",
                    GROUPS = "1"  // 添加GROUPS字段
                };
            }).ToList();

            return dviList;
        }
        public List<xtes_DVO> ConvertToDvoList(IEnumerable<IoFullData> substations)
        {
            var DVOList = substations.Select(s => new xtes_DVO
            {
                CHN = s.Channel.ToString(),
                PN = s.TagName,
                DESC = s.Description,
                TRAIN = "NULL",
                IH = "1",
                SYS = s.SystemCode,
                INLOG = "0",  // 修正：应为0而非1
                SUBNET = s.SubNet,
                CLN = ((s.Cage - 1) == 0 ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
                MON = s.CardType,
                SN = s.StationNumber,
                FAVTYPE = "1",
                FAV = "0",
                RG = s.RGRelatedScreen,
                GROUPS = "1"  // 添加GROUPS字段
            }).ToList();

            return DVOList;
        }

        /// <summary>
        /// 转换为AM（Real型模拟量一层中间点）
        /// 过滤条件：供电类型为FF1~FF6
        /// </summary>
        public List<xtes_AM> ConvertToAMList(IEnumerable<IoFullData> substations)
        {
            var amList = substations.Select(s =>
            {
                // 尝试将 RangeUpperLimit 和 RangeLowerLimit 转换为数值类型
                double upperLimit, lowerLimit;
                bool isUpperValid = double.TryParse(s.RangeUpperLimit, out upperLimit);
                bool isLowerValid = double.TryParse(s.RangeLowerLimit, out lowerLimit);

                // 如果是异常情况，MU 和 MD 赋值为 0，否则按正常赋值
                double muValue = (!isUpperValid) ? 100 : upperLimit;
                double mdValue = (!isLowerValid) ? 0 : lowerLimit;
                
                // 计算OF - 显示格式
                string ofValue = (muValue < mdValue) ? "Err" :
                                 Math.Abs(upperLimit - lowerLimit) <= 10 ? "3" :
                                 Math.Abs(upperLimit - lowerLimit) <= 100 ? "2" :
                                 Math.Abs(upperLimit - lowerLimit) <= 1000000 ? "1" : "4";
                
                return new xtes_AM
                {
                    PN = s.TagName,
                    DESC = s.Description,
                    UNIT = s.EngineeringUnit,
                    MU = muValue,
                    MD = mdValue,
                    SYS = s.SystemCode,
                    SUBNET = s.SubNet,
                    SN = s.StationNumber,                  
                    OF = ofValue,
                    QFIA = "1",
                    SD = "1.0",
                    OLQ = "1",
                    OLT = "3",
                    GROUPS = "1"
                };
            }).ToList();

            return amList;
        }

        /// <summary>
        /// 转换为DM_FEW（开关量一层中间点）
        /// 过滤条件：供电类型为FF7~FF8、DP2
        /// </summary>
        public List<xtes_DM_FEW> ConvertToDM_FEWList(IEnumerable<IoFullData> substations)
        {
            var dmFewList = substations.Select(s => new xtes_DM_FEW
            {
                PN = s.TagName,
                DESC = s.Description,
                IH = "1",
                SYS = s.SystemCode,
                INLOG = "0",
                SUBNET = s.SubNet,
                SN = s.StationNumber,
                E1 = "1",
                E0 = "0",
                HIGH = "0",
                GROUPS = "1"
            }).ToList();

            return dmFewList;
        }

        public List<xtes_GBP> ConvertToGBPList(IEnumerable<IoFullData> substations)
        {
            // GBP生成逻辑：
            // 1. 筛选DO点（供电类型前两个字符为DO，扩展码含K/G/KG/Q/T/QT）
            // 2. 去掉扩展码，得到设备点名
            // 3. 只包含纯DO点（去掉扩展码后不重名，或只有DO类型）
            // 4. 如遇到重名的既有DO也有AO的，则建到GCP里
            
            var allPoints = substations.ToList();
            
            // 筛选DO点（供电类型前两个字符为DO，扩展码含K/G/KG/Q/T/QT）
            var doPoints = allPoints
                .Where(s => !string.IsNullOrEmpty(s.PowerType) && 
                           s.PowerType.Length >= 2 &&
                           s.PowerType.Substring(0, 2).Equals("DO", StringComparison.OrdinalIgnoreCase) &&
                           !string.IsNullOrEmpty(s.ExtensionCode) &&
                           (s.ExtensionCode.Contains("K") || s.ExtensionCode.Contains("G") || 
                            s.ExtensionCode.Contains("Q") || s.ExtensionCode.Contains("T")))
                .ToList();
            
            // 筛选AO点（供电类型前两个字符为AO）
            var aoPoints = allPoints
                .Where(s => !string.IsNullOrEmpty(s.PowerType) && 
                           s.PowerType.Length >= 2 &&
                           s.PowerType.Substring(0, 2).Equals("AO", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            // 获取所有设备名（去DO扩展码）
            var doDeviceNames = new HashSet<string>();
            foreach (var point in doPoints)
            {
                var deviceName = GetDeviceName(point.TagName, point.ExtensionCode);
                if (!string.IsNullOrEmpty(deviceName))
                {
                    doDeviceNames.Add(deviceName);
                }
            }
            
            // 获取所有AO设备名（去扩展码）
            var aoDeviceNames = new HashSet<string>();
            foreach (var point in aoPoints)
            {
                var deviceName = GetDeviceName(point.TagName, point.ExtensionCode);
                if (!string.IsNullOrEmpty(deviceName))
                {
                    aoDeviceNames.Add(deviceName);
                }
            }
            
            // 找出既有DO又有AO的设备名（这些应该建到GCP）
            var mixedDeviceNames = new HashSet<string>(doDeviceNames.Intersect(aoDeviceNames));
            
            // GBP：只有DO的设备
            var gbpDeviceNames = doDeviceNames.Except(mixedDeviceNames).ToHashSet();
            
            // 按设备名分组，每个设备只取第一个
            var gbpPoints = doPoints
                .GroupBy(s => GetDeviceName(s.TagName, s.ExtensionCode))
                .Where(g => gbpDeviceNames.Contains(g.Key))
                .Select(g => g.First())
                .ToList();
            
            var GBPList = gbpPoints.Select(s =>
            {
                // 处理点名：去掉扩展码
                string devicePN = GetDeviceName(s.TagName, s.ExtensionCode);
                
                // 处理描述：去掉"开、关、开关、启动、停止"字样
                string deviceDesc = s.Description ?? "";
                deviceDesc = deviceDesc
                    .Replace("开关", "")
                    .Replace("启动", "")
                    .Replace("停止", "")
                    .Replace("开", "")
                    .Replace("关", "")
                    .Trim();
                
                return new xtes_GBP
                {
                    PN = devicePN,                   // 序号1 - 去掉扩展码
                    DESC = deviceDesc,               // 序号2 - 去掉开关等字样
                    SYS = s.SystemCode,              // 序号3
                    SUBNET = s.SubNet,               // 序号4
                    SN = s.StationNumber,            // 序号5
                    GROUPS = "1"                     // 序号6
                };
            }).ToList();

            return GBPList;
        }
        
        /// <summary>
        /// 获取设备名（去掉扩展码）
        /// </summary>
        private string GetDeviceName(string tagName, string extensionCode)
        {
            if (string.IsNullOrEmpty(tagName)) return "";
            
            if (!string.IsNullOrEmpty(extensionCode) && tagName.EndsWith(extensionCode))
            {
                return tagName.Substring(0, tagName.Length - extensionCode.Length);
            }
            
            return tagName;
        }
        public List<xtes_GCP> ConvertToGCPList(IEnumerable<IoFullData> substations)
        {
            // GCP生成逻辑：
            // 1. 筛选DO点（供电类型前两个字符为DO，扩展码含K/G/KG/Q/T/QT）
            // 2. 去掉扩展码，得到设备点名
            // 3. 如遇到重名的既有DO也有AO的，则建到GCP里
            
            var allPoints = substations.ToList();
            
            // 筛选DO点（供电类型前两个字符为DO，扩展码含K/G/KG/Q/T/QT）
            var doPoints = allPoints
                .Where(s => !string.IsNullOrEmpty(s.PowerType) && 
                           s.PowerType.Length >= 2 &&
                           s.PowerType.Substring(0, 2).Equals("DO", StringComparison.OrdinalIgnoreCase) &&
                           !string.IsNullOrEmpty(s.ExtensionCode) &&
                           (s.ExtensionCode.Contains("K") || s.ExtensionCode.Contains("G") || 
                            s.ExtensionCode.Contains("Q") || s.ExtensionCode.Contains("T")))
                .ToList();
            
            // 筛选AO点（供电类型前两个字符为AO）
            var aoPoints = allPoints
                .Where(s => !string.IsNullOrEmpty(s.PowerType) && 
                           s.PowerType.Length >= 2 &&
                           s.PowerType.Substring(0, 2).Equals("AO", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            // 获取所有设备名（去DO扩展码）
            var doDeviceNames = new HashSet<string>();
            foreach (var point in doPoints)
            {
                var deviceName = GetDeviceName(point.TagName, point.ExtensionCode);
                if (!string.IsNullOrEmpty(deviceName))
                {
                    doDeviceNames.Add(deviceName);
                }
            }
            
            // 获取所有AO设备名（去扩展码）
            var aoDeviceNames = new HashSet<string>();
            foreach (var point in aoPoints)
            {
                var deviceName = GetDeviceName(point.TagName, point.ExtensionCode);
                if (!string.IsNullOrEmpty(deviceName))
                {
                    aoDeviceNames.Add(deviceName);
                }
            }
            
            // 找出既有DO又有AO的设备名（这些应该建到GCP）
            var mixedDeviceNames = new HashSet<string>(doDeviceNames.Intersect(aoDeviceNames));
            
            // 按设备名分组，每个设备只取第一个
            var gcpPoints = doPoints
                .GroupBy(s => GetDeviceName(s.TagName, s.ExtensionCode))
                .Where(g => mixedDeviceNames.Contains(g.Key))
                .Select(g => g.First())
                .ToList();
            
            var GCPList = gcpPoints.Select(s =>
            {
                // 处理点名：去掉扩展码
                string devicePN = GetDeviceName(s.TagName, s.ExtensionCode);
                
                // 处理描述：去掉"开、关、开关、启动、停止"字样
                string deviceDesc = s.Description ?? "";
                deviceDesc = deviceDesc
                    .Replace("开关", "")
                    .Replace("启动", "")
                    .Replace("停止", "")
                    .Replace("开", "")
                    .Replace("关", "")
                    .Trim();
                
                // 尝试将 RangeUpperLimit 和 RangeLowerLimit 转换为数值类型
                double upperLimit, lowerLimit;
                bool isUpperValid = double.TryParse(s.RangeUpperLimit, out upperLimit);
                bool isLowerValid = double.TryParse(s.RangeLowerLimit, out lowerLimit);

                // 如果是异常情况，MU 和 MD 赋值为 0，否则按正常赋值
                double muValue = (!isUpperValid) ? 100 : upperLimit;
                double mdValue = (!isLowerValid) ? 0 : lowerLimit;
                
                return new xtes_GCP
                {
                    PN = devicePN,                   // 序号1 - 去掉扩展码
                    DESC = deviceDesc,               // 序号2 - 去掉开关等字样
                    SYS = s.SystemCode,              // 序号3
                    SUBNET = s.SubNet,               // 序号5
                    SN = s.StationNumber,            // 序号6
                    UNIT = s.EngineeringUnit,
                    INH = muValue,
                    INL = mdValue,
                    FRATE = "5",
                    SRATE = "1",
                    GROUPS = "1"                     // 序号7
                };
            }).ToList();
            
            return GCPList;
        }
        public List<xtes_GST> ConvertToGSTList(IEnumerable<IoFullData> substations)
        {
            return new List<xtes_GST>();
        }
        public List<xtes_GKC> ConvertToGKCList(IEnumerable<IoFullData> substations)
        {
            return new List<xtes_GKC>();
        }

        #region 自动IO分配
        /// <summary>
        /// 自动IO分配
        /// </summary>
        /// <param name="iODatas"></param>
        /// <returns></returns>
        public List<IoFullData> AutoAllocateXT1IO(List<IoFullData> datas, List<config_card_type_judge> configs, double rate)
        {
            var cabinetInfos = datas.BuildCabinetStructureXT1(configs);
            return cabinetInfos.ToPoint();
        }
        public List<IoFullData> AutoAllocateIO(List<IoFullData> datas, List<config_card_type_judge> configs, double rate, List<CabinetReservedSlotConfig>? reservedConfigs = null)
        {
            // 初始化总报告
            allocationReport.Clear();
            allocationReport.AppendLine("========== IO自动分配报告 ==========");
            allocationReport.AppendLine($"开始时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            allocationReport.AppendLine($"冗余率：{rate:P0}");
            if (reservedConfigs != null && reservedConfigs.Count > 0)
            {
                int totalReservedSlots = reservedConfigs.Where(c => c.IsSelected).Sum(c => c.SlotConfigs.Count);
                allocationReport.AppendLine($"预留插槽数量：{totalReservedSlots} 个");
            }
            allocationReport.AppendLine();
            
            var cabinetInfos = datas.BuildCabinetStructureOther(configs);
            allocationReport.AppendLine($"总机柜数量：{cabinetInfos.Count}");
            allocationReport.AppendLine();
            
            //所有点都变成unsetpoint;
            //2.每个机柜将点分为3部分，硬接线，总线箱，B类阀箱
            int cabinetIndex = 0;
            foreach (StdCabinet cabinet in cabinetInfos)
            {
                cabinetIndex++;
                allocationReport.AppendLine($"\n{'='*60}");
                allocationReport.AppendLine($"【机柜 {cabinetIndex}/{cabinetInfos.Count}】{cabinet.Name}");
                allocationReport.AppendLine($"{'='*60}\n");
                
                // 🔑 传入当前机柜的预留配置
                var cabinetReservedConfig = reservedConfigs?.FirstOrDefault(c => c.CabinetName == cabinet.Name && c.IsSelected);
                AutoAllocateIOSingleCabinet(cabinet, configs, rate, cabinetReservedConfig);
            }
            
            return cabinetInfos.ToPoint();
        }
        /// <summary>
        /// 单个机柜的IO自动分配（供外部调用）
        /// </summary>
        public StdCabinet AutoAllocateIO(StdCabinet cabinet, List<config_card_type_judge> configs, double rate)
        {
            // 初始化报告（单机柜模式）
            allocationReport.Clear();
            allocationReport.AppendLine("========== IO自动分配报告 ==========");
            allocationReport.AppendLine($"机柜名称：{cabinet.Name}");
            allocationReport.AppendLine($"开始时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            allocationReport.AppendLine($"冗余率：{rate:P0}");
            allocationReport.AppendLine();
            
            return AutoAllocateIOSingleCabinet(cabinet, configs, rate);
        }
        
        private StdCabinet AutoAllocateIOSingleCabinet(StdCabinet cabinet, List<config_card_type_judge> configs, double rate, CabinetReservedSlotConfig? reservedConfig = null)
        {
            // 不再清空报告，而是追加到报告中
            // allocationReport.Clear();  // ❌ 删除这行
            
            var allpoints = cabinet.ToPoint().Where(c => c.PowerType != null).Select(t => t.PowerType).Distinct();
            foreach (var tag in allpoints) //添加点
            {
                if (!powerSupplyGrouping.ContainsKey(tag))
                {
                    powerSupplyGrouping.Add(tag, tag);
                }
            }

            // 🔑 步骤1：清空现有分配
            allocationReport.AppendLine("【步骤1：清空现有分配】");
            int totalPoints = cabinet.ToPoint().Count;
            allocationReport.AppendLine($"总信号数量：{totalPoints}");
            
            ClearPointsAndAddToUnset(cabinet);
            
            allocationReport.AppendLine($"已清空所有板卡插槽和虚拟插槽");
            allocationReport.AppendLine($"已将 {cabinet.UnsetPoints.Count} 个信号移至未分配列表");
            allocationReport.AppendLine($"已清除信号的机笼、插槽、通道信息（设为0）");
            allocationReport.AppendLine();
            
            // 🔑 步骤2：根据预留配置生成预留信号并占位
            if (reservedConfig != null && reservedConfig.IsSelected && reservedConfig.SlotConfigs.Any())
            {
                allocationReport.AppendLine("【步骤2：预留插槽占位】");
                allocationReport.AppendLine($"预留插槽数量：{reservedConfig.SlotConfigs.Count} 个");
                
                // 获取最后一个机笼
                var lastCage = cabinet.Cages.OrderByDescending(c => c.Index).FirstOrDefault();
                if (lastCage == null)
                {
                    allocationReport.AppendLine($"警告：机柜 {cabinet.Name} 没有机笼，跳过预留");
                    allocationReport.AppendLine();
                }
                else
                {
                    var slotsInLastCage = lastCage.Slots.OrderBy(s => s.Index).ToList();
                    int reservedCount = 0;
                    
                    // 🔑 查找已有的预留信号（通讯预留和报警预留），确定起始插槽位置
                    var existingReservedPoints = cabinet.ToPoint()
                        .Where(p => (p.PointType == TagType.CommunicationReserved || p.PointType == TagType.AlarmReserved) && p.Cage == lastCage.Index)
                        .ToList();
                    
                    int startSlotIndex;
                    if (existingReservedPoints.Any())
                    {
                        // 如果已有预留信号，从最小Slot索引-1继续往前分配
                        int minSlot = existingReservedPoints.Min(p => p.Slot);
                        startSlotIndex = minSlot - 1;
                        allocationReport.AppendLine($"检测到 {existingReservedPoints.Count} 个已有预留信号，最小插槽索引：{minSlot}");
                        allocationReport.AppendLine($"新预留信号将从插槽 {startSlotIndex} 继续往前分配");
                    }
                    else
                    {
                        // 如果没有预留信号，从最后一个插槽开始往前分配
                        startSlotIndex = slotsInLastCage.Count - 1;
                        allocationReport.AppendLine($"没有检测到已有预留信号");
                        allocationReport.AppendLine($"新预留信号将从最后一个插槽（索引 {startSlotIndex}）开始往前分配");
                    }
                    
                    allocationReport.AppendLine();
                    
                    // 生成并分配预留信号（统一往前分配）
                    for (int i = 0; i < reservedConfig.SlotConfigs.Count; i++)
                    {
                        int targetSlotIndex = startSlotIndex - i;
                        
                        if (targetSlotIndex < 0 || targetSlotIndex >= slotsInLastCage.Count)
                        {
                            allocationReport.AppendLine($"  ✗ 插槽索引 {targetSlotIndex} 超出范围，跳过");
                            continue;
                        }
                        
                        var targetSlot = slotsInLastCage[targetSlotIndex];
                        var slotConfig = reservedConfig.SlotConfigs[i];
                        
                        // 根据预留目的创建不同的预留信号
                        string signalName, description, ioType, remarks;
                        TagType pointType;
                        
                        if (slotConfig.ReservedPurpose == ReservedPurpose.报警预留)
                        {
                            // 报警预留 - 也创建虚拟信号，只不过内容是报警
                            signalName = slotConfig.SelectedCardType switch
                            {
                                "MD211" => "Alarm_RTU",
                                "MD216" => "Alarm_TCP",
                                "DP211" => "Alarm_DP",
                                _ => "Alarm_Reserved"
                            };
                            description = $"报警预留_{signalName}";
                            ioType = "MD"; // 统一使用MD类型
                            remarks = "预留报警板卡";
                            pointType = TagType.AlarmReserved; // 使用报警预留类型
                        }
                        else
                        {
                            // 通讯预留
                            signalName = slotConfig.SelectedCardType switch
                            {
                                "MD211" => "RTU",
                                "MD216" => "TCP",
                                "DP211" => "DP",
                                _ => "COMM"
                            };
                            description = $"通讯预留_{signalName}";
                            ioType = "MD";
                            remarks = "预留通讯板卡";
                            pointType = TagType.CommunicationReserved;
                        }
                        
                        var reservedSignal = new IoFullData
                        {
                            CabinetNumber = cabinet.Name,
                            SystemCode = signalName,
                            Description = description,
                            CardType = slotConfig.SelectedCardType,
                            IoType = ioType,
                            ElectricalCharacteristics = "--",
                            PowerType = "--",
                            PointType = pointType,
                            Cage = lastCage.Index,
                            Slot = targetSlot.Index,
                            Channel = 1,
                            Remarks = remarks
                        };
                        
                        // 在插槽中创建预留板卡
                        var config = configs.FirstOrDefault(c => c.IoCardType == slotConfig.SelectedCardType);
                        if (config == null)
                        {
                            reservedSignal.UnsetReason = $"未找到板卡类型配置：{slotConfig.SelectedCardType}";
                            cabinet.UnsetPoints.Add(reservedSignal);
                            allocationReport.AppendLine($"  ✗ 插槽 {targetSlot.Index}: {reservedSignal.UnsetReason}");
                            continue;
                        }
                        
                        if (targetSlot.Board != null)
                        {
                            allocationReport.AppendLine($"  警告：插槽 {targetSlot.Index} 已有板卡，跳过");
                            continue;
                        }
                        
                        // 根据板卡类型创建通讯板卡（带com端口结构）
                        if (slotConfig.SelectedCardType == "MD211" || slotConfig.SelectedCardType == "MD216" || slotConfig.SelectedCardType == "DP211")
                        {
                            // 创建通讯板卡（类似FF总线箱，但是com端口结构）
                            targetSlot.Board = Board.CreateCommunication(config);
                            
                            // 将信号放入第一个端口的第一个虚拟信号槽
                            var firstPort = targetSlot.Board.CommPorts.FirstOrDefault();
                            if (firstPort != null && firstPort.VirtualSignals.Count > 0)
                            {
                                var firstVirtualSignal = firstPort.VirtualSignals[0];
                                firstVirtualSignal.Signal = reservedSignal;
                            }
                        }
                        else
                        {
                            // 其他类型板卡使用普通创建方式
                            targetSlot.Board = Board.Create(config);
                            var channel = targetSlot.Board.Channels[0];
                            channel.Point = reservedSignal;
                        }
                        
                        reservedCount++;
                        
                        string purposeText = slotConfig.ReservedPurpose == ReservedPurpose.报警预留 ? "报警预留" : "通讯预留";
                        allocationReport.AppendLine($"  ✓ 插槽 {targetSlot.Index} (机笼{lastCage.Index}): {slotConfig.SelectedCardType} - {purposeText}");
                    }
                    
                    allocationReport.AppendLine($"预留插槽占位完成，成功 {reservedCount}/{reservedConfig.SlotConfigs.Count} 个");
                    allocationReport.AppendLine();
                }
            }
            
            var unsetTags = cabinet.UnsetPoints.Where(u => u.PointType == TagType.Normal).ToList();
            var alarmTags = cabinet.UnsetPoints.Where(u => u.PointType == TagType.Alarm).ToList();

            // 记录信号分类统计
            int stepNumber = (reservedConfig != null && reservedConfig.IsSelected && reservedConfig.SlotConfigs.Any()) ? 3 : 2;
            allocationReport.AppendLine($"【步骤{stepNumber}：信号分类统计】");
            allocationReport.AppendLine($"普通信号：{unsetTags.Count} 个");
            allocationReport.AppendLine($"报警信号：{alarmTags.Count} 个");
            allocationReport.AppendLine();
            
            // 【修改】合并FF总线箱和FF从站箱为一个处理逻辑，一个箱子分配到一个网段
            var allFF = unsetTags.Where(c => c.IoType.ToUpper().Contains("FF")).ToList();
            var lff = allFF; // 所有FF信号统一处理

            var lboxB = unsetTags.Where(c => c.IoType.ToUpper().Contains("PROFIBUS")).ToList();//B类阀箱
            var lleft = unsetTags.Except(allFF).Except(lboxB).ToList();//硬接线
            var lnormal = lleft.Where(l => string.IsNullOrEmpty(l.LocalBoxNumber));//正常的硬接线点
            var lboxA = lleft.Where(l => !string.IsNullOrEmpty(l.LocalBoxNumber));//有就地箱号的硬接线点
            
            // 记录各类信号数量
            stepNumber++;
            allocationReport.AppendLine($"【步骤{stepNumber}：信号类型分组】");
            allocationReport.AppendLine($"FF总线信号：{lff.Count} 个");
            allocationReport.AppendLine($"B类阀箱(PROFIBUS)信号：{lboxB.Count} 个");
            allocationReport.AppendLine($"硬接线信号：{lleft.Count} 个");
            allocationReport.AppendLine($"  - 普通硬接线：{lnormal.Count()} 个");
            allocationReport.AppendLine($"  - A类阀箱（有就地箱号）：{lboxA.Count()} 个");
            allocationReport.AppendLine();
            
            cabinet.UnsetPoints.Clear();

            #region 硬接点
            // 2.根据IO类型对机柜内的IO点进行分组和排序
            stepNumber++;
            allocationReport.AppendLine($"【步骤{stepNumber}：硬接线信号分配】");
            int hardwiredCount = 0;
            var orderedIOTypes = lnormal.GroupBy(t => t.IoType).OrderBy(g => GetIOTypeOrder(g.Key));
            foreach (var ioType in orderedIOTypes)
            {
                // 3.根据IO卡型号对IO点进行进一步分组
                var cardGroups = ioType.GroupBy(i => i.CardType);
                foreach (var card in cardGroups)
                {
                    //4.相同供电类型的分到一组
                    var powerTypeGroups = card.GroupBy(tag => GetGroupName(tag.PowerType));
                    var config = configs.FirstOrDefault(c => c.IoCardType == card.Key);
                    if (config == null)
                        throw new Exception($"IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：{card.Key}的板卡，请手动添加后再进行分配");
                    foreach (var powerTypeGroup in powerTypeGroups)
                    {
                        int groupCount = powerTypeGroup.Count();
                        hardwiredCount += groupCount;
                        allocationReport.AppendLine($"  → {ioType.Key} | {card.Key} | {powerTypeGroup.Key}：{groupCount} 个信号");
                        AllocateTagToSameTypeCard(cabinet, card.Key, powerTypeGroup.Key, [.. powerTypeGroup], config, rate);
                    }
                }
            }
            allocationReport.AppendLine($"硬接线信号分配完成，共 {hardwiredCount} 个");
            allocationReport.AppendLine();
            #endregion

            #region A类阀箱
            allocationReport.AppendLine("【步骤5：A类阀箱信号分配】");
            int boxACount = 0;
            var orderedIOTypes2 = lboxA.GroupBy(t => t.IoType).OrderBy(g => GetIOTypeOrder(g.Key));
            foreach (var item in orderedIOTypes2)
            {
                var boxGroups = item.GroupBy(i => i.LocalBoxNumber);
                foreach (var box in boxGroups)
                {
                    string cardType = box.ToList().FirstOrDefault().CardType;
                    var config = configs.FirstOrDefault(c => c.IoCardType == cardType);
                    if (config == null)
                        throw new Exception($"IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：{cardType}的板卡，请手动添加后再进行分配");
                    int groupCount = box.Count();
                    boxACount += groupCount;
                    allocationReport.AppendLine($"  → 箱号 [{box.Key}] | {item.Key} | {cardType}：{groupCount} 个信号");
                    AllocateToCardForBoxA(cabinet, cardType, box.ToList(), box.Key, config, rate);
                }
            }
            allocationReport.AppendLine($"A类阀箱信号分配完成，共 {boxACount} 个");
            allocationReport.AppendLine();
            #endregion

            #region B类阀箱
            allocationReport.AppendLine("【步骤6：B类阀箱(PROFIBUS)信号分配】");
            int boxBCount = 0;
            var boxes = lboxB.GroupBy(f => f.LocalBoxNumber);
            foreach (var box in boxes)
            {
                string cardType = box.ToList().FirstOrDefault().CardType;
                var config = configs.FirstOrDefault(c => c.IoCardType == cardType);
                if (config == null)
                    throw new Exception($"IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：{cardType}的板卡，请手动添加后再进行分配");
                int groupCount = box.Count();
                boxBCount += groupCount;
                allocationReport.AppendLine($"  → 箱号 [{box.Key}] | {cardType}：{groupCount} 个信号");
                AllocateToCard(cabinet, cardType, box.ToList(), box.Key, config, rate);
            }
            allocationReport.AppendLine($"B类阀箱信号分配完成，共 {boxBCount} 个");
            allocationReport.AppendLine();
            #endregion

            #region FF总线箱（统一处理）
            // 【修改】合并FF总线箱和FF从站箱的处理逻辑，统一使用AllocateToCardFF方法
            allocationReport.AppendLine("【步骤7：FF板卡分配】");
            var ffStations = lff.GroupBy(f => f.LocalBoxNumber).OrderBy(f => f.Key);//先根据就地箱号分组
            allocationReport.AppendLine($"找到 {ffStations.Count()} 个FF就地箱");
            allocationReport.AppendLine();
            
            List<List<IoFullData>> xt2IoSubstations = new List<List<IoFullData>>();//分站清单
            foreach (var ffStation in ffStations)
            {
                string cardType = ffStation.ToList().FirstOrDefault().CardType;//卡件类型
                var config = configs.FirstOrDefault(c => c.IoCardType == cardType);
                if (config == null)
                    throw new Exception($"IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：{cardType}的板卡，请手动添加后再进行分配");
                var list = ffStation.ToList();
                
                // 【修改】统一使用FF总线分配方法，不再区分FF7/FF8和其他FF类型
                AllocateToCardFF(cabinet, cardType, ffStation.ToList(), ffStation.Key, config, rate);
            }
            #endregion

            // 【注释】原来单独的FF从站箱处理逻辑，现在已合并到上面的统一处理中
            /*
            #region FF从站箱(FF7/FF8)
            // FF从站箱按独立分配逻辑处理，每个从站分配到独立的板卡
            var ffSlaveStations = lffSlaveBox.GroupBy(f => f.LocalBoxNumber).OrderBy(f => f.Key);
            foreach (var ffSlaveStation in ffSlaveStations)
            {
                string cardType = ffSlaveStation.ToList().FirstOrDefault().CardType;
                var config = configs.FirstOrDefault(c => c.IoCardType == cardType);
                if (config == null)
                    throw new Exception($"IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：{cardType}的板卡，请手动添加后再进行分配");
                var orderedfflist = ffSlaveStation.ToList().OrderBy(f => f.PowerType).ToList();
                AllocateToCardForFFSlave(cabinet, cardType, orderedfflist, ffSlaveStation.Key, config, rate);
            }
            #endregion
            */

            #region 最后分报警点
            allocationReport.AppendLine("【步骤8：报警信号分配】");
            int alarmCount = 0;
            if (alarmTags != null && alarmTags.Count > 0)
            {
                //分组报警点
                var groupAlarm = alarmTags.GroupBy(a => a.CardType);
                foreach (var card in groupAlarm)
                {

                    //判断有没有插槽，如果有，新建板卡，如果没有，往历史板卡里边放
                    bool hasEmptySlot = cabinet.Cages.Any(cage => cage.Slots.Any(slot => slot.Board == null));
                    var firstSpareSlot = cabinet.Cages.SelectMany(cage => cage.Slots.Select(slot => slot.Board)).Where(b => b == null);
                    if (hasEmptySlot)
                    {
                        var config = configs.FirstOrDefault(c => c.IoCardType == card.Key);
                        var newCard = Board.Create(config);
                        SetBoard(cabinet, newCard);
                        int cardAlarmCount = 0;
                        foreach (var tag in card.ToList())
                        {
                            var channel = newCard.Channels.FirstOrDefault(c => c.Point == null);
                            if (channel != null)
                            {
                                channel.Point = tag;
                                cardAlarmCount++;
                            }
                            else
                            { 
                                PlacePointToUnset(cabinet, tag, $"报警信号数量超出板卡通道数{newCard.Channels.Count}"); 
                            }
                        }
                        alarmCount += cardAlarmCount;
                        allocationReport.AppendLine($"  → 新建板卡 {card.Key}：{cardAlarmCount} 个报警信号");
                    }
                    else
                    {
                        string powerTypeGroup = GetGroupName(card.FirstOrDefault().PowerType);
                        var config = configs.FirstOrDefault(c => c.IoCardType == card.Key);
                        if (config == null)
                            throw new Exception($"IO分配遇到问题，未在配置表中找到{card.Key}，请手动添加后再进行分配");
                        int groupCount = card.Count();
                        alarmCount += groupCount;
                        allocationReport.AppendLine($"  → 复用板卡 {card.Key}：{groupCount} 个报警信号");
                        AllocateTagToSameTypeCard(cabinet, card.Key, powerTypeGroup, card.ToList(), config, rate);
                    }
                }
            }
            allocationReport.AppendLine($"报警信号分配完成，共 {alarmCount} 个");
            allocationReport.AppendLine();

            #endregion

            // 记录未分配信号统计（排除已成功分配的信号）
            // 已成功分配的信号：NetType不为空且UnsetReason为空
            var actualUnsetPoints = cabinet.UnsetPoints
                .Where(p => string.IsNullOrEmpty(p.NetType) || !string.IsNullOrEmpty(p.UnsetReason))
                .ToList();
            
            if (actualUnsetPoints.Count > 0)
            {
                allocationReport.AppendLine("\n【未分配信号统计】");
                allocationReport.AppendLine($"共 {actualUnsetPoints.Count} 个信号未能分配");
                
                // 按未分配原因分组统计
                var reasonGroups = actualUnsetPoints
                    .GroupBy(p => p.UnsetReason ?? "未知原因")
                    .OrderByDescending(g => g.Count());
                
                allocationReport.AppendLine("\n未分配原因明细：");
                foreach (var group in reasonGroups)
                {
                    allocationReport.AppendLine($"\n  【{group.Key}】共 {group.Count()} 个");
                    
                    // 列出每个信号的详细信息
                    int index = 1;
                    foreach (var signal in group.OrderBy(s => s.TagName))
                    {
                        allocationReport.AppendLine($"    {index}. 位号: {signal.TagName ?? "--"} | IO类型: {signal.IoType ?? "--"} | 板卡类型: {signal.CardType ?? "--"}");
                        if (!string.IsNullOrEmpty(signal.LocalBoxNumber))
                        {
                            allocationReport.AppendLine($"       就地箱号: {signal.LocalBoxNumber}");
                        }
                        index++;
                    }
                }
            }
            else
            {
                allocationReport.AppendLine("\n【分配结果】所有信号均已成功分配！");
            }

            return cabinet;
        }
        
        /// <summary>
        /// 获取IO分配报告
        /// </summary>
        public string GetAllocationReport()
        {
            if (allocationReport.Length == 0)
            {
                return "暂无分配报告";
            }
            
            allocationReport.AppendLine();
            allocationReport.AppendLine("========== 分配完成 ==========");
            allocationReport.AppendLine($"结束时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            return allocationReport.ToString();
        }
        public void ReassignPoints(IEnumerable<Board> boards)
        {
            if (boards == null || boards.Count() == 0)
                return;
            foreach (var board in boards)
            {
                // 收集并逆序Net1和Net2的点
                var net1Points = board.Channels
                    .Where(c => c.Point != null && c.Point.NetType == Xt2NetType.Net1.ToString())
                    .Select(c => c.Point)
                    .ToList();

                var net2Points = board.Channels
                    .Where(c => c.Point != null && c.Point.NetType == Xt2NetType.Net2.ToString())
                    .Reverse()  // 逆序收集
                    .Select(c => c.Point)
                    .ToList();

                // 合并两个列表
                var allPoints = net1Points.Concat(net2Points).ToList();

                // 将合并后的点列表重新赋值到channels
                int pointIndex = 0;
                foreach (var channel in board.Channels)
                {
                    if (pointIndex < allPoints.Count)
                    {
                        channel.Point = allPoints[pointIndex++];
                    }
                    else
                    {
                        channel.Point = null;  // 超出已有点的通道设置为空
                    }
                }
            }
        }

        private void AllocateTagToSameTypeCard(StdCabinet xt2structure, string cardType, string powerTypeValue, List<IoFullData> powerTypeTags, config_card_type_judge config, double rate)
        {
            foreach (var group in powerTypeTags.GroupBy(tag => tag.SignalPositionNumber))
            {
                //信号位号是一组
                List<IoFullData> tags = group.ToList();

                // 尝试在现有的板卡中分配点
                bool isAllocated = false;
                var totalBoards = xt2structure.Cages.SelectMany(cage => cage.Slots.Select(slot => slot.Board))
                                               .Where(board => board != null && board.Type == cardType)
                                               .Union(xt2structure.VirtualSlots.Where(vs => vs.Board != null && vs.Board.Type == cardType).Select(vs => vs.Board));
                foreach (var card in totalBoards)
                {
                    // 计算可用通道数，考虑冗余率                   
                    int availableChannels = card.Channels.Count - (int)Math.Ceiling(card.Channels.Count * rate) - card.Channels.Count(c => c.Point != null);
                    // 获取当前卡上的所有点的详细信息
                    var pointsOnCard = card.Channels.Where(c => c.Point != null).Select(c => c.Point).ToList();

                    //检查是否相同供电组
                    bool isSameGroup = true;
                    if (pointsOnCard.Any() && tags.Any())
                    {
                        // 获取板卡上的第一个点的供电类型，假设板卡上所有点的供电类型都是相同的
                        string cardPowerType = pointsOnCard.First().PowerType;
                        // 找到板卡上点的供电类型对应的分组
                        string cardPower = powerSupplyGrouping.FirstOrDefault(x => x.Key == cardPowerType).Value;
                        // 确认stationTags中的所有点是否与板卡上的点属于同一供电分组                        
                        isSameGroup = cardPower == powerTypeValue;
                    }

                    if (isSameGroup && availableChannels >= tags.Count)
                    {
                        foreach (var tag in group)
                        {
                            var channel = card.Channels.FirstOrDefault(c => c.Point == null);
                            if (channel != null)
                                channel.Point = tag;
                        }
                        isAllocated = true;
                        break;
                    }
                }
                // 如果没有分配，则创建并分配到新的板卡
                if (!isAllocated)
                {
                    var newCard = Board.Create(config);
                    SetBoard(xt2structure, newCard);

                    foreach (var tag in group)
                    {
                        var channel = newCard.Channels.FirstOrDefault(c => c.Point == null);
                        if (channel != null)
                            channel.Point = tag;
                        else
                        { PlacePointToUnset(xt2structure, tag, $"硬接点点数量超出{newCard.Channels.Count}"); }
                    }
                }
            }
        }

        /// <summary>
        /// 【已废弃】FF从站模块独立分配方法
        /// 【注释原因】现在已合并到AllocateToCardFF方法中，不再单独处理FF7/FF8从站模块
        /// 原为为FF7和FF8从站模块提供板卡分配逻辑，保持FF模块的网段分配特性
        /// FF从站模块之间可以复用板卡，但不与FF总线模块（FF1-FF6）复用同一块板卡
        /// 适用于从站系统的板卡分配、网段管理、资源优化配置
        /// </summary>
        /// <param name="xt2structure">机柜结构对象，包含机笼、插槽和板卡信息</param>
        /// <param name="cardType">板卡类型，用于确定板卡的类型和规格</param>
        /// <param name="stationTags">从站模块的IO数据列表，包含所有需要分配的信号点</param>
        /// <param name="stationNumber">从站编号，用于标识和管理从站设备</param>
        /// <param name="config">板卡配置信息，包含通道数量等参数</param>
        /// <param name="rate">冗余率，用于计算板卡可用通道数</param>
        /// <exception cref="Exception">当从站模块信号数量超出板卡网段通道数时抛出异常</exception>
        [Obsolete("该方法已废弃，现在使用AllocateToCardFF方法统一处理所有FF模块", true)]
        private void AllocateToCardForFFSlave(StdCabinet xt2structure, string cardType, List<IoFullData> stationTags, string stationNumber, config_card_type_judge config, double rate)
        {
            bool isAllocated = false;

            // 查找现有的FF从站模块板卡（只复用FF从站模块创建的板卡，不与FF总线模块复用）
            var existingFFSlaveBoards = xt2structure.Cages.SelectMany(cage => cage.Slots.Select(slot => slot.Board))
                .Where(board => board != null && board.Type == cardType && IsFFSlaveBoard(board))
                .Union(xt2structure.VirtualSlots.Where(vs => vs.Board != null && vs.Board.Type == cardType && IsFFSlaveBoard(vs.Board)).Select(vs => vs.Board));

            foreach (var card in existingFFSlaveBoards)
            {
                // 按照FF模块逻辑，将板卡分为前后两个网段
                int halfPointCount = card.Channels.Count / 2;
                var frontChannels = card.Channels.Take(halfPointCount).ToList();//前一半通道(Net1)
                var backChannels = card.Channels.Skip(halfPointCount).ToList();//后一半通道(Net2)

                int availableChannels1 = frontChannels.Count - (int)Math.Ceiling(frontChannels.Count * rate) - frontChannels.Count(c => c.Point != null);
                int availableChannels2 = backChannels.Count - (int)Math.Ceiling(backChannels.Count * rate) - backChannels.Count(c => c.Point != null);

                var pointsOnCard1 = frontChannels.Where(c => c.Point != null).Select(c => c.Point).ToList();
                var pointsOnCard2 = backChannels.Where(c => c.Point != null).Select(c => c.Point).ToList();

                // 检查是否存在串联关系
                bool isSeriesConnectNet1 = pointsOnCard1.Any(p => p != null && p.NetType == Xt2NetType.Net1.ToString() &&
                                                       !string.IsNullOrEmpty(p.Remarks) && (p.Remarks.Contains("串") || (!string.IsNullOrEmpty(stationNumber) && p.Remarks.Contains(stationNumber))));
                bool isSeriesConnectNet2 = pointsOnCard2.Any(p => p != null && p.NetType == Xt2NetType.Net2.ToString() &&
                                                                      !string.IsNullOrEmpty(p.Remarks) && (p.Remarks.Contains("串") || (!string.IsNullOrEmpty(stationNumber) && p.Remarks.Contains(stationNumber))));

                // 优先分配到Net1网段
                if (isSeriesConnectNet1 && availableChannels1 >= stationTags.Count)
                {
                    foreach (var tag in stationTags)
                    {
                        tag.NetType = Xt2NetType.Net1.ToString();
                        var channel = frontChannels.FirstOrDefault(c => c.Point == null);
                        if (channel != null)
                            channel.Point = tag;
                    }
                    isAllocated = true;
                    break;
                }
                // 如果Net1不可用或容量不够，尝试分配到Net2
                else if ((backChannels.All(p => p.Point == null) || isSeriesConnectNet2) && availableChannels2 >= stationTags.Count)
                {
                    foreach (var tag in stationTags)
                    {
                        tag.NetType = Xt2NetType.Net2.ToString();
                        var channel = backChannels.FirstOrDefault(c => c.Point == null);
                        if (channel != null)
                            channel.Point = tag;
                    }
                    isAllocated = true;
                    break;
                }
            }

            // 如果没有分配成功，创建新的FF从站板卡
            if (!isAllocated)
            {
                var newCard = Board.Create(config);
                SetBoard(xt2structure, newCard);

                //// 标记为FF从站板卡（通过在第一个通道上放置特殊标记）
                //MarkAsFFSlaveBoard(newCard);

                // 按照FF模块逻辑，将板卡分为前后两个网段
                int halfPointCount = newCard.Channels.Count / 2;
                var frontChannels = newCard.Channels.Take(halfPointCount).ToList();//前一半通道(Net1)

                // 默认分配到Net1网段
                foreach (var tag in stationTags)
                {
                    tag.NetType = Xt2NetType.Net1.ToString();
                    var channel = frontChannels.FirstOrDefault(c => c.Point == null);
                    if (channel != null)
                    {
                        channel.Point = tag;
                    }
                    else
                    {
                        PlacePointToUnset(xt2structure, tag, $"FF从站模块{stationNumber}网段1数量超出{halfPointCount}");
                    }
                }
            }
        }

        /// <summary>
        /// 【已废弃】判断板卡是否为FF从站模块创建的板卡
        /// 【注释原因】现在所有FF模块统一处理，不再区分FF从站和FF总线板卡
        /// 原通过检查板卡上的信号点的供电类型来判断是否为FF从站板卡
        /// 适用于板卡复用判断、资源管理、系统分类
        /// </summary>
        /// <param name="board">需要判断的板卡对象</param>
        /// <returns>如果是FF从站板卡返回true，否则返回false</returns>
        [Obsolete("该方法已废弃，现在所有FF模块统一处理不再区分类型", true)]
        private bool IsFFSlaveBoard(Board board)
        {
            // 检查板卡上是否有FF从站模块的信号点（供电类型为FF7或FF8）
            return board.Channels.Any(c => c.Point != null &&
                                          c.Point.PowerType != null &&
                                          (c.Point.PowerType.Contains("FF7") || c.Point.PowerType.Contains("FF8")));
        }
        
        private void AllocateToCardForBoxA(StdCabinet xt2structure, string cardType, List<IoFullData> stationTags, string stationNumber, config_card_type_judge config, double rate)
        {
            bool isAllocated = false;
            var totalBoards = xt2structure.Cages.SelectMany(cage => cage.Slots.Select(slot => slot.Board))
                                             .Where(board => board != null && board.Type == cardType)
                                             .Union(xt2structure.VirtualSlots.Where(vs => vs.Board != null && vs.Board.Type == cardType).Select(vs => vs.Board));

            foreach (var card in totalBoards)
            {
                // 计算可用通道数，考虑冗余率
                int availableChannels = card.Channels.Count - (int)Math.Ceiling(card.Channels.Count * rate) - card.Channels.Count(c => c.Point != null);

                // 获取当前卡上的所有点的详细信息
                var pointsOnCard = card.Channels.Where(c => c.Point != null).Select(c => c.Point).ToList();

                //检查是否相同供电组
                bool isSameGroup = true;
                if (pointsOnCard.Any() && stationTags.Any())
                {
                    // 获取板卡上的第一个点的供电类型，假设板卡上所有点的供电类型都是相同的
                    string cardPowerType = pointsOnCard.First().PowerType;
                    // 找到板卡上点的供电类型对应的分组
                    string cardGroup = powerSupplyGrouping.FirstOrDefault(x => x.Key == cardPowerType).Value;
                    // 确认stationTags中的所有点是否与板卡上的点属于同一供电分组
                    isSameGroup = stationTags.All(tag =>
                    {
                        string tagGroupKey = powerSupplyGrouping.FirstOrDefault(x => x.Key == tag.PowerType).Value;
                        return tagGroupKey == cardGroup;
                    });
                }

                if (isSameGroup && availableChannels >= stationTags.Count)
                {
                    // 如果存在串联关系并且可用通道足够，将点分配给板卡
                    foreach (var tag in stationTags)
                    {
                        var channel = card.Channels.FirstOrDefault(c => c.Point == null);
                        if (channel != null)
                            channel.Point = tag;
                    }
                    isAllocated = true;
                    break; // 分配后退出循环
                }
            }
            if (!isAllocated)
            {
                // 创建新板卡并添加到机柜
                var newCard = Board.Create(config);
                SetBoard(xt2structure, newCard);

                // 分配点到新板卡的通道，不再考虑冗余率，因为是新板卡
                foreach (var tag in stationTags)
                {
                    var channel = newCard.Channels.FirstOrDefault(c => c.Point == null);
                    if (channel != null)
                        channel.Point = tag;
                    else
                    { PlacePointToUnset(xt2structure, tag, $"A类阀箱点数量超出{newCard.Channels.Count}"); }
                }
            }
        }
        private void AllocateToCard(StdCabinet xt2structure, string cardType, List<IoFullData> stationTags, string stationNumber, config_card_type_judge config, double rate)
        {
            bool isAllocated = false;
            var totalBoards = xt2structure.Cages.SelectMany(cage => cage.Slots.Select(slot => slot.Board))
                                             .Where(board => board != null && board.Type == cardType)
                                             .Union(xt2structure.VirtualSlots.Where(vs => vs.Board != null && vs.Board.Type == cardType).Select(vs => vs.Board));

            foreach (var card in totalBoards)
            {
                // 计算可用通道数，考虑冗余率
                int availableChannels = card.Channels.Count - (int)Math.Ceiling(card.Channels.Count * rate) - card.Channels.Count(c => c.Point != null);

                // 获取当前卡上的所有点的详细信息
                var pointsOnCard = card.Channels.Where(c => c.Point != null).Select(c => c.Point).ToList();

                // 检查是否存在“串联”关系
                bool isSeriesConnect = pointsOnCard.Any(p => p != null && !string.IsNullOrEmpty(p.Remarks) && (p.Remarks.Contains("串联") || p.Remarks.Contains(stationNumber)))
                    || stationTags.Any(tag => !string.IsNullOrEmpty(tag.Remarks) && (tag.Remarks.Contains("串联") || pointsOnCard.Any(p => p != null && tag.Remarks.Contains(p.StationNumber))));

                if (isSeriesConnect && availableChannels >= stationTags.Count)
                {
                    // 如果存在串联关系并且可用通道足够，将点分配给板卡
                    foreach (var tag in stationTags)
                    {
                        var channel = card.Channels.FirstOrDefault(c => c.Point == null);
                        if (channel != null)
                            channel.Point = tag;
                    }
                    isAllocated = true;
                    break; // 分配后退出循环
                }
            }

            if (!isAllocated)
            {
                // 创建新板卡并添加到机柜
                var newCard = Board.Create(config);
                SetBoard(xt2structure, newCard);

                // 分配点到新板卡的通道，不再考虑冗余率，因为是新板卡
                foreach (var tag in stationTags)
                {
                    var channel = newCard.Channels.FirstOrDefault(c => c.Point == null);
                    if (channel != null)
                        channel.Point = tag;
                    else
                    { PlacePointToUnset(xt2structure, tag, $"B类阀箱点数量超出{newCard.Channels.Count}"); }
                }
            }
        }
        /// <summary>
        /// FF模块统一分配方法
        /// 【修改】为所有FF模块提供统一的板卡分配逻辑，包括FF总线模块（FF1-FF6）和FF从站模块（FF7-FF8）
        /// 支持FF模块之间的板卡复用，一个箱子分配到一个网段上
        /// 适用于FF总线系统的板卡分配、网段管理、资源优化配置
        /// </summary>
        /// <param name="xt2structure">机柜结构对象，包含机笼、插槽和板卡信息</param>
        /// <param name="cardType">板卡类型，用于确定板卡的类型和规格</param>
        /// <param name="stationTags">FF模块的IO数据列表，包含所有需要分配的信号点</param>
        /// <param name="stationNumber">站号，用于标识和管理FF设备</param>
        /// <param name="config">板卡配置信息，包含通道数量等参数</param>
        /// <param name="rate">冗余率，用于计算板卡可用通道数</param>
        private void AllocateToCardFF(StdCabinet xt2structure, string cardType, List<IoFullData> stationTags, string stationNumber, config_card_type_judge config, double rate)
        {
            bool isAllocated = false;
            
            // 先判断当前信号组应该创建哪种类型的FF板卡
            bool isFFSlave = stationTags.Any(tag => 
                !string.IsNullOrEmpty(tag.IoType) &&
                tag.IoType.Contains("FF", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(tag.FFSlaveModuleModel));
            
            BoardType targetBoardType = isFFSlave ? BoardType.FFSlave : BoardType.FFBus;
            
            // 获取当前箱子的箱号
            var currentBoxNumber = stationTags.FirstOrDefault()?.LocalBoxNumber ?? "";
            
            // 记录分配开始
            allocationReport.AppendLine($"  → 箱号 {currentBoxNumber}（{(isFFSlave ? "FF从站" : "FF总线")}，{stationTags.Count} 个信号）");
            
            // 第1步：尝试复用已有板卡（只复用相同FFBoardType的板卡）
            var existingFFBoards = xt2structure.Cages.SelectMany(cage => cage.Slots.Select(slot => slot.Board))
                                            .Where(board => board != null && board.Type == cardType && board.FFBoardType == targetBoardType)
                                            .Union(xt2structure.VirtualSlots.Where(vs => vs.Board != null && vs.Board.Type == cardType && vs.Board.FFBoardType == targetBoardType).Select(vs => vs.Board));

            foreach (var card in existingFFBoards)
            {
                // 确保复用的板卡有正确的FFBoardType标志
                if (card.FFBoardType == BoardType.Normal)
                {
                    // 如果板卡的FFBoardType还是默认值，根据当前判断结果设置
                    card.FFBoardType = targetBoardType;
                }
                
                // 获取Net1和Net2网段
                var net1 = card.Networks.FirstOrDefault(n => n.NetworkType == Xt2NetType.Net1);
                var net2 = card.Networks.FirstOrDefault(n => n.NetworkType == Xt2NetType.Net2);
                if (net1 == null || net2 == null)
                    continue;
                
                // ✅ 检查串联关系：判断当前箱子是否与网段1或网段2中的箱子串联
                bool canUseNet1 = false;
                bool canUseNet2 = false;
                
                // 获取网段1和网段2中已有的箱号
                var net1Boxes = (targetBoardType == BoardType.FFSlave ? 
                    net1.UnallocatedSignals : 
                    net1.FFBusChannels.Where(c => c.Point != null).Select(c => c.Point))
                    .Select(p => p.LocalBoxNumber)
                    .Where(b => !string.IsNullOrEmpty(b))
                    .Distinct()
                    .ToList();
                
                var net2Boxes = (targetBoardType == BoardType.FFSlave ? 
                    net2.UnallocatedSignals : 
                    net2.FFBusChannels.Where(c => c.Point != null).Select(c => c.Point))
                    .Select(p => p.LocalBoxNumber)
                    .Where(b => !string.IsNullOrEmpty(b))
                    .Distinct()
                    .ToList();
                
                // ✅ 检查当前箱子是否与网段中的任一箱子串联
                // 逻辑：查找当前箱子的备注，看是否与网段中的箱子之间有"串"字
                var currentRemark = stationTags
                    .Where(s => !string.IsNullOrEmpty(s.Remarks))
                    .Select(s => s.Remarks)
                    .FirstOrDefault() ?? "";
                
                // 检查与Net1中箱子的串联关系
                foreach (var net1Box in net1Boxes)
                {
                    // 检查当前箱号和Net1箱号之间是否有"串"字
                    int currentIndex = currentRemark.IndexOf(currentBoxNumber);
                    int net1Index = currentRemark.IndexOf(net1Box);
                    
                    if (currentIndex >= 0 && net1Index >= 0)
                    {
                        int minIndex = Math.Min(currentIndex, net1Index);
                        int maxIndex = Math.Max(currentIndex, net1Index);
                        string between = currentRemark.Substring(minIndex, maxIndex - minIndex);
                        
                        if (between.Contains("串"))
                        {
                            canUseNet1 = true;
                            break;
                        }
                    }
                }
                
                // 检查与Net2中箱子的串联关系
                if (!canUseNet1)
                {
                    foreach (var net2Box in net2Boxes)
                    {
                        // 检查当前箱号和Net2箱号之间是否有"串"字
                        int currentIndex = currentRemark.IndexOf(currentBoxNumber);
                        int net2Index = currentRemark.IndexOf(net2Box);
                        
                        if (currentIndex >= 0 && net2Index >= 0)
                        {
                            int minIndex = Math.Min(currentIndex, net2Index);
                            int maxIndex = Math.Max(currentIndex, net2Index);
                            string between = currentRemark.Substring(minIndex, maxIndex - minIndex);
                            
                            if (between.Contains("串"))
                            {
                                canUseNet2 = true;
                                break;
                            }
                        }
                    }
                }
                
                // 如果没有串联关系，检查网段是否为空
                if (!canUseNet1 && !canUseNet2)
                {
                    canUseNet1 = net1Boxes.Count == 0;
                    canUseNet2 = !canUseNet1 && net2Boxes.Count == 0;
                }
                
                // 尝试分配到Net1
                if (canUseNet1)
                {
                    if (targetBoardType == BoardType.FFSlave)
                    {
                        // FF从站箱：添加到UnallocatedSignals
                        foreach (var tag in stationTags)
                        {
                            tag.NetType = Xt2NetType.Net1.ToString();
                            net1.UnallocatedSignals.Add(tag);
                        }
                        
                        // 记录分配原因
                        if (net1Boxes.Count > 0)
                        {
                            allocationReport.AppendLine($"    ✓ 分配到Net1（与箱号 {string.Join(", ", net1Boxes)} 串联）");
                        }
                        else
                        {
                            allocationReport.AppendLine($"    ✓ 分配到Net1（首个箱子）");
                        }
                        
                        isAllocated = true;
                        break;
                    }
                    else
                    {
                        // FF总线箱：分配到FFBusChannels
                        var availableChannels = net1.FFBusChannels.Count(c => c.Point == null) - (int)Math.Ceiling(net1.FFBusChannels.Count * rate);
                        if (availableChannels >= stationTags.Count)
                        {
                            foreach (var tag in stationTags)
                            {
                                var channel = net1.FFBusChannels.FirstOrDefault(c => c.Point == null);
                                if (channel != null)
                                {
                                    channel.Point = tag;
                                    tag.NetType = Xt2NetType.Net1.ToString();
                                    tag.Channel = channel.Index;
                                }
                            }
                            
                            // 记录分配原因
                            if (net1Boxes.Count > 0)
                            {
                                allocationReport.AppendLine($"    ✓ 分配到Net1（与箱号 {string.Join(", ", net1Boxes)} 串联）");
                            }
                            else
                            {
                                allocationReport.AppendLine($"    ✓ 分配到Net1（首个箱子）");
                            }
                            
                            isAllocated = true;
                            break;
                        }
                    }
                }
                // 尝试分配到Net2
                else if (canUseNet2)
                {
                    if (targetBoardType == BoardType.FFSlave)
                    {
                        // FF从站箱：添加到UnallocatedSignals
                        foreach (var tag in stationTags)
                        {
                            tag.NetType = Xt2NetType.Net2.ToString();
                            net2.UnallocatedSignals.Add(tag);
                        }
                        
                        // 记录分配原因
                        if (net2Boxes.Count > 0)
                        {
                            allocationReport.AppendLine($"    ✓ 分配到Net2（与箱号 {string.Join(", ", net2Boxes)} 串联）");
                        }
                        else
                        {
                            allocationReport.AppendLine($"    ✓ 分配到Net2（Net1已有其他箱子）");
                        }
                        isAllocated = true;
                        break;
                    }
                    else
                    {
                        // FF总线箱：分配到FFBusChannels
                        var availableChannels = net2.FFBusChannels.Count(c => c.Point == null) - (int)Math.Ceiling(net2.FFBusChannels.Count * rate);
                        if (availableChannels >= stationTags.Count)
                        {
                            foreach (var tag in stationTags)
                            {
                                var channel = net2.FFBusChannels.FirstOrDefault(c => c.Point == null);
                                if (channel != null)
                                {
                                    channel.Point = tag;
                                    tag.NetType = Xt2NetType.Net2.ToString();
                                    tag.Channel = channel.Index;
                                }
                            }
                            
                            // 记录分配原因
                            if (net2Boxes.Count > 0)
                            {
                                allocationReport.AppendLine($"    ✓ 分配到Net2（与箱号 {string.Join(", ", net2Boxes)} 串联）");
                            }
                            else
                            {
                                allocationReport.AppendLine($"    ✓ 分配到Net2（Net1已有其他箱子）");
                            }
                            
                            isAllocated = true;
                            break;
                        }
                    }
                }
            }

            // 第2步：没有可复用板卡，创建新板卡（只分配当前箱子）
            if (!isAllocated)
            {
                Board newCard;

                if (isFFSlave)
                {
                    // 从站箱：创建支持模块的FF从站板卡（双网段，每个网段支持多个模块）
                    newCard = Board.CreateFFSlave(config);
                    
                    // 获取Net1网段，默认分配到Net1
                    var net1 = newCard.Networks.FirstOrDefault(n => n.NetworkType == Xt2NetType.Net1);
                    if (net1 == null)
                        throw new InvalidOperationException("FF从站板卡网段未初始化");
                    
                    // 将当前箱子的信号分配到Net1
                    foreach (var tag in stationTags)
                    {
                        tag.NetType = Xt2NetType.Net1.ToString();
                        net1.UnallocatedSignals.Add(tag);
                    }
                    
                    allocationReport.AppendLine($"    ✓ 创建新FF从站板卡，分配到Net1（无可复用板卡）");
                }
                else
                {
                    // 总线箱：创建双网段的FF总线板卡
                    newCard = Board.CreateFFBus(config);
                    
                    // 获取Net1网段，默认分配到Net1
                    var net1 = newCard.Networks.FirstOrDefault(n => n.NetworkType == Xt2NetType.Net1);
                    if (net1 == null)
                        throw new InvalidOperationException("FF总线板卡网段未初始化");
                    
                    // 将当前箱子的信号分配到Net1的FFBusChannels
                    foreach (var tag in stationTags)
                    {
                        var channel = net1.FFBusChannels.FirstOrDefault(c => c.Point == null);
                        if (channel != null)
                        {
                            channel.Point = tag;
                            tag.NetType = Xt2NetType.Net1.ToString();
                            tag.Channel = channel.Index;
                        }
                        else
                        {
                            PlacePointToUnset(xt2structure, tag, $"网段1数量超出{net1.FFBusChannels.Count}");
                        }
                    }
                    
                    allocationReport.AppendLine($"    ✓ 创建新FF总线板卡，分配到Net1（无可复用板卡）");
                }
                
                SetBoard(xt2structure, newCard);
            }
        }
        public void ClearPointsAndAddToUnset(StdCabinet cabinet)
        {
            int boardCount = 0;
            int signalCount = 0;
            int reservedSlotCount = 0;  // 记录预留插槽数量
            
            // 遍历机柜中的所有机笼、插槽和通道
            foreach (var cage in cabinet.Cages)
            {
                foreach (var slot in cage.Slots)
                {
                    if (slot.Board != null)
                    {
                        // 检查该插槽是否全部是预留信号（通讯预留或报警预留）
                        var allSignals = StdCabinet.GetAllSignals(slot.Board);
                        bool isReservedSlot = allSignals.Any() && allSignals.All(s => s.PointType == TagType.CommunicationReserved || s.PointType == TagType.AlarmReserved);
                        
                        if (isReservedSlot)
                        {
                            // 跳过预留插槽，不清空
                            reservedSlotCount++;
                            continue;
                        }
                        
                        boardCount++;
                        // 收集板卡上的所有信号点
                        foreach (var signal in allSignals)
                        {
                            // 将信号添加到未分配点集合中
                            cabinet.UnsetPoints.Add(signal);
                            signalCount++;
                        }
                        slot.Board = null;//清空板卡
                    }
                }
            }

            // 处理虚拟插槽
            int virtualSlotCount = 0;
            foreach (var virtualSlot in cabinet.VirtualSlots)
            {
                if (virtualSlot.Board != null)
                {
                    virtualSlotCount++;
                    // 收集板卡上的所有信号点
                    var allSignals = StdCabinet.GetAllSignals(virtualSlot.Board);
                    foreach (var signal in allSignals)
                    {
                        // 将信号添加到未分配点集合中
                        cabinet.UnsetPoints.Add(signal);
                        signalCount++;
                    }
                }
            }
            cabinet.VirtualSlots.Clear();//清除虚拟插槽

            // 清空所有信号的分配信息（跳过预留信号）
            int ffSlaveSignalCount = 0;
            int clearedSignalCount = 0;
            foreach (var point in cabinet.UnsetPoints)
            {
                // 🔑 跳过预留信号（通讯预留和报警预留），保持其分配信息不变
                if (point.PointType == TagType.CommunicationReserved || point.PointType == TagType.AlarmReserved)
                {
                    continue;
                }
                
                // 清空IO分配字段
                point.Cage = 0;
                point.Slot = 0;
                point.Channel = 0;
                point.NetType = null;
                point.UnsetReason = null; // 清空未分配原因
                clearedSignalCount++;
                
                // 清空FF从站分配字段（只清空分配结果，保留输入配置FFSlaveModuleModel）
                if (!string.IsNullOrEmpty(point.IoType) &&
                    point.IoType.Contains("FF", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(point.FFSlaveModuleModel))
                {
                    point.FFDPStaionNumber = null;
                    point.FFTerminalChannel = null;
                    point.FFSlaveModuleID = null;
                    point.FFSlaveModuleSignalPositive = null;
                    point.FFSlaveModuleSignalNegative = null;
                    ffSlaveSignalCount++;
                }
            }
            
            // 记录清空统计
            allocationReport.AppendLine($"清除了 {boardCount} 个实体插槽板卡");
            if (reservedSlotCount > 0)
            {
                allocationReport.AppendLine($"✅ 保留了 {reservedSlotCount} 个预留板卡插槽（通讯预留/报警预留，不参与分配）");
            }
            if (virtualSlotCount > 0)
            {
                allocationReport.AppendLine($"清除了 {virtualSlotCount} 个虚拟插槽板卡");
            }
            allocationReport.AppendLine($"清空了 {clearedSignalCount} 个信号的IO分配信息（机笼、插槽、通道、网段类型）");
            if (ffSlaveSignalCount > 0)
            {
                allocationReport.AppendLine($"清空了 {ffSlaveSignalCount} 个FF从站信号的分配结果：");
                allocationReport.AppendLine($"  - FF从站站号（FFDPStaionNumber）");
                allocationReport.AppendLine($"  - FF从站通道号（FFTerminalChannel）");
                allocationReport.AppendLine($"  - FF从站模块ID（FFSlaveModuleID）");
                allocationReport.AppendLine($"  - FF从站模块信号正极（FFSlaveModuleSignalPositive）");
                allocationReport.AppendLine($"  - FF从站模块信号负极（FFSlaveModuleSignalNegative）");
                allocationReport.AppendLine($"  ✓ 保留了FF从站模块配置（FFSlaveModuleModel）");
            }
        }

        public void PlacePointToUnset(StdCabinet cabinet, IoFullData tag, string reason)
        {
            tag.UnsetReason = reason;
            cabinet.UnsetPoints.Add(tag);
        }
        private void SetBoard(StdCabinet cabinetInfo, Board board)
        {
            // 判断板卡类型是否为FF
            bool isFFType = board.Type.Contains("FF");

            // 尝试按顺序轮流选择机笼放入板卡
            for (int i = 0; i < cabinetInfo.Cages.Count; i++)
            {
                var cage = cabinetInfo.Cages[(currentCageIndex + i) % cabinetInfo.Cages.Count];

                foreach (var slot in cage.Slots)
                {
                    // 检查当前插槽是否为空
                    if (slot.Board != null)
                        continue;

                    // 如果是FF板卡，需要保证后一个偶数插槽为空
                    if (isFFType)
                    {
                        // 获取紧邻的后一个插槽的编号（Index，不是数组索引）
                        int nextSlotNumber = slot.Index + 1;
                        
                        // 确保FF板卡只放在奇数插槽
                        if (slot.Index % 2 == 1)
                        {
                            // 查找后一个插槽对象（根据Index编号查找）
                            var nextSlot = cage.Slots.FirstOrDefault(s => s.Index == nextSlotNumber);
                            
                            // 如果后一个插槽不存在或为空，则可以分配
                            if (nextSlot == null || nextSlot.Board == null)
                            {
                                slot.Board = board;
                                currentCageIndex = (currentCageIndex + i + 1) % cabinetInfo.Cages.Count; // 更新当前机笼索引
                                return;
                            }
                        }
                    }
                    else // 如果不是FF板卡，正常放置
                    {
                        slot.Board = board;
                        currentCageIndex = (currentCageIndex + i + 1) % cabinetInfo.Cages.Count; // 更新当前机笼索引
                        return;
                    }
                }
            }

            // 如果所有符合条件的插槽都放满了，则将板卡添加到UnsetBoards集合中    
            cabinetInfo.AddBoardToVirtualSlot(board);
        }
        private int GetIOTypeOrder(string ioType)
        {
            var order = new List<string> { "AI", "PI", "AO", "DI", "DO" };
            return order.IndexOf(ioType);
        }
        private string GetGroupName(string powerType)
        {
            if (string.IsNullOrEmpty(powerType))
            {
                throw new Exception("供电类型有空值，无法分配");
            }
            if (powerSupplyGrouping.ContainsKey(powerType))
            {
                return powerSupplyGrouping[powerType];
            }
            else if (powerSupplyGrouping.Keys.Any(k => powerType.Contains(k)))
            {
                return powerSupplyGrouping[powerSupplyGrouping.Keys.FirstOrDefault(p => powerType.Contains(p))!];
            }
            return "GROUP0";
        }
        #endregion

        #region 龙核IO分配

        public StdCabinet AutoAllocateLongHeIOSingle(StdCabinet acabinet, List<config_card_type_judge> configs, double rate)
        {
            List<IoFullData> fullDatas = acabinet.ToPoint();
            var cabinetInfo = StdCabinet.CreateLH(acabinet.Name); // 创建龙核的机箱与插槽
                                                                  // 排序信号
            var ioTypeOrder = new Dictionary<string, int> { { "AI", 1 }, { "DI", 2 }, { "AO", 3 }, { "DO", 4 } };
            var sortedSignalGroup = fullDatas
                .OrderBy(sg => ioTypeOrder.TryGetValue(sg.IoType, out var order) ? order : int.MaxValue)
                .ToList();

            // 反馈信号的分组及优先分配
            var feedbackKeywords = new List<string> { "KW", "GW", "QF", "TF", "GZ" };
            var groupedSignals = sortedSignalGroup
                         .GroupBy(s => new { s.IoType, s.CardType, s.PowerSupplyMethod, s.VoltageLevel, s.Destination, DevicePrefix = GetDevicePrefix(s.TagName) })
                         .OrderBy(g => ioTypeOrder.TryGetValue(g.Key.IoType, out var order) ? order : int.MaxValue) // 先按 IoType 排序
                         .ThenByDescending(g => g.Any(s => feedbackKeywords.Any(keyword => s.TagName.Contains(keyword)))); // 反馈信号优先
            foreach (var signalGroup in groupedSignals)
            {
                var signals = signalGroup.OrderBy(s => s.TagName).ToList();
                var config = configs.SingleOrDefault(c => c.IoCardType == signalGroup.Key.CardType);

                if (config == null)
                {
                    throw new Exception($"未在配置表中找到{signalGroup.Key.CardType},请配置后再生成");
                }

                // 先尝试分配反馈信号
                var feedbackSignals = signals.Where(s => feedbackKeywords.Any(keyword => s.TagName.Contains(keyword))).ToList();
                var otherSignals = signals.Except(feedbackSignals).ToList();

                if (feedbackSignals.Any())
                {
                    var assigned = cabinetInfo.Cages
                    .SelectMany(c => c.Slots.Select(slot => new { slot, c })) // 包含 Cage 信息
                    .Where(sc => sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot))
                    .Any(sc => AssignGroupToSlot(sc.slot, sc.c, feedbackSignals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel, signalGroup.Key.Destination));

                    if (!assigned)
                    {
                        AssignToNewSlotOrUnset(cabinetInfo, config, feedbackSignals);
                    }
                }


                // 尝试分配同一设备的其他信号，优先放在同一块卡上
                if (otherSignals.Any())
                {
                    var assigned = cabinetInfo.Cages
                         .SelectMany(c => c.Slots.Select(slot => new { slot, c })) // 包含 Cage 信息
                         .Where(sc => sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot))
                         .Any(sc => AssignGroupToSlot(sc.slot, sc.c, otherSignals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel, signalGroup.Key.Destination));

                    if (!assigned)
                    {
                        AssignToNewSlotOrUnset(cabinetInfo, config, otherSignals);
                    }
                }
            }

            // 对剩余未分配的信号进行进一步分配
            if (cabinetInfo.UnsetPoints.Any())
            {
                AssignRemainingSignals(cabinetInfo, rate);
            }

            return cabinetInfo;
        }


        public List<StdCabinet> AutoAllocateLongHeIO(List<IoFullData> fullDatas, List<config_card_type_judge> configs, double rate)
        {
            var cabinets = new List<StdCabinet>();

            foreach (var cabinetGroup in fullDatas.GroupBy(f => f.CabinetNumber)) // 按照机柜分组
            {
                var cabinetInfo = StdCabinet.CreateLH(cabinetGroup.Key); // 创建龙核的机箱与插槽
                cabinets.Add(cabinetInfo);

                // 排序信号
                var ioTypeOrder = new Dictionary<string, int> { { "AI", 1 }, { "DI", 2 }, { "AO", 3 }, { "DO", 4 } };
                var sortedSignalGroup = cabinetGroup
                    .OrderBy(sg => ioTypeOrder.TryGetValue(sg.IoType, out var order) ? order : int.MaxValue)
                    .ToList();

                // 反馈信号的分组及优先分配
                var feedbackKeywords = new List<string> { "KW", "GW", "QF", "TF", "GZ" };
                var groupedSignals = sortedSignalGroup
                             .GroupBy(s => new { s.IoType, s.CardType, s.PowerSupplyMethod, s.VoltageLevel, s.Destination, DevicePrefix = GetDevicePrefix(s.TagName) })
                             .OrderBy(g => ioTypeOrder.TryGetValue(g.Key.IoType, out var order) ? order : int.MaxValue) // 先按 IoType 排序
                             .ThenByDescending(g => g.Any(s => feedbackKeywords.Any(keyword => s.TagName.Contains(keyword)))); // 反馈信号优先


                foreach (var signalGroup in groupedSignals)
                {
                    var signals = signalGroup.OrderBy(s => s.TagName).ToList();
                    var config = configs.SingleOrDefault(c => c.IoCardType == signalGroup.Key.CardType);

                    if (config == null)
                    {
                        throw new Exception($"未在配置表中找到{signalGroup.Key.CardType},请配置后再生成");
                    }

                    // 先尝试分配反馈信号
                    var feedbackSignals = signals.Where(s => feedbackKeywords.Any(keyword => s.TagName.Contains(keyword))).ToList();
                    var otherSignals = signals.Except(feedbackSignals).ToList();

                    if (feedbackSignals.Any())
                    {
                        var assigned = cabinetInfo.Cages
                        .SelectMany(c => c.Slots.Select(slot => new { slot, c })) // 包含 Cage 信息
                        .Where(sc => sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot))
                        .Any(sc => AssignGroupToSlot(sc.slot, sc.c, feedbackSignals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel, signalGroup.Key.Destination));

                        if (!assigned)
                        {
                            AssignToNewSlotOrUnset(cabinetInfo, config, feedbackSignals);
                        }
                    }


                    // 尝试分配同一设备的其他信号，优先放在同一块卡上
                    if (otherSignals.Any())
                    {
                        var assigned = cabinetInfo.Cages
                             .SelectMany(c => c.Slots.Select(slot => new { slot, c })) // 包含 Cage 信息
                             .Where(sc => sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot))
                             .Any(sc => AssignGroupToSlot(sc.slot, sc.c, otherSignals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel, signalGroup.Key.Destination));

                        if (!assigned)
                        {
                            AssignToNewSlotOrUnset(cabinetInfo, config, otherSignals);
                        }
                    }
                }

                // 对剩余未分配的信号进行进一步分配
                if (cabinetInfo.UnsetPoints.Any())
                {
                    AssignRemainingSignals(cabinetInfo, rate);
                }
            }

            return cabinets;
        }

        // 获取设备前缀
        private string GetDevicePrefix(string tagName)
        {
            var parts = tagName.Split('_');
            return parts.Length > 0 ? parts[0] : tagName;
        }

        // 检查是否是最后两个槽位
        private bool IsLastTwoSlots(ChassisInfo cage, SlotInfo slot)
        {
            return (slot.Index == cage.Slots[cage.Slots.Count - 1].Index || slot.Index == cage.Slots[cage.Slots.Count - 2].Index);

        }

        // 分配信号到新的插槽或未分配列表
        private void AssignToNewSlotOrUnset(StdCabinet cabinetInfo, config_card_type_judge config, List<IoFullData> signals)
        {
            var emptySlot = cabinetInfo.Cages
                .SelectMany(c => c.Slots.Select(slot => new { slot, c }))
                .FirstOrDefault(sc => sc.slot.Board == null && !IsLastTwoSlots(sc.c, sc.slot))?.slot;

            if (emptySlot != null)
            {
                emptySlot.Board = Board.Create(config);
                for (int i = 0; i < signals.Count; i++)
                {
                    emptySlot.Board.Channels[i].Point = signals[i];
                }
            }
            else
            {
                cabinetInfo.UnsetPoints.AddRange(signals);
            }
        }

        // 分配剩余信号
        private void AssignRemainingSignals(StdCabinet cabinetInfo, double rate)
        {
            var remainingSignals = cabinetInfo.UnsetPoints.ToList();
            cabinetInfo.UnsetPoints.Clear();

            foreach (var signalGroup in remainingSignals.GroupBy(s => new { s.CardType, s.PowerSupplyMethod, s.VoltageLevel }))
            {
                var signals = signalGroup.OrderBy(s => s.TagName).ToList();

                var assigned = cabinetInfo.Cages
                    .SelectMany(c => c.Slots.Select(slot => new { slot, c }))
                    .Where(sc => sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot))
                    .Any(sc => AssignGroupToSlot(sc.slot, sc.c, signals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel));

                if (!assigned)
                {
                    var emptySlot = cabinetInfo.Cages
                        .SelectMany(c => c.Slots.Select(slot => new { slot, c }))
                        .FirstOrDefault(sc => sc.slot.Board == null && !IsLastTwoSlots(sc.c, sc.slot))?.slot;

                    if (emptySlot != null)
                    {
                        var config = new config_card_type_judge { IoCardType = signalGroup.Key.CardType };
                        emptySlot.Board = Board.Create(config);
                        for (int i = 0; i < signals.Count; i++)
                        {
                            emptySlot.Board.Channels[i].Point = signals[i];
                        }
                    }
                    else
                    {
                        cabinetInfo.UnsetPoints.AddRange(signals);
                    }
                }
            }
        }

        // 检查是否可以将信号分配到当前槽位
        private bool AssignGroupToSlot(SlotInfo slot, ChassisInfo cage, List<IoFullData> signals, double rate, string powerSupplyMethod, string voltageLevel, string dest = null)
        {
            if (slot.Board?.Type != signals.First().CardType)
                return false;

            var usedChannels = slot.Board.Channels.Count(c => c.Point != null);
            var totalChannels = slot.Board.Channels.Count;

            // 获取插槽中已存在的信号的电压等级、供电方式和目的地
            var existingVoltageLevels = slot.Board.Channels.Where(c => c.Point != null).Select(c => c.Point.VoltageLevel).Distinct().ToList();
            var existingPowerSupplyMethods = slot.Board.Channels.Where(c => c.Point != null).Select(c => c.Point.PowerSupplyMethod).Distinct().ToList();
            var existingDestinations = slot.Board.Channels.Where(c => c.Point != null).Select(c => c.Point.Destination).Distinct().ToList();

            // 检查现有信号和新信号之间的电压等级一致性和供电方式一致性
            bool voltageLevelMatches = (existingVoltageLevels.Count == 0 || // 插槽中没有已分配的信号
                                       (existingVoltageLevels.Count == 1 && existingVoltageLevels[0] == voltageLevel) || // 插槽中的信号和新信号的电压等级一致
                                       (existingVoltageLevels.Count == 1 && existingVoltageLevels[0] == null && voltageLevel == null)); // 插槽和新信号的电压等级都为 null

            bool powerSupplyMatches = (existingPowerSupplyMethods.Count == 0 || // 插槽中没有已分配的信号
                                       (existingPowerSupplyMethods.Count == 1 && existingPowerSupplyMethods[0] == powerSupplyMethod) || // 插槽中的信号和新信号的供电方式一致
                                       (existingPowerSupplyMethods.Count == 1 && existingPowerSupplyMethods[0] == null && powerSupplyMethod == null)); // 插槽和新信号的供电方式都为 null

            // 检查目的地是否一致，传入的 dest 为 null 时忽略此检查
            bool destinationMatches = (dest == null || // 如果 dest 是 null，则忽略检查
                                       existingDestinations.Count == 0 || // 插槽中没有已分配的信号
                                       (existingDestinations.Count == 1 && existingDestinations[0] == dest) || // 插槽中的信号和新信号的目的地一致
                                       (existingDestinations.Count == 1 && existingDestinations[0] == null && dest == null)); // 插槽和新信号的目的地都为 null

            // 仅在供电方式、电压等级和目的地匹配时才分配
            if (voltageLevelMatches && powerSupplyMatches && destinationMatches &&
                (usedChannels + signals.Count <= totalChannels) &&
                ((usedChannels + signals.Count) / (double)totalChannels <= (1 - rate)))
            {
                for (int i = 0; i < signals.Count; i++)
                {
                    slot.Board.Channels[usedChannels + i].Point = signals[i];
                }
                return true;
            }
            return false;
        }



        #endregion

        #region FF串联关系分析

        /// <summary>
        /// 串联箱子信息
        /// </summary>
        private class BoxCascadeInfo
        {
            public string LeftBox { get; set; } = string.Empty;  // 左边串联的箱子
            public string RightBox { get; set; } = string.Empty; // 右边串联的箱子
            public bool HasLeftCascade { get; set; } = false;    // 是否有左串联
            public bool HasRightCascade { get; set; } = false;   // 是否有右串联
        }

        /// <summary>
        /// 分析FF信号的串联关系
        /// </summary>
        private Dictionary<string, BoxCascadeInfo> AnalyzeCascadeRelations(List<IoFullData> ffSignals)
        {
            var relations = new Dictionary<string, BoxCascadeInfo>();
            var allBoxes = ffSignals
                .Where(s => !string.IsNullOrEmpty(s.LocalBoxNumber))
                .Select(s => s.LocalBoxNumber)
                .Distinct()
                .ToList();

            // 初始化所有箱子
            foreach (var boxNumber in allBoxes)
            {
                relations[boxNumber] = new BoxCascadeInfo();
            }

            // 按箱子分组
            var boxGroups = ffSignals.GroupBy(s => s.LocalBoxNumber);

            // 扫描串接备注
            foreach (var boxGroup in boxGroups)
            {
                var currentBox = boxGroup.Key;
                var signals = boxGroup.ToList();

                // 查找串接备注（包含"串"字）
                var cascadeRemark = signals
                    .Where(s => !string.IsNullOrEmpty(s.Remarks) && s.Remarks.Contains("串"))
                    .Select(s => s.Remarks)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(cascadeRemark))
                {
                    // 找到目标箱子链（从备注中提取所有箱号）
                    var targetBoxes = FindTargetBoxes(cascadeRemark, currentBox, allBoxes);
                    if (!targetBoxes.Contains(currentBox))
                    {
                        targetBoxes.Insert(0, currentBox);
                    }
                    
                    // 按箱号在备注中出现的顺序排序
                    targetBoxes = targetBoxes.OrderBy(t => cascadeRemark.IndexOf(t)).ToList();
                    
                    // 建立前后串联关系
                    for (int i = 0; i < targetBoxes.Count - 1; i++)
                    {
                        relations[targetBoxes[i]].RightBox = targetBoxes[i + 1];
                        relations[targetBoxes[i]].HasRightCascade = true;
                    }
                    for (int i = targetBoxes.Count - 1; i > 0; i--)
                    {
                        relations[targetBoxes[i]].LeftBox = targetBoxes[i - 1];
                        relations[targetBoxes[i]].HasLeftCascade = true;
                    }
                }
            }

            return relations;
        }

        /// <summary>
        /// 从备注中提取所有相关的箱号
        /// </summary>
        private List<string> FindTargetBoxes(string remark, string currentBox, List<string> allBoxes)
        {
            List<string> boxes = new List<string>();
            foreach (var box in allBoxes)
            {
                if (!string.IsNullOrEmpty(box) && remark.Contains(box))
                {
                    boxes.Add(box);
                }
            }
            return boxes;
        }

        #endregion

        #region 通讯报警分配

        /// <summary>
        /// 通讯报警分配方法：将通讯报警点从后往前分配到预留板卡
        /// </summary>
        /// <param name="allData">所有IO数据（包含报警预留板卡和通讯报警点）</param>
        /// <param name="configs">板卡类型配置</param>
        /// <param name="cabinetName">机柜名称，如果为null则分配所有机柜</param>
        /// <returns>分配报告（总数、已分配、未分配）</returns>
        public (int total, int allocated, int unallocated) AllocateCommunicationAlarmPoints(
            List<IoFullData> allData, 
            List<config_card_type_judge> configs, 
            string? cabinetName = null)
        {
            int totalCount = 0;
            int allocatedCount = 0;
            int unallocatedCount = 0;

            // 步骤1：清空所有现有通讯报警点的分配信息（Cage=0, Slot=0, Channel=0）
            // 注意：这里只清空分配信息，不删除点本身
            var existingAlarmPoints = allData
                .Where(p => p.PointType == TagType.CommunicationAlarm)
                .ToList();
            
            if (cabinetName != null)
            {
                existingAlarmPoints = existingAlarmPoints.Where(p => p.CabinetNumber == cabinetName).ToList();
            }

            foreach (var point in existingAlarmPoints)
            {
                point.Cage = 0;  // 清空机笼号
                point.Slot = 0;  // 清空插槽号
                point.Channel = 0;  // 清空通道号
                point.UnsetReason = null;  // 清空未分配原因
            }

            // 步骤2：筛选出需要分配的通讯报警点（未分配状态）
            var alarmPoints = allData
                .Where(p => p.PointType == TagType.CommunicationAlarm && p.Cage == 0 && p.Slot == 0 && p.Channel == 0)
                .ToList();

            if (cabinetName != null)
            {
                alarmPoints = alarmPoints.Where(p => p.CabinetNumber == cabinetName).ToList();
            }

            if (!alarmPoints.Any())
            {
                return (0, 0, 0);
            }

            totalCount = alarmPoints.Count;

            // 步骤3：按机柜分组处理
            var cabinetGroups = alarmPoints.GroupBy(p => p.CabinetNumber);

            foreach (var group in cabinetGroups)
            {
                var cabinet = group.Key;
                var pointsInCabinet = group.ToList();

                // 查找该机柜的报警预留板卡（从后往前排序）
                var reservedPoints = allData
                    .Where(p => p.PointType == TagType.AlarmReserved && p.CabinetNumber == cabinet)
                    .OrderByDescending(p => p.Cage)
                    .ThenByDescending(p => p.Slot)
                    .ThenByDescending(p => p.Channel)
                    .ToList();

                if (!reservedPoints.Any())
                {
                    // 没有预留板卡，设置未分配原因
                    foreach (var point in pointsInCabinet)
                    {
                        point.UnsetReason = "无报警预留板卡";
                        unallocatedCount++;
                    }
                    continue;
                }

                // 按插槽分组（从后往前）
                var reservedSlots = reservedPoints
                    .GroupBy(p => new { p.Cage, p.Slot, p.CardType })
                    .OrderByDescending(g => g.Key.Cage)
                    .ThenByDescending(g => g.Key.Slot)
                    .ToList();

                int pointIndex = 0;

                // 遍历每个预留板卡插槽（从后往前）
                foreach (var slotGroup in reservedSlots)
                {
                    if (pointIndex >= pointsInCabinet.Count) break;

                    // 获取板卡配置信息
                    var cardConfig = configs.FirstOrDefault(c => c.IoCardType == slotGroup.Key.CardType);
                    if (cardConfig == null) continue;

                    // 把当前插槽的所有通道都填满（从通道1到最大通道数）
                    for (int channel = 1; channel <= cardConfig.PinsCount; channel++)
                    {
                        // 如果所有报警点都已分配完，退出
                        if (pointIndex >= pointsInCabinet.Count) break;

                        // 检查通道是否已被占用（预留信号占用）
                        if (slotGroup.Any(p => p.Channel == channel))
                            continue;

                        // 检查是否已有报警点在此位置
                        if (allData.Any(p =>
                            p.CabinetNumber == cabinet &&
                            p.Cage == slotGroup.Key.Cage &&
                            p.Slot == slotGroup.Key.Slot &&
                            p.Channel == channel &&
                            (p.PointType == TagType.Alarm || p.PointType == TagType.CommunicationAlarm)))
                            continue;

                        // 分配报警点到该位置
                        var point = pointsInCabinet[pointIndex];
                        point.Cage = slotGroup.Key.Cage;
                        point.Slot = slotGroup.Key.Slot;
                        point.Channel = channel;
                        point.CardType = slotGroup.Key.CardType;  // 设置板卡类型，确保构建机柜时能正确匹配
                        point.UnsetReason = null; // 清空未分配原因

                        allocatedCount++;
                        pointIndex++;
                    }
                }

                // 剩余的点设置为未分配
                for (; pointIndex < pointsInCabinet.Count; pointIndex++)
                {
                    var point = pointsInCabinet[pointIndex];
                    point.UnsetReason = "预留板卡已满";
                    unallocatedCount++;
                }
            }

            return (totalCount, allocatedCount, unallocatedCount);
        }

        #endregion

    }

}

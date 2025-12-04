using Aspose.Cells;
using Aspose.Cells.Charts;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IODataPlatform.Views.Pages
{
    partial class DepYJViewModel
    {
        [RelayCommand]
        public async void GenerateDuanjie()
        {
            string log = "";
            try
            {
                // 打开文件选择器以选择内部接线清单
                if (picker.OpenFile("请选择要提取的内部接线清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx") is not string selectedFilePath || string.IsNullOrEmpty(selectedFilePath)) return;
                // 从所选文件和典回模板中获取数据
                List<内部接线清单> wiringListData = GetQingdanData(selectedFilePath);
                List<典回> templateLoops = GetLoopData();
                List<config_termination_yjs> finalResult = new List<config_termination_yjs>();
                // 按典回类型分组数据
                var groupedByLoopType = wiringListData.GroupBy(data => data.典回类型);
                // 处理每个典回组
                foreach (var loopGroup in groupedByLoopType)
                {
                    var loopType = loopGroup.Key; // 要处理的典回类型
                    var matchingTemplateLoop = templateLoops.FirstOrDefault(template => template.典回类型 == loopType);

                    if (matchingTemplateLoop != null) // 如果找到匹配的典回模板
                    {
                        log += ($"已找到【{loopType}】典回");
                        // 按信号名称分组
                        var groupedBySignalName = loopGroup.ToList().GroupBy(data => data.信号名称);
                        foreach (var signalGroup in groupedBySignalName)
                        {
                            var signalDataList = signalGroup.ToList();
                            var interval = signalDataList.Min(data => ExtractNumber(data.端子号)) - 1; // 计算差值
                            var referenceData = signalDataList.FirstOrDefault();
                            // 处理并更新每行数据
                            foreach (var templateRow in matchingTemplateLoop.loopRows)
                            {
                                var newRow = new config_termination_yjs
                                {
                                    SignalName = signalGroup.Key,
                                    SafetyLevel = referenceData.安全等级,
                                    SafetyColumn = referenceData.安全列,
                                    TerminalGroupName = referenceData.端子组名,
                                    XCoordinate = referenceData.位置.Length == 4 ? referenceData.位置.Substring(0, 2) : referenceData.位置,
                                    YCoordinate = referenceData.位置.Length == 4 ? referenceData.位置.Substring(2, 2) : referenceData.位置,
                                    TerminalNumberP = !string.IsNullOrEmpty(templateRow.TerminalNumberP) ? templateRow.TerminalNumberP.Replace(ExtractNumber(templateRow.TerminalNumberP).ToString(), (ExtractNumber(templateRow.TerminalNumberP) + interval).ToString()) : templateRow.TerminalNumberP,
                                    TerminalNumberN = !string.IsNullOrEmpty(templateRow.TerminalNumberN) ? templateRow.TerminalNumberN.Replace(ExtractNumber(templateRow.TerminalNumberN).ToString(), (ExtractNumber(templateRow.TerminalNumberN) + interval).ToString()) : templateRow.TerminalNumberN,
                                    // 根据需要复制其他属性
                                };
                                finalResult.Add(newRow);
                            }
                        }
                    }
                    else
                    {
                        log += ($"【错误】未在典回模板中找到【{loopType}】典回类型");
                    }
                }
                // 从最终结果生成IO数据
                AllData = [..finalResult.Select(item => new IoData
                {
                    TagName = item.SignalName + item.ExtensionCode,
                    OldVarName = item.SignalName,
                    OldExtCode = item.ExtensionCode,
                    Unit = item.UnitNumber,
                    Cabinet = item.PanelNumber,
                    IoType = item.IOType,
                    Destination = item.Terminal,
                    PowerType = item.PowerSupply,
                    LoopVoltage = item.LoopVoltage,
                    Ees = item.EmergencyPowerSupply,
                    TypicalLoopDrawing = item.LoopType,
                    SafetyClassDivision = item.SafetyLevel,
                    System = item.SystemNumber,
                    TerminalBlock = item.TerminalGroupName,
                    Connection1 = item.TerminalNumberP,
                    Connection2 = item.TerminalNumberN,
                }).ToList()]; 
                //将其存为实时数据，展示
                model.Status.Busy("已完成");
                model.Status.Reset();
                await SaveAndUploadFileAsync();


            }
            catch (Exception ex)
            {
                log += ($"Error: {ex.Message}");
            }

        }

        /// <summary>
        /// 提取字符串中的数字部分
        /// </summary>
        /// <param name="terminalNumber">包含数字的字符串</param>
        /// <returns>提取出的数字</returns>
        private int ExtractNumber(string terminalNumber)
        {
            return int.Parse(new string(terminalNumber.Where(char.IsDigit).ToArray()));
        }       
        
        public List<内部接线清单> GetQingdanData(string filePath)
        {
            // 创建一个 List 来保存所有工作表的数据
            List<内部接线清单> wiringListInfos = new List<内部接线清单>();
            var workbook = excel.GetWorkbook(filePath);

            foreach (var worksheet in workbook.Worksheets)
            {
                // 跳过名为 "目录" 和 "说明" 的工作表
                if (worksheet.Name != "目录" && worksheet.Name != "说明"&& worksheet.Name != "对照表")
                {
                    var sheetData = ReadDataFromWorksheet(worksheet).ToList();
                    if (sheetData != null && sheetData.Count > 0)
                    {
                        // 为每个对象设置 SheetName 属性
                        sheetData.ForEach(item => item.SheetName = worksheet.Name);

                        // 过滤出典回类型不为空且不为 "-" 的数据
                        var filteredData = sheetData.Where(item => !string.IsNullOrEmpty(item.典回类型) && item.典回类型 != "-").ToList();
                        if (filteredData.Count > 0)
                        {
                            wiringListInfos.AddRange(filteredData);
                        }
                    }
                }
            }

            return wiringListInfos;
        }

        // 直接从 Worksheet 读取数据的方法
        private IEnumerable<内部接线清单> ReadDataFromWorksheet(Worksheet worksheet)
        {
            for (int rowIndex = 2; rowIndex <= worksheet.Cells.MaxDataRow; rowIndex++)
            {
                yield return new 内部接线清单
                {
                    位置 = worksheet.Cells[rowIndex, 1].StringValue,
                    典回类型 = worksheet.Cells[rowIndex, 7].StringValue,
                    信号名称 = worksheet.Cells[rowIndex, 3].StringValue,
                    安全等级 = worksheet.Cells[rowIndex, 5].StringValue,
                    安全列 = worksheet.Cells[rowIndex, 6].StringValue,
                    端子组名 = worksheet.Cells[rowIndex, 8].StringValue,
                    端子号 = worksheet.Cells[rowIndex, 9].StringValue
                };
            }
        }


        /// <summary>
        /// 获取数据库中的典回数据
        /// </summary>
        /// <returns></returns>
        public List<典回> GetLoopData()
        {
            //读取模板
            List<典回> loopDetails = new List<典回>();

            List<config_termination_yjs> typicalLoopInfos = context.Db.Queryable<config_termination_yjs>().ToList();
            var gr = typicalLoopInfos.GroupBy(g => g.LoopType);
            foreach (var g in gr)
            {
                典回 LoopDetail = new()
                {
                    典回类型 = g.Key,
                    loopRows = g.ToList()
                };
                loopDetails.Add(LoopDetail);
            }
            return loopDetails;
        }
    }
}

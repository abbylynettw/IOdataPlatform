using Aspose.Cells;
using Aspose.Words.XAttr;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Style = Aspose.Cells.Style;

namespace IODataPlatform.Views.Pages
{
    partial class DepXT2ViewModel
    {

        /// <summary>
        /// 导出FF从站箱端接表
        /// </summary>
        private async Task ExportFFDuanjie(IList<IoFullData> data, string filePath)
        {
            // 筛选并排序信号
            var ffSlaveSignals = data.Where(d => d.SensorType != null &&!string.IsNullOrEmpty(d.LocalBoxNumber) && d.SensorType.Contains("从站")).ToList();
            var sortedGroups = SortByCascadeOrder(ffSlaveSignals);

            var exportDataList = new List<FF从战箱端接表>();

            // 获取所有的就地箱号（即sortedGroups的Key）
            var allBoxNumbers = sortedGroups.Select(g => g.Key).ToList();
            // 从第一个就地箱开始处理
            foreach (var boxGroup in sortedGroups)
            {
                int power220StartIndex = 1;
                int power24StartIndex = 7;
                var localBoxNumber = boxGroup.Key;

                // 查找串接到当前就地箱的就地箱号（leftBoxNumber）
                var leftBoxNumber = allBoxNumbers
                    .FirstOrDefault(box => box != localBoxNumber &&
                                           ffSlaveSignals.Any(signal => signal.LocalBoxNumber == box && signal.Remarks.Contains("串接") && signal.Remarks.Contains(localBoxNumber)));

                // 查找当前就地箱串接到的就地箱号（rightBoxNumber）
                var rightBoxNumber = allBoxNumbers
                    .FirstOrDefault(box => box != localBoxNumber && boxGroup.Any(signal =>!string.IsNullOrEmpty(signal.Remarks) && signal.Remarks.Contains("串接") && signal.Remarks.Contains(box)));


                // 添加主干信号（供电信息）
                var powerList=AddPowerSupplySignals(localBoxNumber, boxGroup.FirstOrDefault().CabinetNumber);
                exportDataList.AddRange(powerList);
                // 添加FF接口信号
                var ffList = AddFFConnectionSignals(boxGroup.FirstOrDefault().CabinetNumber, leftBoxNumber, rightBoxNumber, boxGroup);
                exportDataList.AddRange(ffList);
                foreach (var signal in boxGroup.OrderBy(b => b.FFSlaveModuleID))
                { 
                    //添加每个信号的接线信息
                    var signalP= new FF从战箱端接表
                    {
                        LocalBoxNumber = localBoxNumber,
                        TerminalBlockNumber = signal.FFSlaveModuleID,
                        Terminal = signal.FFSlaveModuleSignalPositive,
                        SignalFunction = signal.Description,
                        SignalPositionNumber = signal.SignalPositionNumber,
                        CableDestination = signal.TagName                       
                    };
                    var signalN = new FF从战箱端接表
                    {
                        LocalBoxNumber = localBoxNumber,
                        TerminalBlockNumber = signal.FFSlaveModuleID,
                        Terminal = signal.FFSlaveModuleSignalNegative,
                        SignalFunction = signal.Description,
                        SignalPositionNumber = signal.SignalPositionNumber,
                        CableDestination = signal.TagName
                    };
                    exportDataList.Add(signalP);
                    exportDataList.Add(signalN);
                    if (signal.PowerSupplyMethod.Contains("DCS") && signal.PowerSupplyMethod.Contains("220V"))
                    {
                        var powerP = new FF从战箱端接表
                        {
                            LocalBoxNumber = localBoxNumber,
                            TerminalBlockNumber = "102BN",
                            Terminal = (power220StartIndex++).ToString(),
                            SignalFunction = "220V供电L",
                            SignalPositionNumber = signal.SignalPositionNumber,
                            CableDestination = signal.TagName
                        };
                        var powerN = new FF从战箱端接表
                        {
                            LocalBoxNumber = localBoxNumber,
                            TerminalBlockNumber = "102BN",
                            Terminal = (power220StartIndex++).ToString(),
                            SignalFunction = "220V供电N",
                            SignalPositionNumber = signal.SignalPositionNumber,
                            CableDestination = signal.TagName
                        };
                        exportDataList.Add(powerP);
                        exportDataList.Add(powerN);
                    }
                    if (signal.PowerSupplyMethod.Contains("DCS") && signal.PowerSupplyMethod.Contains("24V"))
                    {
                        var powerP = new FF从战箱端接表
                        {
                            LocalBoxNumber = localBoxNumber,
                            TerminalBlockNumber = "103BN",
                            Terminal = (power24StartIndex++).ToString(),
                            SignalFunction = "24V供电L",
                            SignalPositionNumber = signal.SignalPositionNumber,
                            CableDestination = signal.TagName
                        };
                        var powerN = new FF从战箱端接表
                        {
                            LocalBoxNumber = localBoxNumber,
                            TerminalBlockNumber = "103BN",
                            Terminal = (power24StartIndex++).ToString(),
                            SignalFunction = "24V供电N",
                            SignalPositionNumber = signal.SignalPositionNumber,
                            CableDestination = signal.TagName
                        };
                        exportDataList.Add(powerP);
                        exportDataList.Add(powerN);
                    }
                }
                
            }
            exportDataList = exportDataList .Select((item, index) =>{
                item.SerialNumber = index + 1; // 序号从 1 开始
                return item;
            }).ToList();

            // 下载模板，填入数据
            model.Status.Busy("正在下载电缆模板……");
            var cableLacalPath = await storage.DownloadtemplatesDepFileAsync("FF端子接线箱端接表模板.xlsx");

            var dt = await exportDataList.ToTableByDisplayAttributeAsync();

            // 创建工作簿
            Workbook workbook = new Workbook(cableLacalPath);
            Worksheet ws = workbook.Worksheets[0];

            // 填充数据到工作表
            var cells = ws.Cells;
            cells.ImportData(dt, 2, 0, new ImportTableOptions { IsFieldNameShown = false });

            // 定义黄色背景色样式
            Style yellowStyle = workbook.CreateStyle();
            yellowStyle.Pattern = BackgroundType.Solid;
            yellowStyle.ForegroundColor = Color.Yellow;

            int currentRow = 2; // 从第2行开始，因为第0 1行是标题
            string previousBoxNumber = string.Empty;

            // 遍历每一行数据
            for (int i = 2; i < cells.MaxDataRow + 1; i++)  // MaxDataRow + 1 代表总行数
            {
                string currentBoxNumber = cells[i, 1].StringValue;  // 假设就地箱号在第2 列，即 index 为1 

                if (!currentBoxNumber.Equals(previousBoxNumber))  // 如果当前箱号与上一个不同
                {
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        ws.Cells[i, col].SetStyle(yellowStyle);  // 标记为黄色
                    }
                }

                previousBoxNumber = currentBoxNumber;  // 更新前一个箱号
            }

            // 保存文件
            workbook.Save(filePath);
            model.Status.Success($"已成功导出到{filePath}");

        }

        /// <summary>
        /// 添加供电信号
        /// </summary>
        /// <param name="exportDataList">导出的数据列表</param>
        /// <param name="localBoxNumber">就地箱号</param>
        private List<FF从战箱端接表> AddPowerSupplySignals(string localBoxNumber, string cabinetName)
        {
            List<FF从战箱端接表> exportDataList = new List<FF从战箱端接表>();
            exportDataList.Add(new FF从战箱端接表
            {
                LocalBoxNumber = localBoxNumber,
                TerminalBlockNumber = "101BN",
                Terminal = "01",
                SignalFunction = "箱体供电L端",
                SignalPositionNumber = "L",
                CableDestination = cabinetName
            }); ;

            exportDataList.Add(new FF从战箱端接表
            {
                LocalBoxNumber = localBoxNumber,
                TerminalBlockNumber = "101BN",
                Terminal = "02",
                SignalFunction = "箱体供电N端",
                SignalPositionNumber = "N",
                CableDestination = cabinetName
            });
            return exportDataList;
        }

        /// <summary>
        /// 添加FF接线信号
        /// </summary>
        /// <param name="exportDataList">导出的数据列表</param>
        /// <param name="boxGroup">当前就地箱分组</param>
        /// <param name="isCascaded">是否串接</param>
        /// <param name="cascadeTargetBoxNumber">串接目标箱号</param>
        private List<FF从战箱端接表> AddFFConnectionSignals(string cabinetName,string leftBoxNumber,string rightBoxNumber, IGrouping<string, IoFullData> boxGroup)
        {
            List<FF从战箱端接表> result = new List<FF从战箱端接表>();
            var localBoxNumber = boxGroup.Key;
            var ffModules = boxGroup.GroupBy(b => b.FFSlaveModuleID).OrderBy(g => g.Key);//按照模块编号排序

            if (ffModules.Count() > 2) throw new Exception($"{localBoxNumber}有{ffModules.Count()}个模块编号，大于2个，错误");
            var firstCard = ffModules.First();
            var sencondCard = ffModules.Last();

            result.Add(new FF从战箱端接表
            {
                LocalBoxNumber = localBoxNumber,
                TerminalBlockNumber = firstCard.Key,
                Terminal = "FF+",
                SignalFunction = "FF接口+",
                SignalPositionNumber = "FF+",
                CableDestination = string.IsNullOrEmpty(leftBoxNumber)? cabinetName:leftBoxNumber
            });
            result.Add(new FF从战箱端接表
            {
                LocalBoxNumber = localBoxNumber,
                TerminalBlockNumber = firstCard.Key,
                Terminal = "FF-",
                SignalFunction = "FF接口-",
                SignalPositionNumber = "FF-",
                CableDestination = string.IsNullOrEmpty(leftBoxNumber) ? cabinetName : leftBoxNumber
            });

            if (!string.IsNullOrEmpty(rightBoxNumber) && ffModules.Count() == 2)
            {
                // 添加第二块卡的信号（串接）
                result.Add(new FF从战箱端接表
                {
                    LocalBoxNumber = localBoxNumber,
                    TerminalBlockNumber = sencondCard.Key,
                    Terminal = "FF+",
                    SignalFunction = "FF接口+",
                    SignalPositionNumber = "FF+",
                    CableDestination = rightBoxNumber
                });

                result.Add(new FF从战箱端接表
                {
                    LocalBoxNumber = localBoxNumber,
                    TerminalBlockNumber = sencondCard.Key,
                    Terminal = "FF-",
                    SignalFunction = "FF接口-",
                    SignalPositionNumber = "FF-",
                    CableDestination = rightBoxNumber
                });
            }                    
            return result;
        }


        /// <summary>
        /// 根据箱子的串接顺序排序
        /// </summary>
        private IList<IGrouping<string, IoFullData>> SortByCascadeOrder(IList<IoFullData> data)
        {
            // 按照就地箱号分组
            var groupedByBox = data.GroupBy(f => f.LocalBoxNumber).ToDictionary(g => g.Key);

            // 建立一个字典来存储每个箱子的串接目标箱号
            var cascadeMap = groupedByBox.ToDictionary(
                g => g.Key, // 就地箱号作为键
                g => groupedByBox.Keys.FirstOrDefault(key =>
                    g.Value.Any(x => !string.IsNullOrEmpty(x.Remarks) &&
                                     x.Remarks.Contains("串接") &&
                                     x.Remarks.Contains(key))) // 备注同时包含“串接”和其他箱号
            );

            // 存储已处理的箱子
            var visited = new HashSet<string>();
            var sortedGroups = new List<IGrouping<string, IoFullData>>();

            // 用于递归处理串接链
            void ProcessChain(string box)
            {
                if (visited.Contains(box)) return;

                // 查找所有串接到当前箱子的箱子并先处理它们
                var predecessors = cascadeMap
                    .Where(kvp => kvp.Value == box)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var predecessor in predecessors)
                {
                    ProcessChain(predecessor);
                }

                // 处理当前箱子
                if (!visited.Contains(box))
                {
                    sortedGroups.Add(groupedByBox[box]);
                    visited.Add(box);
                }

                // 再处理当前箱子串接到的箱子
                var targetBox = cascadeMap[box];
                if (!string.IsNullOrEmpty(targetBox))
                {
                    ProcessChain(targetBox);
                }
            }

            // 遍历所有箱子，处理每个独立的串接链
            foreach (var box in groupedByBox.Keys)
            {
                ProcessChain(box);
            }

            return sortedGroups;
        }


    }
}

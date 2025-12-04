using Aspose.Cells;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Views.Pages
{
    public partial class DepXT2ViewModel
    {
        /// <summary>
        /// 导出FF总线箱端接表
        /// </summary>
        private async Task ExportFFZXDuanjie(IList<IoFullData> data, string filePath)
        {
            try
            {
                // 数据验证
                if (data == null || !data.Any())
                    throw new ArgumentException("输入数据为空，无法生成FF总线箱端接表");

                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("文件路径不能为空");

                // 复制数据避免修改原始数据
                var workingData = data.JsonClone().ToList();

                // 1. 筛选FF总线箱信号
                var ffSignals = FilterFFSignals(workingData);
                if (!ffSignals.Any())
                    throw new InvalidOperationException("未找到供电类型包含FF1~FF6的信号");

                // 2.              
                var cascadeRelations = AnalyzeCascadeRelations(ffSignals);

                // 3. 按箱号分组并排序
                var boxGroups = GroupAndSortBoxes(ffSignals);

                // 4. 生成端接表数据
                var exportData = GenerateConnectionData(boxGroups, cascadeRelations);

                // 5. 导出到Excel
                await ExportToExcel(exportData, filePath);

                model.Status.Success($"已成功导出到{filePath}");
            }
            catch (Exception ex)
            {
                model.Status.Error($"导出FF总线箱端接表失败：{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 筛选FF总线箱信号
        /// </summary>
        private List<IoFullData> FilterFFSignals(List<IoFullData> data)
        {
            var ffTypes = new[] { "FF1", "FF2", "FF3", "FF4", "FF5", "FF6" };
            return data.Where(d => ffTypes.Any(ff => d.PowerType != null && !string.IsNullOrEmpty(d.LocalBoxNumber) && d.PowerType.Contains(ff))).ToList();
        }

        /// <summary>
        /// 分析串接关系
        /// </summary>
        private Dictionary<string, BoxCascadeInfo> AnalyzeCascadeRelations(List<IoFullData> ffSignals)
        {
            var relations = new Dictionary<string, BoxCascadeInfo>();
            var allBoxes = ffSignals.Where(s => !string.IsNullOrEmpty(s.LocalBoxNumber)).Select(s => s.LocalBoxNumber).Distinct().ToList();

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

                // 查找串接备注
                var cascadeRemark = signals
                    .Where(s => !string.IsNullOrEmpty(s.Remarks) && s.Remarks.Contains("串"))
                    .Select(s => s.Remarks)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(cascadeRemark))
                {
                   
                    // 找到目标箱子链
                    var targetBoxes = FindTargetBox(cascadeRemark, currentBox, allBoxes);                    
                    if (!targetBoxes.Contains(currentBox))
                    {
                        targetBoxes.Insert(0, currentBox);
                    }
                    targetBoxes = targetBoxes.OrderBy(t => cascadeRemark.IndexOf(t)).ToList();
                    for (int i = 0; i < targetBoxes.Count - 1; i++)
                    {
                        // 设置目标箱子的左边串接
                        relations[targetBoxes[i]].RightBox = targetBoxes[i + 1];
                        relations[targetBoxes[i]].HasRightCascade = true;
                        var a = relations[targetBoxes[i]];
                    }
                    for (int i = targetBoxes.Count - 1; i >0; i--)
                    {
                        relations[targetBoxes[i]].LeftBox = targetBoxes[i - 1];
                        relations[targetBoxes[i]].HasLeftCascade = true;
                        var a = relations[targetBoxes[i]];
                    }                   
                }
            }

            return relations;
        }

        /// <summary>
        /// 从备注中找到目标箱子
        /// </summary>
        private List<string> FindTargetBox(string remark, string currentBox, List<string> allBoxes)
        {
            List<string> boxes = new List<string>();
            foreach (var box in allBoxes)
            {
                if ( !string.IsNullOrEmpty(box) && remark.Contains(box))
                {
                    boxes.Add(box);
                }
            }
            return boxes;
        }

        /// <summary>
        /// 按箱号分组并排序
        /// </summary>
        private List<IGrouping<string, IoFullData>> GroupAndSortBoxes(List<IoFullData> ffSignals)
        {
            return ffSignals
                .GroupBy(s => s.LocalBoxNumber)
                .OrderBy(g => g.Key)
                .ToList();
        }

        /// <summary>
        /// 生成端接表数据
        /// </summary>
        private List<FF总线箱端接表> GenerateConnectionData(List<IGrouping<string, IoFullData>> boxGroups, Dictionary<string, BoxCascadeInfo> cascadeRelations)
        {
            var result = new List<FF总线箱端接表>();
            int serialNumber = 1;

            foreach (var boxGroup in boxGroups)
            {
                var boxNumber = boxGroup.Key;
                var signals = boxGroup.ToList();
                var cabinetNumber = signals.FirstOrDefault()?.CabinetNumber ?? "";
                var cascadeInfo = cascadeRelations.GetValueOrDefault(boxNumber, new BoxCascadeInfo());

                // 生成主干信号
                var mainSignals = GenerateMainSignals(boxNumber, cabinetNumber, cascadeInfo);
                foreach (var signal in mainSignals)
                {
                    signal.SerialNumber = serialNumber++;
                    result.Add(signal);
                }

                // 生成分支信号
                var branchSignals = GenerateBranchSignals(boxNumber, signals);
                foreach (var signal in branchSignals)
                {
                    signal.SerialNumber = serialNumber++;
                    result.Add(signal);
                }
            }

            return result;
        }

        /// <summary>
        /// 生成主干信号
        /// </summary>
        private List<FF总线箱端接表> GenerateMainSignals(string boxNumber, string cabinetNumber, BoxCascadeInfo cascadeInfo)
        {
            var signals = new List<FF总线箱端接表>();

            // AB端子去向：有右边串接接右边箱子，否则接机柜
            string abDestination = cascadeInfo.HasRightCascade ? cascadeInfo.RightBox : cabinetNumber;

            // 第1行：主干信号+ (001FI-A)
            signals.Add(new FF总线箱端接表
            {
                LocalBoxNumber = boxNumber,
                TerminalBlockNumber = "001FI",
                Terminal = "A",
                SignalFunction = "主干信号+",
                SignalPositionNumber = "R",
                CableDestination = abDestination
            });

            // 第2行：主干信号- (001FI-B)
            signals.Add(new FF总线箱端接表
            {
                LocalBoxNumber = boxNumber,
                TerminalBlockNumber = "001FI",
                Terminal = "B",
                SignalFunction = "主干信号-",
                SignalPositionNumber = "IV",
                CableDestination = abDestination
            });

            // 第3行：主干信号屏蔽 (IE-BUS)
            signals.Add(new FF总线箱端接表
            {
                LocalBoxNumber = boxNumber,
                TerminalBlockNumber = "IE-BUS",
                Terminal = "/",
                SignalFunction = "主干信号屏蔽",
                SignalPositionNumber = "",
                CableDestination = abDestination
            });

            // 只有左边有串接才生成T1记录
            if (cascadeInfo.HasLeftCascade)
            {
                // 第4行：T1+ (001BN)
                signals.Add(new FF总线箱端接表
                {
                    LocalBoxNumber = boxNumber,
                    TerminalBlockNumber = "001BN",
                    Terminal = "T1+",
                    SignalFunction = "主干信号+",
                    SignalPositionNumber = "R",
                    CableDestination = cascadeInfo.LeftBox
                });

                // 第5行：T1- (001BN)
                signals.Add(new FF总线箱端接表
                {
                    LocalBoxNumber = boxNumber,
                    TerminalBlockNumber = "001BN",
                    Terminal = "T1-",
                    SignalFunction = "主干信号-",
                    SignalPositionNumber = "IV",
                    CableDestination = cascadeInfo.LeftBox
                });
            }

            return signals;
        }

        /// <summary>
        /// 生成分支信号
        /// </summary>
        private List<FF总线箱端接表> GenerateBranchSignals(string boxNumber, List<IoFullData> signals)
        {
            var branchSignals = new List<FF总线箱端接表>();

            // 按标签名称分组
            var tagGroups = signals
                .Where(s => !string.IsNullOrWhiteSpace(s.TagName))
                .GroupBy(s => s.TagName)
                .OrderBy(g => g.Key)
                .ToList();

            int branchIndex = 1;
            foreach (var tagGroup in tagGroups)
            {
                var tagName = tagGroup.Key;

                // 每个标签生成3条记录：+、-、S
                branchSignals.Add(new FF总线箱端接表
                {
                    LocalBoxNumber = boxNumber,
                    TerminalBlockNumber = "001BN",
                    Terminal = $"{branchIndex}+",
                    SignalFunction = $"{branchIndex}分支信号+",
                    SignalPositionNumber = "R",
                    CableDestination = tagName
                });

                branchSignals.Add(new FF总线箱端接表
                {
                    LocalBoxNumber = boxNumber,
                    TerminalBlockNumber = "001BN",
                    Terminal = $"{branchIndex}-",
                    SignalFunction = $"{branchIndex}分支信号-",
                    SignalPositionNumber = "IV",
                    CableDestination = tagName
                });

                branchSignals.Add(new FF总线箱端接表
                {
                    LocalBoxNumber = boxNumber,
                    TerminalBlockNumber = "001BN",
                    Terminal = $"{branchIndex}S",
                    SignalFunction = $"{branchIndex}分支信号屏蔽",
                    SignalPositionNumber = "",
                    CableDestination = tagName
                });

                branchIndex++;
            }

            return branchSignals;
        }

        /// <summary>
        /// 导出到Excel
        /// </summary>
        private async Task ExportToExcel(List<FF总线箱端接表> exportData, string filePath)
        {
            try
            {
                model.Status.Busy("正在下载模板...");
                var templatePath = await storage.DownloadtemplatesDepFileAsync("FF端子接线箱端接表模板.xlsx");

                if (!System.IO.File.Exists(templatePath))
                    throw new FileNotFoundException("模板文件未找到");

                var dataTable = await exportData.ToTableByDisplayAttributeAsync();

                // 创建工作簿
                var workbook = new Workbook(templatePath);
                var worksheet = workbook.Worksheets[0];
                var cells = worksheet.Cells;

                // 导入数据
                cells.ImportData(dataTable, 2, 0, new ImportTableOptions { IsFieldNameShown = false });

                // 设置黄色背景样式
                var yellowStyle = workbook.CreateStyle();
                yellowStyle.Pattern = BackgroundType.Solid;
                yellowStyle.ForegroundColor = Color.Yellow;

                // 标记不同箱号的第一行
                string previousBox = string.Empty;
                for (int i = 2; i < cells.MaxDataRow + 1; i++)
                {
                    string currentBox = cells[i, 1].StringValue; // 就地箱位号列

                    if (!currentBox.Equals(previousBox))
                    {
                        for (int col = 0; col < dataTable.Columns.Count; col++)
                        {
                            worksheet.Cells[i, col].SetStyle(yellowStyle);
                        }
                    }
                    previousBox = currentBox;
                }

                // 保存文件
                workbook.Save(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"导出Excel文件失败：{ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// 箱子串接信息
    /// </summary>
    public class BoxCascadeInfo
    {
        /// <summary>
        /// 左边是否有箱子串接到当前箱子
        /// </summary>
        public bool HasLeftCascade { get; set; } = false;

        /// <summary>
        /// 左边箱子号
        /// </summary>
        public string LeftBox { get; set; } = string.Empty;

        /// <summary>
        /// 右边是否有箱子被当前箱子串接
        /// </summary>
        public bool HasRightCascade { get; set; } = false;

        /// <summary>
        /// 右边箱子号
        /// </summary>
        public string RightBox { get; set; } = string.Empty;
    }
}
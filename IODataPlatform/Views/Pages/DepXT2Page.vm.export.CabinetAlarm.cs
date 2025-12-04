using System.Data;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;

namespace IODataPlatform.Views.Pages
{
    partial class DepXT2ViewModel
    {
        /// <summary>
        /// 导出机柜报警表
        /// </summary>
        private async Task ExportCabinetAlarmTable(IList<IoFullData> data, string filePath)
        {
            try
            {
                // 1. 从配置表读取机柜报警清单
                var cabinetConfigs = await context.Db.Queryable<config_xt2_cabinet_alarm>().ToListAsync();
                
                if (!cabinetConfigs.Any())
                {
                    throw new InvalidOperationException("机柜报警配置表中没有数据，请先导入机柜报警清单");
                }

                // 2. 生成导出数据列表
                var exportData = new List<CabinetAlarmTable>();

                // 3. 按RTU板卡所在机柜号分组
                var groupedByRTU = cabinetConfigs.GroupBy(c => c.RTUCabinetNumber);

                foreach (var rtuGroup in groupedByRTU)
                {
                    var rtuCabinetNumber = rtuGroup.Key;
                    var cabinetsInGroup = rtuGroup.ToList();

                    // 获取RTU板卡信息（从第一个机柜的配置中获取，用于生成RTU板卡编号）
                    var firstCabinet = cabinetsInGroup.First();
                    var rtuBoardPositionStr = firstCabinet.RTUBoardPosition; // 格式：机笼-端口-插槽（如：1-0-04）

                    if (string.IsNullOrEmpty(rtuBoardPositionStr))
                    {
                        model.Status.Warn($"RTU板卡所在机柜号 [{rtuCabinetNumber}] 的RTU板卡位置为空，跳过");
                        continue;
                    }

                    // 解析RTU板卡位置：机笼-端口-插槽
                    var rtuParts = rtuBoardPositionStr.Split('-');
                    if (rtuParts.Length != 3)
                    {
                        model.Status.Warn($"RTU板卡位置格式错误：{rtuBoardPositionStr}，应为：机笼-端口-插槽");
                        continue;
                    }

                    if (!int.TryParse(rtuParts[0], out int rtuCage))
                    {
                        model.Status.Warn($"RTU板卡位置机笼号解析失败：{rtuParts[0]}");
                        continue;
                    }

                    if (!int.TryParse(rtuParts[2], out int rtuSlot))
                    {
                        model.Status.Warn($"RTU板卡位置插槽号解析失败：{rtuParts[2]}");
                        continue;
                    }

                    // 生成RTU板卡编号（调用计算方法）
                    // 假设RTU板卡类型为MD211
                    string rtuCardType = "MD211";
                    string rtuCardInCabinetNumber = GetCardInCabinetNumber(rtuCage, rtuSlot, rtuCardType);
                    string rtuBoardPosition = GetCardNumber(rtuCabinetNumber, rtuCardInCabinetNumber, rtuCardType);

                    // 计算RTU板卡地址：机笼号*10 + 插槽号
                    int rtuBoardAddress = (rtuCage - 1) * 10 + rtuSlot;

                    // 4. 为同一RTU分组的每个机柜生成记录
                    foreach (var cabinet in cabinetsInGroup)
                    {
                        // 解析当前机柜的RTU板卡位置以获取端口号
                        var cabinetBoardPositionStr = cabinet.RTUBoardPosition;
                        if (string.IsNullOrEmpty(cabinetBoardPositionStr))
                        {
                            model.Status.Warn($"机柜 [{cabinet.CabinetName}] 的RTU板卡位置为空，跳过");
                            continue;
                        }

                        var cabinetParts = cabinetBoardPositionStr.Split('-');
                        if (cabinetParts.Length != 3)
                        {
                            model.Status.Warn($"机柜 [{cabinet.CabinetName}] RTU板卡位置格式错误：{cabinetBoardPositionStr}");
                            continue;
                        }

                        if (!int.TryParse(cabinetParts[1], out int portMiddle))
                        {
                            model.Status.Warn($"机柜 [{cabinet.CabinetName}] RTU板卡位置端口号解析失败：{cabinetParts[1]}");
                            continue;
                        }

                        if (!int.TryParse(cabinetParts[2], out int cabinetSlot))
                        {
                            model.Status.Warn($"机柜 [{cabinet.CabinetName}] RTU板卡位置插槽号解析失败：{cabinetParts[2]}");
                            continue;
                        }

                        // 解析端口号：中间位 0→COM0, 1→COM1
                        string portNumber = portMiddle == 0 ? "COM0" : "COM1";

                        // 从站设备名称：机柜名称 + "_ALM"
                        string slaveDevice = $"{cabinet.CabinetName}_ALM";

                        // 从站地址：当前机柜RTU板卡位置的最后一位数字
                        string slaveAddress = cabinetSlot.ToString();

                        // 生成串接顺序
                        string cascadeOrder = GenerateCascadeOrder(cabinetsInGroup, cabinet, rtuBoardPosition, portNumber);

                        // 添加导出记录
                        exportData.Add(new CabinetAlarmTable
                        {
                            RTUCabinetNumber = rtuCabinetNumber,
                            RTUBoardPosition = rtuBoardPosition,
                            RTUBoardAddress = rtuBoardAddress.ToString(),
                            PortNumber = portNumber,
                            SlaveDevice = slaveDevice,
                            SlaveAddress = slaveAddress,
                            CascadeOrder = cascadeOrder,
                            Version = "A",
                            ModificationRecord = "",
                            ModificationDate = "",
                            Room = cabinet.Room ?? ""
                        });
                    }
                }

                // 5. 导出到Excel
                if (!exportData.Any())
                {
                    throw new InvalidOperationException("没有生成任何导出数据，请检查配置表数据");
                }

                using var dataTable = await exportData.ToTableByDisplayAttributeAsync();
                await excel.FastExportSheetAsync(dataTable, filePath, "机柜报警表");

                model.Status.Success($"已成功导出机柜报警表到：{filePath}");
            }
            catch (Exception ex)
            {
                model.Status.Error($"导出机柜报警表失败：{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 生成串接顺序
        /// 规则：同一RTU分组下的所有机柜，按序号从后往前串接
        /// 格式：机柜A串机柜B串...接{板卡位置+端口号}
        /// </summary>
        private string GenerateCascadeOrder(List<config_xt2_cabinet_alarm> cabinetsInGroup, 
            config_xt2_cabinet_alarm currentCabinet, string rtuBoardPosition, string portNumber)
        {
            // 获取同一RTU分组下的所有机柜，按序号从大到小排序（从后往前）
            var allCabinets = cabinetsInGroup
                .OrderByDescending(c => c.SerialNumber) // 从后往前
                .Select(c => c.CabinetName)
                .ToList();

            // 拼接串接顺序：所有机柜用"串"连接，最后接RTU板卡
            string cascadeChain = string.Join("串", allCabinets);
            string finalOrder = $"{cascadeChain}接{rtuBoardPosition}{portNumber}";

            return finalOrder;
        }
    }
}

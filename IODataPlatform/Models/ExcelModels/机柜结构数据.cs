using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.Pages;

using SqlSugar;
using Wpf.Ui.Controls;

namespace IODataPlatform.Models.ExcelModels;

// 标准机柜、板卡、板卡通道结构数据
// 以文件的形式保存到服务器上，优点是可直接包含嵌套关系，且此对象可直接用于绑定到视图中
// 文件相对路径：
// 实时版本：projects/subproject_{子项id}/io/realtime.csv
// 发布版本：projects/subproject_{子项id}/io/publish_{发布id}.csv

// 全部机柜名从xt2.AllData中获取，不需要保存在数据库中

/// <summary>机柜</summary>
public partial class StdCabinet : ObservableObject {
   

    /// <summary>使用此方法创建Cabinet，而不是直接new，保留new方法给序列化器使用</summary>
    public static StdCabinet Create(string name)
    {
        return new()
        {
            Name = name,
            Cages = [
                new() { Index = 1, Slots = CreateSlots(3, 8) }, // 龙鳍
                new() { Index = 2, Slots = CreateSlots(1, 10) },
                new() { Index = 3, Slots = CreateSlots(1, 10) }]
        };
    }
    //扩展柜 1
    public static StdCabinet CreateEx(string name)
    {
        return new()
        {
            Name = name,
            Cages = [
                new() { Index = 4, Slots = CreateSlots(1, 10) }, // 龙鳍
                new() { Index = 5, Slots = CreateSlots(1, 10) },
                new() { Index = 6, Slots = CreateSlots(1, 10) }]
        };
    }
    //扩展柜 2
    public static StdCabinet CreateExEx(string name)
    {
        return new()
        {
            Name = name,
            Cages = [
                new() { Index = 7, Slots = CreateSlots(1, 10) }, // 龙鳍
                new() { Index = 8, Slots = CreateSlots(1, 10) }]
        };
    }
    public static StdCabinet CreateLH(string name)
    {
        return new()
        {
            Name = name,
            Cages = [
                new() { Index = 1, Slots = CreateSlots(7, 7) }, // 二室从索引3开始创建8个插槽
                new() { Index = 2, Slots = CreateSlots(0, 14) },
                new() { Index = 3, Slots = CreateSlots(0, 14) }]
        };
    }

    private static List<SlotInfo> CreateSlots(int startIndex, int count)
    {
        return Enumerable.Range(startIndex, count).Select(x => new SlotInfo() { Index = x }).ToList();
    }

    /// <summary>机柜名</summary>
    public required string Name { get; set; }

    /// <summary>机笼列表</summary>
    public required List<ChassisInfo> Cages { get; set; }

    /// <summary>虚拟插槽列表，用于存放未分配的板卡</summary>
    public ObservableCollection<SlotInfo> VirtualSlots { get; set; } = [];

    [ObservableProperty]
    private ObservableCollection<IoFullData> unsetPoints = [];//未分配点

    [JsonIgnore]
    public CabinetSummaryInfo SummaryInfo { get => CabinetCalc.GetCabinetSummaryInfo(this); }

    //[ObservableProperty]
    //private ObservableCollection<Board> unsetBoards = [];//未分配板卡

    /// <summary>获取所有插槽（包括虚拟插槽）</summary>
    [JsonIgnore]
    public IEnumerable<SlotInfo> AllSlots
    {
        get
        {
            // 正常插槽
            foreach (var cage in Cages)
            {
                foreach (var slot in cage.Slots)
                {
                    yield return slot;
                }
            }
            // 虚拟插槽
            foreach (var virtualSlot in VirtualSlots)
            {
                yield return virtualSlot;
            }
        }
    }
    /// <summary>添加板卡到虚拟插槽</summary>
    public void AddBoardToVirtualSlot(Board board)
    {
        var virtualSlot = new SlotInfo
        {
            Index = -(VirtualSlots.Count + 1), // 负数索引
            Board = board,
            IsVirtual = true
        };
        VirtualSlots.Add(virtualSlot);
    }

    /// <summary>获取板卡的所有通道（支持FF板卡和普通板卡）</summary>
    /// <remarks>
    /// 注意：此方法已过时，因为FF总线箱和从站箱使用不同的通道类型
    /// 推荐使用GetAllSignals()方法获取所有信号
    /// </remarks>
    [Obsolete("推荐使用GetAllSignals()方法，因为FF板卡使用不同的通道类型")]
    public static IEnumerable<Xt2Channel> GetAllChannels(Board board)
    {
        switch (board.FFBoardType)
        {
            case BoardType.Normal:
                // 普通板卡：直接获取通道
                foreach (var channel in board.Channels)
                {
                    yield return channel;
                }
                break;

            case BoardType.FFBus:
            case BoardType.FFSlave:
                // FF板卡：此方法已不适用，返回空集合
                // 请使用GetAllSignals()方法
                yield break;
        }
    }

    /// <summary>获取板卡的所有信号（支持所有板卡类型）</summary>
    public static IEnumerable<IoFullData> GetAllSignals(Board board)
    {
        switch (board.FFBoardType)
        {
            case BoardType.Normal:
                // 普通板卡：从Channels获取
                foreach (var channel in board.Channels)
                {
                    if (channel.Point != null)
                    {
                        yield return channel.Point;
                    }
                }
                break;

            case BoardType.Communication:
                // 通讯板卡：从CommPorts.VirtualSignals获取
                foreach (var port in board.CommPorts)
                {
                    foreach (var virtualSignal in port.VirtualSignals)
                    {
                        if (virtualSignal.Signal != null)
                        {
                            yield return virtualSignal.Signal;
                        }
                    }
                }
                break;

            case BoardType.FFBus:
                // FF总线箱：从FFBusChannels获取
                foreach (var network in board.Networks)
                {
                    foreach (var channel in network.FFBusChannels)
                    {
                        if (channel.Point != null)
                        {
                            yield return channel.Point;
                        }
                    }
                }
                break;

            case BoardType.FFSlave:
                // FF从站箱：从 Modules.FFSlaveChannels 和 UnallocatedSignals 获取
                foreach (var network in board.Networks)
                {
                    // 1. 从模块通道获取（已分配的信号）
                    foreach (var module in network.Modules)
                    {
                        foreach (var channel in module.FFSlaveChannels)
                        {
                            if (channel.Point != null)
                            {
                                yield return channel.Point;
                            }
                        }
                    }
                    
                    // 2. 从未分配列表获取（未分配的信号）
                    foreach (var signal in network.UnallocatedSignals)
                    {
                        yield return signal;
                    }
                }
                break;
        }
    }

}

/// <summary>机笼</summary>
public class ChassisInfo {

    /// <summary>序号</summary>
    public required int Index { get; set; }

    /// <summary>插槽列表</summary>
    public required List<SlotInfo> Slots { get; set; }
}

/// <summary>插槽</summary>
public partial class SlotInfo : ObservableObject {

    /// <summary>序号</summary>
    public required int Index { get; set; }

    /// <summary>端子板，如插槽中没有端子板，则为null</summary>
    [ObservableProperty]
    private Board? board;

    /// <summary>是否为虚拟插槽（用于存放未分配的板卡）</summary>
    public bool IsVirtual { get; set; } = false;

}

/// <summary>端子板，UI中不提供端子板的编辑方法，因为通道数可能变化</summary>
public class Board {

    /// <summary>使用此方法创建Board，而不是直接new，保留new方法给序列化器使用</summary>
    /// <remarks>
    /// 注意：此方法仅用于从已有数据重建机柜结构时创建普通板卡
    /// FF板卡的创建应使用CreateFFBus()或CreateFFSlave()方法
    /// </remarks>
    public static Board Create(config_card_type_judge type) {
        var channels = Enumerable.Range(1, type.PinsCount).Select(x => new Xt2Channel() { Index = x });
        return new() { Type = type.IoCardType, Channels = [.. channels] };
    }

    /// <summary>创建FF总线板卡（双网段，每个网段直接存储信号）</summary>
    public static Board CreateFFBus(config_card_type_judge type) {
        int totalChannels = type.PinsCount;
        int channelsPerNetwork = totalChannels / 2;
        
        return new() { 
            Type = type.IoCardType,
            FFBoardType = BoardType.FFBus,
            Networks = [
                new FFNetwork { 
                    NetworkType = Xt2NetType.Net1,
                    FFBusChannels = new ObservableCollection<FFBusChannel>(
                        Enumerable.Range(1, channelsPerNetwork).Select(x => new FFBusChannel() { Index = x })
                    )
                },
                new FFNetwork { 
                    NetworkType = Xt2NetType.Net2,
                    FFBusChannels = new ObservableCollection<FFBusChannel>(
                        Enumerable.Range(1, channelsPerNetwork).Select(x => new FFBusChannel() { Index = x })
                    )
                }
            ]
        };
    }

    /// <summary>创建FF从站板卡（支持双网段，每个网段支持多个模块）</summary>
    public static Board CreateFFSlave(config_card_type_judge type) {
        return new() { 
            Type = type.IoCardType,
            FFBoardType = BoardType.FFSlave,
            Networks = [
                new FFNetwork { NetworkType = Xt2NetType.Net1 },
                new FFNetwork { NetworkType = Xt2NetType.Net2 }
            ]
        };
    }

    /// <summary>创建通讯板卡（MD211/MD216/DP211）</summary>
    /// <param name="type">板卡类型配置</param>
    /// <param name="virtualSignalCountPerPort">每个端口的虚拟信号数量，默认32</param>
    /// <returns>通讯板卡</returns>
    public static Board CreateCommunication(config_card_type_judge type, int virtualSignalCountPerPort = 32) {
        var ports = new List<CommPort>();
        
        // 根据板卡类型决定端口数量
        int portCount = type.IoCardType switch {
            "MD211" => 2,  // MD211有com0、com1两个端口
            "MD216" => 1,  // MD216有com0一个端口
            "DP211" => 1,  // DP211有一个端口
            _ => 1
        };
        
        // 创建端口
        for (int i = 0; i < portCount; i++) {
            var port = new CommPort {
                PortName = $"com{i}",
                Index = i,
                VirtualSignals = new ObservableCollection<VirtualSignal>(
                    Enumerable.Range(1, virtualSignalCountPerPort).Select(x => new VirtualSignal() { Index = x })
                )
            };
            ports.Add(port);
        }
        
        return new() { 
            Type = type.IoCardType,
            FFBoardType = BoardType.Communication,
            CommPorts = new ObservableCollection<CommPort>(ports)
        };
    }

    /// <summary>类型</summary>
    public required string Type { get; set; }

    /// <summary>FF板卡类型（Normal=普通板卡，FFBus=FF总线箱，FFSlave=FF从站箱，Communication=通讯板卡）</summary>
    public BoardType FFBoardType { get; set; } = BoardType.Normal;

    /// <summary>通道列表（普通板卡使用）</summary>
    public ObservableCollection<Xt2Channel> Channels { get; set; } = [];

    /// <summary>网段列表（FF板卡使用）</summary>
    public ObservableCollection<FFNetwork> Networks { get; set; } = [];

    /// <summary>通讯端口列表（通讯板卡使用）</summary>
    public ObservableCollection<CommPort> CommPorts { get; set; } = [];

    /// <summary>获取当前板卡的所有通道（实例方法）</summary>
    public IEnumerable<Xt2Channel> GetAllChannels() => StdCabinet.GetAllChannels(this);

}

/// <summary>FF网段（总线箱和从站箱使用不同结构）</summary>
public partial class FFNetwork : ObservableObject {

    /// <summary>网段类型（Net1或Net2）</summary>
    public required Xt2NetType NetworkType { get; set; }

    /// <summary>FF总线箱信号列表（仅总线箱使用，直接在网段下存储信号）</summary>
    /// <remarks>
    /// FF总线箱：网段 → FFBusChannels（信号列表）
    /// 不使用模块和通道的概念
    /// </remarks>
    public ObservableCollection<FFBusChannel> FFBusChannels { get; set; } = [];

    /// <summary>FF从站箱模块列表（仅从站箱使用）</summary>
    /// <remarks>
    /// FF从站箱：网段 → Modules → FFSlaveChannels（从站通道）
    /// 每个网段最多2个模块
    /// </remarks>
    public ObservableCollection<FFSlaveModule> Modules { get; set; } = [];

    /// <summary>FF从站箱未分配信号列表（IO分配后、FF分配前的信号）</summary>
    /// <remarks>
    /// IO自动分配阶段：信号被分配到网段，存放在这个列表中
    /// FF从站模块分配阶段：从这个列表中取出信号，分配到真实模块
    /// </remarks>
    public ObservableCollection<IoFullData> UnallocatedSignals { get; set; } = [];

}

/// <summary>FF从站模块</summary>
public partial class FFSlaveModule : ObservableObject {

    /// <summary>模块型号（如FS201、FS202）</summary>
    public required string ModuleType { get; set; }

    /// <summary>从站号（01、02、03...）</summary>
    public required int StationNumber { get; set; }

    /// <summary>从站通道列表（FF从站箱专用）</summary>
    public ObservableCollection<FFSlaveChannel> FFSlaveChannels { get; set; } = [];

    /// <summary>创建FF从站模块</summary>
    public static FFSlaveModule Create(string moduleType, int stationNumber, int channelCount) {
        // 根据模块型号确定固定通道数量
        int fixedChannelCount = GetModuleChannelCount(moduleType, channelCount);
        
        return new() {
            ModuleType = moduleType,
            StationNumber = stationNumber,
            // 创建固定数量的通道，Index从1开始
            FFSlaveChannels = new ObservableCollection<FFSlaveChannel>(
                Enumerable.Range(1, fixedChannelCount).Select(i => new FFSlaveChannel { Index = i })
            )
        };
    }
    
    /// <summary>获取模块固定通道数量</summary>
    private static int GetModuleChannelCount(string moduleType, int defaultCount)
    {
        if (string.IsNullOrEmpty(moduleType))
            return defaultCount;
            
        // 判断包含型号即可（如"2FS201"、"FS201"都能匹配）
        if (moduleType.Contains("FS201", StringComparison.OrdinalIgnoreCase))
            return 14;  // FS201固定14个通道
        if (moduleType.Contains("FS202", StringComparison.OrdinalIgnoreCase))
            return 16;  // FS202固定16个通道
            
        return defaultCount;  // 其他模块或"未分配"使用传入的数量
    }
    
    /// <summary>解析FFTerminalChannel中的数字部分</summary>
    /// <param name="ffTerminalChannel">从站通道号，如"1A"、"5"、"11"等</param>
    /// <returns>通道索引（基于1），解析失败返回0</returns>
    private static int ParseFFTerminalChannelNumber(string ffTerminalChannel)
    {
        if (string.IsNullOrEmpty(ffTerminalChannel))
            return 0;
            
        // 提取开头的数字部分，如"1A" -> 1, "5" -> 5, "11" -> 11
        var match = System.Text.RegularExpressions.Regex.Match(ffTerminalChannel, @"^(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var channelNumber))
        {
            return channelNumber;
        }
        
        return 0;
    }

}

/// <summary>通道（普通板卡使用）</summary>
public partial class Xt2Channel : ObservableObject {

    /// <summary>序号</summary>
    public required int Index { get; set; }

    /// <summary>点，如通道上没有点，则为null</summary>
    [ObservableProperty]
    private IoFullData? point;

}

/// <summary>FF总线箱通道（直接存储信号，不通过模块）</summary>
public partial class FFBusChannel : ObservableObject {

    /// <summary>序号</summary>
    public required int Index { get; set; }

    /// <summary>信号</summary>
    [ObservableProperty]
    private IoFullData? signal;

    /// <summary>Point属性别名（用于兼容XAML绑定）</summary>
    public IoFullData? Point {
        get => Signal;
        set => Signal = value;
    }

}

/// <summary>FF从站通道（存储信号，属于模块）</summary>
public partial class FFSlaveChannel : ObservableObject {

    /// <summary>序号</summary>
    public required int Index { get; set; }

    /// <summary>从站通道名称（固定值，如"1A"、"2B"、"5"、"11"等）</summary>
    public string? ChannelName { get; set; }

    /// <summary>信号</summary>
    [ObservableProperty]
    private IoFullData? signal;

    /// <summary>Point属性别名（用于兼容XAML绑定）</summary>
    public IoFullData? Point {
        get => Signal;
        set => Signal = value;
    }

}

/// <summary>通讯端口（通讯板卡使用，com0/com1等）</summary>
public partial class CommPort : ObservableObject {

    /// <summary>端口名称（如"com0"、"com1"）</summary>
    public required string PortName { get; set; }

    /// <summary>端口索引（从0开始）</summary>
    public required int Index { get; set; }

    /// <summary>虚拟信号列表</summary>
    public ObservableCollection<VirtualSignal> VirtualSignals { get; set; } = [];

}

/// <summary>虚拟信号（通讯端口内的虚拟信号）</summary>
public partial class VirtualSignal : ObservableObject {

    /// <summary>序号</summary>
    public required int Index { get; set; }

    /// <summary>信号</summary>
    [ObservableProperty]
    private IoFullData? signal;

    /// <summary>Point属性别名（用于兼容XAML绑定）</summary>
    public IoFullData? Point {
        get => Signal;
        set => Signal = value;
    }

}







public enum TagType {
    Normal=0,
    Alarm=1,  // 硬点报警（默认）
    BackUp=2,
    CommunicationReserved=3,  // 通讯预留板卡
    AlarmReserved=4,  // 报警预留板卡
    CommunicationAlarm=5  // 通讯报警点
}

public enum Xt2NetType {
    Net1,
    Net2
}

/// <summary>板卡类型枚举</summary>
public enum BoardType {
    /// <summary>普通板卡（AI、DI、AO、DO等）</summary>
    Normal = 0,
    /// <summary>FF总线板卡（FF1-FF6）</summary>
    FFBus = 1,
    /// <summary>FF从站板卡（FF7-FF8），每个网段最多包含2个模块</summary>
    FFSlave = 2,
    /// <summary>通讯板卡（MD211/MD216/DP211），支持多个通讯端口</summary>
    Communication = 3
}

public static class CabinetCalc {

    public static List<StdCabinet> BuildCabinetStructureLH(this List<IoFullData> points, List<config_card_type_judge> configs) {
        var cabinets = new Dictionary<string, StdCabinet>();

        foreach (var point in points) {
            // 获取或创建机柜
            if (!cabinets.TryGetValue(point.CabinetNumber, out var cabinet)) {
                cabinet = StdCabinet.CreateLH(point.CabinetNumber);
                cabinets.Add(point.CabinetNumber, cabinet);
            }

            // 获取机柜中对应的机笼和插槽
            var cage = cabinet.Cages.FirstOrDefault(c => c.Index == point.Cage);
            var slot = cage?.Slots.FirstOrDefault(s => s.Index == point.Slot);

            if (cage == null || slot == null) {
                // 如果找不到对应的机笼或插槽，将点添加到未分配列表
                cabinet.UnsetPoints.Add(point);
                continue;
            }

            // 如果插槽中没有板卡，创建一个新的板卡
            if (slot.Board == null) {
                var config = configs.Single(c => c.IoCardType == point.CardType);
                slot.Board = Board.Create(config);
            }

            // 如果板卡类型不匹配，将点添加到未分配列表
            if (slot.Board.Type != point.CardType) {
                cabinet.UnsetPoints.Add(point);
                continue;
            }

            // 检查通道位置是否有效
            if (point.Channel <= 0 || point.Channel > slot.Board.Channels.Count) {
                // 如果通道位置无效，将点添加到未分配列表
                cabinet.UnsetPoints.Add(point);
                continue;
            }

            // 将点分配给对应的通道
            var channel = slot.Board.Channels[point.Channel - 1];
            if (channel.Point == null) {
                channel.Point = point;
            } else {
                // 如果通道已被占用，将点添加到未分配列表
                cabinet.UnsetPoints.Add(point);
            }
        }

        return [.. cabinets.Values];
    }

    #region 系统统室分配逻辑
    
    public static List<StdCabinet> BuildCabinetStructureXT1(this List<IoFullData> points, List<config_card_type_judge> configs)
    {
        var cabinets = new Dictionary<string, StdCabinet>();

        //步骤1：所有点按类型分成较小分组单位
        /***
         * 系统·安全·EES·
         * **/





        return [.. cabinets.Values];
    }
    #endregion


    public static List<StdCabinet> BuildCabinetStructureOther(this List<IoFullData> points, List<config_card_type_judge> configs)
    {
        var cabinets = new Dictionary<string, StdCabinet>();

        foreach (var point in points)
        {
            // 获取或创建机柜
            if (!cabinets.TryGetValue(point.CabinetNumber, out var cabinet))
            {
                cabinet = StdCabinet.Create(point.CabinetNumber);
                cabinets.Add(point.CabinetNumber, cabinet);
            }
            // 检查是否是虚拟插槽的点
            if (point.Cage == -1)
            {
                // 查找对应的虚拟插槽
                var virtualSlot = cabinet.VirtualSlots.FirstOrDefault(vs => vs.Index == point.Slot);
                if (virtualSlot == null)
                {
                    // 创建新的虚拟插槽
                    var config = configs.FirstOrDefault(c => c.IoCardType == point.CardType);
                    if (config == null)
                    {
                        cabinet.UnsetPoints.Add(point);
                        continue;
                    }

                    virtualSlot = new SlotInfo
                    {
                        Index = point.Slot,
                        IsVirtual = true
                    };
                    
                    // 根据信号类型决定创建哪种板卡
                    bool isFFSlave = !string.IsNullOrEmpty(point.IoType) &&
                                     point.IoType.Contains("FF", StringComparison.OrdinalIgnoreCase) &&
                                     !string.IsNullOrEmpty(point.FFSlaveModuleModel);
                    
                    bool isFFBus = !string.IsNullOrEmpty(point.IoType) &&
                                   point.IoType.Contains("FF", StringComparison.OrdinalIgnoreCase) &&
                                   string.IsNullOrEmpty(point.FFSlaveModuleModel);
                    
                    // 判断是否为通讯板卡
                    bool isCommunication = point.CardType == "MD211" || 
                                          point.CardType == "MD216" || 
                                          point.CardType == "DP211";
                    
                    if (isFFSlave) {
                        virtualSlot.Board = Board.CreateFFSlave(config);
                        
                        // 根据信号是否已分配FF从站模块来决定创建哪种模块
                        // 先找到该板卡上的所有FF从站信号
                        var ffSlaveSignalsOnThisBoard = points
                            .Where(p => p.CabinetNumber == point.CabinetNumber && 
                                        p.Cage == -1 && 
                                        p.Slot == point.Slot &&
                                        !string.IsNullOrEmpty(p.FFSlaveModuleModel))
                            .ToList();
                        
                        // 判断是否已经FF从站模块分配（有FFDPStaionNumber字段）
                        bool hasAllocatedModules = ffSlaveSignalsOnThisBoard.Any(s => !string.IsNullOrEmpty(s.FFDPStaionNumber));
                        
                        int channelsPerNetwork = config.PinsCount / 2;
                        var net1 = virtualSlot.Board.Networks.FirstOrDefault(n => n.NetworkType == Xt2NetType.Net1);
                        var net2 = virtualSlot.Board.Networks.FirstOrDefault(n => n.NetworkType == Xt2NetType.Net2);
                        
                        if (hasAllocatedModules)
                        {
                            // 已分配：根据站号和模块型号重建实际模块
                            // 按网段和站号分组
                            var moduleGroups = ffSlaveSignalsOnThisBoard
                                .Where(s => !string.IsNullOrEmpty(s.FFDPStaionNumber))
                                .GroupBy(s => new { NetType = s.NetType ?? "Net1", StationNumber = s.FFDPStaionNumber })
                                .OrderBy(g => g.Key.NetType)
                                .ThenBy(g => int.TryParse(g.Key.StationNumber, out var num) ? num : 0);
                            
                            foreach (var moduleGroup in moduleGroups)
                            {
                                var firstSignal = moduleGroup.First();
                                var network = moduleGroup.Key.NetType == "Net2" ? net2 : net1;
                                
                                if (network != null)
                                {
                                    // 直接使用FFSlaveModuleModel作为模块型号（已经是纯型号，如"FS201"）
                                    string moduleType = firstSignal.FFSlaveModuleModel ?? "未分配";
                                    int stationNumber = int.TryParse(moduleGroup.Key.StationNumber, out var num) ? num : 0;
                                    
                                    // 创建模块（会根据型号自动确定固定通道数）
                                    var module = FFSlaveModule.Create(moduleType, stationNumber, 0);
                                    network.Modules.Add(module);
                                }
                            }
                        }
                        else
                        {
                            // ✅ 未分配：不创建"未分配"模块，Modules保持为空
                            // 信号将在后续被放入network.UnallocatedSignals
                        }
                    } else if (isFFBus) {
                        virtualSlot.Board = Board.CreateFFBus(config);
                    } else if (isCommunication) {
                        // 创建通讯板卡
                        virtualSlot.Board = Board.CreateCommunication(config);
                    } else {
                        virtualSlot.Board = Board.Create(config);
                    }
                    
                    cabinet.VirtualSlots.Add(virtualSlot);
                }

                // 分配点到虚拟插槽的板卡（支持FF板卡、通讯板卡和普通板卡）
                if (virtualSlot.Board != null)
                {
                    if (virtualSlot.Board.FFBoardType == BoardType.Normal)
                    {
                        // 普通板卡：直接使用Channels
                        if (point.Channel > 0 && point.Channel <= virtualSlot.Board.Channels.Count)
                        {
                            var virchannel = virtualSlot.Board.Channels[point.Channel - 1];
                            if (virchannel.Point == null)
                            {
                                virchannel.Point = point;
                            }
                            else
                            {
                                cabinet.UnsetPoints.Add(point);
                            }
                        }
                        else
                        {
                            cabinet.UnsetPoints.Add(point);
                        }
                    }
                    else if (virtualSlot.Board.FFBoardType == BoardType.Communication)
                    {
                        // 通讯板卡：使用CommPorts.VirtualSignals
                        // 根据Channel字段定位虚拟信号槽
                        if (point.Channel > 0)
                        {
                            // 遍历所有端口查找对应的虚拟信号槽
                            bool allocated = false;
                            foreach (var port in virtualSlot.Board.CommPorts)
                            {
                                var virtualSignal = port.VirtualSignals.FirstOrDefault(vs => vs.Index == point.Channel);
                                if (virtualSignal != null && virtualSignal.Signal == null)
                                {
                                    virtualSignal.Signal = point;
                                    allocated = true;
                                    break;
                                }
                            }
                            
                            if (!allocated)
                            {
                                cabinet.UnsetPoints.Add(point);
                            }
                        }
                        else
                        {
                            cabinet.UnsetPoints.Add(point);
                        }
                    }
                    else
                    {
                        // FF板卡：根据板卡类型使用不同的通道结构
                        var networkType = point.NetType == "Net2" ? Xt2NetType.Net2 : Xt2NetType.Net1;
                        var network = virtualSlot.Board.Networks.FirstOrDefault(n => n.NetworkType == networkType);
                        
                        if (network == null)
                        {
                            point.UnsetReason = $"虚拟插槽板卡网段初始化失败（NetType={point.NetType}）";
                            cabinet.UnsetPoints.Add(point);
                            continue;
                        }
                        else if (virtualSlot.Board.FFBoardType == BoardType.FFBus)
                        {
                            // FF总线箱：使用FFBusChannels
                            if (point.Channel > 0 && point.Channel <= network.FFBusChannels.Count)
                            {
                                var virchannel = network.FFBusChannels[point.Channel - 1];
                                if (virchannel.Point == null)
                                {
                                    virchannel.Point = point;
                                }
                                else
                                {
                                    cabinet.UnsetPoints.Add(point);
                                }
                            }
                            else
                            {
                                cabinet.UnsetPoints.Add(point);
                            }
                        }
                        else if (virtualSlot.Board.FFBoardType == BoardType.FFSlave)
                        {
                            // FF从站箱：判断是否已FF分配（有有效的FFDPStaionNumber字段）
                            // 注意：FFDPStaionNumber是字符串，"01", "02"等才是有效值
                            if (string.IsNullOrWhiteSpace(point.FFDPStaionNumber))
                            {
                                // ✅ 未FF分配：放入UnallocatedSignals（显示在网段下）
                                network.UnallocatedSignals.Add(point);
                                continue;
                            }
                            
                            // ✅ 已FF分配：根据FFDPStaionNumber找到对应模块，分配到通道
                            // 根据FfDPStaionNumber找到对应的模块
                            FFSlaveModule? module = null;
                            if (!string.IsNullOrEmpty(point.FFDPStaionNumber) && int.TryParse(point.FFDPStaionNumber, out var stationNum))
                            {
                                module = network.Modules.FirstOrDefault(m => m.StationNumber == stationNum);
                            }
                            
                            if (module == null)
                            {
                                cabinet.UnsetPoints.Add(point);
                                continue;
                            }
                            
                            // 仅使用FFTerminalChannel，不回退到Channel（两者含义不同）
                            if (!point.FFTerminalChannel.HasValue)
                            {
                                cabinet.UnsetPoints.Add(point);
                                continue;
                            }
                            
                            int channelIndex = point.FFTerminalChannel.Value;
                            if (channelIndex <= 0 || channelIndex > module.FFSlaveChannels.Count)
                            {
                                cabinet.UnsetPoints.Add(point);
                                continue;
                            }
                            
                            var virchannel = module.FFSlaveChannels[channelIndex - 1];
                            if (virchannel.Point == null)
                            {
                                virchannel.Point = point;
                            }
                            else
                            {
                                cabinet.UnsetPoints.Add(point);
                            }
                        }
                    }
                }
                else
                {
                    cabinet.UnsetPoints.Add(point);
                }
                continue;
            }
            // 获取机柜中对应的机笼和插槽
            var cage = cabinet.Cages.FirstOrDefault(c => c.Index == point.Cage);
            var slot = cage?.Slots.FirstOrDefault(s => s.Index == point.Slot);

            if (cage == null || slot == null)
            {
                // 如果找不到对应的机笼或插槽，将点添加到未分配列表
                if (string.IsNullOrEmpty(point.UnsetReason))
                {
                    point.UnsetReason = $"未找到机笼或插槽（Cage={point.Cage}, Slot={point.Slot}）";
                }
                cabinet.UnsetPoints.Add(point);
                continue;
            }

            // 如果插槽中没有板卡，创建一个新的板卡
            if (slot.Board == null)
            {
                var config = configs.SingleOrDefault(c => c.IoCardType == point.CardType);
                if (config == null) throw new Exception($"IO卡型号配置表中缺少{point.CardType}这种板卡类型，请检查输入数据或者公式");
                
                // 根据信号类型决定创建哪种板卡
                // 判断逻辑：IO类型包含FF + FFSlaveModuleModel不为空 → 从站箱
                bool isFFSlave = !string.IsNullOrEmpty(point.IoType) &&
                                 point.IoType.Contains("FF", StringComparison.OrdinalIgnoreCase) &&
                                 !string.IsNullOrEmpty(point.FFSlaveModuleModel);
                
                // 判断是否为FF总线箱
                bool isFFBus = !string.IsNullOrEmpty(point.IoType) &&
                               point.IoType.Contains("FF", StringComparison.OrdinalIgnoreCase) &&
                               string.IsNullOrEmpty(point.FFSlaveModuleModel);
                
                // 判断是否为通讯板卡
                bool isCommunication = point.CardType == "MD211" || 
                                      point.CardType == "MD216" || 
                                      point.CardType == "DP211";
                
                if (isFFSlave) {
                    slot.Board = Board.CreateFFSlave(config);
                                    
                    // 根据信号是否已分配FF从站模块来决定创建哪种模块
                    // 先找到该板卡上的所有FF从站信号
                    var ffSlaveSignalsOnThisBoard = points
                        .Where(p => p.CabinetNumber == point.CabinetNumber && 
                                    p.Cage == point.Cage && 
                                    p.Slot == point.Slot &&
                                    !string.IsNullOrEmpty(p.FFSlaveModuleModel))
                        .ToList();
                                    
                    // 判断是否已经FF从站模块分配（有FFDPStaionNumber字段）
                    bool hasAllocatedModules = ffSlaveSignalsOnThisBoard.Any(s => !string.IsNullOrEmpty(s.FFDPStaionNumber));
                                    
                    int channelsPerNetwork = config.PinsCount / 2;
                    var net1 = slot.Board.Networks.FirstOrDefault(n => n.NetworkType == Xt2NetType.Net1);
                    var net2 = slot.Board.Networks.FirstOrDefault(n => n.NetworkType == Xt2NetType.Net2);
                                    
                    if (hasAllocatedModules)
                    {
                        // 已分配：根据站号和模块型号重建实际模块
                        // 按网段和站号分组
                        var moduleGroups = ffSlaveSignalsOnThisBoard
                            .Where(s => !string.IsNullOrEmpty(s.FFDPStaionNumber))
                            .GroupBy(s => new { NetType = s.NetType ?? "Net1", StationNumber = s.FFDPStaionNumber })
                            .OrderBy(g => g.Key.NetType)
                            .ThenBy(g => int.TryParse(g.Key.StationNumber, out var num) ? num : 0);
                                        
                        foreach (var moduleGroup in moduleGroups)
                        {
                            var firstSignal = moduleGroup.First();
                            var network = moduleGroup.Key.NetType == "Net2" ? net2 : net1;
                                            
                            if (network != null)
                            {
                                // 直接使用FFSlaveModuleModel作为模块型号（已经是纯型号，如"FS201"）
                                string moduleType = firstSignal.FFSlaveModuleModel?.Trim() ?? "未分配";
                                int stationNumber = int.TryParse(moduleGroup.Key.StationNumber, out var num) ? num : 0;
                                
                                // 创建模块（会根据型号自动确定固定通道数）
                                var module = FFSlaveModule.Create(moduleType, stationNumber, 0);
                                System.Diagnostics.Debug.WriteLine($"创建FF从站模块: {moduleType}, 站号:{stationNumber}, 通道数:{module.FFSlaveChannels.Count}");
                                network.Modules.Add(module);
                            }
                        }
                    }
                    else
                    {
                        // 未分配：不创建模块，信号将直接放入UnallocatedSignals
                        // （网段已初始化，Modules为空列表）
                    }
                } else if (isFFBus) {
                    slot.Board = Board.CreateFFBus(config);
                } else if (isCommunication) {
                    // 创建通讯板卡
                    slot.Board = Board.CreateCommunication(config);
                } else {
                    slot.Board = Board.Create(config);
                }
            }

            // 如果板卡类型不匹配，将点添加到未分配列表
            if (slot.Board.Type != point.CardType)
            {
                cabinet.UnsetPoints.Add(point);
                continue;
            }

            // 分配信号到板卡通道（支持FF板卡、通讯板卡和普通板卡）
            if (slot.Board.FFBoardType == BoardType.Normal)
            {
                // 普通板卡：直接使用Channels
                if (point.Channel <= 0 || point.Channel > slot.Board.Channels.Count)
                {
                    cabinet.UnsetPoints.Add(point);
                    continue;
                }
                var channel = slot.Board.Channels[point.Channel - 1];
                if (channel.Point == null)
                {
                    channel.Point = point;
                }
                else
                {
                    cabinet.UnsetPoints.Add(point);
                }
            }
            else if (slot.Board.FFBoardType == BoardType.Communication)
            {
                // 通讯板卡：使用CommPorts.VirtualSignals
                if (point.Channel <= 0)
                {
                    cabinet.UnsetPoints.Add(point);
                    continue;
                }
                
                // 遍历所有端口查找对应的虚拟信号槽
                bool allocated = false;
                foreach (var port in slot.Board.CommPorts)
                {
                    var virtualSignal = port.VirtualSignals.FirstOrDefault(vs => vs.Index == point.Channel);
                    if (virtualSignal != null && virtualSignal.Signal == null)
                    {
                        virtualSignal.Signal = point;
                        allocated = true;
                        break;
                    }
                }
                
                if (!allocated)
                {
                    cabinet.UnsetPoints.Add(point);
                }
            }
            else
            {
                // FF板卡：根据板卡类型使用不同的通道结构
                var networkType = point.NetType == "Net2" ? Xt2NetType.Net2 : Xt2NetType.Net1;
                var network = slot.Board.Networks.FirstOrDefault(n => n.NetworkType == networkType);
                
                if (network == null)
                {
                    point.UnsetReason = $"板卡网段初始化失败（NetType={point.NetType}）";
                    cabinet.UnsetPoints.Add(point);
                    continue;
                }
                
                if (slot.Board.FFBoardType == BoardType.FFBus)
                {
                    // FF总线箱：使用FFBusChannels
                    if (point.Channel <= 0 || point.Channel > network.FFBusChannels.Count)
                    {
                        cabinet.UnsetPoints.Add(point);
                        continue;
                    }
                    
                    var channel = network.FFBusChannels[point.Channel - 1];
                    if (channel.Point == null)
                    {
                        channel.Point = point;
                    }
                    else
                    {
                        cabinet.UnsetPoints.Add(point);
                    }
                }
                else if (slot.Board.FFBoardType == BoardType.FFSlave)
                {
                    // FF从站箱：判断是否已FF分配（有有效的FFDPStaionNumber字段）
                    // 注意：FFDPStaionNumber是字符串，"01", "02"等才是有效值
                    if (string.IsNullOrWhiteSpace(point.FFDPStaionNumber))
                    {
                        // ✅ 未FF分配：放入UnallocatedSignals（显示在网段下）
                        network.UnallocatedSignals.Add(point);
                        continue;
                    }
                    
                    // ✅ 已FF分配：根据FFDPStaionNumber找到对应模块，分配到通道
                    // 根据FFDPStaionNumber找到对应的模块
                    FFSlaveModule? module = null;
                    if (!string.IsNullOrEmpty(point.FFDPStaionNumber) && int.TryParse(point.FFDPStaionNumber, out var stationNum))
                    {
                        module = network.Modules.FirstOrDefault(m => m.StationNumber == stationNum);
                    }
                    
                    if (module == null)
                    {
                        cabinet.UnsetPoints.Add(point);
                        continue;
                    }
                    
                    // 仅使用FFTerminalChannel，不回退到Channel（两者含义不同）
                    if (!point.FFTerminalChannel.HasValue)
                    {
                        cabinet.UnsetPoints.Add(point);
                        continue;
                    }
                    
                    int channelIndex = point.FFTerminalChannel.Value;
                    if (channelIndex <= 0 || channelIndex > module.FFSlaveChannels.Count)
                    {
                        cabinet.UnsetPoints.Add(point);
                        continue;
                    }
                    
                    var channel = module.FFSlaveChannels[channelIndex - 1];
                    if (channel.Point == null)
                    {
                        channel.Point = point;
                    }
                    else
                    {
                        cabinet.UnsetPoints.Add(point);
                    }
                }
            }
        }

        return [.. cabinets.Values];
    }

    public static List<IoFullData> CabinetStructureToPoint(IEnumerable<StdCabinet> cabinets)
    {
        var points = new List<IoFullData>();

        foreach (var cabinet in cabinets)
        {
            // 处理正常插槽
            foreach (var cage in cabinet.Cages)
            {
                foreach (var slot in cage.Slots)
                {
                    if (slot.Board != null)
                    {
                        // 获取所有信号（支持FF板卡和普通板卡）
                        foreach (var signal in StdCabinet.GetAllSignals(slot.Board))
                        {
                            signal.CabinetNumber = cabinet.Name;
                            signal.Cage = cage.Index;
                            signal.Slot = slot.Index;
                            signal.CardType = slot.Board.Type;
                            points.Add(signal);
                        }
                    }
                }
            }

            // 处理虚拟插槽
            foreach (var virtualSlot in cabinet.VirtualSlots)
            {
                if (virtualSlot.Board != null)
                {
                    // 获取所有信号（支持FF板卡和普通板卡）
                    foreach (var signal in StdCabinet.GetAllSignals(virtualSlot.Board))
                    {
                        signal.CabinetNumber = cabinet.Name;
                        signal.Cage = -1; // 虚拟插槽标记
                        signal.Slot = virtualSlot.Index;
                        signal.CardType = virtualSlot.Board.Type;
                        points.Add(signal);
                    }
                }
            }

            // 处理未分配的点
            foreach (var item in cabinet.UnsetPoints)
            {
                item.CabinetNumber = cabinet.Name;
                points.Add(item);
            }
        }

        return points;
    }

    // 4. 修改转换方法（支持FF板卡）
    public static List<IoFullData> ToPoint(this StdCabinet cabinet)
    {
        var points = new List<IoFullData>();

        // 处理所有插槽（包括虚拟插槽）
        foreach (var slot in cabinet.AllSlots)
        {
            if (slot.Board != null)
            {
                // 获取板卡的所有信号（支持FF板卡和普通板卡）
                foreach (var signal in StdCabinet.GetAllSignals(slot.Board))
                {
                    signal.CabinetNumber = cabinet.Name;
                    signal.CardType = slot.Board.Type;

                    if (slot.IsVirtual)
                    {
                        // 虚拟插槽，使用负数标记
                        signal.Cage = -1;
                        signal.Slot = slot.Index; // 保持虚拟插槽的索引
                    }
                    else
                    {
                        // 正常插槽
                        var cage = cabinet.Cages.First(c => c.Slots.Contains(slot));
                        signal.Cage = cage.Index;
                        signal.Slot = slot.Index;
                    }

                    points.Add(signal);
                }
            }
        }

        // 处理未分配的点
        foreach (var item in cabinet.UnsetPoints)
        {
            item.CabinetNumber = cabinet.Name;
            points.Add(item);
        }

        return points;
    }
    public static List<IoFullData> Recalc(IEnumerable<IoFullData> data)
    {
        var points = new List<IoFullData>();

        foreach (var cabinet in data.GroupBy(d=>d.CabinetNumber))
        {
            int doTerminalNumber = 111;
            int diTerminalNumber = 211;
            int aiTerminalNumber = 311;
            int aoTerminalNumber = 411;

            foreach (var cage in cabinet.GroupBy(d=>d.Cage))
            {
                foreach (var slot in cage.GroupBy(c=>c.Slot))
                {
                    var pInSlot = slot.FirstOrDefault();
                    if (pInSlot!=null&&pInSlot.CardType != null)
                    {
                        string terminalBoardNumber = null;

                        // 根据卡件类型分配端子板编号
                        switch (pInSlot.CardType.Substring(0, 2))
                        {
                            case "DO":
                                terminalBoardNumber = $"{doTerminalNumber}BN";
                                doTerminalNumber++;
                                break;
                            case "DI":
                                terminalBoardNumber = $"{diTerminalNumber}BN";
                                diTerminalNumber++;
                                break;
                            case "AI":
                                terminalBoardNumber = $"{aiTerminalNumber}BN";
                                aiTerminalNumber++;
                                break;
                            case "AO":
                                terminalBoardNumber = $"{aoTerminalNumber}BN";
                                aoTerminalNumber++;
                                break;
                        }

                        foreach (var point in slot)
                        {
                            if (point != null)
                            {
                                point.TerminalBoardNumber = terminalBoardNumber;

                                // 根据卡件类型分配端子号
                                if (point.IoType == "AO")
                                {
                                    point.SignalPlus = $"I{point.Channel}+";
                                    point.SignalMinus = $"I{point.Channel}-";
                                }
                                else
                                {
                                    point.SignalPlus = $"A{(point.Channel):D2}";
                                    point.SignalMinus = $"B{(point.Channel):D2}";
                                }

                                points.Add(point);
                            }
                        }
                    }
                }
            }           
        }
        return points;
    }

    public static List<IoFullData> ToPoint(this IEnumerable<StdCabinet> cabinets)
    {
        var points = new List<IoFullData>();

        foreach (var cabinet in cabinets)
        {
            // 处理正常插槽
            foreach (var cage in cabinet.Cages)
            {
                foreach (var slot in cage.Slots)
                {
                    if (slot.Board != null)
                    {
                        // 根据板卡类型处理
                        if (slot.Board.FFBoardType == BoardType.Normal)
                        {
                            // 普通板卡：遍历Channels
                            foreach (var channel in slot.Board.Channels)
                            {
                                if (channel.Point != null)
                                {
                                    channel.Point.CabinetNumber = cabinet.Name;
                                    channel.Point.Cage = cage.Index;
                                    channel.Point.Slot = slot.Index;
                                    channel.Point.CardType = slot.Board.Type;
                                    channel.Point.Channel = channel.Index;
                                    points.Add(channel.Point);
                                }
                            }
                        }
                        else if (slot.Board.FFBoardType == BoardType.Communication)
                        {
                            // 通讯板卡：遍历CommPorts和VirtualSignals
                            foreach (var port in slot.Board.CommPorts)
                            {
                                foreach (var virtualSignal in port.VirtualSignals)
                                {
                                    if (virtualSignal.Signal != null)
                                    {
                                        virtualSignal.Signal.CabinetNumber = cabinet.Name;
                                        virtualSignal.Signal.Cage = cage.Index;
                                        virtualSignal.Signal.Slot = slot.Index;
                                        virtualSignal.Signal.CardType = slot.Board.Type;
                                        virtualSignal.Signal.Channel = virtualSignal.Index;
                                        points.Add(virtualSignal.Signal);
                                    }
                                }
                            }
                        }
                        else if (slot.Board.FFBoardType == BoardType.FFBus)
                        {
                            // FF总线箱：遍历FFBusChannels
                            foreach (var network in slot.Board.Networks)
                            {
                                foreach (var channel in network.FFBusChannels)
                                {
                                    if (channel.Point != null)
                                    {
                                        channel.Point.CabinetNumber = cabinet.Name;
                                        channel.Point.Cage = cage.Index;
                                        channel.Point.Slot = slot.Index;
                                        channel.Point.CardType = slot.Board.Type;
                                        channel.Point.NetType = network.NetworkType.ToString();
                                        // ✅ FF从站箱：不覆盖Channel字段，保持IO分配时的状态
                                        // channel.Point.Channel = channel.Index; // 已移除
                                        points.Add(channel.Point);
                                    }
                                }
                            }
                        }
                        else if (slot.Board.FFBoardType == BoardType.FFSlave)
                        {
                            // FF从站箱：遍历FFSlaveChannels 和 UnallocatedSignals
                            foreach (var network in slot.Board.Networks)
                            {
                                // 1. 已分配的信号（在模块通道中）
                                foreach (var module in network.Modules)
                                {
                                    foreach (var channel in module.FFSlaveChannels)
                                    {
                                        if (channel.Point != null)
                                        {
                                            channel.Point.CabinetNumber = cabinet.Name;
                                            channel.Point.Cage = cage.Index;
                                            channel.Point.Slot = slot.Index;
                                            channel.Point.CardType = slot.Board.Type;
                                            channel.Point.NetType = network.NetworkType.ToString();
                                            // ✅ FF从站箱：不覆盖Channel字段，保持IO分配时的状态
                                            // channel.Point.Channel = channel.Index; // 已移除
                                            points.Add(channel.Point);
                                        }
                                    }
                                }
                                
                                // 2. 未分配的信号（在UnallocatedSignals中）
                                foreach (var signal in network.UnallocatedSignals)
                                {
                                    signal.CabinetNumber = cabinet.Name;
                                    signal.Cage = cage.Index;
                                    signal.Slot = slot.Index;
                                    signal.CardType = slot.Board.Type;
                                    signal.NetType = network.NetworkType.ToString();
                                    // Channel = 0, FFTerminalChannel = null (保持默认值)
                                    points.Add(signal);
                                }
                            }
                        }
                    }
                }
            }

            // 处理虚拟插槽
            foreach (var virtualSlot in cabinet.VirtualSlots)
            {
                if (virtualSlot.Board != null)
                {
                    // 根据板卡类型处理
                    if (virtualSlot.Board.FFBoardType == BoardType.Normal)
                    {
                        // 普通板卡：遍历Channels
                        foreach (var channel in virtualSlot.Board.Channels)
                        {
                            if (channel.Point != null)
                            {
                                channel.Point.CabinetNumber = cabinet.Name;
                                channel.Point.Cage = -1; // 虚拟插槽标记
                                channel.Point.Slot = virtualSlot.Index;
                                channel.Point.CardType = virtualSlot.Board.Type;
                                channel.Point.Channel = channel.Index;
                                points.Add(channel.Point);
                            }
                        }
                    }
                    else if (virtualSlot.Board.FFBoardType == BoardType.Communication)
                    {
                        // 通讯板卡：遍历CommPorts和VirtualSignals
                        foreach (var port in virtualSlot.Board.CommPorts)
                        {
                            foreach (var virtualSignal in port.VirtualSignals)
                            {
                                if (virtualSignal.Signal != null)
                                {
                                    virtualSignal.Signal.CabinetNumber = cabinet.Name;
                                    virtualSignal.Signal.Cage = -1; // 虚拟插槽标记
                                    virtualSignal.Signal.Slot = virtualSlot.Index;
                                    virtualSignal.Signal.CardType = virtualSlot.Board.Type;
                                    virtualSignal.Signal.Channel = virtualSignal.Index;
                                    points.Add(virtualSignal.Signal);
                                }
                            }
                        }
                    }
                    else if (virtualSlot.Board.FFBoardType == BoardType.FFBus)
                    {
                        // FF总线箱：遍历FFBusChannels
                        foreach (var network in virtualSlot.Board.Networks)
                        {
                            foreach (var channel in network.FFBusChannels)
                            {
                                if (channel.Point != null)
                                {
                                    channel.Point.CabinetNumber = cabinet.Name;
                                    channel.Point.Cage = -1; // 虚拟插槽标记
                                    channel.Point.Slot = virtualSlot.Index;
                                    channel.Point.CardType = virtualSlot.Board.Type;
                                    channel.Point.NetType = network.NetworkType.ToString();
                                    channel.Point.Channel = channel.Index;
                                    points.Add(channel.Point);
                                }
                            }
                        }
                    }
                    else if (virtualSlot.Board.FFBoardType == BoardType.FFSlave)
                    {
                        // FF从站箱：遍历FFSlaveChannels 和 UnallocatedSignals
                        foreach (var network in virtualSlot.Board.Networks)
                        {
                            // 1. 已分配的信号（在模块通道中）
                            foreach (var module in network.Modules)
                            {
                                foreach (var channel in module.FFSlaveChannels)
                                {
                                    if (channel.Point != null)
                                    {
                                        channel.Point.CabinetNumber = cabinet.Name;
                                        channel.Point.Cage = -1; // 虚拟插槽标记
                                        channel.Point.Slot = virtualSlot.Index;
                                        channel.Point.CardType = virtualSlot.Board.Type;
                                        channel.Point.NetType = network.NetworkType.ToString();
                                        // ✅ FF从站箱：不覆盖Channel字段，保持IO分配时的状态
                                        // channel.Point.Channel = channel.Index; // 已移除
                                        points.Add(channel.Point);
                                    }
                                }
                            }
                            
                            // 2. 未分配的信号（在UnallocatedSignals中）
                            foreach (var signal in network.UnallocatedSignals)
                            {
                                signal.CabinetNumber = cabinet.Name;
                                signal.Cage = -1; // 虚拟插槽标记
                                signal.Slot = virtualSlot.Index;
                                signal.CardType = virtualSlot.Board.Type;
                                signal.NetType = network.NetworkType.ToString();
                                // Channel = 0, FFTerminalChannel = null (保持默认值)
                                points.Add(signal);
                            }
                        }
                    }
                }
            }

            // 处理未分配的点
            foreach (var item in cabinet.UnsetPoints)
            {
                item.CabinetNumber = cabinet.Name;
                points.Add(item);
            }
        }

        return points;
    }



    // 修改需要更新的方法（支持FF板卡）
    public static void RemovePoints(this StdCabinet cabinet, TagType type)
    {
        // 处理正常插槽
        foreach (var cage in cabinet.Cages)
        {
            foreach (var slot in cage.Slots)
            {
                if (slot.Board != null)
                {
                    foreach (var channel in StdCabinet.GetAllChannels(slot.Board))
                    {
                        if (channel.Point != null && channel.Point.PointType == type)
                        {
                            channel.Point = null;
                        }
                    }
                }
            }
        }

        // 处理虚拟插槽
        foreach (var virtualSlot in cabinet.VirtualSlots)
        {
            if (virtualSlot.Board != null)
            {
                foreach (var channel in StdCabinet.GetAllChannels(virtualSlot.Board))
                {
                    if (channel.Point != null && channel.Point.PointType == type)
                    {
                        channel.Point = null;
                    }
                }
            }
        }

        cabinet.UnsetPoints.RemoveWhere(c => c.PointType == type);
    }


    #region 子项统计信息
    //机柜数量

    public static NumberCheck CountTotalPoints(IEnumerable<StdCabinet> cabinets)
    {
        var totalPoints = cabinets.SelectMany(cabinet => cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .SelectMany(slot => StdCabinet.GetAllChannels(slot.Board!)
                    .Where(channel => channel.Point != null))))
            .Count();

        var numberCheck = new NumberCheck {
            Number = totalPoints,
            Illegal = totalPoints > 0 // 根据您的需求设置这个属性
        };

        return numberCheck;
    }
    public static NumberCheck CountBackupPoints(IEnumerable<StdCabinet> cabinets)
    {
        var backupPoints = cabinets.SelectMany(cabinet => cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .SelectMany(slot => StdCabinet.GetAllChannels(slot.Board!)
                    .Where(channel => channel.Point != null && channel.Point.PointType == TagType.BackUp))))
            .Count();

        var numberCheck = new NumberCheck
        {
            Number = backupPoints,
            Illegal = backupPoints > 0 // 根据您的需求设置这个属性
        };

        return numberCheck;
    }
    public static NumberCheck CountAlarmPoints(IEnumerable<StdCabinet> cabinets)
    {
        NumberCheck numberCheck = new NumberCheck
        {
            Number = cabinets.SelectMany(cabinet => cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .SelectMany(slot => StdCabinet.GetAllChannels(slot.Board!)
                    .Where(channel => channel.Point != null && channel.Point.PointType == TagType.Alarm))))
            .Count()
        };
        numberCheck.Illegal = numberCheck.Number == cabinets.Count() * 7;// 根据您的需求设置这个属性
        return numberCheck;
    }
    public static NumberCheck CountNormalPoints(IEnumerable<StdCabinet> cabinets)
    {
        int normalPoints = cabinets.SelectMany(cabinet => cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .SelectMany(slot => StdCabinet.GetAllChannels(slot.Board!)
                    .Where(channel => channel.Point != null && channel.Point.PointType == TagType.Normal))))
            .Count();

        NumberCheck numberCheck = new NumberCheck
        {
            Number = normalPoints,
            Illegal = normalPoints > 0 // 根据您的需求设置这个属性
        };

        return numberCheck;
    }
    public static List<CardSpareRate> CalculateRedundancyRate(IEnumerable<StdCabinet> cabinets)
    {
        return cabinets.SelectMany(cabinet => cabinet.Cages
                .SelectMany(cage => cage.Slots
                    .Where(slot => slot.Board != null)
                    .Select(slot => new { 
                        BoardType = slot.Board!.Type, 
                        Channels = StdCabinet.GetAllChannels(slot.Board).ToList()
                    })
                    .Select(x => new { 
                        x.BoardType, 
                        TotalChannels = x.Channels.Count, 
                        UnusedChannels = x.Channels.Count(channel => channel.Point == null) 
                    }))
            )
            .GroupBy(x => x.BoardType)
            .Select(grp => new CardSpareRate
            {
                CardType = grp.Key,
                Rate = Math.Round(grp.Sum(x => x.UnusedChannels) * 100.0 / grp.Sum(x => x.TotalChannels), 1)
            })
            .ToList();
    }

    public static TotalSummaryInfo GetTotalSummaryInfo(List<IoFullData> data,List<config_card_type_judge> configs) {
        //把Iofulldata 转换成cabinets

        IEnumerable<StdCabinet> cabinets = BuildCabinetStructureOther(data, configs);
        return new TotalSummaryInfo {
            TotalPoints = CountTotalPoints(cabinets),
            BackupPoints = CountBackupPoints(cabinets),
            AlarmPoints = CountAlarmPoints(cabinets),
            NormalPoints = CountNormalPoints(cabinets),
            RedundancyRates = CalculateRedundancyRate(cabinets)
        };
    }
    #endregion

    #region 单个机柜
    public static NumberCheck CalculateTotalPoints(StdCabinet cabinet) {
        NumberCheck numberCheck = new NumberCheck
        {
            Number = cabinet.Cages
            .SelectMany(cage => cage.Slots
             .Where(slot => slot.Board != null)
             .SelectMany(slot => StdCabinet.GetAllChannels(slot.Board!))
             .Where(channel => channel.Point != null))
         .Count(),
            Illegal = true
        };
        return numberCheck;
    }

    public static NumberCheck CalculateBackupPoints(StdCabinet cabinet) {
        NumberCheck numberCheck = new NumberCheck
        {
            Number = cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .SelectMany(slot => StdCabinet.GetAllChannels(slot.Board!)))
            .Count(channel => channel.Point?.PointType == TagType.BackUp),
            Illegal = true
        };
        return numberCheck;
    }

    public static NumberCheck CalculateAlarmPoints(StdCabinet cabinet)
    {
        NumberCheck numberCheck = new NumberCheck
        {
            Number = cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .SelectMany(slot => StdCabinet.GetAllChannels(slot.Board!)))
            .Count(channel => channel.Point?.PointType == TagType.Alarm),
        };
        numberCheck.Illegal = numberCheck.Number == 7;
        return numberCheck;
    }

    public static NumberCheck CalculateNormalPoints(StdCabinet cabinet) {
        NumberCheck numberCheck = new NumberCheck
        {
            Number = cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .SelectMany(slot => StdCabinet.GetAllChannels(slot.Board!)))
            .Count(channel => channel.Point?.PointType == TagType.Normal),
            Illegal = true
        };
        return numberCheck;
    }

    public static NumberCheck CalculateUnsetPoints(StdCabinet cabinet) {
        NumberCheck numberCheck = new NumberCheck
        {
            Number = cabinet.UnsetPoints.Count,       
        };
        numberCheck.Illegal = numberCheck.Number == 0;
        return numberCheck;       
    }

    public static NumberCheck CalculateUnsetBoards(StdCabinet cabinet)
    {
        NumberCheck numberCheck = new NumberCheck
        {
            Number = cabinet.VirtualSlots.Count(vs => vs.Board != null),
        };
        numberCheck.Illegal = numberCheck.Number == 0;
        return numberCheck;
    }

    public static List<CardSpareRate> CalculateRedundancyRates(StdCabinet cabinet)
    {
        var redundancyRates = new List<CardSpareRate>();

        // 计算每种卡类型的总通道数
        var totalChannelsByCardType = cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .Select(slot => new { CardType = slot.Board!.Type, Channels = StdCabinet.GetAllChannels(slot.Board).ToList() }))
            .SelectMany(x => x.Channels.Select(channel => new { x.CardType, Channel = channel }))
            .GroupBy(x => x.CardType)
            .ToDictionary(group => group.Key ?? "Unknown", group => group.Count());

        // 计算每种卡类型的空余通道数
        var unusedChannelsByCardType = cabinet.Cages
            .SelectMany(cage => cage.Slots
                .Where(slot => slot.Board != null)
                .Select(slot => new { CardType = slot.Board!.Type, Channels = StdCabinet.GetAllChannels(slot.Board).ToList() }))
            .SelectMany(x => x.Channels.Where(channel => channel.Point == null).Select(channel => new { x.CardType, Channel = channel }))
            .GroupBy(x => x.CardType)
            .ToDictionary(group => group.Key ?? "Unknown", group => group.Count());

        // 计算冗余率并判断是否超过指定比率
        foreach (var cardType in totalChannelsByCardType.Keys)
        {
            var totalChannels = totalChannelsByCardType[cardType];
            var unusedChannels = unusedChannelsByCardType.ContainsKey(cardType) ? unusedChannelsByCardType[cardType] : 0;
            var redundancyRate = totalChannels > 0 ? Math.Round(unusedChannels * 100.0 / totalChannels, 1) : 0;

            redundancyRates.Add(new CardSpareRate
            {
                CardType = cardType,
                Rate = redundancyRate
            });
        }

        return redundancyRates;
    }

    public static CabinetSummaryInfo GetCabinetSummaryInfo(StdCabinet cabinet) {
        return new CabinetSummaryInfo
        {
            TotalPoints = CalculateTotalPoints(cabinet),
            BackupPoints = CalculateBackupPoints(cabinet),
            AlarmPoints = CalculateAlarmPoints(cabinet),
            NormalPoints = CalculateNormalPoints(cabinet),
            UnsetPoints = CalculateUnsetPoints(cabinet),
            UnsetBoards = CalculateUnsetBoards(cabinet),
            RedundancyRates = CalculateRedundancyRates(cabinet)
        };
    }
    #endregion

}

public class CardSpareRate {
    public required string CardType { get; set; }
    public required double Rate { get; set; }
}

public class TotalSummaryInfo {
    public required NumberCheck TotalPoints { get; set; }
    public required NumberCheck BackupPoints { get; set; }
    public required NumberCheck AlarmPoints { get; set; }
    public required NumberCheck NormalPoints { get; set; }
    public required List<CardSpareRate> RedundancyRates { get; set; }
}

public class CabinetSummaryInfo {
    public required NumberCheck TotalPoints { get; set; }
    public required NumberCheck BackupPoints { get; set; }
    public required NumberCheck AlarmPoints { get; set; }
    public required NumberCheck NormalPoints { get; set; }
    public required NumberCheck UnsetPoints { get; set; }
    public required NumberCheck UnsetBoards { get; set; }
    public required List<CardSpareRate> RedundancyRates { get; set; }
}

public class NumberCheck
{
    public double Number { get; set; }

    public bool Illegal { get; set; }
}

/// <summary>FF板卡辅助类</summary>
public static class FFBoardHelper
{
    /// <summary>为FF总线板卡的网段添加信号</summary>
    /// <param name="board">FF板卡</param>
    /// <param name="networkType">网段类型</param>
    /// <param name="signal">信号</param>
    /// <returns>是否添加成功</returns>
    public static bool AddSignalToFFBusNetwork(Board board, Xt2NetType networkType, IoFullData signal)
    {
        if (board.FFBoardType != BoardType.FFBus)
            return false;

        var network = board.Networks.FirstOrDefault(n => n.NetworkType == networkType);
        if (network == null)
            return false;

        var emptyChannel = network.FFBusChannels.FirstOrDefault(c => c.Point == null);
        if (emptyChannel == null)
            return false;

        emptyChannel.Point = signal;
        signal.NetType = networkType.ToString();
        return true;
    }

    /// <summary>为FF从站板卡添加模块</summary>
    /// <param name="board">FF从站板卡</param>
    /// <param name="networkType">网段类型</param>
    /// <param name="moduleType">模块型号</param>
    /// <param name="stationNumber">从站号</param>
    /// <param name="channelCount">通道数</param>
    /// <returns>创建的模块</returns>
    public static FFSlaveModule AddModuleToFFSlaveNetwork(Board board, Xt2NetType networkType, string moduleType, int stationNumber, int channelCount)
    {
        if (board.FFBoardType != BoardType.FFSlave)
            throw new InvalidOperationException("只能为FF从站板卡添加模块");

        var network = board.Networks.FirstOrDefault(n => n.NetworkType == networkType);
        if (network == null)
            throw new InvalidOperationException($"找不到网段{networkType}");

        var module = FFSlaveModule.Create(moduleType, stationNumber, channelCount);
        network.Modules.Add(module);
        return module;
    }

    /// <summary>为FF从站模块添加信号</summary>
    /// <param name="module">FF从站模块</param>
    /// <param name="signal">信号</param>
    /// <returns>是否添加成功</returns>
    public static bool AddSignalToFFSlaveModule(FFSlaveModule module, IoFullData signal)
    {
        var emptyChannel = module.FFSlaveChannels.FirstOrDefault(c => c.Point == null);
        if (emptyChannel == null)
            return false;

        emptyChannel.Point = signal;
        signal.FFDPStaionNumber = module.StationNumber.ToString("D2");
        signal.FFTerminalChannel = emptyChannel.Index;
        return true;
    }

    /// <summary>获取FF板卡的网段剩余通道数</summary>
    /// <param name="board">FF板卡</param>
    /// <param name="networkType">网段类型</param>
    /// <returns>剩余通道数</returns>
    public static int GetNetworkRemainingChannels(Board board, Xt2NetType networkType)
    {
        if (board.FFBoardType == BoardType.Normal)
            return 0;

        var network = board.Networks.FirstOrDefault(n => n.NetworkType == networkType);
        if (network == null)
            return 0;

        if (board.FFBoardType == BoardType.FFSlave)
        {
            // FF从站：计算所有模块的空闲通道总数
            return network.Modules.Sum(m => m.FFSlaveChannels.Count(c => c.Point == null));
        }
        else if (board.FFBoardType == BoardType.FFBus)
        {
            // FF总线：直接计算网段的空闲通道数
            return network.FFBusChannels.Count(c => c.Point == null);
        }
        
        return 0;
    }

    /// <summary>获取FF从站网段的模块统计信息</summary>
    /// <param name="board">FF从站板卡</param>
    /// <param name="networkType">网段类型</param>
    /// <returns>模块统计信息（模块型号 -> 数量）</returns>
    public static Dictionary<string, int> GetFFSlaveNetworkModuleSummary(Board board, Xt2NetType networkType)
    {
        if (board.FFBoardType != BoardType.FFSlave)
            return new Dictionary<string, int>();

        var network = board.Networks.FirstOrDefault(n => n.NetworkType == networkType);
        if (network == null)
            return new Dictionary<string, int>();

        return network.Modules
            .GroupBy(m => m.ModuleType)
            .ToDictionary(g => g.Key, g => g.Count());
    }
    
    /// <summary>从FFSlaveModuleModel字段中提取模块型号</summary>
    /// <param name="ffSlaveModuleModel">模块型号字段（如"FS201*2"）</param>
    /// <returns>提取的模块型号（如"FS201"）</returns>
    public static string ExtractModuleType(string? ffSlaveModuleModel)
    {
        if (string.IsNullOrEmpty(ffSlaveModuleModel))
            return "未分配";
        
        // 移除数量信息（*后面的部分）
        var parts = ffSlaveModuleModel.Split('*');
        return parts[0].Trim();
    }
}

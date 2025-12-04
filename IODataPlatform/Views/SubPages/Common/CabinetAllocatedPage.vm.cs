using Aspose.Pdf;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using System.Windows.Forms;

namespace IODataPlatform.Views.SubPages.Common;

public partial class CabinetAllocatedViewModel(SqlSugarContext context, GlobalModel model, IMessageService message,DepXT2ViewModel xt2,DepAQJViewModel aqj, NavigationParameterService parameterService) : ObservableObject, INavigationAware {


    #region 传过来的属性   
    private ControlSystem controlSystem;

    #endregion

    /// <summary>脏数据标记，用于追踪是否有未保存的修改</summary>
    private bool isDirty = false;

    /// <summary>全部机柜</summary>
    [ObservableProperty]
    private List<StdCabinet>? cabinets;

    [ObservableProperty]
    private StdCabinet? cabinet;//当前机柜

    /// <summary>查看的板</summary>
    [ObservableProperty]
    private Board? viewBoard;
    
    /// <summary>选中的网段</summary>
    [ObservableProperty]
    private FFNetwork? selectedNetwork;
    
    /// <summary>选中的模块</summary>
    [ObservableProperty]
    private FFSlaveModule? selectedModule;
    
    /// <summary>选中的板卡</summary>
    [ObservableProperty]
    private Board? selectedBoard;

    /// <summary>数据库中全部板卡信息</summary>
    [ObservableProperty]
    private List<config_card_type_judge>? boardOptions;

    /// <summary> 冗余率 </summary>
    [ObservableProperty]
    private int redundancyRate = 20;

    /// <summary>
    /// 获取默认显示字段列表（根据控制系统类型）
    /// </summary>
    public List<string> GetDefaultField()
    {
        return controlSystem switch
        {
            ControlSystem.龙鳍 => xt2.GetDefaultField(),
            ControlSystem.中控 => xt2.GetDefaultField(),
            ControlSystem.龙核 => aqj.GetDefaultField(),
            ControlSystem.安全级模拟系统 => aqj.GetDefaultField(),
            _ => new List<string>()
        };
    }


    public async void OnNavigatedTo()
    {
        // 确保参数不为 null，并检查参数类型是否正确
        var value = parameterService.GetParameter<ControlSystem>("controlSystem");
        this.controlSystem = value;       
        BoardOptions = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
        switch (controlSystem)
        {
            case ControlSystem.龙鳍:
                _ = xt2.AllData ?? throw new("没数据");
                Cabinets = xt2.AllData.ToList().BuildCabinetStructureOther(BoardOptions);
                break;
            case ControlSystem.中控:
                _ = xt2.AllData ?? throw new("没数据");
                Cabinets = xt2.AllData.ToList().BuildCabinetStructureOther(BoardOptions);                
                break;
            case ControlSystem.龙核:
                _ = aqj.AllData ?? throw new("没数据");
                Cabinets = aqj.AllData.ToList().BuildCabinetStructureLH(BoardOptions);
                break;
            case ControlSystem.一室:
                break;
            default: break;
        }
        await Task.Delay(200);
        Messengers.FullScreen.OnNext(true);
    }
    public async void OnNavigatedFrom()
    {
        // 只有存在未保存的修改时才提示用户
        if (isDirty)
        {
            if (await message.ConfirmAsync("确认操作\r\n保存IO分配的结果?"))
            {
                await Save();
            }
            else
            {
                model.Status.Busy("正在重置数据……");
                switch (controlSystem)
                {
                    case ControlSystem.龙鳍:                   
                        await xt2.ReloadAllData();
                        break;
                    case ControlSystem.中控:
                        break;
                    case ControlSystem.龙核:                 
                        await aqj.ReloadAllData();
                        break;
                    case ControlSystem.一室:
                        break;
                    default:
                        break;
                }
                model.Status.Reset();
            }
        }
        
        // 重置脏标记
        isDirty = false;
        await Task.Delay(700);
        Messengers.FullScreen.OnNext(false);
    }


    [RelayCommand]
    private async Task Save() {
        _ = Cabinets ?? throw new("无数据可保存");
        model.Status.Busy("正在保存……");
        switch (controlSystem)
        {
            case ControlSystem.龙鳍:
                xt2.AllData= [.. CabinetCalc.ToPoint([.. Cabinets])];
                await xt2.SaveAndUploadFileAsync();
                await xt2.ReloadAllData();
                break;
            case ControlSystem.中控:
                xt2.AllData = [.. CabinetCalc.ToPoint([.. Cabinets])];
                await xt2.SaveAndUploadFileAsync();
                await xt2.ReloadAllData();
                break;                
            case ControlSystem.龙核:
                aqj.AllData = [.. CabinetCalc.ToPoint([.. Cabinets])];
                await aqj.SaveAndUploadFileAsync();
                await aqj.ReloadAllData();
                break;
            case ControlSystem.一室:
                break;
            default:
                break;
        }
        
        // 保存成功后清除脏标记
        isDirty = false;
        model.Status.Success("保存成功！");
    }

    [RelayCommand]
    private void AddBoardToSlot(SlotInfo slot) {
        var obj = new Xt2BoardEditObj();
        if (!Edit(obj, "在插槽中添加端子板")) { return; }
        slot.Board = Board.Create(obj.Type!);
        isDirty = true; // 标记为已修改
    }
    
    [RelayCommand]
    private void AddBoardToUnset() {
        _ = Cabinet ?? throw new();
        var obj = new Xt2BoardEditObj();
        if (!Edit(obj, "添加未分配端子板")) { return; }
        Cabinet.AddBoardToVirtualSlot(Board.Create(obj.Type!));
        isDirty = true; // 标记为已修改
    }

    async partial void OnViewBoardChanged(Board? value) {
        await ResetPoints(false);
        Filter();
    }

    private bool Edit(Xt2BoardEditObj board, string title) {
        var builder = board.CreateEditorBuilder();

        builder.WithTitle(title).WithEditorHeight(250).WithValidator(x => {
            if (x.Type is null) { return "请选择类型"; }
            return string.Empty;
        });
        builder.AddProperty<config_card_type_judge>(nameof(Board.Type)).WithHeader("类型").EditAsCombo<config_card_type_judge?>().WithOptions([.. BoardOptions!.Select(x => (x.IoCardType, x))]);
        
        return builder.Build().EditWithWpfUI();
    }

    public void RemoveFromAllParents(IoFullData point) {
        _ = Cabinet ?? throw new();
        Cabinet.UnsetPoints.Remove(point);
        DisplayBoardPoints?.Remove(point);
        
        // 检查虚拟插槽中的板卡
        var virtualBoards = Cabinet.VirtualSlots.Where(vs => vs.Board != null).Select(vs => vs.Board!);
        // 检查正常插槽中的板卡
        var normalBoards = Cabinet.Cages.SelectMany(x => x.Slots).Select(x => x.Board).Where(x => x != null).Select(x => x!);
        var allBoards = virtualBoards.Concat(normalBoards);
        
        foreach (var board in allBoards)
        {
            if (board.FFBoardType == BoardType.Normal)
            {
                // 普通板卡：从 Channels 中移除
                var channel = board.Channels.FirstOrDefault(x => x.Point == point);
                if (channel != null)
                {
                    channel.Point = null;
                    return;
                }
            }
            else if (board.FFBoardType == BoardType.FFBus)
            {
                // FF总线箱：从 FFBusChannels 中移除
                foreach (var network in board.Networks)
                {
                    var channel = network.FFBusChannels.FirstOrDefault(x => x.Point == point);
                    if (channel != null)
                    {
                        channel.Point = null;
                        return;
                    }
                }
            }
            else if (board.FFBoardType == BoardType.FFSlave)
            {
                // FF从站箱：从 FFSlaveChannels 中移除
                foreach (var network in board.Networks)
                {
                    foreach (var module in network.Modules)
                    {
                        var channel = module.FFSlaveChannels.FirstOrDefault(x => x.Point == point);
                        if (channel != null)
                        {
                            channel.Point = null;
                            return;
                        }
                    }
                }
            }
        }
    }

    public void RemoveFromAllParents(Board board) {
        _ = Cabinet ?? throw new();

        // 从虚拟插槽中移除
        var virtualSlotToRemove = Cabinet.VirtualSlots.FirstOrDefault(vs => vs.Board == board);
        if (virtualSlotToRemove != null)
        {
            Cabinet.VirtualSlots.Remove(virtualSlotToRemove);
        }

        // 从正常插槽中移除
        if (Cabinet.Cages.SelectMany(x => x.Slots).SingleOrDefault(x => x.Board == board) is SlotInfo slot)
        {
            slot.Board = null;
        }
    }

    private async Task ResetPoints(bool isRecalc)
    {
        if (Cabinet != null && Cabinets != null)
        {
            // 先更新Cabinets中的当前机柜
            var index = Cabinets.FindIndex(c => c.Name == Cabinet.Name);
            if (index != -1) 
            {
                Cabinets[index] = Cabinet;
            }

            // 保存当前机柜名称，用于后续查找
            var currentCabinetName = Cabinet.Name;
            
            // 将机柜数据同步回AllData，确保计算时使用最新的插槽、机笼信息
            switch (controlSystem)
            {
                case ControlSystem.龙鳍:
                    // 将所有机柜转回IO点数据，更新到xt2.AllData
                    xt2.AllData = [.. CabinetCalc.ToPoint([.. Cabinets])];
                    
                    if (isRecalc)
                    {
                        // 在预览界面移动板卡后自动计算，不弹确认框
                        await xt2.RecalcMethodInternal(controlSystem, currentCabinetName, showStatus: false);
                        
                        // 计算完成后重新构建机柜结构，获取最新的计算字段
                        if (BoardOptions != null)
                        {
                            Cabinets = xt2.AllData.ToList().BuildCabinetStructureOther(BoardOptions);
                            var cabinetIndex = Cabinets?.FindIndex(c => c.Name == currentCabinetName) ?? -1;
                            if (cabinetIndex != -1 && Cabinets != null)
                            {
                                Cabinet = Cabinets[cabinetIndex];
                            }
                        }
                    }
                    break;
                case ControlSystem.中控:
                    // 将所有机柜转回IO点数据，更新到xt2.AllData
                    xt2.AllData = [.. CabinetCalc.ToPoint([.. Cabinets])];
                    
                    if (isRecalc)
                    {
                        // 在预览界面移动板卡后自动计算，不弹确认框
                        await xt2.RecalcMethodInternal(controlSystem, currentCabinetName, showStatus: false);
                        
                        // 计算完成后重新构建机柜结构，获取最新的计算字段
                        if (BoardOptions != null)
                        {
                            Cabinets = xt2.AllData.ToList().BuildCabinetStructureOther(BoardOptions);
                            var cabinetIndex = Cabinets?.FindIndex(c => c.Name == currentCabinetName) ?? -1;
                            if (cabinetIndex != -1 && Cabinets != null)
                            {
                                Cabinet = Cabinets[cabinetIndex];
                            }
                        }
                    }
                    break;
                case ControlSystem.龙核:
                    // 龙核系统暂不支持自动计算
                    break;
                case ControlSystem.一室:
                    break;
                default:
                    break;
            }
            
            DisplayBoardPoints = [.. DisplayBoardPoints ?? []];
            if (Cabinet != null)
            {
                Cabinet.UnsetPoints = [.. Cabinet.UnsetPoints];
            }
        }
    }

    public async void Move(Board board, SlotInfo slot) {
        _ = Cabinet ?? throw new();
        if (slot.Board is Board oldBoard)
        {
            Cabinet.AddBoardToVirtualSlot(oldBoard);
        }
        RemoveFromAllParents(board);
        slot.Board = board;
        await ResetPoints(true);

        // 按板卡通道数排序虚拟插槽
        var sortedVirtualSlots = Cabinet.VirtualSlots.OrderBy(vs => vs.Board?.Channels.Count ?? 0).ToList();
        Cabinet.VirtualSlots.Clear();
        foreach (var vs in sortedVirtualSlots)
        {
            Cabinet.VirtualSlots.Add(vs);
        }
        
        isDirty = true; // 标记为已修改
    }

    public async void Move(IoFullData point, Xt2Channel channel) {
        _ = Cabinet ?? throw new();
        var targetBoard = GetParent(channel);
        if (point.CardType != targetBoard.Type) { throw new("点类型和卡件类型不一致"); }
        if (channel.Point is IoFullData oldPoint) { Cabinet.UnsetPoints.Add(oldPoint); }
        RemoveFromAllParents(point);
        channel.Point = point;
        await ResetPoints(true);
        Filter();
        isDirty = true; // 标记为已修改
    }
    
    public async void Move(List<IoFullData> points, Xt2Channel channel) {
        _ = Cabinet ?? throw new();
        var targetBoard = GetParent(channel);
        foreach (var point in points)
        {
            if (point.CardType != targetBoard.Type) { throw new("点类型和卡件类型不一致"); }
            var emptyChannel = targetBoard.Channels.SkipWhile(x => x.Index < channel.Index).FirstOrDefault(x => x.Point == null) ?? throw new("没有空白通道");
            RemoveFromAllParents(point);
            emptyChannel.Point = point;
        }
        await ResetPoints(true);
        Filter();
        isDirty = true; // 标记为已修改
    }

    /// <summary>FF总线箱通道移动（单个信号）</summary>
    public async void Move(IoFullData point, FFBusChannel channel) {
        _ = Cabinet ?? throw new();
        var targetBoard = GetParent(channel);
        if (point.CardType != targetBoard.Type) { throw new("点类型和卡件类型不一致"); }
        if (channel.Point is IoFullData oldPoint) { Cabinet.UnsetPoints.Add(oldPoint); }
        RemoveFromAllParents(point);
        channel.Point = point;
        
        // 更新点的NetType和Channel信息
        var network = GetParentNetwork(channel, targetBoard);
        point.NetType = network.NetworkType.ToString();
        point.Channel = channel.Index;
        
        await ResetPoints(true);
        Filter();
        isDirty = true;
    }
    
    /// <summary>FF总线箱通道移动（多个信号）</summary>
    public async void Move(List<IoFullData> points, FFBusChannel channel) {
        _ = Cabinet ?? throw new();
        var targetBoard = GetParent(channel);
        var network = GetParentNetwork(channel, targetBoard);
        
        foreach (var point in points)
        {
            if (point.CardType != targetBoard.Type) { throw new("点类型和卡件类型不一致"); }
            var emptyChannel = network.FFBusChannels.SkipWhile(x => x.Index < channel.Index).FirstOrDefault(x => x.Point == null) ?? throw new("没有空白通道");
            RemoveFromAllParents(point);
            emptyChannel.Point = point;
            
            // 更新点的NetType和Channel信息
            point.NetType = network.NetworkType.ToString();
            point.Channel = emptyChannel.Index;
        }
        await ResetPoints(true);
        Filter();
        isDirty = true;
    }

    /// <summary>FF从站箱通道移动（单个信号）</summary>
    public async void Move(IoFullData point, FFSlaveChannel channel) {
        _ = Cabinet ?? throw new();
        var targetBoard = GetParent(channel);
        if (point.CardType != targetBoard.Type) { throw new("点类型和卡件类型不一致"); }
        if (channel.Point is IoFullData oldPoint) { Cabinet.UnsetPoints.Add(oldPoint); }
        RemoveFromAllParents(point);
        channel.Point = point;
        
        // 更新点的NetType和Channel信息
        var module = GetParentModule(channel, targetBoard);
        var network = targetBoard.Networks.First(n => n.Modules.Contains(module));
        point.NetType = network.NetworkType.ToString();
        point.Channel = channel.Index;
        
        await ResetPoints(true);
        Filter();
        isDirty = true;
    }
    
    /// <summary>FF从站箱通道移动（多个信号）</summary>
    public async void Move(List<IoFullData> points, FFSlaveChannel channel) {
        _ = Cabinet ?? throw new();
        var targetBoard = GetParent(channel);
        var module = GetParentModule(channel, targetBoard);
        var network = targetBoard.Networks.First(n => n.Modules.Contains(module));
        
        foreach (var point in points)
        {
            if (point.CardType != targetBoard.Type) { throw new("点类型和卡件类型不一致"); }
            var emptyChannel = module.FFSlaveChannels.SkipWhile(x => x.Index < channel.Index).FirstOrDefault(x => x.Point == null) ?? throw new("没有空白通道");
            RemoveFromAllParents(point);
            emptyChannel.Point = point;
            
            // 更新点的NetType和Channel信息
            point.NetType = network.NetworkType.ToString();
            point.Channel = emptyChannel.Index;
        }
        await ResetPoints(true);
        Filter();
        isDirty = true;
    }

    public async void Unset(Board board) {
        _ = Cabinet ?? throw new();
        RemoveFromAllParents(board);
        Cabinet.AddBoardToVirtualSlot(board);

        // 按板卡通道数排序虚拟插槽
        var sortedVirtualSlots = Cabinet.VirtualSlots.OrderBy(vs => vs.Board?.Channels.Count ?? 0).ToList();
        Cabinet.VirtualSlots.Clear();
        foreach (var vs in sortedVirtualSlots)
        {
            Cabinet.VirtualSlots.Add(vs);
        }

        await ResetPoints(true);
        isDirty = true; // 标记为已修改
    }

    public async void Unset(List<IoFullData> points) {
        _ = Cabinet ?? throw new();
        foreach (var point in points)
        {
            RemoveFromAllParents(point);
            Cabinet.UnsetPoints.Add(point);
        }
        await ResetPoints(true);
        Filter();
        isDirty = true; // 标记为已修改
    }

    public async void Unset(IoFullData point) {
        _ = Cabinet ?? throw new();
        RemoveFromAllParents(point);
        Cabinet.UnsetPoints.Add(point);
        await ResetPoints(true);
        Filter();
        isDirty = true; // 标记为已修改
    }

    public void Delete(Board board) {
        if (board.Channels.Any(x => x.Point != null)) { throw new("无法删除，卡件上还有点"); }
        RemoveFromAllParents(board);
        isDirty = true; // 标记为已修改
    }

    public void View(Board board) {
        ViewBoardPoints(board);
    }

    [RelayCommand]
    private void ViewBoardPoints(Board board) {
        ViewBoard = board;
    }

    public Board GetParent(Xt2Channel channel) {
        _ = Cabinet ?? throw new();

        // 在正常插槽中查找
        var allSetBoards = Cabinet.Cages.SelectMany(x => x.Slots).Select(x => x.Board).Where(x => x != null);

        // 在虚拟插槽中查找
        var allVirtualBoards = Cabinet.VirtualSlots.Where(vs => vs.Board != null).Select(vs => vs.Board);

        IEnumerable<Board> allBoards = allSetBoards.Concat(allVirtualBoards);
        return allBoards.Single(x => x.Channels.Contains(channel));
    }

    /// <summary>获取FF总线箱通道所属的板卡</summary>
    public Board GetParent(FFBusChannel channel) {
        _ = Cabinet ?? throw new();
        var allSetBoards = Cabinet.Cages.SelectMany(x => x.Slots).Select(x => x.Board).Where(x => x != null);
        var allVirtualBoards = Cabinet.VirtualSlots.Where(vs => vs.Board != null).Select(vs => vs.Board);
        IEnumerable<Board> allBoards = allSetBoards.Concat(allVirtualBoards);
        
        return allBoards.Single(board => 
            board.FFBoardType == BoardType.FFBus && 
            board.Networks.Any(network => network.FFBusChannels.Contains(channel)));
    }

    /// <summary>获取FF从站通道所属的板卡</summary>
    public Board GetParent(FFSlaveChannel channel) {
        _ = Cabinet ?? throw new();
        var allSetBoards = Cabinet.Cages.SelectMany(x => x.Slots).Select(x => x.Board).Where(x => x != null);
        var allVirtualBoards = Cabinet.VirtualSlots.Where(vs => vs.Board != null).Select(vs => vs.Board);
        IEnumerable<Board> allBoards = allSetBoards.Concat(allVirtualBoards);
        
        return allBoards.Single(board => 
            board.FFBoardType == BoardType.FFSlave && 
            board.Networks.Any(network => network.Modules.Any(module => module.FFSlaveChannels.Contains(channel))));
    }

    /// <summary>获取FF总线箱通道所属的网段</summary>
    private FFNetwork GetParentNetwork(FFBusChannel channel, Board board) {
        return board.Networks.Single(network => network.FFBusChannels.Contains(channel));
    }

    /// <summary>获取FF从站通道所属的模块</summary>
    private FFSlaveModule GetParentModule(FFSlaveChannel channel, Board board) {
        return board.Networks
            .SelectMany(network => network.Modules)
            .Single(module => module.FFSlaveChannels.Contains(channel));
    }

    [RelayCommand]
    private async Task AddTag(TagType type)
    {
        _ = Cabinet ?? throw new();
        if (type == TagType.Alarm)
        {
            var configs = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            var configDI211 = configs.FirstOrDefault(c => c.IoCardType == "DI211");
            var configDO211 = configs.FirstOrDefault(c => c.IoCardType == "DO211");
            if (configDI211 == null) throw new Exception("未找到卡件DI211的数量");
            if (configDO211 == null) throw new Exception("未找到卡件DO211的数量");
            model.Status.Busy($"正在添加报警点……");
            // 检查机柜是否已经有报警点
            var hasAlarmPoint = cabinet.Cages
             .SelectMany(cage => cage.Slots)         // Flatten all slots from all cages
             .SelectMany(slot => slot.Board?.Channels ?? Enumerable.Empty<Xt2Channel>())  // Flatten all channels from each board, handling null boards
             .Any(channel => channel.Point?.PointType == TagType.Alarm);
            if (!hasAlarmPoint)
            {  // 添加DI211报警点
                AddAlarmPoints(cabinet, configDI211);
                // 添加DO211报警点
                AddControlAlarmPoint(cabinet, configDO211);
            }
            model.Status.Busy($"添加报警点完毕……");
        }
        else if (type == TagType.BackUp)
        {
            //添加备用点
            model.Status.Busy($"正在添加备用点……");
            foreach (var cage in cabinet.Cages)
            {
                foreach (var slot in cage.Slots)
                {
                    var cardType = slot.Board?.Type;
                    if (cardType == null || cardType.Contains("DP") || cardType.Contains("FF")) continue;
                    for (int i = 0; i < slot.Board!.Channels.Count; i++)
                    {
                        if (slot.Board.Channels[i].Point == null)
                        {
                            var lastTag = slot.Board.Channels.FirstOrDefault(c => c.Point != null)?.Point;
                            if (lastTag == null) throw new Exception($"机柜{cabinet.Name}机笼{cage.Index}插槽{slot.Index.ToString("00")}卡件{cardType.Substring(0, 2)}没有通道，无法添加备用点！");
                            slot.Board.Channels[i].Point = new IoFullData()
                            {
                                CabinetNumber = cabinet.Name,
                                SignalPositionNumber = $"{cabinet.Name}{cage.Index}{slot.Index.ToString("00")}{cardType.Substring(0, 2)}CH{i.ToString("00")}",
                                SystemCode = "BEIYONG",
                                Cage = cage.Index,
                                Slot = slot.Index,
                                CardType = cardType,
                                Description = "备用",
                                Channel = i,
                                IoType = lastTag.IoType,
                                PowerType = lastTag.PowerType,
                                ElectricalCharacteristics = lastTag.ElectricalCharacteristics,
                                SignalEffectiveMode = lastTag.SignalEffectiveMode,
                                PointType = TagType.BackUp,
                                Version = "A",
                                ModificationDate = DateTime.Now
                            };
                        }
                    }
                }
            }
            model.Status.Busy($"添加备用点完毕……");
        }

        // 添加DI211报警点的方法
        void AddAlarmPoints(StdCabinet cabinet, config_card_type_judge config)
        {
            var alarmDescriptions = new[] { "电源A故障报警", "电源B故障报警", "机柜门开", "温度高报警", "风扇故障", "网络故障" };
            var extensionCodes = new[] { "PWFA", "PWFB", "DROP", "TEPH", "FAN", "SWF" };

            for (int i = 0; i < 6; i++)
            {
                var point = new IoFullData
                {
                    CabinetNumber = cabinet.Name,
                    PointType = TagType.Alarm,
                    SignalPositionNumber = cabinet.Name,
                    Cage = 0,
                    Slot = 0,
                    Channel = 0,
                    IoType = "DI",
                    PowerType = "DI1",
                    ElectricalCharacteristics = "无源常开",
                    SignalEffectiveMode = "NO",
                    SystemCode = "JIGUIBAOJING",
                    ExtensionCode = extensionCodes[i],
                    Description = $"控制柜{cabinet.Name}机柜{alarmDescriptions[i]}"
                };
                cabinet.UnsetPoints.Add(point);
            }
        }

        // 添加DO211报警点的方法
        void AddControlAlarmPoint(StdCabinet cabinet, config_card_type_judge config)
        {
            var point = new IoFullData
            {
                CabinetNumber = cabinet.Name,
                PointType = TagType.Alarm,
                Cage = 0,
                Slot = 0,
                Channel = 0,
                SignalPositionNumber = cabinet.Name,
                SystemCode = "JIGUIBAOJING",
                ExtensionCode = "ALM",
                Description = $"控制柜{cabinet.Name}机柜报警灯",
                IoType = "DO",
                PowerType = "DO2",
                ElectricalCharacteristics = "有源常闭",
                SignalEffectiveMode = "NO"
            };
            cabinet.UnsetPoints.Add(point);
        }
        await ResetPoints(true);
        Filter();
        isDirty = true; // 标记为已修改
        model.Status.Reset();
    }

    [RelayCommand]
    private async Task DeleteTag(TagType type)
    {
        _ = Cabinet ?? throw new();       
        model.Status.Busy($"正在删除点……");
        Cabinet.RemovePoints(type);
        isDirty = true; // 标记为已修改
        model.Status.Reset();
    }

    [RelayCommand]
    private async Task AllocateIO()
    {
        _ = Cabinet ?? throw new();
        
        // 🔑 根据控制系统类型调用不同的分配逻辑
        if (controlSystem == ControlSystem.龙鳍 || controlSystem == ControlSystem.中控)
        {
            // 调用主界面的公共方法，包含预留确认逻辑
            await xt2.PerformIOAllocationWithReservedConfirmation(Cabinet.Name);
            
            // 重新从xt2.AllData构建当前机柜结构
            var boardOptions = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            var allCabinets = xt2.AllData!.ToList().BuildCabinetStructureOther(boardOptions);
            Cabinet = allCabinets.FirstOrDefault(c => c.Name == Cabinet.Name);
        }
        else if (controlSystem == ControlSystem.龙核)
        {
            // 龙核系统使用原有逻辑（暂不支持预留功能）
            model.Status.Busy($"正在分配……");
            var formularHelper = new FormularHelper();
            List<config_card_type_judge> config_Card_Type_Judges = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            Cabinet = formularHelper.AutoAllocateLongHeIOSingle(Cabinet, config_Card_Type_Judges, RedundancyRate / 100.0);
            model.Status.Success($"分配完毕！");
        }
        
        await ResetPoints(true);
        Filter();
        isDirty = true; // 标记为已修改
    }
    [RelayCommand]
    private async Task Recalc()
    {
        await ResetPoints(true);
        Filter();
    }

}

public class Xt2BoardEditObj {
    public config_card_type_judge? Type { get; set; }
}

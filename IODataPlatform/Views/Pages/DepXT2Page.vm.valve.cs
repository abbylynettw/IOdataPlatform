using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Views.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace IODataPlatform.Views.Pages;

partial class DepXT2ViewModel
{
	/// <summary>
	/// 通用排序命令
	/// 对所有数据按标准字段进行排序(箱外信号):
	/// - 非FF信号:按机柜号、机笼号、插槽、通道号排序
	/// - FF信号:按机柜号、机笼号、插槽、网段、FF从站号、就地箱号排序(不含DP阀位顺序)
	/// </summary>
	[RelayCommand]
	private async void GeneralSorting()
	{
		try
		{
			if (AllData == null || AllData.Count == 0)
			{
				await message.ErrorAsync("当前没有IO数据，请先导入数据！");
				return;
			}
			if (await message.ConfirmAsync("确认要进行通用排序吗？\n\n排序规则:\n- 非FF信号:按机柜号、机笼号、插槽、通道号排序\n- FF信号:按机柜号、机笼号、插槽、网段、FF从站号、就地箱号排序\n\n注意:不包含DP阀位顺序排序"))
			{
				List<IoFullData> sortedData = ApplyGeneralSorting(AllData.ToList());
				AllData = new ObservableCollection<IoFullData>(sortedData);
				await SaveAndUploadFileAsync();
				await message.SuccessAsync("通用排序完成！");
			}
		}
		catch (Exception ex)
		{
			await message.ErrorAsync("通用排序功能出现错误：" + ex.Message);
		}
	}

	/// <summary>
	/// 阀门排序命令(箱内信号排序)
	/// 仅对箱内FF信号按DP阀位顺序进行排序
	/// 其他信号保持原有顺序不变
	/// </summary>
	[RelayCommand]
	private async void ValvePositionSorting()
	{
		try
		{
			if (AllData == null || AllData.Count == 0)
			{
				await message.ErrorAsync("当前没有IO数据，请先导入数据！");
				return;
			}
			if (await message.ConfirmAsync("确认要进行阀门排序吗？\n\n排序规则：\n- 以就地箱号为单位分组\n- 箱内有DP阀位顺序的记录按阀位顺序排在前面\n- 箱内无DP阀位顺序的记录排在后面（保持原顺序）\n- 就地箱号为空的记录保持原顺序不变\n\n注意：请先使用导入阀门顺序功能填充DP阀位顺序字段"))
			{
				List<IoFullData> sortedData = ApplyValveSorting(AllData.ToList());
				AllData = new ObservableCollection<IoFullData>(sortedData);
				await SaveAndUploadFileAsync();
				await message.SuccessAsync("阀门排序完成！");
			}
		}
		catch (Exception ex)
		{
			await message.ErrorAsync("阀门排序功能出现错误：" + ex.Message);
		}
	}

	/// <summary>
	/// 导入阀门顺序命令
	/// 打开导入阀门顺序窗口，支持：
	/// 1. 下载 Excel 模板
	/// 2. 上传阀门数据文件
	/// 3. 匹配阀门编号到信号位号
	/// 4. 填充气口顺序到 DP阀位顺序字段
	/// 注意：此功能只导入数据，不进行排序
	/// </summary>
	[RelayCommand]
	private void ValveSorting()
	{
		try
		{
			if (AllData == null || AllData.Count == 0)
			{
				message.ErrorAsync("当前没有IO数据，请先导入数据！");
				return;
			}
			IServiceProvider service = App.GetService<IServiceProvider>();
			ValveSortingWindow sortingWindow = service.GetRequiredService<ValveSortingWindow>();
			sortingWindow.InitializeData(AllData.ToList());
			sortingWindow.Owner = Application.Current.MainWindow;
			if (sortingWindow.ShowDialog() == true)
			{
				List<IoFullData> sortedData = sortingWindow.GetSortedData();
				if (sortedData != null && sortedData.Any())
				{
					AllData = new ObservableCollection<IoFullData>(sortedData);
					SaveAndUploadFileAsync();
					message.SuccessAsync("导入阀门顺序完成！");
				}
			}
		}
		catch (Exception ex)
		{
			message.ErrorAsync("导入阀门顺序功能出现错误：" + ex.Message);
		}
	}

	/// <summary>
	/// 应用阀门排序规则(仅对箱内FF信号)
	/// 以就地箱号为单位，箱内有DP阀位顺序的记录按阀位顺序排在前面，空的排在后面
	/// 就地箱号为空的记录保持原有顺序不变
	/// 整个箱子都没有阀位顺序的也保持原有顺序不变
	/// </summary>
	private List<IoFullData> ApplyValveSorting(List<IoFullData> data)
	{
		List<IoFullData> result = new List<IoFullData>();
		
		// 按就地箱号分组(保留原始索引)
		var groups = data.Select((item, index) => new { Item = item, OriginalIndex = index })
			.GroupBy(x => x.Item.LocalBoxNumber ?? string.Empty)
			.ToList();

		foreach (var group in groups)
		{
			string boxNumber = group.Key;
			
			// 就地箱号为空的记录保持原顺序
			if (string.IsNullOrEmpty(boxNumber))
			{
				result.AddRange(group.OrderBy(x => x.OriginalIndex).Select(x => x.Item));
				continue;
			}

			// 检查整个箱子是否有阀位顺序数据
			bool hasValvePosition = group.Any(x => !string.IsNullOrWhiteSpace(x.Item.DPTerminalChannel));
			
			// 整个箱子都没有阀位顺序的保持原顺序
			if (!hasValvePosition)
			{
				result.AddRange(group.OrderBy(x => x.OriginalIndex).Select(x => x.Item));
				continue;
			}

			// 分离有阀位顺序和无阀位顺序的记录
			var withValvePosition = group.Where(x => !string.IsNullOrWhiteSpace(x.Item.DPTerminalChannel)).ToList();
			var withoutValvePosition = group.Where(x => string.IsNullOrWhiteSpace(x.Item.DPTerminalChannel)).ToList();

			// 有阀位顺序的按阀位顺序排序
			var sortedWithValve = withValvePosition
				.OrderBy(x => ExtractValvePositionNumber(x.Item.DPTerminalChannel))
				.Select(x => x.Item)
				.ToList();

			// 无阀位顺序的保持原顺序
			var sortedWithoutValve = withoutValvePosition
				.OrderBy(x => x.OriginalIndex)
				.Select(x => x.Item)
				.ToList();

			// 先添加有阀位顺序的，再添加无阀位顺序的
			result.AddRange(sortedWithValve);
			result.AddRange(sortedWithoutValve);
		}

		return result;
	}

	/// <summary>
	/// 从阀位顺序字符串中提取第一个数字用于排序
	/// 支持F9、F3/F4、F10等多种格式的智能解析
	/// 对于F3/F4这种多值格式，以第一个数字(3)为准
	/// </summary>
	/// <param name="valvePosition">阀位顺序字符串</param>
	/// <returns>用于排序的数字，无法解析时返回int.MaxValue</returns>
	private static int ExtractValvePositionNumber(string valvePosition)
	{
		if (string.IsNullOrWhiteSpace(valvePosition))
		{
			return int.MaxValue;
		}

		// 提取字符串中的所有数字
		MatchCollection matches = Regex.Matches(valvePosition, @"\d+");
		
		if (matches.Count > 0 && int.TryParse(matches[0].Value, out int number))
		{
			return number;
		}

		return int.MaxValue;
	}

	/// <summary>
	/// 应用通用排序规则
	/// - 非FF信号:按机柜号、机笼号、插槽、通道号排序
	/// - FF信号:按机柜号、机笼号、插槽、网段、FF从站号、就地箱号排序(不含DP阀位顺序)
	/// </summary>
	private List<IoFullData> ApplyGeneralSorting(List<IoFullData> data)
	{
		// 分离FF信号和非FF信号
		var ffSignals = data.Where(IsFFSignal).ToList();
		var nonFFSignals = data.Where(x => !IsFFSignal(x)).ToList();

		// 非FF信号排序:机柜号、机笼号、插槽、通道号
		var sortedNonFF = nonFFSignals
			.OrderBy(x => x.CabinetNumber ?? string.Empty)
			.ThenBy(x => x.Cage)
			.ThenBy(x => x.Slot)
			.ThenBy(x => x.Channel)
			.ToList();

		// FF信号排序:机柜号、机笼号、插槽、网段、FF从站号、就地箱号
		var sortedFF = ffSignals
			.OrderBy(x => x.CabinetNumber ?? string.Empty)
			.ThenBy(x => x.Cage)
			.ThenBy(x => x.Slot)
			.ThenBy(x => x.NetType ?? string.Empty)
			.ThenBy(x => x.FFDPStaionNumber ?? string.Empty)
			.ThenBy(x => x.LocalBoxNumber ?? string.Empty)
			.ToList();

		// 先非FF信号，后FF信号
		var result = new List<IoFullData>();
		result.AddRange(sortedNonFF);
		result.AddRange(sortedFF);

		return result;

		// 本地函数:判断是否为FF信号
		static bool IsFFSignal(IoFullData item)
		{
			return item.PowerType != null && item.PowerType.Contains("FF");
		}
	}
}

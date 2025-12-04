using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Models.ExportModels;

namespace IODataPlatform.Services;

/// <summary>
/// 云端导出配置服务
/// 基于现有的StorageService文件存储架构，提供导出配置的云端存储和管理功能
/// 支持用户级、项目级、全局级配置的分层管理和多用户配置共享
/// 实现配置的智能同步、字段版本管理和自动合并功能
/// </summary>
public class CloudExportConfigService
{
	private readonly StorageService _storageService;

	private readonly GlobalModel _globalModel;

	public CloudExportConfigService(StorageService storageService, GlobalModel globalModel)
	{
		_storageService = storageService;
		_globalModel = globalModel;
		Task.Run(async delegate
		{
			await CheckAndMigrateLocalConfigsAsync();
		});
	}

	/// <summary>
	/// 获取当前用户的所有导出配置
	/// 按优先级合并全局配置、项目配置和用户配置
	/// 优先级：用户配置 &gt; 项目配置 &gt; 全局配置
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="projectId">项目ID，可选</param>
	/// <returns>合并后的配置列表</returns>
	public async Task<List<ExportConfig>> GetUserConfigsAsync(ExportType exportType, int? projectId = null)
	{
		List<ExportConfig> allConfigs = new List<ExportConfig>();
		int currentUserId = _globalModel.User?.Id ?? 0;
		foreach (ExportConfig item in await _storageService.DownloadGlobalExportConfigAsync(exportType))
		{
			item.ConfigName = "[全局] " + item.ConfigName;
			allConfigs.Add(item);
		}
		if (projectId.HasValue)
		{
			foreach (ExportConfig item2 in await _storageService.DownloadProjectExportConfigAsync(projectId.Value, exportType))
			{
				item2.ConfigName = "[项目] " + item2.ConfigName;
				allConfigs.Add(item2);
			}
		}
		foreach (ExportConfig item3 in await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType))
		{
			item3.ConfigName = "[我的] " + item3.ConfigName;
			allConfigs.Add(item3);
		}
		return allConfigs;
	}

	/// <summary>
	/// 保存用户配置到云端
	/// 将配置保存到用户的云端存储，实现多设备同步
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="config">要保存的配置</param>
	/// <returns>异步任务</returns>
	public async Task SaveUserConfigAsync(ExportType exportType, ExportConfig config)
	{
		int currentUserId = _globalModel.User?.Id ?? 0;
		if (currentUserId != 0)
		{
			List<ExportConfig> list = await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType);
			list.RemoveAll((ExportConfig c) => c.ConfigName == config.ConfigName);
			list.Add(config);
			await _storageService.UploadUserExportConfigAsync(currentUserId, exportType, list);
		}
	}

	/// <summary>
	/// 删除用户配置
	/// 从用户的云端存储中删除指定的配置
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="configName">配置名称</param>
	/// <returns>异步任务</returns>
	public async Task DeleteUserConfigAsync(ExportType exportType, string configName)
	{
		int currentUserId = _globalModel.User?.Id ?? 0;
		if (currentUserId != 0)
		{
			List<ExportConfig> list = await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType);
			list.RemoveAll((ExportConfig c) => c.ConfigName == configName);
			await _storageService.UploadUserExportConfigAsync(currentUserId, exportType, list);
		}
	}

	/// <summary>
	/// 保存项目级配置到云端
	/// 管理员或项目负责人可以保存项目级配置，供团队共享
	/// </summary>
	/// <param name="projectId">项目ID</param>
	/// <param name="exportType">导出类型</param>
	/// <param name="config">要保存的配置</param>
	/// <returns>异步任务</returns>
	public async Task SaveProjectConfigAsync(int projectId, ExportType exportType, ExportConfig config)
	{
		List<ExportConfig> list = await _storageService.DownloadProjectExportConfigAsync(projectId, exportType);
		list.RemoveAll((ExportConfig c) => c.ConfigName == config.ConfigName);
		list.Add(config);
		await _storageService.UploadProjectExportConfigAsync(projectId, exportType, list);
	}

	/// <summary>
	/// 删除项目级配置
	/// 管理员或项目负责人可以删除项目级配置
	/// </summary>
	/// <param name="projectId">项目ID</param>
	/// <param name="exportType">导出类型</param>
	/// <param name="configName">配置名称</param>
	/// <returns>异步任务</returns>
	public async Task DeleteProjectConfigAsync(int projectId, ExportType exportType, string configName)
	{
		List<ExportConfig> list = await _storageService.DownloadProjectExportConfigAsync(projectId, exportType);
		list.RemoveAll((ExportConfig c) => c.ConfigName == configName);
		await _storageService.UploadProjectExportConfigAsync(projectId, exportType, list);
	}

	/// <summary>
	/// 保存全局配置到云端
	/// 仅系统管理员可以保存全局配置，供所有用户使用
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="config">要保存的配置</param>
	/// <returns>异步任务</returns>
	public async Task SaveGlobalConfigAsync(ExportType exportType, ExportConfig config)
	{
		List<ExportConfig> list = await _storageService.DownloadGlobalExportConfigAsync(exportType);
		list.RemoveAll((ExportConfig c) => c.ConfigName == config.ConfigName);
		list.Add(config);
		await _storageService.UploadGlobalExportConfigAsync(exportType, list);
	}

	/// <summary>
	/// 删除全局配置
	/// 仅系统管理员可以删除全局配置
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="configName">配置名称</param>
	/// <returns>异步任务</returns>
	public async Task DeleteGlobalConfigAsync(ExportType exportType, string configName)
	{
		List<ExportConfig> list = await _storageService.DownloadGlobalExportConfigAsync(exportType);
		list.RemoveAll((ExportConfig c) => c.ConfigName == configName);
		await _storageService.UploadGlobalExportConfigAsync(exportType, list);
	}

	/// <summary>
	/// 同步字段变更到现有配置
	/// 当系统字段发生增加或删除时，自动更新所有现有配置以保持一致性
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="availableFields">当前可用的字段列表</param>
	/// <param name="projectId">项目ID，可选</param>
	/// <returns>异步任务</returns>
	public async Task SyncFieldChangesAsync(ExportType exportType, List<string> availableFields, int? projectId = null)
	{
		int currentUserId = _globalModel.User?.Id ?? 0;
		if (currentUserId > 0)
		{
			List<ExportConfig> list = SyncConfigFields(await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType), availableFields);
			if (list.Any())
			{
				await _storageService.UploadUserExportConfigAsync(currentUserId, exportType, list);
			}
		}
		if (projectId.HasValue)
		{
			List<ExportConfig> list2 = SyncConfigFields(await _storageService.DownloadProjectExportConfigAsync(projectId.Value, exportType), availableFields);
			if (list2.Any())
			{
				await _storageService.UploadProjectExportConfigAsync(projectId.Value, exportType, list2);
			}
		}
		List<ExportConfig> list3 = SyncConfigFields(await _storageService.DownloadGlobalExportConfigAsync(exportType), availableFields);
		if (list3.Any())
		{
			await _storageService.UploadGlobalExportConfigAsync(exportType, list3);
		}
	}

	/// <summary>
	/// 同步配置中的字段
	/// 移除不存在的字段，保持现有字段的顺序和设置
	/// </summary>
	/// <param name="configs">配置列表</param>
	/// <param name="availableFields">可用字段列表</param>
	/// <returns>更新后的配置列表</returns>
	private List<ExportConfig> SyncConfigFields(List<ExportConfig> configs, List<string> availableFields)
	{
		bool flag = false;
		foreach (ExportConfig config in configs)
		{
			if (config.SelectedFields != null)
			{
				int count = config.SelectedFields.Count;
				config.SelectedFields.RemoveAll((string field) => !availableFields.Contains(field));
				if (config.SelectedFields.Count != count)
				{
					flag = true;
				}
			}
			if (config.ColumnOrder != null)
			{
				int count2 = config.ColumnOrder.Count;
				config.ColumnOrder.RemoveAll((ColumnInfo col) => !availableFields.Contains(col.FieldName));
				if (config.ColumnOrder.Count != count2)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return new List<ExportConfig>();
		}
		return configs;
	}

	/// <summary>
	/// 获取当前项目ID
	/// 由于项目信息分散在各个ViewModel中管理，此方法返回传入的项目ID
	/// </summary>
	/// <returns>项目ID，如果没有项目返回null</returns>
	public int? GetCurrentProjectId()
	{
		return null;
	}

	/// <summary>
	/// 检查配置是否需要字段同步
	/// 比较配置中的字段与当前可用字段，判断是否需要同步
	/// </summary>
	/// <param name="config">导出配置</param>
	/// <param name="availableFields">可用字段列表</param>
	/// <returns>如果需要同步返回true</returns>
	public bool NeedFieldSync(ExportConfig config, List<string> availableFields)
	{
		if (config.SelectedFields != null && config.SelectedFields.Any((string field) => !availableFields.Contains(field)))
		{
			return true;
		}
		if (config.ColumnOrder != null && config.ColumnOrder.Any((ColumnInfo col) => !availableFields.Contains(col.FieldName)))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// 检查并迁移本地配置到云端
	/// 在服务初始化时自动执行，确保用户的历史配置不丢失
	/// </summary>
	private async Task CheckAndMigrateLocalConfigsAsync()
	{
		_ = 2;
		try
		{
			int currentUserId = _globalModel.User?.Id ?? 0;
			if (currentUserId != 0)
			{
				await MigrateLocalConfigsForType(ExportType.CompleteList, currentUserId);
				await MigrateLocalConfigsForType(ExportType.CurrentSystemList, currentUserId);
				await MigrateLocalConfigsForType(ExportType.PublishedList, currentUserId);
			}
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// 迁移指定类型的本地配置
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="userId">用户ID</param>
	private async Task MigrateLocalConfigsForType(ExportType exportType, int userId)
	{
		_ = 3;
		try
		{
			List<ExportConfig> localConfigs = await LoadLocalConfigs(exportType);
			if (localConfigs == null || localConfigs.Count <= 0)
			{
				return;
			}
			List<ExportConfig> cloudConfigs = await _storageService.DownloadUserExportConfigAsync(userId, exportType);
			foreach (ExportConfig localConfig in localConfigs)
			{
				if (!cloudConfigs.Any((ExportConfig c) => c.ConfigName == localConfig.ConfigName))
				{
					localConfig.ConfigName = "[本地迁移] " + localConfig.ConfigName;
					localConfig.LastModified = DateTime.Now;
					await SaveUserConfigAsync(exportType, localConfig);
				}
			}
			await BackupLocalConfigFile(exportType);
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// 加载本地配置文件
	/// 支持多种可能的本地配置文件位置和格式
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <returns>本地配置列表</returns>
	private async Task<List<ExportConfig>?> LoadLocalConfigs(ExportType exportType)
	{
		_ = 1;
		try
		{
			List<string> possibleLocalConfigPaths = GetPossibleLocalConfigPaths(exportType);
			foreach (string item in possibleLocalConfigPaths)
			{
				if (!File.Exists(item))
				{
					continue;
				}
				string text = await File.ReadAllTextAsync(item);
				if (!string.IsNullOrEmpty(text))
				{
					List<ExportConfig> list = JsonSerializer.Deserialize<List<ExportConfig>>(text);
					if (list != null && list.Count > 0)
					{
						return await ProcessFieldCompatibility(list);
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	/// <summary>
	/// 获取可能的本地配置文件路径
	/// 根据项目的历史版本和配置文件存储约定
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <returns>可能的配置文件路径列表</returns>
	private List<string> GetPossibleLocalConfigPaths(ExportType exportType)
	{
		List<string> list = new List<string>();
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string text = exportType switch
		{
			ExportType.CompleteList => "complete_export_configs.json", 
			ExportType.CurrentSystemList => "current_system_export_configs.json", 
			ExportType.PublishedList => "published_export_configs.json", 
			_ => "export_configs.json", 
		};
		list.Add(Path.Combine(baseDirectory, "Configs", text));
		list.Add(Path.Combine(baseDirectory, "Assets", "Configs", text));
		list.Add(Path.Combine(folderPath, "IODataPlatform", text));
		list.Add(Path.Combine(folderPath2, "IODataPlatform", text));
		list.Add(Path.Combine(baseDirectory, text));
		list.Add(Path.Combine(baseDirectory, "Configs", "export_configs.json"));
		list.Add(Path.Combine(baseDirectory, "Assets", "Configs", "export_configs.json"));
		return list;
	}

	/// <summary>
	/// 处理字段兼容性
	/// 检查并修复配置中过时或缺失的字段
	/// </summary>
	/// <param name="configs">配置列表</param>
	/// <returns>处理后的配置列表</returns>
	private async Task<List<ExportConfig>> ProcessFieldCompatibility(List<ExportConfig> configs)
	{
		try
		{
			List<string> availableFields = GetCurrentAvailableFields();
			foreach (ExportConfig config in configs)
			{
				if (config.SelectedFields != null)
				{
					config.SelectedFields.RemoveAll((string field) => !availableFields.Contains(field));
					AddNewCoreFields(config.SelectedFields, availableFields);
				}
				if (config.ColumnOrder != null)
				{
					config.ColumnOrder.RemoveAll((ColumnInfo col) => !availableFields.Contains(col.FieldName));
					AddNewFieldColumns(config.ColumnOrder, availableFields);
					ReorderColumns(config.ColumnOrder);
				}
				config.LastModified = DateTime.Now;
			}
			return configs;
		}
		catch (Exception)
		{
			return configs;
		}
	}

	/// <summary>
	/// 获取当前系统所有可用字段
	/// 基于IoFullData类的属性和Display特性
	/// </summary>
	/// <returns>可用字段列表</returns>
	private List<string> GetCurrentAvailableFields()
	{
		try
		{
			return (from p in typeof(IoFullData).GetProperties()
				where p.CanRead && IsDisplayableProperty(p)
				select p.Name).ToList();
		}
		catch
		{
			return GetBasicFields();
		}
	}

	/// <summary>
	/// 判断属性是否应该显示
	/// </summary>
	private static bool IsDisplayableProperty(PropertyInfo property)
	{
		string[] array = new string[4] { "Id", "IsDeleted", "CreateUser", "ModifyUser" };
		if (!((ReadOnlySpan<string>)array).Contains(property.Name) && property.PropertyType != typeof(object))
		{
			if (property.PropertyType.IsClass)
			{
				return property.PropertyType == typeof(string);
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// 获取基础字段列表（在反射失败时使用）
	/// </summary>
	private List<string> GetBasicFields()
	{
		return new List<string>
		{
			"TagName", "IoType", "StationName", "SignalPositionNumber", "Description", "CabinetNumber", "CardType", "SlotNumber", "TerminalBoardType", "ElectricalCharacteristics",
			"PowerSupplyMethod"
		};
	}

	/// <summary>
	/// 添加新增的核心字段
	/// 确保核心业务字段在兼容性处理时不会丢失
	/// </summary>
	/// <param name="selectedFields">已选择的字段列表</param>
	/// <param name="availableFields">可用字段列表</param>
	private void AddNewCoreFields(List<string> selectedFields, List<string> availableFields)
	{
		string[] array = new string[3] { "StationName", "IoType", "TagName" };
		string[] array2 = array;
		foreach (string item in array2)
		{
			if (availableFields.Contains(item))
			{
				selectedFields.Contains(item);
			}
		}
	}

	/// <summary>
	/// 为新字段添加默认列配置
	/// </summary>
	/// <param name="columnOrder">列配置列表</param>
	/// <param name="availableFields">可用字段列表</param>
	/// <param name="addToFront">是否添加到最前面，默认false（添加到最后）</param>
	private void AddNewFieldColumns(List<ColumnInfo> columnOrder, List<string> availableFields, bool addToFront = false)
	{
		HashSet<string> existingFields = columnOrder.Select((ColumnInfo c) => c.FieldName).ToHashSet();
		List<string> list = availableFields.Where((string field) => !existingFields.Contains(field)).ToList();
		if (list.Count == 0)
		{
			return;
		}
		list.Sort();
		if (addToFront)
		{
			foreach (ColumnInfo item in columnOrder)
			{
				item.Order += list.Count;
			}
			for (int num = 0; num < list.Count; num++)
			{
				string fieldName = list[num];
				string fieldDisplayName = GetFieldDisplayName(fieldName);
				columnOrder.Add(new ColumnInfo
				{
					FieldName = fieldName,
					DisplayName = fieldDisplayName,
					Order = num,
					IsVisible = false,
					Type = ColumnType.Text
				});
			}
			return;
		}
		int num2 = ((columnOrder.Count > 0) ? columnOrder.Max((ColumnInfo c) => c.Order) : (-1));
		foreach (string item2 in list)
		{
			string fieldDisplayName2 = GetFieldDisplayName(item2);
			ColumnInfo obj = new ColumnInfo
			{
				FieldName = item2,
				DisplayName = fieldDisplayName2
			};
			num2 = (obj.Order = num2 + 1);
			obj.IsVisible = false;
			obj.Type = ColumnType.Text;
			columnOrder.Add(obj);
		}
	}

	/// <summary>
	/// 获取字段显示名称
	/// </summary>
	private string GetFieldDisplayName(string fieldName)
	{
		try
		{
			PropertyInfo property = typeof(IoFullData).GetProperty(fieldName);
			if (property != null)
			{
				DisplayAttribute customAttribute = property.GetCustomAttribute<DisplayAttribute>();
				if (customAttribute != null && customAttribute.Name != null)
				{
					return customAttribute.Name;
				}
			}
		}
		catch
		{
		}
		return fieldName;
	}

	/// <summary>
	/// 重新排序列配置
	/// 确保列配置的顺序连续且合理
	/// </summary>
	/// <param name="columnOrder">列配置列表</param>
	private void ReorderColumns(List<ColumnInfo> columnOrder)
	{
		List<ColumnInfo> list = columnOrder.OrderBy((ColumnInfo c) => c.Order).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			list[num].Order = num;
		}
	}

	/// <summary>
	/// 备份本地配置文件
	/// 在迁移完成后，将原始配置文件备份以防需要回滚
	/// </summary>
	/// <param name="exportType">导出类型</param>
	private async Task BackupLocalConfigFile(ExportType exportType)
	{
		_ = 1;
		try
		{
			List<string> possibleLocalConfigPaths = GetPossibleLocalConfigPaths(exportType);
			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			foreach (string item in possibleLocalConfigPaths)
			{
				if (File.Exists(item))
				{
					string text = item + ".migrated_" + timestamp + ".bak";
					string path = text;
					await File.WriteAllTextAsync(path, await File.ReadAllTextAsync(item));
				}
			}
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// 公开的字段同步方法
	/// 供外部调用，当检测到字段变化时主动同步配置
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="availableFields">当前可用字段列表</param>
	/// <param name="projectId">项目ID</param>
	/// <returns>异步任务</returns>
	public async Task SyncFieldChangesForUser(ExportType exportType, List<string> availableFields, int? projectId = null)
	{
		_ = 2;
		try
		{
			int currentUserId = _globalModel.User?.Id ?? 0;
			if (currentUserId == 0)
			{
				return;
			}
			List<ExportConfig> userConfigs = await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType);
			if (userConfigs != null && userConfigs.Count > 0)
			{
				_ = userConfigs.Count;
				List<ExportConfig> list = await ProcessFieldCompatibility(userConfigs);
				if (HasConfigChanges(userConfigs, list))
				{
					await _storageService.UploadUserExportConfigAsync(currentUserId, exportType, list);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// 检查配置是否有实际变化
	/// </summary>
	private bool HasConfigChanges(List<ExportConfig> original, List<ExportConfig> updated)
	{
		if (original.Count != updated.Count)
		{
			return true;
		}
		for (int i = 0; i < original.Count; i++)
		{
			ExportConfig exportConfig = original[i];
			ExportConfig exportConfig2 = updated[i];
			if (exportConfig.SelectedFields?.Count != exportConfig2.SelectedFields?.Count || exportConfig.ColumnOrder?.Count != exportConfig2.ColumnOrder?.Count)
			{
				return true;
			}
		}
		return false;
	}
}

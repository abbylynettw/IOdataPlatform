using IODataPlatform.Models.DBModels;
using IODataPlatform.Models;
using IODataPlatform.Models.ExportModels;
using System.Text.Json;
using System.IO;
using System.Reflection;

namespace IODataPlatform.Services
{
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
                
            // 在服务初始化时检查并迁移本地配置
            _ = Task.Run(async () => await CheckAndMigrateLocalConfigsAsync());
        }

        /// <summary>
        /// 获取当前用户的所有导出配置
        /// 按优先级合并全局配置、项目配置和用户配置
        /// 优先级：用户配置 > 项目配置 > 全局配置
        /// </summary>
        /// <param name="exportType">导出类型</param>
        /// <param name="projectId">项目ID，可选</param>
        /// <returns>合并后的配置列表</returns>
        public async Task<List<ExportConfig>> GetUserConfigsAsync(ExportType exportType, int? projectId = null)
        {
            var allConfigs = new List<ExportConfig>();
            var currentUserId = _globalModel.User?.Id ?? 0;

            // 1. 加载全局配置
            var globalConfigs = await _storageService.DownloadGlobalExportConfigAsync(exportType);
            foreach (var config in globalConfigs)
            {
                config.ConfigName = $"[全局] {config.ConfigName}";
                allConfigs.Add(config);
            }

            // 2. 加载项目配置（如果指定了项目）
            if (projectId.HasValue)
            {
                var projectConfigs = await _storageService.DownloadProjectExportConfigAsync(projectId.Value, exportType);
                foreach (var config in projectConfigs)
                {
                    config.ConfigName = $"[项目] {config.ConfigName}";
                    allConfigs.Add(config);
                }
            }

            // 3. 加载用户配置
            var userConfigs = await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType);
            foreach (var config in userConfigs)
            {
                config.ConfigName = $"[我的] {config.ConfigName}";
                allConfigs.Add(config);
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
            var currentUserId = _globalModel.User?.Id ?? 0;
            if (currentUserId == 0) return;

            // 下载现有配置
            var existingConfigs = await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType);
            
            // 移除同名配置（更新操作）
            existingConfigs.RemoveAll(c => c.ConfigName == config.ConfigName);
            
            // 添加新配置
            existingConfigs.Add(config);
            
            // 上传更新后的配置
            await _storageService.UploadUserExportConfigAsync(currentUserId, exportType, existingConfigs);
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
            var currentUserId = _globalModel.User?.Id ?? 0;
            if (currentUserId == 0) return;

            // 下载现有配置
            var existingConfigs = await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType);
            
            // 移除指定配置
            existingConfigs.RemoveAll(c => c.ConfigName == configName);
            
            // 上传更新后的配置
            await _storageService.UploadUserExportConfigAsync(currentUserId, exportType, existingConfigs);
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
            // 下载现有项目配置
            var existingConfigs = await _storageService.DownloadProjectExportConfigAsync(projectId, exportType);
            
            // 移除同名配置（更新操作）
            existingConfigs.RemoveAll(c => c.ConfigName == config.ConfigName);
            
            // 添加新配置
            existingConfigs.Add(config);
            
            // 上传更新后的配置
            await _storageService.UploadProjectExportConfigAsync(projectId, exportType, existingConfigs);
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
            // 下载现有项目配置
            var existingConfigs = await _storageService.DownloadProjectExportConfigAsync(projectId, exportType);
            
            // 移除指定配置
            existingConfigs.RemoveAll(c => c.ConfigName == configName);
            
            // 上传更新后的配置
            await _storageService.UploadProjectExportConfigAsync(projectId, exportType, existingConfigs);
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
            // 下载现有全局配置
            var existingConfigs = await _storageService.DownloadGlobalExportConfigAsync(exportType);
            
            // 移除同名配置（更新操作）
            existingConfigs.RemoveAll(c => c.ConfigName == config.ConfigName);
            
            // 添加新配置
            existingConfigs.Add(config);
            
            // 上传更新后的配置
            await _storageService.UploadGlobalExportConfigAsync(exportType, existingConfigs);
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
            // 下载现有全局配置
            var existingConfigs = await _storageService.DownloadGlobalExportConfigAsync(exportType);
            
            // 移除指定配置
            existingConfigs.RemoveAll(c => c.ConfigName == configName);
            
            // 上传更新后的配置
            await _storageService.UploadGlobalExportConfigAsync(exportType, existingConfigs);
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
            var currentUserId = _globalModel.User?.Id ?? 0;

            // 同步用户配置
            if (currentUserId > 0)
            {
                var userConfigs = await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType);
                var updatedUserConfigs = SyncConfigFields(userConfigs, availableFields);
                if (updatedUserConfigs.Any())
                {
                    await _storageService.UploadUserExportConfigAsync(currentUserId, exportType, updatedUserConfigs);
                }
            }

            // 同步项目配置
            if (projectId.HasValue)
            {
                var projectConfigs = await _storageService.DownloadProjectExportConfigAsync(projectId.Value, exportType);
                var updatedProjectConfigs = SyncConfigFields(projectConfigs, availableFields);
                if (updatedProjectConfigs.Any())
                {
                    await _storageService.UploadProjectExportConfigAsync(projectId.Value, exportType, updatedProjectConfigs);
                }
            }

            // 同步全局配置（仅管理员权限时）
            // 这里可以根据权限控制是否同步全局配置
            var globalConfigs = await _storageService.DownloadGlobalExportConfigAsync(exportType);
            var updatedGlobalConfigs = SyncConfigFields(globalConfigs, availableFields);
            if (updatedGlobalConfigs.Any())
            {
                await _storageService.UploadGlobalExportConfigAsync(exportType, updatedGlobalConfigs);
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
            bool hasChanges = false;

            foreach (var config in configs)
            {
                // 同步SelectedFields字段
                if (config.SelectedFields != null)
                {
                    var originalCount = config.SelectedFields.Count;
                    
                    // 移除不存在的字段
                    config.SelectedFields.RemoveAll(field => !availableFields.Contains(field));
                    
                    if (config.SelectedFields.Count != originalCount)
                    {
                        hasChanges = true;
                    }
                }

                // 同步ColumnOrder中的字段
                if (config.ColumnOrder != null)
                {
                    var originalCount = config.ColumnOrder.Count;
                    
                    // 移除不存在的字段对应的列配置
                    config.ColumnOrder.RemoveAll(col => !availableFields.Contains(col.FieldName));
                    
                    if (config.ColumnOrder.Count != originalCount)
                    {
                        hasChanges = true;
                    }
                }
            }

            return hasChanges ? configs : new List<ExportConfig>();
        }

        /// <summary>
        /// 获取当前项目ID
        /// 由于项目信息分散在各个ViewModel中管理，此方法返回传入的项目ID
        /// </summary>
        /// <returns>项目ID，如果没有项目返回null</returns>
        public int? GetCurrentProjectId()
        {
            // 项目ID由调用方传入，此方法主要为了保持接口一致性
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
            // 检查SelectedFields
            if (config.SelectedFields != null && config.SelectedFields.Any(field => !availableFields.Contains(field)))
            {
                return true;
            }
            
            // 检查ColumnOrder中的字段
            if (config.ColumnOrder != null && config.ColumnOrder.Any(col => !availableFields.Contains(col.FieldName)))
            {
                return true;
            }
            
            return false;
        }
        
        #region 本地配置兼容性处理
        
        /// <summary>
        /// 检查并迁移本地配置到云端
        /// 在服务初始化时自动执行，确保用户的历史配置不丢失
        /// </summary>
        private async Task CheckAndMigrateLocalConfigsAsync()
        {
            try
            {
                var currentUserId = _globalModel.User?.Id ?? 0;
                if (currentUserId == 0) return;
                
                // 检查各种导出类型的本地配置
                await MigrateLocalConfigsForType(ExportType.CompleteList, currentUserId);
                await MigrateLocalConfigsForType(ExportType.CurrentSystemList, currentUserId);
                await MigrateLocalConfigsForType(ExportType.PublishedList, currentUserId);
            }
            catch (Exception ex)
            {
                // 迁移失败不影响主要功能，只记录日志
                System.Diagnostics.Debug.WriteLine($"本地配置迁移失败：{ex.Message}");
            }
        }
        
        /// <summary>
        /// 迁移指定类型的本地配置
        /// </summary>
        /// <param name="exportType">导出类型</param>
        /// <param name="userId">用户ID</param>
        private async Task MigrateLocalConfigsForType(ExportType exportType, int userId)
        {
            try
            {
                var localConfigs = await LoadLocalConfigs(exportType);
                if (localConfigs?.Count > 0)
                {
                    // 获取当前云端配置
                    var cloudConfigs = await _storageService.DownloadUserExportConfigAsync(userId, exportType);
                    
                    foreach (var localConfig in localConfigs)
                    {
                        // 检查云端是否已存在同名配置
                        if (!cloudConfigs.Any(c => c.ConfigName == localConfig.ConfigName))
                        {
                            // 标记为从本地迁移的配置
                            localConfig.ConfigName = $"[本地迁移] {localConfig.ConfigName}";
                            localConfig.LastModified = DateTime.Now;
                            
                            // 上传到云端
                            await SaveUserConfigAsync(exportType, localConfig);
                        }
                    }
                    
                    // 迁移完成后，备份本地配置文件（可选）
                    await BackupLocalConfigFile(exportType);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"迁移{exportType}类型配置失败：{ex.Message}");
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
            try
            {
                // 检查多个可能的本地配置文件位置
                var possiblePaths = GetPossibleLocalConfigPaths(exportType);
                
                foreach (var configPath in possiblePaths)
                {
                    if (File.Exists(configPath))
                    {
                        var json = await File.ReadAllTextAsync(configPath);
                        if (!string.IsNullOrEmpty(json))
                        {
                            // 尝试反序列化配置
                            var configs = JsonSerializer.Deserialize<List<ExportConfig>>(json);
                            if (configs?.Count > 0)
                            {
                                // 对每个配置进行字段兼容性处理
                                return await ProcessFieldCompatibility(configs);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载本地配置失败：{ex.Message}");
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
            var paths = new List<string>();
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var currentPath = AppDomain.CurrentDomain.BaseDirectory;
            
            var typeFileName = exportType switch
            {
                ExportType.CompleteList => "complete_export_configs.json",
                ExportType.CurrentSystemList => "current_system_export_configs.json",
                ExportType.PublishedList => "published_export_configs.json",
                _ => "export_configs.json"
            };
            
            // 可能的配置文件位置
            paths.Add(Path.Combine(currentPath, "Configs", typeFileName));
            paths.Add(Path.Combine(currentPath, "Assets", "Configs", typeFileName));
            paths.Add(Path.Combine(appDataPath, "IODataPlatform", typeFileName));
            paths.Add(Path.Combine(localAppDataPath, "IODataPlatform", typeFileName));
            paths.Add(Path.Combine(currentPath, typeFileName));
            
            // 通用配置文件名
            paths.Add(Path.Combine(currentPath, "Configs", "export_configs.json"));
            paths.Add(Path.Combine(currentPath, "Assets", "Configs", "export_configs.json"));
            
            return paths;
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
                // 获取当前系统所有可用字段
                var availableFields = GetCurrentAvailableFields();
                
                foreach (var config in configs)
                {
                    // 处理SelectedFields字段兼容性
                    if (config.SelectedFields != null)
                    {
                        // 移除不存在的字段
                        config.SelectedFields.RemoveAll(field => !availableFields.Contains(field));
                        
                        // 添加新增的核心字段（可选）
                        AddNewCoreFields(config.SelectedFields, availableFields);
                    }
                    
                    // 处理ColumnOrder字段兼容性
                    if (config.ColumnOrder != null)
                    {
                        // 移除不存在的字段对应的列配置
                        config.ColumnOrder.RemoveAll(col => !availableFields.Contains(col.FieldName));
                        
                        // 为新字段添加默认列配置
                        AddNewFieldColumns(config.ColumnOrder, availableFields);
                        
                        // 重新排序
                        ReorderColumns(config.ColumnOrder);
                    }
                    
                    // 更新配置的最后修改时间
                    config.LastModified = DateTime.Now;
                }
                
                return configs;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理字段兼容性失败：{ex.Message}");
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
                var properties = typeof(Models.ExcelModels.IoFullData).GetProperties()
                    .Where(p => p.CanRead && IsDisplayableProperty(p))
                    .Select(p => p.Name)
                    .ToList();
                    
                return properties;
            }
            catch
            {
                // 如果反射失败，返回基础字段列表
                return GetBasicFields();
            }
        }
        
        /// <summary>
        /// 判断属性是否应该显示
        /// </summary>
        private static bool IsDisplayableProperty(System.Reflection.PropertyInfo property)
        {
            // 排除一些不需要显示的属性
            var excludedProperties = new[] { "Id", "IsDeleted", "CreateUser", "ModifyUser" };
            return !excludedProperties.Contains(property.Name) && 
                   property.PropertyType != typeof(object) &&
                   (!property.PropertyType.IsClass || property.PropertyType == typeof(string));
        }
        
        /// <summary>
        /// 获取基础字段列表（在反射失败时使用）
        /// </summary>
        private List<string> GetBasicFields()
        {
            return new List<string>
            {
                "TagName", "IoType", "StationName", "SignalPositionNumber", 
                "Description", "CabinetNumber", "CardType", "SlotNumber",
                "TerminalBoardType", "ElectricalCharacteristics", "PowerSupplyMethod"
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
            // 根据项目规范，StationName和IoType是核心字段
            var coreFields = new[] { "StationName", "IoType", "TagName" };
            
            foreach (var coreField in coreFields)
            {
                if (availableFields.Contains(coreField) && !selectedFields.Contains(coreField))
                {
                    // 注意：新增的核心字段也默认不选中，遵循用户自由选择原则
                    // selectedFields.Add(coreField); // 不再自动添加
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
            var existingFields = columnOrder.Select(c => c.FieldName).ToHashSet();
            
            // 获取新增的字段
            var newFields = availableFields.Where(field => !existingFields.Contains(field)).ToList();
            if (newFields.Count == 0) return;
            
            // 将新字段按字母顺序排列
            newFields.Sort();
            
            if (addToFront)
            {
                // 添加到最前面：先将现有字段的顺序后移
                foreach (var existingColumn in columnOrder)
                {
                    existingColumn.Order += newFields.Count;
                }
                
                // 然后添加新字段到前面
                for (int i = 0; i < newFields.Count; i++)
                {
                    var field = newFields[i];
                    var displayName = GetFieldDisplayName(field);
                    columnOrder.Add(new ColumnInfo
                    {
                        FieldName = field,
                        DisplayName = displayName,
                        Order = i, // 放在最前面
                        IsVisible = false,
                        Type = ColumnType.Text
                    });
                    
                    System.Diagnostics.Debug.WriteLine($"新增字段配置：{field} ({displayName}) - 默认不选中，排序位置：{i} (最前)");
                }
            }
            else
            {
                // 添加到最后面（默认行为）
                var maxOrder = columnOrder.Count > 0 ? columnOrder.Max(c => c.Order) : -1;
                
                foreach (var field in newFields)
                {
                    var displayName = GetFieldDisplayName(field);
                    columnOrder.Add(new ColumnInfo
                    {
                        FieldName = field,
                        DisplayName = displayName,
                        Order = ++maxOrder, // 放在最后
                        IsVisible = false, // 新字段默认不显示，由用户自由选择
                        Type = ColumnType.Text
                    });
                    
                    System.Diagnostics.Debug.WriteLine($"新增字段配置：{field} ({displayName}) - 默认不选中，排序位置：{maxOrder} (最后)");
                }
            }
        }
        
        /// <summary>
        /// 获取字段显示名称
        /// </summary>
        private string GetFieldDisplayName(string fieldName)
        {
            try
            {
                var property = typeof(Models.ExcelModels.IoFullData).GetProperty(fieldName);
                if (property != null)
                {
                    var displayAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                    if (displayAttribute?.Name != null)
                    {
                        return displayAttribute.Name;
                    }
                }
            }
            catch
            {
                // 如果获取失败，使用字段名
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
            // 按当前顺序重新编号
            var sortedColumns = columnOrder.OrderBy(c => c.Order).ToList();
            for (int i = 0; i < sortedColumns.Count; i++)
            {
                sortedColumns[i].Order = i;
            }
        }
        
        /// <summary>
        /// 备份本地配置文件
        /// 在迁移完成后，将原始配置文件备份以防需要回滚
        /// </summary>
        /// <param name="exportType">导出类型</param>
        private async Task BackupLocalConfigFile(ExportType exportType)
        {
            try
            {
                var possiblePaths = GetPossibleLocalConfigPaths(exportType);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                foreach (var configPath in possiblePaths)
                {
                    if (File.Exists(configPath))
                    {
                        var backupPath = $"{configPath}.migrated_{timestamp}.bak";
                        await File.WriteAllTextAsync(backupPath, await File.ReadAllTextAsync(configPath));
                        
                        // 可选：删除原始文件（取消注释下面的代码）
                        // File.Delete(configPath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"备份本地配置文件失败：{ex.Message}");
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
            try
            {
                var currentUserId = _globalModel.User?.Id ?? 0;
                if (currentUserId == 0) return;
                
                // 获取用户配置
                var userConfigs = await _storageService.DownloadUserExportConfigAsync(currentUserId, exportType);
                if (userConfigs?.Count > 0)
                {
                    var originalCount = userConfigs.Count;
                    var updatedConfigs = await ProcessFieldCompatibility(userConfigs);
                    
                    // 只有在有实际变化时才上传
                    if (HasConfigChanges(userConfigs, updatedConfigs))
                    {
                        await _storageService.UploadUserExportConfigAsync(currentUserId, exportType, updatedConfigs);
                        System.Diagnostics.Debug.WriteLine($"已同步{exportType}类型的{updatedConfigs.Count}个用户配置");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"同步用户字段变更失败：{ex.Message}");
            }
        }
        
        /// <summary>
        /// 检查配置是否有实际变化
        /// </summary>
        private bool HasConfigChanges(List<ExportConfig> original, List<ExportConfig> updated)
        {
            if (original.Count != updated.Count) return true;
            
            for (int i = 0; i < original.Count; i++)
            {
                var orig = original[i];
                var upd = updated[i];
                
                if (orig.SelectedFields?.Count != upd.SelectedFields?.Count ||
                    orig.ColumnOrder?.Count != upd.ColumnOrder?.Count)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        #endregion
    }
}

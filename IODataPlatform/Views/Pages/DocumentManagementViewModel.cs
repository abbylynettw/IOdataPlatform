using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 文档管理页面ViewModel
/// </summary>
/// <summary>
/// 文档管理ViewModel - 同步和移动功能扩展
/// </summary>
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public partial class DocumentManagementViewModel : ObservableObject, INavigationAware
{
	/// <summary>
	/// 同步统计信息
	/// </summary>
	private class SyncStats
	{
		public int SuccessCount { get; set; }

		public int SkipCount { get; set; }

		public int FailCount { get; set; }

		public List<string> Errors { get; set; } = new List<string>();
	}

	public readonly IMessageService message;

	public readonly IPickerService picker;

	public readonly SqlSugarContext context;

	public readonly StorageService storage;

	public readonly GlobalModel model;

	/// <summary>
	/// 目录树根节点列表（旧版，保留兼容）
	/// </summary>
	[ObservableProperty]
	private ObservableCollection<DocCategory> categories = new ObservableCollection<DocCategory>();

	/// <summary>
	/// 统一的文档树（文件夹+文件）
	/// </summary>
	[ObservableProperty]
	private ObservableCollection<DocumentTreeNode> treeItems = new ObservableCollection<DocumentTreeNode>();

	/// <summary>
	/// 当前目录下的文件列表
	/// </summary>
	[ObservableProperty]
	private ObservableCollection<DocFile> currentFiles = new ObservableCollection<DocFile>();

	/// <summary>
	/// 当前选中的文件
	/// </summary>
	[ObservableProperty]
	private DocFile? selectedFile;

	/// <summary>
	/// 当前选中的树节点
	/// </summary>
	[ObservableProperty]
	private DocumentTreeNode? selectedTreeNode;

	/// <summary>
	/// 文档大纲列表
	/// </summary>
	[ObservableProperty]
	private ObservableCollection<OutlineItem> outlineItems = new ObservableCollection<OutlineItem>();

	public DocumentManagementViewModel(IMessageService message, IPickerService picker, SqlSugarContext context, StorageService storage, GlobalModel model)
	{
		this.message = message;
		this.picker = picker;
		this.context = context;
		this.storage = storage;
		this.model = model;
	}

	/// <summary>
	/// 加载目录下的文件列表
	/// </summary>
	public async Task LoadFilesAsync(DocCategory category)
	{
		try
		{
			CurrentFiles = new ObservableCollection<DocFile>(await (from x in context.Db.Queryable<DocFile>()
				where x.CategoryId == category.Id
				orderby x.SortOrder
				select x).ToListAsync());
			SelectedFile = null;
		}
		catch (Exception ex)
		{
			await message.ErrorAsync("加载文件失败: " + ex.Message);
		}
	}

	public void OnNavigatedFrom()
	{
	}

	public async void OnNavigatedTo()
	{
		await LoadTreeAsync();
	}

	/// <summary>
	/// 加载统一的文档树（文件夹+文件）
	/// </summary>
	public async Task LoadTreeAsync()
	{
		try
		{
			model.Status.Busy("加载文档树中...");
			List<DocumentTreeNode> list = BuildUnifiedTree(await (from x in context.Db.Queryable<DocCategory>()
				orderby x.SortOrder
				select x).ToListAsync(), await (from x in context.Db.Queryable<DocFile>()
				orderby x.SortOrder
				select x).ToListAsync());
			TreeItems = new ObservableCollection<DocumentTreeNode>(list);
			model.Status.Reset();
		}
		catch (Exception ex)
		{
			model.Status.Reset();
			await message.ErrorAsync("加载文档树失败: " + ex.Message);
		}
	}

	/// <summary>
	/// 构建统一树（文件夹+文件混合）
	/// </summary>
	private List<DocumentTreeNode> BuildUnifiedTree(List<DocCategory> allCategories, List<DocFile> allFiles)
	{
		List<DocCategory> list = (from x in allCategories
			where !x.ParentId.HasValue
			orderby x.SortOrder
			select x).ToList();
		List<DocumentTreeNode> list2 = new List<DocumentTreeNode>();
		foreach (DocCategory item2 in list)
		{
			DocumentTreeNode item = CreateFolderNode(item2, allCategories, allFiles);
			list2.Add(item);
		}
		return list2;
	}

	/// <summary>
	/// 创建文件夹节点（递归包含子文件夹和文件）
	/// </summary>
	private DocumentTreeNode CreateFolderNode(DocCategory category, List<DocCategory> allCategories, List<DocFile> allFiles)
	{
		DocumentTreeNode documentTreeNode = new DocumentTreeNode
		{
			NodeType = DocumentNodeType.Folder,
			Name = category.Name,
			Category = category,
			IsExpanded = true
		};
		List<DocCategory> list = (from x in allCategories
			where x.ParentId == category.Id
			orderby x.SortOrder
			select x).ToList();
		foreach (DocCategory item3 in list)
		{
			DocumentTreeNode item = CreateFolderNode(item3, allCategories, allFiles);
			documentTreeNode.Children.Add(item);
		}
		List<DocFile> list2 = (from x in allFiles
			where x.CategoryId == category.Id
			orderby x.SortOrder
			select x).ToList();
		foreach (DocFile item4 in list2)
		{
			string text = item4.FileType?.ToLower() switch
			{
				"markdown" => ".md", 
				"word" => ".docx", 
				"excel" => ".xlsx", 
				"pdf" => ".pdf", 
				_ => "", 
			};
			DocumentTreeNode item2 = new DocumentTreeNode
			{
				NodeType = DocumentNodeType.Document,
				Name = item4.Title + text,
				File = item4
			};
			documentTreeNode.Children.Add(item2);
		}
		return documentTreeNode;
	}

	/// <summary>
	/// 加载目录树（旧方法，保留兼容）
	/// </summary>
	private async Task LoadCategories()
	{
		try
		{
			model.Status.Busy("加载目录中...");
			List<DocCategory> list = await (from x in context.Db.Queryable<DocCategory>()
				orderby x.SortOrder
				select x).ToListAsync();
			List<DocCategory> list2 = list.Where((DocCategory x) => !x.ParentId.HasValue).ToList();
			foreach (DocCategory item in list2)
			{
				BuildTree(item, list);
			}
			Categories = new ObservableCollection<DocCategory>(list2);
			model.Status.Reset();
		}
		catch (Exception ex)
		{
			model.Status.Reset();
			await message.ErrorAsync("加载目录失败: " + ex.Message);
		}
	}

	/// <summary>
	/// 递归构建树形结构
	/// </summary>
	private void BuildTree(DocCategory parent, List<DocCategory> allCategories)
	{
		List<DocCategory> list = (from x in allCategories
			where x.ParentId == parent.Id
			orderby x.SortOrder
			select x).ToList();
		parent.Children = new ObservableCollection<DocCategory>(list);
		foreach (DocCategory item in list)
		{
			BuildTree(item, allCategories);
		}
	}

	/// <summary>
	/// 新建根文件夹
	/// </summary>
	[RelayCommand]
	public async Task AddFolder()
	{
		string folderName = await ShowInputDialog("新建文件夹", "请输入文件夹名称：", "新建文件夹");
		if (string.IsNullOrWhiteSpace(folderName))
		{
			return;
		}
		try
		{
			model.Status.Busy("创建中...");
			int valueOrDefault = (await (from x in context.Db.Queryable<DocCategory>()
				where x.ParentId == (long?)null
				select x).MaxAsync((Expression<Func<DocCategory, int?>>)((DocCategory x) => x.SortOrder))).GetValueOrDefault();
			DocCategory insertObj = new DocCategory
			{
				Name = folderName,
				ParentId = null,
				SortOrder = valueOrDefault + 1,
				CreatedAt = DateTime.Now
			};
			await context.Db.Insertable(insertObj).ExecuteCommandAsync();
			await LoadTreeAsync();
			model.Status.Success("文件夹已创建：" + folderName);
		}
		catch (Exception ex)
		{
			model.Status.Reset();
			await message.ErrorAsync("创建失败: " + ex.Message);
		}
	}

	/// <summary>
	/// 新建子文件夹
	/// </summary>
	[RelayCommand]
	public async Task AddSubFolderAsync(DocCategory parent)
	{
		string folderName = await ShowInputDialog("新建子文件夹", "请输入文件夹名称：", "新建文件夹");
		if (string.IsNullOrWhiteSpace(folderName))
		{
			return;
		}
		try
		{
			model.Status.Busy("创建中...");
			int valueOrDefault = (await (from x in context.Db.Queryable<DocCategory>()
				where x.ParentId == (long?)parent.Id
				select x).MaxAsync((Expression<Func<DocCategory, int?>>)((DocCategory x) => x.SortOrder))).GetValueOrDefault();
			DocCategory insertObj = new DocCategory
			{
				Name = folderName,
				ParentId = parent.Id,
				SortOrder = valueOrDefault + 1,
				CreatedAt = DateTime.Now
			};
			await context.Db.Insertable(insertObj).ExecuteCommandAsync();
			await LoadTreeAsync();
			model.Status.Success("已在 \"" + parent.Name + "\" 下创建：" + folderName);
		}
		catch (Exception ex)
		{
			model.Status.Reset();
			await message.ErrorAsync("创建失败: " + ex.Message);
		}
	}

	/// <summary>
	/// 编辑文件夹名称
	/// </summary>
	[RelayCommand]
	public async Task EditFolderAsync(DocCategory category)
	{
		string newName = await ShowInputDialog("编辑文件夹", "请输入新名称：", category.Name);
		if (string.IsNullOrWhiteSpace(newName) || newName == category.Name)
		{
			return;
		}
		try
		{
			model.Status.Busy("更新中...");
			await (from x in context.Db.Updateable<DocCategory>().SetColumns((DocCategory x) => x.Name == newName)
				where x.Id == category.Id
				select x).ExecuteCommandAsync();
			await LoadTreeAsync();
			model.Status.Success("文件夹已重命名：" + newName);
		}
		catch (Exception ex)
		{
			model.Status.Reset();
			await message.ErrorAsync("更新失败: " + ex.Message);
		}
	}

	/// <summary>
	/// 删除文件夹
	/// </summary>
	[RelayCommand]
	public async Task DeleteFolderAsync(DocCategory category)
	{
		int num = await (from x in context.Db.Queryable<DocCategory>()
			where x.ParentId == (long?)category.Id
			select x).CountAsync();
		string text = ((num > 0) ? $"确认删除文件夹 \"{category.Name}\"?\n\n警告：该文件夹包含 {num} 个子项，将一并删除！" : ("确认删除文件夹 \"" + category.Name + "\"?"));
		if (await message.ConfirmAsync(text))
		{
			try
			{
				model.Status.Busy("删除中...");
				await DeleteCategoryRecursive(category.Id);
				await LoadTreeAsync();
				model.Status.Success("文件夹已删除：" + category.Name);
			}
			catch (Exception ex)
			{
				model.Status.Reset();
				await message.ErrorAsync("删除失败: " + ex.Message);
			}
		}
	}

	/// <summary>
	/// 递归删除目录及其子项
	/// </summary>
	private async Task DeleteCategoryRecursive(long categoryId)
	{
		foreach (DocFile item in await (from x in context.Db.Queryable<DocFile>()
			where x.CategoryId == categoryId
			select x).ToListAsync())
		{
			DocFile file = item;
			try
			{
				if (!string.IsNullOrEmpty(file.FilePath))
				{
					await storage.WebDeleteFileAsync(file.FilePath);
					if (file.FileType == "markdown")
					{
						string text = file.FilePath;
						if (text.EndsWith(".md") || text.EndsWith(".markdown"))
						{
							text = Path.ChangeExtension(text, null);
						}
						try
						{
							await storage.WebDeleteFolderAsync(text);
						}
						catch (Exception)
						{
						}
					}
				}
				await (from x in context.Db.Deleteable<DocFile>()
					where x.Id == file.Id
					select x).ExecuteCommandAsync();
			}
			catch (Exception)
			{
				throw;
			}
		}
		foreach (DocCategory item2 in await (from x in context.Db.Queryable<DocCategory>()
			where x.ParentId == (long?)categoryId
			select x).ToListAsync())
		{
			await DeleteCategoryRecursive(item2.Id);
		}
		try
		{
			string relativePath = $"documents/{categoryId}";
			await storage.WebDeleteFolderAsync(relativePath);
		}
		catch (Exception)
		{
		}
		await (from x in context.Db.Deleteable<DocCategory>()
			where x.Id == categoryId
			select x).ExecuteCommandAsync();
	}

	/// <summary>
	/// 导入文件
	/// </summary>
	[RelayCommand]
	public async Task ImportFileAsync(DocCategory category)
	{
		try
		{
			string[] array = picker.OpenFiles("所有文档|*.md;*.markdown;*.doc;*.docx;*.xls;*.xlsx;*.pdf|Markdown 文件|*.md;*.markdown|Word 文档|*.doc;*.docx|Excel 文件|*.xls;*.xlsx|PDF 文件|*.pdf|所有文件|*.*");
			if (array == null || array.Length == 0)
			{
				return;
			}
			model.Status.Busy($"正在导入 {array.Length} 个文件...");
			int successCount = 0;
			int failCount = 0;
			List<string> errorMessages = new List<string>();
			string[] array2 = array;
			foreach (string filePath in array2)
			{
				try
				{
					string fileName = Path.GetFileNameWithoutExtension(filePath);
					string extension = Path.GetExtension(filePath).ToLower();
					string text;
					switch (extension)
					{
					case ".md":
					case ".markdown":
						text = "markdown";
						break;
					case ".doc":
					case ".docx":
						text = "word";
						break;
					case ".xls":
					case ".xlsx":
						text = "excel";
						break;
					case ".pdf":
						text = "pdf";
						break;
					default:
						text = "unknown";
						break;
					}
					string fileType = text;
					if (fileType == "unknown")
					{
						await message.WarnAsync("不支持的文件类型: " + extension);
						failCount++;
						continue;
					}
					byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
					if (fileType == "pdf" && fileBytes.Length > 5)
					{
						string text2 = Encoding.ASCII.GetString(fileBytes, 0, 5);
						_ = text2 != "%PDF-";
					}
					string fileContent = null;
					if (fileType == "markdown")
					{
						fileContent = await File.ReadAllTextAsync(filePath);
					}
					string fileDir = Path.GetDirectoryName(filePath);
					int maxOrder = (await (from x in context.Db.Queryable<DocFile>()
						where x.CategoryId == category.Id
						select x).MaxAsync((Expression<Func<DocFile, int?>>)((DocFile x) => x.SortOrder))).GetValueOrDefault();
					string text3 = DateTime.Now.ToString("yyyyMMddHHmmss");
					string uniqueFileName = fileName + "_" + text3;
					string relativePath = $"documents/{category.Id}/{uniqueFileName}{extension}";
					await storage.WebUploadFileAsync(relativePath, fileBytes);
					if (fileType == "markdown" && fileContent != null)
					{
						string text4 = $"documents/{category.Id}/{uniqueFileName}";
						string imagesDir = text4 + "/images";
						Dictionary<string, string> dictionary = await UploadMarkdownImagesAsync(fileContent, fileDir, imagesDir);
						if (dictionary.Count > 0)
						{
							fileContent = UpdateImagePathsInMarkdown(fileContent, dictionary);
						}
					}
					DocFile insertObj = new DocFile
					{
						Title = fileName,
						CategoryId = category.Id,
						FileType = fileType,
						Content = (fileContent ?? string.Empty),
						FilePath = relativePath,
						SortOrder = maxOrder + 1,
						CreatedAt = DateTime.Now,
						UpdatedAt = DateTime.Now
					};
					await context.Db.Insertable(insertObj).ExecuteCommandAsync();
					successCount++;
				}
				catch (Exception ex)
				{
					failCount++;
					string item = "导入文件 " + Path.GetFileName(filePath) + " 失败: " + ex.Message;
					errorMessages.Add(item);
					await message.WarnAsync(item);
				}
			}
			if (successCount > 0)
			{
				await LoadTreeAsync();
				string text5 = $"成功导入 {successCount} 个文件" + ((failCount > 0) ? $", {failCount} 个失败" : "");
				model.Status.Success(text5);
			}
			else
			{
				model.Status.Reset();
				string text6 = string.Join("\n", errorMessages.Take(3));
				if (errorMessages.Count > 3)
				{
					text6 += $"\n... 还有 {errorMessages.Count - 3} 个错误";
				}
				await message.ErrorAsync("所有文件导入失败！\n\n" + text6);
			}
		}
		catch (Exception ex2)
		{
			model.Status.Reset();
			string text7 = "导入文件失败: " + ex2.Message;
			await message.ErrorAsync(text7);
		}
	}

	/// <summary>
	/// 提取并上传 Markdown 中的本地图片
	/// </summary>
	private async Task<Dictionary<string, string>> UploadMarkdownImagesAsync(string markdown, string sourceDir, string imagesDir)
	{
		Dictionary<string, string> uploadedImages = new Dictionary<string, string>();
		string pattern = "!\\[([^\\]]*)\\]\\(([^\\)]+)\\)";
		MatchCollection matchCollection = Regex.Matches(markdown, pattern);
		foreach (Match item in matchCollection)
		{
			string imagePath = item.Groups[2].Value;
			if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://") || imagePath.StartsWith("data:image"))
			{
				continue;
			}
			try
			{
				string localImagePath = (Path.IsPathRooted(imagePath) ? imagePath : Path.Combine(sourceDir, imagePath.TrimStart('.', '/', '\\')));
				localImagePath = Path.GetFullPath(localImagePath);
				if (!File.Exists(localImagePath))
				{
					string text = "图片文件不存在: " + imagePath;
					await message.WarnAsync(text);
					continue;
				}
				byte[] body = await File.ReadAllBytesAsync(localImagePath);
				string fileName = Path.GetFileName(localImagePath);
				string serverImagePath = imagesDir + "/" + fileName;
				await storage.WebUploadFileAsync(serverImagePath, body);
				uploadedImages[imagePath] = serverImagePath;
			}
			catch (Exception ex)
			{
				string text2 = "上传图片 " + imagePath + " 失败: " + ex.Message;
				await message.WarnAsync(text2);
			}
		}
		return uploadedImages;
	}

	/// <summary>
	/// 更新 Markdown 内容中的图片路径
	/// </summary>
	private string UpdateImagePathsInMarkdown(string markdown, Dictionary<string, string> imageMapping)
	{
		foreach (KeyValuePair<string, string> item in imageMapping)
		{
			item.Deconstruct(out var key, out var value);
			string text = key;
			string path = value;
			string text2 = "./images/" + Path.GetFileName(path);
			markdown = markdown.Replace("](" + text + ")", "](" + text2 + ")");
		}
		return markdown;
	}

	/// <summary>
	/// 显示输入对话框
	/// </summary>
	private Task<string?> ShowInputDialog(string title, string prompt, string defaultValue)
	{
		TaskCompletionSource<string?> tcs = new TaskCompletionSource<string>();
		((DispatcherObject)Application.Current).Dispatcher.Invoke((Action)delegate
		{
			Window inputWindow = new Window
			{
				Title = title,
				Width = 400.0,
				Height = 180.0,
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				ResizeMode = ResizeMode.NoResize
			};
			Grid grid = new Grid
			{
				RowDefinitions = 
				{
					new RowDefinition
					{
						Height = GridLength.Auto
					},
					new RowDefinition
					{
						Height = GridLength.Auto
					},
					new RowDefinition
					{
						Height = new GridLength(1.0, GridUnitType.Star)
					},
					new RowDefinition
					{
						Height = GridLength.Auto
					}
				}
			};
			Wpf.Ui.Controls.TextBlock element = new Wpf.Ui.Controls.TextBlock
			{
				Text = prompt,
				Margin = new Thickness(20.0, 20.0, 20.0, 10.0),
				FontWeight = FontWeights.Medium
			};
			Grid.SetRow(element, 0);
			grid.Children.Add(element);
			Wpf.Ui.Controls.TextBox inputBox = new Wpf.Ui.Controls.TextBox
			{
				Text = defaultValue,
				Margin = new Thickness(20.0, 0.0, 20.0, 15.0)
			};
			Grid.SetRow(inputBox, 1);
			grid.Children.Add(inputBox);
			Grid.SetRow(new Border(), 2);
			StackPanel stackPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Right,
				Margin = new Thickness(20.0, 10.0, 20.0, 20.0)
			};
			Wpf.Ui.Controls.Button confirmButton = new Wpf.Ui.Controls.Button
			{
				Content = "确认",
				Margin = new Thickness(0.0, 0.0, 10.0, 0.0),
				MinWidth = 80.0,
				Appearance = ControlAppearance.Primary
			};
			Wpf.Ui.Controls.Button button = new Wpf.Ui.Controls.Button
			{
				Content = "取消",
				MinWidth = 80.0
			};
			stackPanel.Children.Add(confirmButton);
			stackPanel.Children.Add(button);
			Grid.SetRow(stackPanel, 3);
			grid.Children.Add(stackPanel);
			inputWindow.Content = grid;
			confirmButton.Click += delegate
			{
				if (string.IsNullOrWhiteSpace(inputBox.Text))
				{
					message.WarnAsync("请输入内容！");
				}
				else
				{
					tcs.SetResult(inputBox.Text.Trim());
					inputWindow.Close();
				}
			};
			button.Click += delegate
			{
				tcs.SetResult(null);
				inputWindow.Close();
			};
			inputWindow.Closed += delegate
			{
				if (!tcs.Task.IsCompleted)
				{
					tcs.SetResult(null);
				}
			};
			inputBox.KeyDown += delegate(object s, KeyEventArgs e)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Invalid comparison between Unknown and I4
				if ((int)e.Key == 6)
				{
					confirmButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
				}
			};
			inputBox.Focus();
			inputBox.SelectAll();
			inputWindow.ShowDialog();
		});
		return tcs.Task;
	}

	/// <summary>
	/// 同步本地文档到服务器
	/// </summary>
	[RelayCommand]
	public async Task SyncLocalDocsAsync()
	{
		try
		{
			string text = picker.PickFolder();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (!Directory.Exists(text))
			{
				await message.WarnAsync("所选目录不存在：\n" + text);
				return;
			}
			List<string> list = (from f in Directory.GetFiles(text, "*.*", SearchOption.AllDirectories)
				where IsSupportedFileType(f)
				select f).ToList();
			if (list.Count == 0)
			{
				await message.MessageAsync("所选目录中没有可同步的文档文件。\n\n路径：" + text + "\n\n支持的文件类型：.md, .markdown, .doc, .docx, .xls, .xlsx, .xlsm, .pdf");
				return;
			}
			string messageBoxText = $"确认同步本地文档到平台？\n\n本地路径：{text}\n找到文件：{list.Count} 个\n\n同步操作将：\n1. 在数据库中创建相同的目录结构\n2. 上传所有支持的文档文件到服务器\n3. 在界面上显示同步后的目录树\n4. 已存在的同名文件会被跳过";
			System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(messageBoxText, "系统提示", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (messageBoxResult != System.Windows.MessageBoxResult.Yes)
			{
				return;
			}
			model.Status.Busy("正在同步本地文档...");
			SyncStats stats = new SyncStats();
			await SyncDirectoryAsync(text, null, stats, text);
			await LoadTreeAsync();
			string text2 = $"同步完成！\n\n✓ 成功：{stats.SuccessCount} 个文件\n○ 跳过：{stats.SkipCount} 个文件（已存在）\n✗ 失败：{stats.FailCount} 个文件";
			if (stats.Errors.Count > 0)
			{
				text2 = text2 + "\n\n错误详情（前3条）：\n" + string.Join("\n", stats.Errors.Take(3));
				if (stats.Errors.Count > 3)
				{
					text2 += $"\n... 还有 {stats.Errors.Count - 3} 个错误";
				}
			}
			if (stats.FailCount > 0)
			{
				await message.WarnAsync(text2);
				return;
			}
			model.Status.Success($"同步完成：成功 {stats.SuccessCount} 个，跳过 {stats.SkipCount} 个");
		}
		catch (Exception ex)
		{
			model.Status.Reset();
			await message.ErrorAsync("同步失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 递归同步目录
	/// </summary>
	private async Task SyncDirectoryAsync(string localPath, long? parentCategoryId, SyncStats stats, string rootPath)
	{
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(localPath);
			string dirName = directoryInfo.Name;
			if (localPath == rootPath)
			{
				dirName = directoryInfo.Name;
			}
			DocCategory docCategory = await (from x in context.Db.Queryable<DocCategory>()
				where x.Name == dirName && x.ParentId == parentCategoryId
				select x).FirstAsync();
			long currentCategoryId;
			if (docCategory == null)
			{
				int valueOrDefault = (await (from x in context.Db.Queryable<DocCategory>()
					where x.ParentId == parentCategoryId
					select x).MaxAsync((Expression<Func<DocCategory, int?>>)((DocCategory x) => x.SortOrder))).GetValueOrDefault();
				DocCategory insertObj = new DocCategory
				{
					Name = dirName,
					ParentId = parentCategoryId,
					SortOrder = valueOrDefault + 1,
					CreatedAt = DateTime.Now
				};
				currentCategoryId = await context.Db.Insertable(insertObj).ExecuteReturnIdentityAsync();
			}
			else
			{
				currentCategoryId = docCategory.Id;
			}
			List<string> list = (from f in Directory.GetFiles(localPath)
				where IsSupportedFileType(f)
				select f).ToList();
			foreach (string filePath in list)
			{
				try
				{
					string fileName = Path.GetFileNameWithoutExtension(filePath);
					string extension = Path.GetExtension(filePath).ToLower();
					if (await (from x in context.Db.Queryable<DocFile>()
						where x.CategoryId == currentCategoryId && x.Title == fileName
						select x).FirstAsync() != null)
					{
						stats.SkipCount++;
						continue;
					}
					string text;
					switch (extension)
					{
					case ".md":
					case ".markdown":
						text = "markdown";
						break;
					case ".doc":
					case ".docx":
						text = "word";
						break;
					case ".xls":
					case ".xlsx":
					case ".xlsm":
						text = "excel";
						break;
					case ".pdf":
						text = "pdf";
						break;
					default:
						text = "unknown";
						break;
					}
					string fileType = text;
					if (fileType == "unknown")
					{
						stats.SkipCount++;
						continue;
					}
					byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
					string fileContent = null;
					if (fileType == "markdown")
					{
						fileContent = await File.ReadAllTextAsync(filePath);
					}
					string text2 = DateTime.Now.ToString("yyyyMMddHHmmss");
					string uniqueFileName = fileName + "_" + text2;
					string relativePath = $"documents/{currentCategoryId}/{uniqueFileName}{extension}";
					await storage.WebUploadFileAsync(relativePath, fileBytes);
					if (fileType == "markdown" && fileContent != null)
					{
						string directoryName = Path.GetDirectoryName(filePath);
						string text3 = $"documents/{currentCategoryId}/{uniqueFileName}";
						string imagesDir = text3 + "/images";
						Dictionary<string, string> dictionary = await UploadMarkdownImagesAsync(fileContent, directoryName, imagesDir);
						if (dictionary.Count > 0)
						{
							fileContent = UpdateImagePathsInMarkdown(fileContent, dictionary);
						}
					}
					int valueOrDefault2 = (await (from x in context.Db.Queryable<DocFile>()
						where x.CategoryId == currentCategoryId
						select x).MaxAsync((Expression<Func<DocFile, int?>>)((DocFile x) => x.SortOrder))).GetValueOrDefault();
					DocFile insertObj2 = new DocFile
					{
						Title = fileName,
						CategoryId = currentCategoryId,
						FileType = fileType,
						Content = (fileContent ?? string.Empty),
						FilePath = relativePath,
						SortOrder = valueOrDefault2 + 1,
						CreatedAt = DateTime.Now,
						UpdatedAt = DateTime.Now
					};
					await context.Db.Insertable(insertObj2).ExecuteCommandAsync();
					stats.SuccessCount++;
				}
				catch (Exception ex)
				{
					stats.FailCount++;
					string item = Path.GetFileName(filePath) + ": " + ex.Message;
					stats.Errors.Add(item);
				}
			}
			string[] directories = Directory.GetDirectories(localPath);
			string[] array = directories;
			foreach (string localPath2 in array)
			{
				await SyncDirectoryAsync(localPath2, currentCategoryId, stats, rootPath);
			}
		}
		catch (Exception ex2)
		{
			stats.Errors.Add("目录 " + Path.GetFileName(localPath) + ": " + ex2.Message);
		}
	}

	/// <summary>
	/// 判断文件类型是否支持
	/// </summary>
	private bool IsSupportedFileType(string filePath)
	{
		switch (Path.GetExtension(filePath).ToLower())
		{
		case ".doc":
		case ".pdf":
		case ".xls":
		case ".docx":
		case ".xlsx":
		case ".xlsm":
		case ".md":
		case ".markdown":
			return true;
		default:
			return false;
		}
	}

	/// <summary>
	/// 打开Markdown文件（使用本地工具）
	/// </summary>
	[RelayCommand]
	public async Task OpenMdFileAsync(DocFile file)
	{
		if (file == null)
		{
			await message.WarnAsync("请先选择一个文件！");
			return;
		}

		if (file.FileType != "markdown")
		{
			await message.WarnAsync("只能打开Markdown文件！");
			return;
		}

		try
		{
			model.Status.Busy("正在下载文件...");
			
			// 下载文件到临时目录
			byte[] fileBytes = await storage.WebDownloadFileAsync(file.FilePath);
			
			// 创建临时文件
			string tempPath = Path.Combine(Path.GetTempPath(), "IOPlatform_Documents");
			if (!Directory.Exists(tempPath))
			{
				Directory.CreateDirectory(tempPath);
			}
			
			string extension = Path.GetExtension(file.FilePath);
			string localFilePath = Path.Combine(tempPath, $"{file.Title}{extension}");
			
			await File.WriteAllBytesAsync(localFilePath, fileBytes);
			
			// 使用系统默认程序打开
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
			{
				FileName = localFilePath,
				UseShellExecute = true
			});
			
			model.Status.Success($"已打开文件：{file.Title}");
		}
		catch (Exception ex)
		{
			model.Status.Reset();
			await message.ErrorAsync("打开文件失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 移动文件到其他目录
	/// </summary>
	[RelayCommand]
	public async Task MoveFileAsync(DocFile file)
	{
		try
		{
			List<DocCategory> list = await (from x in context.Db.Queryable<DocCategory>()
				orderby x.SortOrder
				select x).ToListAsync();
			if (list.Count == 0)
			{
				await message.WarnAsync("没有可用的目标目录！");
				return;
			}
			List<DocCategory> list2 = BuildCategoryPathList(list, file.CategoryId);
			if (list2.Count == 0)
			{
				await message.WarnAsync("没有其他可用的目标目录！");
				return;
			}
			DocCategory selectedCategory = await ShowCategorySelectionDialog(list2);
			if (selectedCategory == null)
			{
				return;
			}
			model.Status.Busy("移动文件中...");
			int maxOrder = (await (from x in context.Db.Queryable<DocFile>()
				where x.CategoryId == selectedCategory.Id
				select x).MaxAsync((Expression<Func<DocFile, int?>>)((DocFile x) => x.SortOrder))).GetValueOrDefault();
			await (from x in context.Db.Updateable<DocFile>().SetColumns((DocFile x) => x.CategoryId == selectedCategory.Id).SetColumns((DocFile x) => x.SortOrder == maxOrder + 1)
					.SetColumns((DocFile x) => x.UpdatedAt == DateTime.Now)
				where x.Id == file.Id
				select x).ExecuteCommandAsync();
			await LoadTreeAsync();
			model.Status.Success("文件已移动到：" + selectedCategory.Name);
		}
		catch (Exception ex)
		{
			model.Status.Reset();
			await message.ErrorAsync("移动文件失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 构建目录路径列表（排除当前目录）
	/// </summary>
	private List<DocCategory> BuildCategoryPathList(List<DocCategory> allCategories, long currentCategoryId)
	{
		List<DocCategory> list = new List<DocCategory>();
		foreach (DocCategory allCategory in allCategories)
		{
			if (allCategory.Id != currentCategoryId)
			{
				string categoryPath = GetCategoryPath(allCategory, allCategories);
				DocCategory item = new DocCategory
				{
					Id = allCategory.Id,
					Name = categoryPath,
					ParentId = allCategory.ParentId,
					SortOrder = allCategory.SortOrder
				};
				list.Add(item);
			}
		}
		return list.OrderBy((DocCategory x) => x.Name).ToList();
	}

	/// <summary>
	/// 获取目录的完整路径
	/// </summary>
	private string GetCategoryPath(DocCategory category, List<DocCategory> allCategories)
	{
		List<string> list = new List<string> { category.Name };
		DocCategory current = category;
		while (current.ParentId.HasValue)
		{
			DocCategory docCategory = allCategories.FirstOrDefault((DocCategory x) => x.Id == current.ParentId);
			if (docCategory == null)
			{
				break;
			}
			list.Insert(0, docCategory.Name);
			current = docCategory;
		}
		return string.Join(" > ", list);
	}

	/// <summary>
	/// 显示目录选择对话框
	/// </summary>
	private Task<DocCategory?> ShowCategorySelectionDialog(List<DocCategory> categories)
	{
		TaskCompletionSource<DocCategory?> tcs = new TaskCompletionSource<DocCategory>();
		Window dialog = null;
		((DispatcherObject)Application.Current).Dispatcher.Invoke((Action)delegate
		{
			dialog = new Window
			{
				Title = "选择目标目录",
				Width = 500.0,
				Height = 400.0,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				Owner = Application.Current.MainWindow,
				ResizeMode = ResizeMode.NoResize,
				Topmost = true,
				ShowInTaskbar = false,
				WindowStyle = WindowStyle.SingleBorderWindow
			};
			Grid grid = new Grid
			{
				Background = Brushes.White
			};
			grid.RowDefinitions.Add(new RowDefinition
			{
				Height = GridLength.Auto
			});
			grid.RowDefinitions.Add(new RowDefinition
			{
				Height = new GridLength(1.0, GridUnitType.Star)
			});
			grid.RowDefinitions.Add(new RowDefinition
			{
				Height = GridLength.Auto
			});
			Wpf.Ui.Controls.TextBlock element = new Wpf.Ui.Controls.TextBlock
			{
				Text = "请选择目标目录：",
				Margin = new Thickness(20.0, 20.0, 20.0, 10.0),
				FontWeight = FontWeights.Medium,
				FontSize = 14.0
			};
			Grid.SetRow(element, 0);
			grid.Children.Add(element);
			ListBox listBox = new ListBox
			{
				Margin = new Thickness(20.0, 0.0, 20.0, 10.0),
				ItemsSource = categories,
				DisplayMemberPath = "Name"
			};
			Grid.SetRow(listBox, 1);
			grid.Children.Add(listBox);
			StackPanel stackPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Right,
				Margin = new Thickness(20.0, 10.0, 20.0, 20.0),
				Background = Brushes.Transparent
			};
			Wpf.Ui.Controls.Button confirmButton = new Wpf.Ui.Controls.Button
			{
				Content = "确认",
				Margin = new Thickness(0.0, 0.0, 10.0, 0.0),
				MinWidth = 80.0,
				MinHeight = 32.0,
				Focusable = true,
				IsEnabled = true
			};
			Wpf.Ui.Controls.Button button = new Wpf.Ui.Controls.Button
			{
				Content = "取消",
				MinWidth = 80.0,
				MinHeight = 32.0,
				Focusable = true,
				IsEnabled = true
			};
			stackPanel.Children.Add(confirmButton);
			stackPanel.Children.Add(button);
			Grid.SetRow(stackPanel, 2);
			grid.Children.Add(stackPanel);
			dialog.Content = grid;
			confirmButton.MouseEnter += delegate
			{
				confirmButton.Background = Brushes.LightBlue;
			};
			confirmButton.MouseLeave += delegate
			{
				confirmButton.Background = Brushes.Transparent;
			};
			confirmButton.PreviewMouseDown += delegate
			{
			};
			confirmButton.Click += delegate
			{
				try
				{
					if (listBox.SelectedItem is DocCategory result)
					{
						tcs.TrySetResult(result);
						dialog?.Close();
					}
					else
					{
						((DispatcherObject)Application.Current).Dispatcher.BeginInvoke((Delegate)(Action)async delegate
						{
							await message.WarnAsync("请选择一个目录！");
						}, Array.Empty<object>());
					}
				}
				catch (Exception)
				{
				}
			};
			button.PreviewMouseDown += delegate
			{
			};
			button.Click += delegate
			{
				try
				{
					tcs.TrySetResult(null);
					dialog?.Close();
				}
				catch (Exception)
				{
				}
			};
			dialog.Closed += delegate
			{
				if (!tcs.Task.IsCompleted)
				{
					tcs.TrySetResult(null);
				}
			};
		});
		((DispatcherObject)Application.Current).Dispatcher.BeginInvoke((Delegate)(Action)delegate
		{
			try
			{
				dialog?.ShowDialog();
			}
			catch (Exception)
			{
			}
		}, Array.Empty<object>());
		return tcs.Task;
	}
}

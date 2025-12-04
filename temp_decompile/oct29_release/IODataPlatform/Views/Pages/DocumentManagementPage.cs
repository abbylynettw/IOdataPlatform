using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.Windows;
using Markdig;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// DocumentManagementPage
/// </summary>
public class DocumentManagementPage : Page, INavigableView<DocumentManagementViewModel>, IComponentConnector, IStyleConnector
{
	private readonly MarkdownPipeline _markdownPipeline;

	internal Wpf.Ui.Controls.Button BtnAddFolder;

	internal Wpf.Ui.Controls.Button BtnUploadDoc;

	internal Wpf.Ui.Controls.Button BtnEdit;

	internal Wpf.Ui.Controls.Button BtnDelete;

	internal Wpf.Ui.Controls.Button BtnSync;

	internal Wpf.Ui.Controls.Button BtnMoveFile;

	internal Wpf.Ui.Controls.Button BtnDownload;

	internal Wpf.Ui.Controls.Button BtnReplace;

	internal Wpf.Ui.Controls.Button BtnExportPdf;

	internal TreeView FileTreeView;

	internal WebView2 MarkdownPreview;

	private bool _contentLoaded;

	public DocumentManagementViewModel ViewModel { get; }

	public DocumentManagementPage(DocumentManagementViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
		_markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
		ViewModel.PropertyChanged += ViewModel_PropertyChanged;
		FileTreeView.SelectedItemChanged += FileTreeView_SelectedItemChanged;
		InitializeWebView();
		UpdateButtonsVisibility();
	}

	private async void InitializeWebView()
	{
		await MarkdownPreview.EnsureCoreWebView2Async();
		if (MarkdownPreview.CoreWebView2 != null)
		{
			MarkdownPreview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
			MarkdownPreview.CoreWebView2.Settings.AreDevToolsEnabled = true;
			MarkdownPreview.CoreWebView2.WebMessageReceived += delegate
			{
			};
			MarkdownPreview.CoreWebView2.NavigationCompleted += delegate(object? s, CoreWebView2NavigationCompletedEventArgs e)
			{
				_ = e.IsSuccess;
			};
		}
	}

	private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "SelectedFile")
		{
			RenderPreview();
			UpdateButtonsVisibility();
		}
		else if (e.PropertyName == "SelectedTreeNode")
		{
			DocumentTreeNode? selectedTreeNode = ViewModel.SelectedTreeNode;
			if (selectedTreeNode != null && selectedTreeNode.NodeType == DocumentNodeType.Document)
			{
				ViewModel.SelectedFile = ViewModel.SelectedTreeNode.File;
			}
			else
			{
				ViewModel.SelectedFile = null;
			}
			UpdateButtonsVisibility();
		}
	}

	/// <summary>
	/// 根据选中的对象更新按钮可见性
	/// </summary>
	private void UpdateButtonsVisibility()
	{
		DocumentTreeNode selectedTreeNode = ViewModel.SelectedTreeNode;
		if (selectedTreeNode == null)
		{
			BtnEdit.Visibility = Visibility.Collapsed;
			BtnDelete.Visibility = Visibility.Collapsed;
			BtnDownload.Visibility = Visibility.Collapsed;
			BtnReplace.Visibility = Visibility.Collapsed;
			BtnExportPdf.Visibility = Visibility.Collapsed;
		}
		else if (selectedTreeNode.NodeType == DocumentNodeType.Folder)
		{
			BtnEdit.Visibility = Visibility.Visible;
			BtnDelete.Visibility = Visibility.Visible;
			BtnDownload.Visibility = Visibility.Collapsed;
			BtnReplace.Visibility = Visibility.Collapsed;
			BtnExportPdf.Visibility = Visibility.Collapsed;
		}
		else if (selectedTreeNode.File != null)
		{
			string text = selectedTreeNode.File.FileType?.ToLower() ?? "markdown";
			BtnEdit.Visibility = ((!(text == "markdown")) ? Visibility.Collapsed : Visibility.Visible);
			BtnDelete.Visibility = Visibility.Visible;
			BtnDownload.Visibility = Visibility.Visible;
			BtnReplace.Visibility = Visibility.Visible;
			BtnExportPdf.Visibility = ((!(text == "markdown")) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	/// <summary>
	/// 树视图选中项变化事件
	/// </summary>
	private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (e.NewValue is DocumentTreeNode selectedTreeNode)
		{
			ViewModel.SelectedTreeNode = selectedTreeNode;
		}
		else if (e.NewValue == null)
		{
			ViewModel.SelectedTreeNode = null;
		}
	}

	/// <summary>
	/// 树视图容器鼠标点击事件 - 点击空白处清除选择
	/// </summary>
	private void TreeBorder_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.OriginalSource is FrameworkElement child)
		{
			System.Windows.Controls.TreeViewItem treeViewItem = FindParent<System.Windows.Controls.TreeViewItem>((DependencyObject)(object)child);
			if (treeViewItem == null)
			{
				ViewModel.SelectedTreeNode = null;
				UnselectAllTreeViewItems(FileTreeView);
			}
		}
	}

	/// <summary>
	/// 取消所有TreeViewItem的选中状态
	/// </summary>
	private void UnselectAllTreeViewItems(ItemsControl parent)
	{
		if (parent == null || parent.Items.Count == 0)
		{
			return;
		}
		for (int i = 0; i < parent.Items.Count; i++)
		{
			if (parent.ItemContainerGenerator.ContainerFromIndex(i) is System.Windows.Controls.TreeViewItem treeViewItem)
			{
				treeViewItem.IsSelected = false;
				UnselectAllTreeViewItems(treeViewItem);
			}
		}
	}

	/// <summary>
	/// 查找父级元素
	/// </summary>
	private T? FindParent<T>(DependencyObject child) where T : DependencyObject
	{
		DependencyObject parent = VisualTreeHelper.GetParent(child);
		if (parent == null)
		{
			return default(T);
		}
		T val = (T)(object)((parent is T) ? parent : null);
		if (val != null)
		{
			return val;
		}
		return FindParent<T>(parent);
	}

	/// <summary>
	/// 根据文件类型渲染预览
	/// </summary>
	private void RenderPreview()
	{
		if (ViewModel.SelectedFile == null || MarkdownPreview.CoreWebView2 == null)
		{
			if (MarkdownPreview.CoreWebView2 != null)
			{
				ShowEmptyPreview();
			}
			return;
		}
		DocFile selectedFile = ViewModel.SelectedFile;
		switch (selectedFile.FileType?.ToLower() ?? "markdown")
		{
		case "markdown":
			RenderMarkdownPreview(selectedFile);
			break;
		case "pdf":
			RenderPdfPreview(selectedFile);
			break;
		case "word":
		case "excel":
			ShowFileInfoCard(selectedFile);
			break;
		default:
			ShowUnsupportedPreview(selectedFile);
			break;
		}
	}

	/// <summary>
	/// 显示空预览
	/// </summary>
	private void ShowEmptyPreview()
	{
		ViewModel.OutlineItems.Clear();
		MarkdownPreview.CoreWebView2.NavigateToString("\r\n<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <style>\r\n        body {\r\n            display: flex;\r\n            justify-content: center;\r\n            align-items: center;\r\n            height: 100vh;\r\n            font-family: 'Segoe UI', sans-serif;\r\n            color: #999;\r\n            font-size: 16px;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <p>\ud83d\udcc4 请选择文档进行预览</p>\r\n</body>\r\n</html>");
	}

	/// <summary>
	/// 渲染 Markdown 预览
	/// </summary>
	private void RenderMarkdownPreview(DocFile file)
	{
		string content = file.Content;
		ExtractOutline(content);
		content = ProcessImagePaths(content, file);
		content = AddHeaderAnchors(content);
		string htmlContent = Markdown.ToHtml(content, _markdownPipeline);
		string htmlContent2 = GenerateMarkdownHtml(htmlContent);
		MarkdownPreview.CoreWebView2.NavigateToString(htmlContent2);
	}

	/// <summary>
	/// 渲染 PDF 预览
	/// </summary>
	private void RenderPdfPreview(DocFile file)
	{
		ViewModel.OutlineItems.Clear();
		string value = ViewModel.storage.Config.BaseUrl.TrimEnd('/');
		string value2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.FilePath));
		string text = $"{value}/FileService/DownloadFile?RelativePath={value2}&ApiKey={ViewModel.storage.Config.Key}";
		string htmlContent = "\r\n<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <style>\r\n        * {\r\n            margin: 0;\r\n            padding: 0;\r\n        }\r\n        html, body {\r\n            width: 100%;\r\n            height: 100%;\r\n            overflow: hidden;\r\n        }\r\n        embed {\r\n            width: 100%;\r\n            height: 100%;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <embed src=\"" + text + "\" type=\"application/pdf\" />\r\n</body>\r\n</html>";
		MarkdownPreview.CoreWebView2.NavigateToString(htmlContent);
	}

	/// <summary>
	/// 显示文件信息卡片（Word/Excel）
	/// </summary>
	private void ShowFileInfoCard(DocFile file)
	{
		ViewModel.OutlineItems.Clear();
		string value = ((file.FileType == "word") ? "\ud83d\udcc4" : "\ud83d\udcc8");
		string value2 = ((file.FileType == "word") ? "Word 文档" : "Excel 文件");
		string extension = Path.GetExtension(file.FilePath);
		string htmlContent = $"\r\n<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <style>\r\n        body {{\r\n            display: flex;\r\n            justify-content: center;\r\n            align-items: center;\r\n            height: 100vh;\r\n            font-family: 'Segoe UI', sans-serif;\r\n            background: #f5f5f5;\r\n        }}\r\n        .file-card {{\r\n            background: white;\r\n            border-radius: 12px;\r\n            padding: 40px;\r\n            box-shadow: 0 4px 12px rgba(0,0,0,0.1);\r\n            text-align: center;\r\n            max-width: 400px;\r\n        }}\r\n        .file-icon {{\r\n            font-size: 64px;\r\n            margin-bottom: 20px;\r\n        }}\r\n        .file-title {{\r\n            font-size: 20px;\r\n            font-weight: 600;\r\n            color: #333;\r\n            margin-bottom: 20px;\r\n        }}\r\n        .file-info {{\r\n            color: #666;\r\n            margin: 8px 0;\r\n            font-size: 14px;\r\n        }}\r\n        .download-btn {{\r\n            margin-top: 30px;\r\n            padding: 12px 30px;\r\n            background: #0078d4;\r\n            color: white;\r\n            border: none;\r\n            border-radius: 6px;\r\n            font-size: 15px;\r\n            cursor: pointer;\r\n            transition: background 0.3s;\r\n        }}\r\n        .download-btn:hover {{\r\n            background: #006cbd;\r\n        }}\r\n        .hint {{\r\n            margin-top: 20px;\r\n            color: #999;\r\n            font-size: 13px;\r\n        }}\r\n    </style>\r\n</head>\r\n<body>\r\n    <div class=\"file-card\">\r\n        <div class=\"file-icon\">{value}</div>\r\n        <div class=\"file-title\">{file.Title}</div>\r\n        <div class=\"file-info\">\ud83d\udcc1 类型：{value2} ({extension})</div>\r\n        <div class=\"file-info\">\ud83d\udcc5 更新时间：{file.UpdatedAt:yyyy-MM-dd HH:mm}</div>\r\n        <div class=\"hint\">\ud83d\udca1 此文件不支持在线预览，请下载后查看</div>\r\n    </div>\r\n</body>\r\n</html>";
		MarkdownPreview.CoreWebView2.NavigateToString(htmlContent);
	}

	/// <summary>
	/// 显示不支持的文件类型
	/// </summary>
	private void ShowUnsupportedPreview(DocFile file)
	{
		ViewModel.OutlineItems.Clear();
		string htmlContent = "\r\n<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <style>\r\n        body {\r\n            display: flex;\r\n            justify-content: center;\r\n            align-items: center;\r\n            height: 100vh;\r\n            font-family: 'Segoe UI', sans-serif;\r\n            color: #999;\r\n        }\r\n        .message {\r\n            text-align: center;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <div class=\"message\">\r\n        <p style=\"font-size: 48px;\">⚠\ufe0f</p>\r\n        <p style=\"font-size: 18px;\">不支持预览此文件类型</p>\r\n        <p style=\"font-size: 14px; color: #ccc;\">" + file.FileType + "</p>\r\n    </div>\r\n</body>\r\n</html>";
		MarkdownPreview.CoreWebView2.NavigateToString(htmlContent);
	}

	/// <summary>
	/// 生成 Markdown HTML
	/// </summary>
	private string GenerateMarkdownHtml(string htmlContent)
	{
		return "\r\n<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n    <style>\r\n        body {\r\n            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;\r\n            padding: 20px;\r\n            line-height: 1.6;\r\n            color: #333;\r\n            max-width: 100%;\r\n            scroll-behavior: smooth;\r\n        }\r\n        h1, h2, h3, h4, h5, h6 {\r\n            margin-top: 24px;\r\n            margin-bottom: 16px;\r\n            font-weight: 600;\r\n            line-height: 1.25;\r\n            scroll-margin-top: 20px;\r\n        }\r\n        h1 { font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }\r\n        h2 { font-size: 1.5em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }\r\n        code {\r\n            background-color: #f6f8fa;\r\n            padding: 2px 6px;\r\n            border-radius: 3px;\r\n            font-family: 'Consolas', 'Monaco', monospace;\r\n        }\r\n        pre {\r\n            background-color: #f6f8fa;\r\n            padding: 16px;\r\n            border-radius: 6px;\r\n            overflow: auto;\r\n        }\r\n        pre code {\r\n            background-color: transparent;\r\n            padding: 0;\r\n        }\r\n        blockquote {\r\n            border-left: 4px solid #dfe2e5;\r\n            padding-left: 16px;\r\n            color: #6a737d;\r\n        }\r\n        table {\r\n            border-collapse: collapse;\r\n            width: 100%;\r\n        }\r\n        table th, table td {\r\n            border: 1px solid #dfe2e5;\r\n            padding: 6px 13px;\r\n        }\r\n        table th {\r\n            background-color: #f6f8fa;\r\n            font-weight: 600;\r\n        }\r\n        img {\r\n            max-width: 100%;\r\n            height: auto;\r\n            display: block;\r\n            margin: 10px 0;\r\n            border: 1px dashed #ddd;\r\n        }\r\n        .img-error {\r\n            color: red;\r\n            padding: 10px;\r\n            background-color: #ffe0e0;\r\n            border: 1px solid #ff0000;\r\n            margin: 10px 0;\r\n        }\r\n    </style>\r\n    <script>\r\n        function scrollToAnchor(anchorId) {\r\n            const element = document.getElementById(anchorId);\r\n            if (element) {\r\n                element.scrollIntoView({\r\n                    behavior: 'smooth',\r\n                    block: 'start'\r\n                });\r\n            }\r\n        }\r\n        \r\n        document.addEventListener('DOMContentLoaded', function() {\r\n            const images = document.querySelectorAll('img');\r\n            images.forEach(img => {\r\n                img.addEventListener('error', function() {\r\n                    const errorDiv = document.createElement('div');\r\n                    errorDiv.className = 'img-error';\r\n                    errorDiv.textContent = '图片加载失败: ' + this.alt;\r\n                    this.parentNode.replaceChild(errorDiv, this);\r\n                });\r\n            });\r\n        });\r\n    </script>\r\n</head>\r\n<body>\r\n    " + htmlContent + "\r\n</body>\r\n</html>";
	}

	/// <summary>
	/// 处理 Markdown 中的图片路径
	/// </summary>
	private string ProcessImagePaths(string markdown, DocFile file)
	{
		string pattern = "!\\[([^\\]]*)\\]\\(([^\\)]+)\\)";
		return Regex.Replace(markdown, pattern, delegate(Match match)
		{
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			if (value2.StartsWith("http://") || value2.StartsWith("https://"))
			{
				return match.Value;
			}
			if (value2.StartsWith("data:image"))
			{
				return match.Value;
			}
			if (!value2.StartsWith("/"))
			{
				try
				{
					string text = file.FilePath;
					if (text.EndsWith(".md"))
					{
						text = text.Substring(0, text.Length - 3);
					}
					string text2 = value2.TrimStart('.', '/', '\\');
					string s = (text + "/" + text2).Replace("\\", "/");
					string value3 = ViewModel.storage.Config.BaseUrl.TrimEnd('/');
					string value4 = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
					string value5 = $"{value3}/FileService/DownloadFile?RelativePath={value4}&ApiKey={ViewModel.storage.Config.Key}";
					return $"![{value}]({value5})";
				}
				catch (Exception)
				{
					return match.Value;
				}
			}
			return match.Value;
		});
	}

	/// <summary>
	/// 顶部按钮：新建文件夹
	/// </summary>
	private async void AddFolder_Click(object sender, RoutedEventArgs e)
	{
		DocumentTreeNode selectedTreeNode = ViewModel.SelectedTreeNode;
		if (selectedTreeNode == null)
		{
			await ViewModel.AddFolder();
			return;
		}
		if (selectedTreeNode.NodeType == DocumentNodeType.Folder)
		{
			await ViewModel.AddSubFolderAsync(selectedTreeNode.Category);
			return;
		}
		DocFile? file = selectedTreeNode.File;
		int num;
		if (file == null)
		{
			num = 0;
		}
		else
		{
			_ = file.CategoryId;
			num = 1;
		}
		if (num != 0)
		{
			DocCategory docCategory = await FindCategoryByIdAsync(selectedTreeNode.File.CategoryId);
			if (docCategory != null)
			{
				await ViewModel.AddSubFolderAsync(docCategory);
			}
		}
	}

	/// <summary>
	/// 顶部按钮：上传文档
	/// </summary>
	private async void UploadDoc_Click(object sender, RoutedEventArgs e)
	{
		DocumentTreeNode selectedTreeNode = ViewModel.SelectedTreeNode;
		if (selectedTreeNode == null)
		{
			await ViewModel.message.WarnAsync("请先选择一个文件夹！");
			return;
		}
		DocCategory docCategory = null;
		if (selectedTreeNode.NodeType == DocumentNodeType.Folder)
		{
			docCategory = selectedTreeNode.Category;
		}
		else if (selectedTreeNode.NodeType == DocumentNodeType.Document)
		{
			DocFile? file = selectedTreeNode.File;
			int num;
			if (file == null)
			{
				num = 0;
			}
			else
			{
				_ = file.CategoryId;
				num = 1;
			}
			if (num != 0)
			{
				docCategory = await FindCategoryByIdAsync(selectedTreeNode.File.CategoryId);
			}
		}
		if (docCategory != null)
		{
			await ViewModel.ImportFileAsync(docCategory);
		}
	}

	/// <summary>
	/// 顶部按钮：编辑
	/// </summary>
	private async void Edit_Click(object sender, RoutedEventArgs e)
	{
		DocumentTreeNode selectedTreeNode = ViewModel.SelectedTreeNode;
		if (selectedTreeNode == null)
		{
			await ViewModel.message.WarnAsync("请先选择要编辑的项！");
			return;
		}
		bool wasEnabled = MarkdownPreview.IsEnabled;
		MarkdownPreview.IsEnabled = false;
		try
		{
			if (selectedTreeNode.NodeType == DocumentNodeType.Folder)
			{
				await ViewModel.EditFolderAsync(selectedTreeNode.Category);
			}
			else if (selectedTreeNode.File != null)
			{
				await EditFileAsync(selectedTreeNode.File);
			}
		}
		finally
		{
			MarkdownPreview.IsEnabled = wasEnabled;
		}
	}

	/// <summary>
	/// 编辑文件
	/// </summary>
	private async Task EditFileAsync(DocFile file)
	{
		MarkdownEditorWindow markdownEditorWindow = new MarkdownEditorWindow(file, ViewModel.storage, ViewModel.context);
		markdownEditorWindow.Owner = Window.GetWindow((DependencyObject)(object)this);
		if (markdownEditorWindow.ShowDialog() == true)
		{
			ViewModel.SelectedFile = null;
			await ViewModel.LoadTreeAsync();
			ViewModel.SelectedFile = file;
			RenderPreview();
		}
	}

	/// <summary>
	/// 顶部按钮：删除
	/// </summary>
	private async void Delete_Click(object sender, RoutedEventArgs e)
	{
		DocumentTreeNode selectedTreeNode = ViewModel.SelectedTreeNode;
		if (selectedTreeNode == null)
		{
			await ViewModel.message.WarnAsync("请先选择要删除的项！");
			return;
		}
		bool wasEnabled = MarkdownPreview.IsEnabled;
		MarkdownPreview.IsEnabled = false;
		try
		{
			if (selectedTreeNode.NodeType == DocumentNodeType.Folder)
			{
				await ViewModel.DeleteFolderAsync(selectedTreeNode.Category);
			}
			else if (selectedTreeNode.File != null)
			{
				await DeleteFileAsync(selectedTreeNode.File);
			}
		}
		finally
		{
			MarkdownPreview.IsEnabled = wasEnabled;
		}
	}

	/// <summary>
	/// 删除文件
	/// </summary>
	private async Task DeleteFileAsync(DocFile file)
	{
		bool wasEnabled = MarkdownPreview.IsEnabled;
		MarkdownPreview.IsEnabled = false;
		try
		{
			if (!(await ViewModel.message.ConfirmAsync("确认删除文件 \"" + file.Title + "\"?\n\n此操作将同时删除服务器上的文件和图片。")))
			{
				return;
			}
			ViewModel.model.Status.Busy("删除中...");
			if (!string.IsNullOrEmpty(file.FilePath))
			{
				await ViewModel.storage.WebDeleteFileAsync(file.FilePath);
				string text = file.FilePath;
				if (text.EndsWith(".md"))
				{
					text = text.Substring(0, text.Length - 3);
				}
				try
				{
					await ViewModel.storage.WebDeleteFolderAsync(text);
				}
				catch (Exception)
				{
				}
			}
			await (from x in ViewModel.context.Db.Deleteable<DocFile>()
				where x.Id == file.Id
				select x).ExecuteCommandAsync();
			await ViewModel.LoadTreeAsync();
			ViewModel.model.Status.Success("文件已删除：" + file.Title);
		}
		catch (Exception ex2)
		{
			ViewModel.model.Status.Reset();
			await ViewModel.message.ErrorAsync("删除失败: " + ex2.Message);
		}
		finally
		{
			MarkdownPreview.IsEnabled = wasEnabled;
		}
	}

	/// <summary>
	/// 根据ID查找文件夹
	/// </summary>
	private async Task<DocCategory?> FindCategoryByIdAsync(long categoryId)
	{
		return await (from x in ViewModel.context.Db.Queryable<DocCategory>()
			where x.Id == categoryId
			select x).FirstAsync();
	}

	/// <summary>
	/// 提取 Markdown 文档大纲
	/// </summary>
	private void ExtractOutline(string markdown)
	{
		ViewModel.OutlineItems.Clear();
		string pattern = "^(#{1,6})\\s+(.+)$";
		MatchCollection matchCollection = Regex.Matches(markdown, pattern, RegexOptions.Multiline);
		int num = 0;
		foreach (Match item2 in matchCollection)
		{
			int length = item2.Groups[1].Value.Length;
			string text = item2.Groups[2].Value.Trim();
			string id = $"heading-{num}";
			OutlineItem item = new OutlineItem
			{
				Id = id,
				Text = text,
				Level = length
			};
			ViewModel.OutlineItems.Add(item);
			num++;
		}
	}

	/// <summary>
	/// 为 Markdown 标题添加锤点 ID
	/// </summary>
	private string AddHeaderAnchors(string markdown)
	{
		string pattern = "^(#{1,6})\\s+(.+)$";
		int index = 0;
		return Regex.Replace(markdown, pattern, delegate(Match match)
		{
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value.Trim();
			string value3 = $"heading-{index}";
			index++;
			return $"{value} <span id=\"{value3}\"></span>{value2}";
		}, RegexOptions.Multiline);
	}

	/// <summary>
	/// 大纲项点击事件
	/// </summary>
	private async void OutlineItem_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is Wpf.Ui.Controls.Button { Tag: string tag }))
		{
			return;
		}
		try
		{
			if (MarkdownPreview.CoreWebView2 != null)
			{
				string javaScript = "scrollToAnchor('" + tag + "');";
				await MarkdownPreview.CoreWebView2.ExecuteScriptAsync(javaScript);
			}
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// 同步本地文档
	/// </summary>
	private async void Sync_Click(object sender, RoutedEventArgs e)
	{
		await ViewModel.SyncLocalDocsAsync();
	}

	/// <summary>
	/// 移动文件
	/// </summary>
	private async void MoveFile_Click(object sender, RoutedEventArgs e)
	{
		DocumentTreeNode selectedTreeNode = ViewModel.SelectedTreeNode;
		if (selectedTreeNode?.File == null)
		{
			await ViewModel.message.WarnAsync("请先选择要移动的文件！");
		}
		else
		{
			await ViewModel.MoveFileAsync(selectedTreeNode.File);
		}
	}

	/// <summary>
	/// 下载文件
	/// </summary>
	private async void Download_Click(object sender, RoutedEventArgs e)
	{
		DocumentTreeNode selectedTreeNode = ViewModel.SelectedTreeNode;
		if (selectedTreeNode?.File == null)
		{
			await ViewModel.message.WarnAsync("请先选择要下载的文档！");
			return;
		}
		DocFile file = selectedTreeNode.File;
		if (string.IsNullOrEmpty(file.FilePath))
		{
			await ViewModel.message.WarnAsync("文件路径为空，无法下载！");
			return;
		}
		string fileName = Path.GetFileName(file.FilePath);
		string extension = Path.GetExtension(file.FilePath);
		string filter = (file.FileType?.ToLower() ?? "markdown") switch
		{
			"markdown" => "Markdown 文件 (*.md;*.markdown)|*.md;*.markdown|所有文件 (*.*)|*.*", 
			"word" => "Word 文档 (*.doc;*.docx)|*.doc;*.docx|所有文件 (*.*)|*.*", 
			"excel" => "Excel 文件 (*.xls;*.xlsx)|*.xls;*.xlsx|所有文件 (*.*)|*.*", 
			"pdf" => "PDF 文件 (*.pdf)|*.pdf|所有文件 (*.*)|*.*", 
			_ => "所有文件 (*.*)|*.*", 
		};
		SaveFileDialog saveDialog = new SaveFileDialog
		{
			FileName = fileName,
			Filter = filter,
			DefaultExt = extension
		};
		if (saveDialog.ShowDialog() == true)
		{
			try
			{
				ViewModel.model.Status.Busy("正在从服务器下载文件...");
				byte[] bytes = await ViewModel.storage.WebDownloadFileAsync(file.FilePath);
				await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
				ViewModel.model.Status.Success("文件已下载：" + Path.GetFileName(saveDialog.FileName));
			}
			catch (Exception ex)
			{
				ViewModel.model.Status.Reset();
				await ViewModel.message.ErrorAsync("下载失败：" + ex.Message);
			}
		}
	}

	/// <summary>
	/// 替换文档
	/// </summary>
	private async void Replace_Click(object sender, RoutedEventArgs e)
	{
		DocumentTreeNode selected = ViewModel.SelectedTreeNode;
		if (selected?.File == null)
		{
			await ViewModel.message.WarnAsync("请先选择要替换的文档！");
			return;
		}
		DocFile file = selected.File;
		string fileType = file.FileType?.ToLower() ?? "markdown";
		string filter = fileType switch
		{
			"markdown" => "Markdown 文件 (*.md;*.markdown)|*.md;*.markdown|所有文件 (*.*)|*.*", 
			"word" => "Word 文档 (*.doc;*.docx)|*.doc;*.docx|所有文件 (*.*)|*.*", 
			"excel" => "Excel 文件 (*.xls;*.xlsx)|*.xls;*.xlsx|所有文件 (*.*)|*.*", 
			"pdf" => "PDF 文件 (*.pdf)|*.pdf|所有文件 (*.*)|*.*", 
			_ => "所有文件 (*.*)|*.*", 
		};
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = filter,
			Multiselect = false
		};
		if (openFileDialog.ShowDialog() != true)
		{
			return;
		}
		try
		{
			ViewModel.model.Status.Busy("正在上传文件到服务器...");
			string newFileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
			byte[] fileBytes = await File.ReadAllBytesAsync(openFileDialog.FileName);
			await ViewModel.storage.WebUploadFileAsync(file.FilePath, fileBytes);
			file.Title = newFileName;
			file.UpdatedAt = DateTime.Now;
			if (!(fileType == "markdown"))
			{
				await ViewModel.context.Db.Updateable(file).UpdateColumns((DocFile x) => new { x.Title, x.UpdatedAt }).ExecuteCommandAsync();
			}
			else
			{
				string content = Encoding.UTF8.GetString(fileBytes);
				file.Content = content;
				await ViewModel.context.Db.Updateable(file).UpdateColumns((DocFile x) => new { x.Title, x.Content, x.UpdatedAt }).ExecuteCommandAsync();
				RenderPreview();
			}
			if (selected != null)
			{
				selected.Name = newFileName;
			}
			ViewModel.model.Status.Success("文件已替换：" + newFileName);
		}
		catch (Exception ex)
		{
			ViewModel.model.Status.Reset();
			await ViewModel.message.ErrorAsync("替换失败：" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	/// <summary>
	/// 导出为 PDF
	/// </summary>
	private async void ExportPdf_Click(object sender, RoutedEventArgs e)
	{
		DocumentTreeNode selectedTreeNode = ViewModel.SelectedTreeNode;
		if (selectedTreeNode?.File == null)
		{
			await ViewModel.message.WarnAsync("请先选择要导出的文档！");
			return;
		}
		DocFile file = selectedTreeNode.File;
		if (MarkdownPreview.CoreWebView2 == null)
		{
			await ViewModel.message.WarnAsync("预览未加载，无法导出 PDF");
			return;
		}
		SaveFileDialog saveDialog = new SaveFileDialog
		{
			FileName = file.Title + ".pdf",
			Filter = "PDF 文件 (*.pdf)|*.pdf|所有文件 (*.*)|*.*",
			DefaultExt = ".pdf"
		};
		if (saveDialog.ShowDialog() == true)
		{
			try
			{
				ViewModel.model.Status.Busy("正在导出 PDF...");
				await MarkdownPreview.CoreWebView2.PrintToPdfAsync(saveDialog.FileName);
				ViewModel.model.Status.Success("PDF 已导出：" + Path.GetFileName(saveDialog.FileName));
			}
			catch (Exception ex)
			{
				ViewModel.model.Status.Reset();
				await ViewModel.message.ErrorAsync("导出 PDF 失败：" + ex.Message);
			}
		}
	}

	/// <summary>
	/// InitializeComponent
	/// </summary>
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/pages/documentmanagementpage.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			BtnAddFolder = (Wpf.Ui.Controls.Button)target;
			BtnAddFolder.Click += AddFolder_Click;
			break;
		case 2:
			BtnUploadDoc = (Wpf.Ui.Controls.Button)target;
			BtnUploadDoc.Click += UploadDoc_Click;
			break;
		case 3:
			BtnEdit = (Wpf.Ui.Controls.Button)target;
			BtnEdit.Click += Edit_Click;
			break;
		case 4:
			BtnDelete = (Wpf.Ui.Controls.Button)target;
			BtnDelete.Click += Delete_Click;
			break;
		case 5:
			BtnSync = (Wpf.Ui.Controls.Button)target;
			BtnSync.Click += Sync_Click;
			break;
		case 6:
			BtnMoveFile = (Wpf.Ui.Controls.Button)target;
			BtnMoveFile.Click += MoveFile_Click;
			break;
		case 7:
			BtnDownload = (Wpf.Ui.Controls.Button)target;
			BtnDownload.Click += Download_Click;
			break;
		case 8:
			BtnReplace = (Wpf.Ui.Controls.Button)target;
			BtnReplace.Click += Replace_Click;
			break;
		case 9:
			BtnExportPdf = (Wpf.Ui.Controls.Button)target;
			BtnExportPdf.Click += ExportPdf_Click;
			break;
		case 10:
			((Border)target).MouseDown += TreeBorder_MouseDown;
			break;
		case 11:
			FileTreeView = (TreeView)target;
			break;
		case 12:
			MarkdownPreview = (WebView2)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 13)
		{
			((Wpf.Ui.Controls.Button)target).Click += OutlineItem_Click;
		}
	}
}

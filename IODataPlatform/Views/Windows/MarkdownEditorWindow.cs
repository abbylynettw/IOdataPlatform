using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using Markdig;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// MarkdownEditorWindow
/// </summary>
public partial class MarkdownEditorWindow : FluentWindow
{
	private readonly DocFile _file;

	private readonly StorageService _storage;

	private readonly SqlSugarContext _context;

	private readonly MarkdownPipeline _markdownPipeline;

	private string _currentContent;

	private bool _isInitialized;

	public MarkdownEditorWindow(DocFile file, StorageService storage, SqlSugarContext context)
	{
		_file = file;
		_storage = storage;
		_context = context;
		_currentContent = file.Content ?? string.Empty;
		InitializeComponent();
		TitleText.Text = "编辑: " + file.Title;
		_markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
		base.Loaded += MarkdownEditorWindow_Loaded;
	}

	private async void MarkdownEditorWindow_Loaded(object sender, RoutedEventArgs e)
	{
		await InitializeEditorAsync();
	}

	private async Task InitializeEditorAsync()
	{
		_ = 1;
		try
		{
			await EditorWebView.EnsureCoreWebView2Async();
			EditorWebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
			await PreviewWebView.EnsureCoreWebView2Async();
			EditorWebView.CoreWebView2.WebMessageReceived += EditorWebView_WebMessageReceived;
			string htmlContent = GenerateMonacoEditorHtml(_currentContent);
			EditorWebView.CoreWebView2.NavigateToString(htmlContent);
			UpdatePreview(_currentContent);
			_isInitialized = true;
		}
		catch (Exception ex)
		{
			System.Windows.MessageBox.Show("编辑器初始化失败: " + ex.Message, "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private string GenerateMonacoEditorHtml(string content)
	{
		string text = JsonSerializer.Serialize(content);
		return "\r\n<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />\r\n    <style>\r\n        body, html {\r\n            margin: 0;\r\n            padding: 0;\r\n            height: 100%;\r\n            overflow: hidden;\r\n        }\r\n        #container {\r\n            width: 100%;\r\n            height: 100%;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <div id=\"container\"></div>\r\n    \r\n    <!-- Monaco Editor CDN -->\r\n    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.44.0/min/vs/editor/editor.main.min.css\" />\r\n    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.44.0/min/vs/loader.min.js\"></script>\r\n    \r\n    <script>\r\n        require.config({ \r\n            paths: { \r\n                'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.44.0/min/vs' \r\n            } \r\n        });\r\n        \r\n        require(['vs/editor/editor.main'], function() {\r\n            console.log('Monaco Editor 开始初始化');\r\n            \r\n            var editor = monaco.editor.create(document.getElementById('container'), {\r\n                value: " + text + ",\r\n                language: 'markdown',\r\n                theme: 'vs-dark',\r\n                automaticLayout: true,\r\n                wordWrap: 'on',\r\n                minimap: {\r\n                    enabled: true\r\n                },\r\n                fontSize: 14,\r\n                lineNumbers: 'on',\r\n                scrollBeyondLastLine: false,\r\n                renderWhitespace: 'selection'\r\n            });\r\n            \r\n            console.log('Monaco Editor 创建完成');\r\n            \r\n            // 监听内容变化\r\n            editor.onDidChangeModelContent(function() {\r\n                var content = editor.getValue();\r\n                console.log('内容变化，长度:', content.length);\r\n                \r\n                try {\r\n                    window.chrome.webview.postMessage({\r\n                        type: 'contentChanged',\r\n                        content: content\r\n                    });\r\n                    console.log('消息已发送');\r\n                } catch(e) {\r\n                    console.error('发送消息失败:', e);\r\n                }\r\n            });\r\n            \r\n            console.log('事件监听器已注册');\r\n            \r\n            // 接收外部插入图片的命令\r\n            window.chrome.webview.addEventListener('message', function(event) {\r\n                console.log('收到外部消息:', event.data);\r\n                if (event.data.type === 'insertImage') {\r\n                    var imageMarkdown = '![' + event.data.alt + '](' + event.data.url + ')';\r\n                    var selection = editor.getSelection();\r\n                    editor.executeEdits('insert-image', [{\r\n                        range: selection,\r\n                        text: imageMarkdown\r\n                    }]);\r\n                    editor.focus();\r\n                }\r\n            });\r\n        });\r\n    </script>\r\n</body>\r\n</html>";
	}

	private void EditorWebView_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
	{
		try
		{
			string webMessageAsJson = e.WebMessageAsJson;
			JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(webMessageAsJson);
			if (jsonElement.GetProperty("type").GetString() == "contentChanged")
			{
				_currentContent = jsonElement.GetProperty("content").GetString() ?? string.Empty;
				UpdatePreview(_currentContent);
			}
		}
		catch (Exception)
		{
		}
	}

	private void UpdatePreview(string markdown)
	{
		if (PreviewWebView?.CoreWebView2 == null)
		{
			return;
		}
		try
		{
			markdown = ProcessImagePaths(markdown);
			string text = Markdown.ToHtml(markdown, _markdownPipeline);
			string htmlContent = "\r\n<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <style>\r\n        * {\r\n            box-sizing: border-box;\r\n        }\r\n        \r\n        html {\r\n            height: 100%;\r\n            overflow-y: scroll;\r\n            scrollbar-gutter: stable;\r\n        }\r\n        \r\n        body {\r\n            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;\r\n            padding: 20px;\r\n            line-height: 1.6;\r\n            color: #333;\r\n            background-color: #ffffff;\r\n            margin: 0;\r\n            height: auto;\r\n            overflow-x: hidden;\r\n        }\r\n        \r\n        /* 自定义滚动条样式 - 始终显示 */\r\n        ::-webkit-scrollbar {\r\n            width: 12px;\r\n            height: 12px;\r\n        }\r\n        \r\n        ::-webkit-scrollbar-track {\r\n            background: #f1f1f1;\r\n            border-radius: 6px;\r\n        }\r\n        \r\n        ::-webkit-scrollbar-thumb {\r\n            background: #888;\r\n            border-radius: 6px;\r\n        }\r\n        \r\n        ::-webkit-scrollbar-thumb:hover {\r\n            background: #555;\r\n        }\r\n        \r\n        h1, h2, h3, h4, h5, h6 {\r\n            margin-top: 24px;\r\n            margin-bottom: 16px;\r\n            font-weight: 600;\r\n            line-height: 1.25;\r\n        }\r\n        h1 { font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }\r\n        h2 { font-size: 1.5em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }\r\n        h3 { font-size: 1.25em; }\r\n        h4 { font-size: 1em; }\r\n        h5 { font-size: 0.875em; }\r\n        h6 { font-size: 0.85em; }\r\n        \r\n        p {\r\n            margin: 16px 0;\r\n        }\r\n        \r\n        code {\r\n            background-color: #f6f8fa;\r\n            padding: 2px 6px;\r\n            border-radius: 3px;\r\n            font-family: 'Consolas', 'Monaco', monospace;\r\n        }\r\n        pre {\r\n            background-color: #f6f8fa;\r\n            padding: 16px;\r\n            border-radius: 6px;\r\n            overflow: auto;\r\n            margin: 16px 0;\r\n        }\r\n        pre code {\r\n            background-color: transparent;\r\n            padding: 0;\r\n        }\r\n        blockquote {\r\n            border-left: 4px solid #dfe2e5;\r\n            padding-left: 16px;\r\n            color: #6a737d;\r\n            margin: 16px 0;\r\n        }\r\n        table {\r\n            border-collapse: collapse;\r\n            width: 100%;\r\n            margin: 16px 0;\r\n        }\r\n        table th, table td {\r\n            border: 1px solid #dfe2e5;\r\n            padding: 6px 13px;\r\n        }\r\n        table th {\r\n            background-color: #f6f8fa;\r\n            font-weight: 600;\r\n        }\r\n        img {\r\n            max-width: 100%;\r\n            height: auto;\r\n            display: block;\r\n            margin: 10px 0;\r\n        }\r\n        \r\n        ul, ol {\r\n            margin: 16px 0;\r\n            padding-left: 32px;\r\n        }\r\n        \r\n        li {\r\n            margin: 4px 0;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    " + text + "\r\n</body>\r\n</html>";
			PreviewWebView.CoreWebView2.NavigateToString(htmlContent);
		}
		catch (Exception)
		{
		}
	}

	private string ProcessImagePaths(string markdown)
	{
		string pattern = "!\\[([^\\]]*)\\]\\(([^\\)]+)\\)";
		return Regex.Replace(markdown, pattern, delegate(Match match)
		{
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			if (value2.StartsWith("http://") || value2.StartsWith("https://") || value2.StartsWith("data:image"))
			{
				return match.Value;
			}
			if (!value2.StartsWith("/"))
			{
				try
				{
					string text = _file.FilePath;
					if (text.EndsWith(".md"))
					{
						text = text.Substring(0, text.Length - 3);
					}
					string text2 = value2.TrimStart('.', '/', '\\');
					string s = (text + "/" + text2).Replace("\\", "/");
					string value3 = _storage.Config.BaseUrl.TrimEnd('/');
					string value4 = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
					string value5 = $"{value3}/FileService/DownloadFile?RelativePath={value4}&ApiKey={_storage.Config.Key}";
					return $"![{value}]({value5})";
				}
				catch
				{
					return match.Value;
				}
			}
			return match.Value;
		});
	}

	private async void InsertImage_Click(object sender, RoutedEventArgs e)
	{
		_ = 2;
		try
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "图片文件|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.svg|所有文件|*.*",
				Title = "选择图片"
			};
			if (openFileDialog.ShowDialog() == true)
			{
				BtnInsertImage.IsEnabled = false;
				string fileName = Path.GetFileName(openFileDialog.FileName);
				byte[] body = await File.ReadAllBytesAsync(openFileDialog.FileName);
				string text = (_file.FilePath.EndsWith(".md") ? _file.FilePath.Substring(0, _file.FilePath.Length - 3) : _file.FilePath);
				string relativePath = text + "/images/" + fileName;
				await _storage.WebUploadFileAsync(relativePath, body);
				string url = "./images/" + fileName;
				var value = new
				{
					type = "insertImage",
					url = url,
					alt = Path.GetFileNameWithoutExtension(fileName)
				};
				string text2 = JsonSerializer.Serialize(value);
				await EditorWebView.CoreWebView2.ExecuteScriptAsync("window.postMessage(" + text2 + ", '*')");
				System.Windows.MessageBox.Show("图片上传成功！", "成功", System.Windows.MessageBoxButton.OK, MessageBoxImage.Asterisk);
			}
		}
		catch (Exception ex)
		{
			System.Windows.MessageBox.Show("插入图片失败: " + ex.Message, "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Hand);
		}
		finally
		{
			BtnInsertImage.IsEnabled = true;
		}
	}

	private async void Save_Click(object sender, RoutedEventArgs e)
	{
		_ = 1;
		try
		{
			BtnSave.IsEnabled = false;
			_file.Content = _currentContent;
			_file.UpdatedAt = DateTime.Now;
			byte[] bytes = Encoding.UTF8.GetBytes(_currentContent);
			await _storage.WebUploadFileAsync(_file.FilePath, bytes);
			await _context.Db.Updateable(_file).UpdateColumns((DocFile x) => new { x.Content, x.UpdatedAt }).ExecuteCommandAsync();
			base.DialogResult = true;
			Close();
		}
		catch (Exception ex)
		{
			System.Windows.MessageBox.Show("保存失败: " + ex.Message, "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Hand);
			BtnSave.IsEnabled = true;
		}
	}

	private void Cancel_Click(object sender, RoutedEventArgs e)
	{
		if (_currentContent != _file.Content)
		{
			System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("您有未保存的更改，确定要关闭吗？", "确认", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (messageBoxResult != System.Windows.MessageBoxResult.Yes)
			{
				return;
			}
		}
		base.DialogResult = false;
		Close();
	}
}

using System.Windows.Markup;

/// <summary>
/// 程序集程序集信息和XAML命名空间定义
/// 为WPF定义自定义的XML命名空间，使得在XAML中可以使用lysoft/wpf命名空间
/// 包含转换器和XAML服务类的命名空间映射，简化XAML中的引用
/// </summary>

/// <summary>
/// 映射Converters命名空间到lysoft/wpf
/// 允许在XAML中直接使用所有转换器类，如BoolToVisibilityConverter等
/// </summary>
[assembly: XmlnsDefinition("http://lysoft/wpf", "LYSoft.Libs.Wpf.Converters")]

/// <summary>
/// 映射XamlServices命名空间到lysoft/wpf
/// 允许在XAML中直接使用XAML服务类，如ItemsControlService等
/// </summary>
[assembly: XmlnsDefinition("http://lysoft/wpf", "LYSoft.Libs.Wpf.XamlServices")]
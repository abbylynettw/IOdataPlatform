﻿﻿#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

namespace LYSoft.Libs.Editor;

/// <summary>
/// 编辑器全部配置选项类
/// 包含编辑器的所有配置信息，包括页面属性、编辑选项、验证规则等
/// 由EditorOptionBuilder构造器生成，用于渲染具体的编辑器界面
/// 支持对编辑对象的验证和各种类型的编辑控件
/// </summary>
public class EditorOptions {
    internal EditorOptions() { }
    /// <summary>所有编辑选项的集合，包含各种类型的编辑控件配置</summary>
    public List<IEditorOption> Options { get; } = [];
    /// <summary>编辑器页面的标题文本</summary>
    public string Title { get; internal set; }
    /// <summary>编辑器页面的图标文件路径</summary>
    public string IconPath { get; internal set; }
    /// <summary>编辑器确认按钮的显示文本</summary>
    public string OkButtonText { get; internal set; }
    /// <summary>编辑器取消按钮的显示文本</summary>
    public string CancelButtonText { get; internal set; }
    /// <summary>编辑器窗口的宽度（像素）</summary>
    public double EditorWidth { get; internal set; }
    /// <summary>编辑器窗口的高度（像素）</summary>
    public double EditorHeight { get; internal set; }
    /// <summary>编辑器要编辑的目标对象实例</summary>
    public object Object { get; internal set; }
    internal Func<object, Task<string>> Validator { get; set; }

    /// <summary>
    /// 验证编辑对象的合法性
    /// 在用户提交编辑结果前调用，如果返回非空字符串则表示验证失败
    /// </summary>
    /// <returns>验证结果，空字符串表示验证通过，非空字符串为错误信息</returns>
    public async Task<string> ValidateAsync() {
        return await Validator(Object);
    }

}

/// <summary>
/// 编辑器选项接口
/// 所有编辑器选项类的基础接口，用于统一管理不同类型的编辑控件
/// </summary>
public interface IEditorOption { }

/// <summary>
/// 命令按钮编辑选项类
/// 在编辑器中显示一个可点击的命令按钮，用于执行特定的操作
/// 适用于触发业务逻辑、打开对话框或执行其他附加功能
/// </summary>
public class CommandEditorOption : IEditorOption {
    /// <summary>字段标题</summary>
    public string Header { get; internal set; }
    /// <summary>点击按钮时要执行的命令方法</summary>
    public Action Command { get; internal set; }
}

/// <summary>
/// 选择器编辑选项基类
/// 为文件选择器、文件夹选择器等提供通用的选择功能
/// 包含选择后的回调处理和初始值设置功能
/// </summary>
public class PickerEditorOption : IEditorOption {
    /// <summary>字段标题</summary>
    public string Header { get; internal set; }
    /// <summary>选择器选择后的回调方法，接收编辑对象和选择结果</summary>
    public Action<object, string> Setter { get; internal set; }
    /// <summary>选择器的初始值获取方法，从编辑对象中获取当前值</summary>
    public Func<object, string> Getter { get; internal set; }
}

/// <summary>
/// 文件选择器编辑选项类
/// 继承自选择器基类，提供文件选择功能
/// 支持打开文件和保存文件两种模式，并支持文件类型筛选
/// </summary>
public class PickFileEditorOption : PickerEditorOption {
    internal PickFileEditorOption() { }
    /// <summary>文件类型筛选器字符串，用于限定可选择的文件类型</summary>
    public string FileFilter { get; internal set; }
    /// <summary>是否为打开文件对话框，true为打开文件，false为保存文件</summary>
    public bool IsOpenFile { get; internal set; }
}

/// <summary>
/// 文件夹选择器编辑选项类
/// 继承自选择器基类，提供文件夹选择功能
/// 用于在编辑器中选择和设置目录路径
/// </summary>
public class PickFolderEditorOption : PickerEditorOption {
    internal PickFolderEditorOption() { }
}

/// <summary>
/// 图片文件选择器编辑选项类
/// 继承自选择器基类，专门用于选择图片文件
/// 支持图片文件类型筛选，适用于头像、图标等图片资源的选择
/// </summary>
public class PickImageEditorOption : PickerEditorOption {
    internal PickImageEditorOption() { }
    /// <summary>图片文件类型筛选器字符串</summary>
    public string FileFilter { get; internal set; }
}

/// <summary>
/// 属性编辑选项基类
/// 为所有属性编辑控件提供通用的基础属性，如属性名、标题、占位符等
/// 所有具体的属性编辑器都继承自此类
/// </summary>
public class PropertyEditorOption : IEditorOption {
    /// <summary>对应的属性名称，用于反射获取和设置属性值</summary>
    public string PropertyName { get; internal set; }
    /// <summary>属性在编辑器中显示的标题文本</summary>
    public string PropertyHeader { get; internal set; }
    /// <summary>输入框的占位提示文本</summary>
    public string PlaceHolder { get; internal set; }
    /// <summary>属性后缀文本，显示在输入框后面，仅用于显示目的</summary>
    public string Suffix { get; internal set; }
}

/// <summary>
/// 文本属性编辑选项类
/// 提供文本输入功能，支持单行和多行文本编辑
/// 包含文本长度限制和数据类型转换功能
/// </summary>
public class TextEditorOption : PropertyEditorOption {
    internal TextEditorOption() { }
    /// <summary>是否为多行文本编辑模式</summary>
    public bool HasMultiLine { get; internal set; }
    /// <summary>文本最小长度</summary>
    public int MinLength { get; internal set; }
    /// <summary>文本最大长度</summary>
    public int MaxLength { get; internal set; }
    /// <summary>从文本转换至属性</summary>
    public Func<string, object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至文本，用于赋初始值</summary>
    public Func<object, string> ConverterFromProperty { get; internal set; }
}

/// <summary>机密文本属性选项</summary>
public class SecretTextEditorOption : PropertyEditorOption {
    internal SecretTextEditorOption() { }
    /// <summary>文本最小长度</summary>
    public int MinLength { get; internal set; }
    /// <summary>文本最大长度</summary>
    public int MaxLength { get; internal set; }
    /// <summary>从文本转换至属性</summary>
    public Func<string, object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至文本，用于赋初始值</summary>
    public Func<object, string> ConverterFromProperty { get; internal set; }
}

/// <summary>整型属性选项</summary>
public class IntEditorOption : PropertyEditorOption {
    internal IntEditorOption() { }
    /// <summary>最小值</summary>
    public int MinValue { get; internal set; }
    /// <summary>最大值</summary>
    public int MaxValue { get; internal set; }
    /// <summary>从整型转换至属性</summary>
    public Func<int, object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至整型，用于赋初始值</summary>
    public Func<object, int> ConverterFromProperty { get; internal set; }
}

/// <summary>浮点型属性选项</summary>
public class DoubleEditorOption : PropertyEditorOption {
    internal DoubleEditorOption() { }
    /// <summary>最小值</summary>
    public double MinValue { get; internal set; }
    /// <summary>最大值</summary>
    public double MaxValue { get; internal set; }
    /// <summary>从浮点型转换至属性</summary>
    public Func<double, object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至浮点型，用于赋初始值</summary>
    public Func<object, double> ConverterFromProperty { get; internal set; }
}

/// <summary>日期型属性选项</summary>
public class DateTimeEditorOption : PropertyEditorOption {
    internal DateTimeEditorOption() { }
    /// <summary>最小值</summary>
    public DateTime MinValue { get; internal set; }
    /// <summary>最大值</summary>
    public DateTime MaxValue { get; internal set; }
    /// <summary>从日期型转换至属性</summary>
    public Func<DateTime, object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至日期型，用于赋初始值</summary>
    public Func<object, DateTime> ConverterFromProperty { get; internal set; }
}

/// <summary>布尔型属性选项</summary>
public class BooleanEditorOption : PropertyEditorOption {
    internal BooleanEditorOption() { }
    /// <summary>从布尔型转换至属性</summary>
    public Func<bool, object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至布尔型，用于赋初始值</summary>
    public Func<object, bool> ConverterFromProperty { get; internal set; }
}

/// <summary>选择型属性选项</summary>
public class SelectEditorOption : PropertyEditorOption {
    /// <summary>选项列表</summary>
    public List<PropertyOption<object>> Options { get; internal set; }
}

/// <summary>单选属性选项</summary>
public class SingleSelectEditorOption : SelectEditorOption {
    internal SingleSelectEditorOption() { }
    /// <summary>从选项类型转换至属性</summary>
    public Func<object, object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至选项类型，用于赋初始值</summary>
    public Func<object, object> ConverterFromProperty { get; internal set; }
}

/// <summary>多选属性选项</summary>
public class MultiSelectEditorOption : SelectEditorOption {
    internal MultiSelectEditorOption() { }
    /// <summary>从选项类型转换至属性</summary>
    public Func<object[], object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至选项类型，用于赋初始值</summary>
    public Func<object, object[]> ConverterFromProperty { get; internal set; }
}

/// <summary>使用组合框的单选属性选项</summary>
public class ComboSelectEditorOption : SelectEditorOption {
    internal ComboSelectEditorOption() { }
    /// <summary>从选项类型转换至属性</summary>
    public Func<object, object> ConverterToProperty { get; internal set; }
    /// <summary>从属性转换至选项类型，用于赋初始值</summary>
    public Func<object, object> ConverterFromProperty { get; internal set; }
}
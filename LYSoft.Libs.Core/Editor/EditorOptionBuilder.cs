#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8603 // 可能返回 null 引用。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
#pragma warning disable CS8605 // 取消装箱可能为 null 的值。
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。

namespace LYSoft.Libs.Editor;

/// <summary>编辑器选项构造器</summary>
public class EditorOptionBuilder<TObject> {

    internal TObject? Object { get; private set; }
    internal string Title { get; private set; } = string.Empty;
    internal string IconPath { get; private set; } = string.Empty;
    internal string OkButtonText { get; private set; } = "确认";
    internal string CancelButtonText { get; private set; } = "取消";
    internal double EditorWidth { get; private set; } = 450;
    internal double EditorHeight { get; private set; } = 800;
    internal Func<TObject, Task<string>> ValidatorTask { get; private set; } = _ => Task.FromResult("");
    internal List<IBuiltable> Options { get; } = [];

    internal EditorOptionBuilder() { }
    internal EditorOptionBuilder(TObject obj) { Object = obj; }

    /// <summary>设置编辑器的标题</summary>
    public EditorOptionBuilder<TObject> WithTitle(string title) { Title = title; return this; }
    /// <summary>设置编辑器的对象</summary>
    public EditorOptionBuilder<TObject> WithObject(TObject obj) { Object = obj; return this; }
    /// <summary>设置编辑器的窗口宽度，默认450</summary>
    public EditorOptionBuilder<TObject> WithEditorWidth(double value) { EditorWidth = value; return this; }
    /// <summary>设置编辑器的窗口宽度，默认800</summary>
    public EditorOptionBuilder<TObject> WithEditorHeight(double value) { EditorHeight = value; return this; }
    /// <summary>设置编辑器的图标</summary>
    public EditorOptionBuilder<TObject> WithIcon(string iconPath) { IconPath = iconPath; return this; }
    /// <summary>设置编辑器提交前的异步验证规则，如返回值不为空，则会将返回值作为错误信息提示给用户，请勿和<see cref="WithValidator(Func{TObject, string})"/>方法共用</summary>
    public EditorOptionBuilder<TObject> WithValidatorAsync(Func<TObject, Task<string>> validator) { ValidatorTask = validator; return this; }
    /// <summary>设置编辑器提交前的验证规则，如返回值不为空，则会将返回值作为错误信息提示给用户，请勿和<see cref="WithValidatorAsync(Func{TObject, Task{string}})"/>方法共用</summary>
    public EditorOptionBuilder<TObject> WithValidator(Func<TObject, string> validator) { ValidatorTask = (o) => Task.FromResult(validator(o)); return this; }
    /// <summary>设置编辑器确认按钮的文本，默认为确认</summary>
    public EditorOptionBuilder<TObject> WithOkButtonText(string text) { OkButtonText = text; return this; }
    /// <summary>设置编辑器取消按钮的文本，默认为取消</summary>
    public EditorOptionBuilder<TObject> WithCancelButtonText(string text) { CancelButtonText = text; return this; }

    /// <summary>添加一个可编辑的属性，添加顺序决定在编辑器中的显示顺序</summary>
    /// <typeparam name="TProperty">属性类型，要和真实的属性类型一致</typeparam>
    /// <param name="propertyName">属性名</param>
    public PropertyEditorOptionBuilder<TObject, TProperty> AddProperty<TProperty>(string propertyName) {
        var option = new PropertyEditorOptionBuilder<TObject, TProperty>() { PropertyName = propertyName };
        Options.Add(option);
        return option;
    }

    /// <summary>添加一个文件选择器，添加顺序决定在编辑器中的显示顺序</summary>
    public PickFileOptionBuilder<TObject> AddFilePicker() {
        var option = new PickFileOptionBuilder<TObject>();
        Options.Add(option);
        return option;
    }

    /// <summary>添加一个文件夹选择器，添加顺序决定在编辑器中的显示顺序</summary>
    public PickFolderOptionBuilder<TObject> AddFolderPicker() {
        var option = new PickFolderOptionBuilder<TObject>();
        Options.Add(option);
        return option;
    }

    /// <summary>添加一个图片选择器，添加顺序决定在编辑器中的显示顺序</summary>
    public PickImageOptionBuilder<TObject> AddImage() {
        var option = new PickImageOptionBuilder<TObject>();
        Options.Add(option);
        return option;
    }

    /// <summary>添加命令，添加顺序决定在编辑器中的显示顺序</summary>
    public CommandOptionBuilder<TObject> AddCommand() {
        var option = new CommandOptionBuilder<TObject>();
        Options.Add(option);
        return option;
    }

    /// <summary>构建此配置，可在构建后调用相应的方法生成编辑页面</summary>
    public EditorOptions Build() {

        if (Object is null) { throw new ArgumentNullException(nameof(Object)); }

        var options = new EditorOptions {
            Object = Object!,
            Title = Title,
            IconPath = IconPath,
            OkButtonText = OkButtonText,
            CancelButtonText = CancelButtonText,
            EditorWidth = EditorWidth,
            EditorHeight = EditorHeight,
            Validator = (obj) => ValidatorTask((TObject)obj),
        };

        foreach (var option in Options.Select(x => x.Build())) {
            options.Options.Add(option);
        }

        return options;
    }

}

/// <summary>可构建对象</summary>
public interface IBuiltable {
    internal IEditorOption Build();
}

/// <summary>可构建属性对象</summary>
public interface IPropertyEditorOptionBuilder {
    internal PropertyEditorOption Build();
}

/// <summary>命令选项构建器</summary>
public class CommandOptionBuilder<TObject> : IBuiltable {

    internal CommandOptionBuilder() { }
    internal string Header { get; private set; } = "操作";
    internal Action Command { get; private set; } = () => { };

    /// <summary>设置此项的标题，默认为"操作"</summary>
    public CommandOptionBuilder<TObject> WithHeader(string header) { Header = header; return this; }
    /// <summary>设置选择文件后的回调，默认无操作</summary>
    public CommandOptionBuilder<TObject> WithCommand(Action action) { Command = action; return this; }

    IEditorOption IBuiltable.Build() {
        return new CommandEditorOption() {
            Header = Header,
            Command = Command,
        };
    }
}

/// <summary>文件选择器选项构建器</summary>
public class PickFileOptionBuilder<TObject> : IBuiltable {

    internal PickFileOptionBuilder() { }
    internal string Header { get; private set; } = "选择文件";
    internal string FileFilter { get; private set; } = "全部文件(*.*)|*.*";
    internal bool IsOpenFile { get; private set; } = true;
    internal Action<TObject, string> Setter { get; private set; } = (_, _) => { };
    internal Func<TObject, string> Getter { get; private set; } = (_) => string.Empty;

    /// <summary>设置此项的标题，默认为"选择文件"</summary>
    public PickFileOptionBuilder<TObject> WithHeader(string header) { Header = header; return this; }
    /// <summary>设置此项的文件类型筛选器默认为"全部文件(*.*)|*.*"</summary>
    public PickFileOptionBuilder<TObject> WithFilter(string filter) { FileFilter = filter; return this; }
    /// <summary>设置此项的显示对话框是打开文件对话框还是保存文件对话框，默认为打开文件对话框</summary>
    public PickFileOptionBuilder<TObject> WithType(bool isOpenFile) { IsOpenFile = isOpenFile; return this; }
    /// <summary>设置选择文件后的回调，默认无操作</summary>
    public PickFileOptionBuilder<TObject> AfterPick(Action<TObject, string> action) { Setter = action; return this; }
    /// <summary>设置选择文件前的操作，默认无操作，返回值会被用作此项的初始值</summary>
    public PickFileOptionBuilder<TObject> BeforePick(Func<TObject, string> action) { Getter = action; return this; }

    IEditorOption IBuiltable.Build() {
        return new PickFileEditorOption() {
            Header = Header,
            FileFilter = FileFilter,
            IsOpenFile = IsOpenFile,
            Setter = (o, s) => Setter((TObject)o, s),
            Getter = (o) => Getter((TObject)o),
        };
    }
}

/// <summary>文件夹选择器选项构建器</summary>
public class PickFolderOptionBuilder<TObject> : IBuiltable {

    internal PickFolderOptionBuilder() { }
    internal string Header { get; private set; } = "选择文件夹";
    internal Action<TObject, string> Setter { get; private set; } = (_, _) => { };
    internal Func<TObject, string> Getter { get; private set; } = (_) => string.Empty;

    /// <summary>设置此项的标题，默认为"选择文件夹"</summary>
    public PickFolderOptionBuilder<TObject> WithHeader(string header) { Header = header; return this; }
    /// <summary>设置选择文件夹后的回调，默认无操作</summary>
    public PickFolderOptionBuilder<TObject> AfterPick(Action<TObject, string> action) { Setter = action; return this; }
    /// <summary>设置选择文件夹前的操作，默认无操作，返回值会被用作此项的初始值</summary>
    public PickFolderOptionBuilder<TObject> BeforePick(Func<TObject, string> action) { Getter = action; return this; }

    IEditorOption IBuiltable.Build() {
        return new PickFolderEditorOption() {
            Header = Header,
            Setter = (o, s) => Setter((TObject)o, s),
            Getter = (o) => Getter((TObject)o),
        };
    }
}

/// <summary>图片选择器选项构建器</summary>
public class PickImageOptionBuilder<TObject> : IBuiltable {

    internal PickImageOptionBuilder() { }
    internal string Header { get; private set; } = "选择图片";
    internal string FileFilter { get; private set; } = "全部文件(*.*)|*.*";
    internal Action<TObject, string> Setter { get; private set; } = (_, _) => { };
    internal Func<TObject, string> Getter { get; private set; } = (_) => string.Empty;

    /// <summary>设置此项的标题，默认为"选择图片"</summary>
    public PickImageOptionBuilder<TObject> WithHeader(string header) { Header = header; return this; }
    /// <summary>设置此项的文件类型筛选器默认为"全部文件(*.*)|*.*"</summary>
    public PickImageOptionBuilder<TObject> WithFilter(string filter) { FileFilter = filter; return this; }
    /// <summary>设置选择文件后的回调，默认无操作</summary>
    public PickImageOptionBuilder<TObject> AfterPick(Action<TObject, string> action) { Setter = action; return this; }
    /// <summary>设置选择文件前的操作，默认无操作，返回值会被用作此项的初始值</summary>
    public PickImageOptionBuilder<TObject> BeforePick(Func<TObject, string> action) { Getter = action; return this; }

    IEditorOption IBuiltable.Build() {
        return new PickImageEditorOption() {
            Header = Header,
            FileFilter = FileFilter,
            Setter = (o, s) => Setter((TObject)o, s),
            Getter = (o) => Getter((TObject)o),
        };
    }
}

/// <summary>属性选项构建器</summary>
public class PropertyEditorOptionBuilder<TObject, TProperty> : IBuiltable {

    internal PropertyEditorOptionBuilder() { }
    internal string PropertyName { get; set; } = string.Empty;
    internal string PropertyHeader { get; private set; } = string.Empty;
    internal string Suffix { get; private set; } = string.Empty;
    internal string PlaceHolder { get; private set; } = string.Empty;
    internal IPropertyEditorOptionBuilder Option { get; private set; }

    /// <summary>设置此项的标题，默认为空</summary>
    public PropertyEditorOptionBuilder<TObject, TProperty> WithHeader(string header) { PropertyHeader = header; return this; }
    /// <summary>设置此项的后缀，默认为空</summary>
    public PropertyEditorOptionBuilder<TObject, TProperty> WithSuffix(string suffix) { Suffix = suffix; return this; }
    /// <summary>设置此项的后缀，默认为空</summary>
    public PropertyEditorOptionBuilder<TObject, TProperty> WithPlaceHoplder(string placeholder) { PlaceHolder = placeholder; return this; }

    /// <summary>使用文本类型编辑器编辑此属性</summary>
    public TextPropertyEditorOptionBuilder<TObject, TProperty> EditAsText() {
        Option = new TextPropertyEditorOptionBuilder<TObject, TProperty>();
        return (TextPropertyEditorOptionBuilder<TObject, TProperty>)Option;
    }
    /// <summary>使用机密文本类型编辑器编辑此属性</summary>
    public SecretTextPropertyEditorOptionBuilder<TObject, TProperty> EditAsSecretText() {
        Option = new SecretTextPropertyEditorOptionBuilder<TObject, TProperty>();
        return (SecretTextPropertyEditorOptionBuilder<TObject, TProperty>)Option;
    }
    /// <summary>使用整数类型编辑器编辑此属性</summary>
    public IntPropertyEditorBuilder<TObject, TProperty> EditAsInt() {
        //if (typeof(int) != typeof(TProperty) && typeof(int?) != typeof(TProperty)) { throw new("只能是 int 类型"); }
        Option = new IntPropertyEditorBuilder<TObject, TProperty>();
        return (IntPropertyEditorBuilder<TObject, TProperty>)Option;
    }
    /// <summary>使用浮点数类型编辑器编辑此属性</summary>
    public DoublePropertyEditorBuilder<TObject, TProperty> EditAsDouble() {
        //if (typeof(double) != typeof(TProperty) && typeof(double?) != typeof(TProperty) && typeof(float) != typeof(TProperty) && typeof(float?) != typeof(TProperty)) { throw new("只能是 double 类型"); }
        Option = new DoublePropertyEditorBuilder<TObject, TProperty>();
        return (DoublePropertyEditorBuilder<TObject, TProperty>)Option;
    }
    /// <summary>使用布尔值类型编辑器编辑此属性</summary>
    public BooleanPropertyEditorBuilder<TObject, TProperty> EditAsBoolean() {
        Option = new BooleanPropertyEditorBuilder<TObject, TProperty>();
        return (BooleanPropertyEditorBuilder<TObject, TProperty>)Option;
    }
    /// <summary>使用日期类型编辑器编辑此属性</summary>
    public DateTimePropertyEditorBuilder<TObject, TProperty> EditAsDateTime() {
        Option = new DateTimePropertyEditorBuilder<TObject, TProperty>();
        return (DateTimePropertyEditorBuilder<TObject, TProperty>)Option;
    }
    /// <summary>使用单选类型编辑器编辑此属性</summary>
    /// <typeparam name="TOption">选项的值类型</typeparam>
    public SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption> EditAsSingleSelect<TOption>() {
        Option = new SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption>();
        return (SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption>)Option;
    }
    /// <summary>使用多选类型编辑器编辑此属性</summary>
    /// <typeparam name="TOption">选项的值类型</typeparam>
    public MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption> EditAsMultiSelect<TOption>() {
        Option = new MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption>();
        return (MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption>)Option;
    }
    /// <summary>使用组合框类型编辑器编辑此属性</summary>
    /// <typeparam name="TOption">选项的值类型</typeparam>
    public ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption> EditAsCombo<TOption>() {
        Option = new ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption>();
        return (ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption>)Option;
    }

    IEditorOption IBuiltable.Build() {
        var option = (Option ?? new TextPropertyEditorOptionBuilder<TObject, TProperty>()).Build();

        option.PropertyName = PropertyName;
        option.PropertyHeader = PropertyHeader;
        option.PlaceHolder = PlaceHolder;
        option.Suffix = Suffix;

        return option;
    }
}

/// <summary>文本类型属性编辑器选项构建器</summary>
public class TextPropertyEditorOptionBuilder<TObject, TProperty> : IPropertyEditorOptionBuilder {

    internal TextPropertyEditorOptionBuilder() { }
    internal bool HasMultiLine { get; private set; } = false;
    internal int MinLength { get; private set; } = 0;
    internal int MaxLength { get; private set; } = 0;
    internal Func<string, TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x;
    internal Func<TProperty, string> ConverterFromProperty { get; private set; } = x => (string)(object)x;

    /// <summary>设置此属性应多行显示</summary>
    public TextPropertyEditorOptionBuilder<TObject, TProperty> WithMultiLine() { HasMultiLine = true; return this; }
    /// <summary>设置此属性最小长度</summary>
    public TextPropertyEditorOptionBuilder<TObject, TProperty> LongerThan(int length) { MinLength = length; return this; }
    /// <summary>设置此属性最大长度</summary>
    public TextPropertyEditorOptionBuilder<TObject, TProperty> ShorterThan(int length) { MaxLength = length; return this; }
    /// <summary>设置从文本转换至属性的方法，返回值会被赋值给此属性</summary>
    public TextPropertyEditorOptionBuilder<TObject, TProperty> ConvertToProperty(Func<string, TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至文本的方法，返回值会被用作此项的初始值</summary>
    public TextPropertyEditorOptionBuilder<TObject, TProperty> ConvertFromProperty(Func<TProperty, string> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new TextEditorOption() {
            HasMultiLine = HasMultiLine,
            MinLength = MinLength,
            MaxLength = MaxLength,
            ConverterToProperty = (t) => ConverterToProperty(t),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o),
        };
    }
}

/// <summary>机密文本类型属性编辑器选项构建器</summary>
public class SecretTextPropertyEditorOptionBuilder<TObject, TProperty> : IPropertyEditorOptionBuilder {

    internal SecretTextPropertyEditorOptionBuilder() { }
    internal int MinLength { get; private set; } = 0;
    internal int MaxLength { get; private set; } = 0;
    internal Func<string, TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x;
    internal Func<TProperty, string> ConverterFromProperty { get; private set; } = x => (string)(object)x;

    /// <summary>设置此属性最小长度</summary>
    public SecretTextPropertyEditorOptionBuilder<TObject, TProperty> LongerThan(int length) { MinLength = length; return this; }
    /// <summary>设置此属性最大长度</summary>
    public SecretTextPropertyEditorOptionBuilder<TObject, TProperty> ShorterThan(int length) { MaxLength = length; return this; }
    /// <summary>设置从文本转换至属性的方法，返回值会被赋值给此属性</summary>
    public SecretTextPropertyEditorOptionBuilder<TObject, TProperty> ConvertToProperty(Func<string, TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至文本的方法，返回值会被用作此项的初始值</summary>
    public SecretTextPropertyEditorOptionBuilder<TObject, TProperty> ConvertFromProperty(Func<TProperty, string> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new SecretTextEditorOption() {
            MinLength = MinLength,
            MaxLength = MaxLength,
            ConverterToProperty = (t) => ConverterToProperty(t),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o),
        };
    }
}

/// <summary>整数类型属性编辑器选项构建器</summary>
public class IntPropertyEditorBuilder<TObject, TProperty> : IPropertyEditorOptionBuilder {

    internal IntPropertyEditorBuilder() { }
    internal int MinValue { get; private set; } = int.MinValue;
    internal int MaxValue { get; private set; } = int.MaxValue;
    internal Func<int, TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x;
    internal Func<TProperty, int> ConverterFromProperty { get; private set; } = x => (int)(object)x;

    /// <summary>设置此属性最小值</summary>
    public IntPropertyEditorBuilder<TObject, TProperty> LargerThan(int value) { MinValue = value; return this; }
    /// <summary>设置此属性最大值</summary>
    public IntPropertyEditorBuilder<TObject, TProperty> SmallerThan(int value) { MaxValue = value; return this; }
    /// <summary>设置从整数转换至属性的方法，返回值会被赋值给此属性</summary>
    public IntPropertyEditorBuilder<TObject, TProperty> ConvertToProperty(Func<int, TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至整数的方法，返回值会被用作此项的初始值</summary>
    public IntPropertyEditorBuilder<TObject, TProperty> ConvertFromProperty(Func<TProperty, int> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new IntEditorOption() {
            MinValue = MinValue,
            MaxValue = MaxValue,
            ConverterToProperty = (t) => ConverterToProperty(t),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o),
        };
    }
}

/// <summary>浮点数类型属性编辑器选项构建器</summary>
public class DoublePropertyEditorBuilder<TObject, TProperty> : IPropertyEditorOptionBuilder {

    internal DoublePropertyEditorBuilder() { }
    internal double MinValue { get; private set; } = double.MinValue;
    internal double MaxValue { get; private set; } = double.MaxValue;
    internal Func<double, TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x;
    internal Func<TProperty, double> ConverterFromProperty { get; private set; } = x => (double)(object)x;

    /// <summary>设置此属性最小值</summary>
    public DoublePropertyEditorBuilder<TObject, TProperty> LargerThan(double value) { MinValue = value; return this; }
    /// <summary>设置此属性最大值</summary>
    public DoublePropertyEditorBuilder<TObject, TProperty> SmallerThan(double value) { MaxValue = value; return this; }
    /// <summary>设置从浮点数转换至属性的方法，返回值会被赋值给此属性</summary>
    public DoublePropertyEditorBuilder<TObject, TProperty> ConvertToProperty(Func<double, TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至浮点数的方法，返回值会被用作此项的初始值</summary>
    public DoublePropertyEditorBuilder<TObject, TProperty> ConvertFromProperty(Func<TProperty, double> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new DoubleEditorOption() {
            MinValue = MinValue,
            MaxValue = MaxValue,
            ConverterToProperty = (t) => ConverterToProperty(t),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o),
        };
    }
}

/// <summary>日期类型属性编辑器选项构建器</summary>
public class DateTimePropertyEditorBuilder<TObject, TProperty> : IPropertyEditorOptionBuilder {

    internal DateTimePropertyEditorBuilder() { }
    internal DateTime MinValue { get; private set; } = DateTime.MinValue;
    internal DateTime MaxValue { get; private set; } = DateTime.MaxValue;
    internal Func<DateTime, TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x;
    internal Func<TProperty, DateTime> ConverterFromProperty { get; private set; } = x => (DateTime)(object)x;

    /// <summary>设置此属性最小值</summary>
    public DateTimePropertyEditorBuilder<TObject, TProperty> After(DateTime value) { MinValue = value; return this; }
    /// <summary>设置此属性最大值</summary>
    public DateTimePropertyEditorBuilder<TObject, TProperty> Before(DateTime value) { MaxValue = value; return this; }
    /// <summary>设置从日期转换至属性的方法，返回值会被赋值给此属性</summary>
    public DateTimePropertyEditorBuilder<TObject, TProperty> ConvertToProperty(Func<DateTime, TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至日期的方法，返回值会被用作此项的初始值</summary>
    public DateTimePropertyEditorBuilder<TObject, TProperty> ConvertFromProperty(Func<TProperty, DateTime> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new DateTimeEditorOption() {
            MinValue = MinValue,
            MaxValue = MaxValue,
            ConverterToProperty = (t) => ConverterToProperty(t),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o),
        };
    }
}

/// <summary>布尔类型属性编辑器选项构建器</summary>
public class BooleanPropertyEditorBuilder<TObject, TProperty> : IPropertyEditorOptionBuilder {

    internal BooleanPropertyEditorBuilder() { }
    internal Func<bool, TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x;
    internal Func<TProperty, bool> ConverterFromProperty { get; private set; } = x => (bool)(object)x;

    /// <summary>设置从布尔值转换至属性的方法，返回值会被赋值给此属性</summary>
    public BooleanPropertyEditorBuilder<TObject, TProperty> ConvertToProperty(Func<bool, TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至布尔值的方法，返回值会被用作此项的初始值</summary>
    public BooleanPropertyEditorBuilder<TObject, TProperty> ConvertFromProperty(Func<TProperty, bool> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new BooleanEditorOption() {
            ConverterToProperty = (t) => ConverterToProperty(t),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o),
        };
    }
}

/// <summary>可选类型的选项</summary>
public class PropertyOption<TOption> {
    /// <summary>要显示的文本</summary>
    public string Text { get; set; }
    /// <summary>实际的值</summary>
    public TOption Value { get; set; }
}

/// <summary>单选类型属性编辑器选项构建器</summary>
public class SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption> : IPropertyEditorOptionBuilder {

    internal SingleSelectPropertyEditorBuilder() { }
    internal List<PropertyOption<TOption>> Options { get; } = [];
    internal Func<TOption, TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x;
    internal Func<TProperty, TOption> ConverterFromProperty { get; private set; } = x => (TOption)(object)x;

    /// <summary>设置此属性的可选项</summary>
    public SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions(List<PropertyOption<TOption>> options) {
        Options.AddRange(options);
        return this;
    }
    /// <summary>设置此属性的可选项</summary>
    public SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions(params (string Text, TOption Value)[] options) {
        Options.AddRange(options.Select(x => new PropertyOption<TOption>() { Text = x.Text, Value = x.Value }));
        return this;
    }
    /// <summary>使用指定枚举类型中的全部值设置此属性的可选项</summary>
    public SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions<TEnum>() where TEnum : Enum {
        Options.AddRange(Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(x => new PropertyOption<TOption>() { Text = x.ToString(), Value = (TOption)(object)x }));
        return this;
    }
    /// <summary>设置从选项转换至属性的方法，返回值会被赋值给此属性</summary>
    public SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption> ConvertToProperty(Func<TOption, TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至选项的方法，返回值会被用作此项的初始值</summary>
    public SingleSelectPropertyEditorBuilder<TObject, TProperty, TOption> ConvertFromProperty(Func<TProperty, TOption> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new SingleSelectEditorOption() {
            Options = Options.Select(x => new PropertyOption<object>() { Text = x.Text, Value = x.Value }).ToList(),
            ConverterToProperty = (t) => ConverterToProperty((TOption)t),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o),
        };
    }

}

/// <summary>多选类型属性编辑器选项构建器</summary>
public class MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption> : IPropertyEditorOptionBuilder {

    internal MultiSelectPropertyEditorBuilder() { }
    internal List<PropertyOption<TOption>> Options { get; } = [];
    internal Func<TOption[], TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x.FirstOrDefault();
    internal Func<TProperty, TOption[]> ConverterFromProperty { get; private set; } = x => [(TOption)(object)x];

    /// <summary>设置此属性的可选项</summary>
    public MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions(List<PropertyOption<TOption>> options) {
        Options.AddRange(options);
        return this;
    }
    /// <summary>设置此属性的可选项</summary>
    public MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions(params (string Text, TOption Value)[] options) {
        Options.AddRange(options.Select(x => new PropertyOption<TOption>() { Text = x.Text, Value = x.Value }));
        return this;
    }
    /// <summary>使用指定枚举类型中的全部值设置此属性的可选项</summary>
    public MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions<TEnum>() where TEnum : Enum {
        Options.AddRange(Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(x => new PropertyOption<TOption>() { Text = x.ToString(), Value = (TOption)(object)x }));
        return this;
    }
    /// <summary>设置从选项转换至属性的方法，返回值会被赋值给此属性</summary>
    public MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption> ConvertToProperty(Func<TOption[], TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至选项的方法，返回值会被用作此项的初始值</summary>
    public MultiSelectPropertyEditorBuilder<TObject, TProperty, TOption> ConvertFromProperty(Func<TProperty, TOption[]> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new MultiSelectEditorOption() {
            Options = Options.Select(x => new PropertyOption<object>() { Text = x.Text, Value = x.Value }).ToList(),
            ConverterToProperty = (t) => ConverterToProperty(t.AsEnumerable().OfType<TOption>().ToArray()),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o).Select(x => (object)x).ToArray(),
        };
    }

}

/// <summary>组合框类型属性编辑器选项构建器</summary>
public class ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption> : IPropertyEditorOptionBuilder {

    internal ComboSelectPropertyEditorBuilder() { }
    internal List<PropertyOption<TOption>> Options { get; } = [];
    internal Func<TOption, TProperty> ConverterToProperty { get; private set; } = x => (TProperty)(object)x;
    internal Func<TProperty, TOption> ConverterFromProperty { get; private set; } = x => (TOption)(object)x;

    /// <summary>设置此属性的可选项</summary>
    public ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions(List<PropertyOption<TOption>> options) {
        Options.AddRange(options);
        return this;
    }
    /// <summary>设置此属性的可选项</summary>
    public ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions(params (string Text, TOption Value)[] options) {
        Options.AddRange(options.Select(x => new PropertyOption<TOption>() { Text = x.Text, Value = x.Value }));
        return this;
    }
    /// <summary>使用指定枚举类型中的全部值设置此属性的可选项</summary>
    public ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption> WithOptions<TEnum>() where TEnum : Enum {
        Options.AddRange(Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(x => new PropertyOption<TOption>() { Text = x.ToString(), Value = (TOption)(object)x }));
        return this;
    }
    /// <summary>设置从选项转换至属性的方法，返回值会被赋值给此属性</summary>
    public ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption> ConvertToProperty(Func<TOption, TProperty> converter) { ConverterToProperty = converter; return this; }
    /// <summary>设置从属性转换至选项的方法，返回值会被用作此项的初始值</summary>
    public ComboSelectPropertyEditorBuilder<TObject, TProperty, TOption> ConvertFromProperty(Func<TProperty, TOption> converter) { ConverterFromProperty = converter; return this; }

    PropertyEditorOption IPropertyEditorOptionBuilder.Build() {
        return new ComboSelectEditorOption() {
            Options = Options.Select(x => new PropertyOption<object>() { Text = x.Text, Value = x.Value }).ToList(),
            ConverterToProperty = (t) => ConverterToProperty((TOption)t),
            ConverterFromProperty = (o) => ConverterFromProperty((TProperty)o),
        };
    }

}
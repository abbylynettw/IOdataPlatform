using SqlSugar;

namespace IODataPlatform.Models.DBModels;

public partial class formular_index_condition : ObservableObject {

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public int IndexId { get; set; }

    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private string fieldName = string.Empty;
    
    [ObservableProperty]
    private string propertyName = string.Empty;

    [ObservableProperty]
    private string fieldValue = string.Empty;

    [ObservableProperty]
    private FieldOperator fieldOperator = FieldOperator.等于;

    [ObservableProperty]
    private ConditionOperator conditionOperator = ConditionOperator.无;

    public string Description { get; set; } = string.Empty;

}

public enum FieldOperator {
    等于,
    不等于,
    包含,
    不包含,
    起始于,
    终止于
}

public enum ConditionOperator {
    无,
    并且,
    或,
}
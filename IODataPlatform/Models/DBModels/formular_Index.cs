using SqlSugar;

namespace IODataPlatform.Models.DBModels;

public partial class formular_Index : ObservableObject {

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [ObservableProperty]
    private int index;

    public int FormulaId { get; set; }

    [ObservableProperty]
    private string returnValue;

    public string Description { get; set; } = string.Empty;

}
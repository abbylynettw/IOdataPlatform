using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace IODataPlatform.Views.Pages
{
    public partial class OtherFunctionViewModel
    {

        [RelayCommand]
        public void ComparePantaiProperties()
        {
            if (picker.OpenFile("请选择设计院的设备清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx") is not string sourceFilePath || string.IsNullOrEmpty(sourceFilePath))
                return;
            if (picker.OpenFile("请选择我的设备清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx") is not string destFilePath || string.IsNullOrEmpty(destFilePath))
                return;

            List<设备清单> equipments = new List<设备清单>();
            var wb = new Workbook(sourceFilePath);
            foreach (var worksheet in wb.Worksheets)
            {
                for (int row = 1; row <= worksheet.Cells.MaxDataRow; row++)
                {
                    equipments.Add(new 设备清单
                    {
                        PKS = worksheet.Cells[row, 5]?.StringValue,
                        Prop1 = worksheet.Cells[row, 14]?.StringValue,
                        Prop2 = worksheet.Cells[row, 15]?.StringValue,
                        Prop3 = worksheet.Cells[row, 16]?.StringValue,
                    });
                }
                break;
            }

            List<设备清单> equipments1 = new List<设备清单>();
            var wb1 = new Workbook(destFilePath);
            foreach (var worksheet in wb1.Worksheets)
            {
                for (int row = 1; row <= worksheet.Cells.MaxDataRow; row++)
                {
                    equipments1.Add(new 设备清单
                    {
                        PKS = worksheet.Cells[row, 5]?.StringValue,
                        Prop1 = worksheet.Cells[row, 10]?.StringValue,
                        Prop2 = worksheet.Cells[row, 11]?.StringValue,
                        Prop3 = worksheet.Cells[row, 12]?.StringValue,
                    });
                }
                break;
            }

            foreach (var equipment in equipments)
            {
                var match = equipments1.FirstOrDefault(e => e.PKS == equipment.PKS);
                if (match != null)
                {
                    if (equipment.Prop1 != match.Prop1 || equipment.Prop2 != match.Prop2 || equipment.Prop3 != match.Prop3)
                    {
                        HighlightDifferences(match, equipment, wb1);
                    }
                }
                else
                {
                    match = equipments1.FirstOrDefault(e => e.PKS == equipment.Prop1);
                    if (match != null)
                    {
                        if (equipment.Prop2 != match.Prop2 || equipment.Prop3 != match.Prop3)
                        {
                            HighlightPartialMatchDifferences(match, equipment, wb1);
                        }
                    }
                    else
                    {
                        HighlightNoMatch(equipment, wb1);
                    }
                }
            }

            wb1.Save(destFilePath);
        }

        private void HighlightDifferences(设备清单 user, 设备清单 design, Workbook workbook)
        {
            foreach (var worksheet in workbook.Worksheets)
            {
                for (int row = 1; row <= worksheet.Cells.MaxDataRow; row++)
                {
                    if (worksheet.Cells[row, 5]?.StringValue == user.PKS)
                    {
                        if (user.Prop1 != design.Prop1)
                        {
                            SetCellColor(worksheet.Cells[row, 10], Color.Red);
                        }
                        if (user.Prop2 != design.Prop2)
                        {
                            SetCellColor(worksheet.Cells[row, 11], Color.Red);
                        }
                        if (user.Prop3 != design.Prop3)
                        {
                            SetCellColor(worksheet.Cells[row, 12], Color.Red);
                        }
                    }
                }
            }
        }

        private void HighlightPartialMatchDifferences(设备清单 user, 设备清单 design, Workbook workbook)
        {
            foreach (var worksheet in workbook.Worksheets)
            {
                for (int row = 1; row <= worksheet.Cells.MaxDataRow; row++)
                {
                    if (worksheet.Cells[row, 5]?.StringValue == user.PKS)
                    {
                        if (user.Prop2 != design.Prop2)
                        {
                            SetCellColor(worksheet.Cells[row, 11], Color.Red);
                        }
                        if (user.Prop3 != design.Prop3)
                        {
                            SetCellColor(worksheet.Cells[row, 12], Color.Red);
                        }
                    }
                }
            }
        }

        private void HighlightNoMatch(设备清单 user, Workbook workbook)
        {
            foreach (var worksheet in workbook.Worksheets)
            {
                for (int row = 1; row <= worksheet.Cells.MaxDataRow; row++)
                {
                    if (worksheet.Cells[row, 5]?.StringValue == user.PKS)
                    {
                        SetCellColor(worksheet.Cells[row, 5], Color.Red);
                        SetCellColor(worksheet.Cells[row, 10], Color.Red);
                        SetCellColor(worksheet.Cells[row, 11], Color.Red);
                        SetCellColor(worksheet.Cells[row, 12], Color.Red);
                    }
                }
            }
        }

        private void SetCellColor(Cell cell, Color color)
        {
            var style = cell.GetStyle();            
            style.ForegroundColor = color;
            style.BackgroundColor = color;
            style.Pattern = BackgroundType.Solid;
            cell.SetStyle(style);
        }
    }

    public class 设备清单
    {
        public string PKS { get; set; }

        public string Prop1 { get; set; }

        public string Prop2 { get; set; }

        public string Prop3 { get; set; }
    }
}

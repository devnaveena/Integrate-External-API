using System.Reflection;
using DocumentFormat.OpenXml;
using IntegrateExternalAPI.Models;
using Microsoft.Extensions.Logging;
using IntegrateExternalAPI.Contracts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace IntegrateExternalAPI.Data
{
    /// <summary>
    /// Writing in SpreadSheet
    /// </summary>
    public class ExportToExcel : IExportToExcel
    {
        private readonly ILogger<ExportToExcel> _logger;
        public ExportToExcel(ILogger<ExportToExcel> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// The function writes data from user, post, comment, and todos lists to a spreadsheet document
        /// with three sheets.
        /// </summary>
        /// <param name="user">The type of the user object.</param>
        /// <param name="post">A list of objects representing posts.</param>
        /// <param name="comment">A list of comments.</param>
        /// <param name="todos">A list of objects representing tasks to be completed.</param>
        public string ExportDataToExcel<P, C, T>(User user, List<P> post, List<List<C>> comments, List<T> todos)
        {
            //Creating Excel for each User
            string fileName = user.Name + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".xlsx";
            string filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\File\\", fileName);
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {

                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());

                _logger.LogDebug("Creating workbook part and workbook...");

                // First sheet
                _logger.LogInformation("Creating first sheet...");

                uint sheetId = 1;
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(workbookPart.AddNewPart<WorksheetPart>()), SheetId = sheetId, Name = "userInfo" };
                sheets.Append(sheet);
                _logger.LogDebug("Adding worksheet to the workbook...");
                WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();
                SheetData sheetData = new SheetData();

                PropertyInfo[] propertyInfos = typeof(User).GetProperties();
                Row headerRow = new Row();
                for (int i = 0; i < propertyInfos.Count(); i++)
                {
                    headerRow.AppendChild(CreateTextCell(propertyInfos[i].Name));
                }
                sheetData.AppendChild(headerRow);

                // Add user data to the sheet
                sheetData.AppendChild(CreateDataRow(propertyInfos, user));
                worksheet.AppendChild(sheetData);
                worksheetPart.Worksheet = worksheet;

                // Second sheet
                _logger.LogInformation("Creating second sheet...");

                sheetId = 2;
                Sheet sheet2 = new Sheet() { Id = workbookPart.GetIdOfPart(workbookPart.AddNewPart<WorksheetPart>()), SheetId = sheetId, Name = "Post and Comments" };
                sheets.Append(sheet2);

                WorksheetPart worksheetPart1 = (WorksheetPart)workbookPart.GetPartById(sheet2.Id);

                //Adding style sheet
                WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = GenerateStylesheet();
                stylesPart.Stylesheet.Save();
                DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet1 = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();
                SheetData sheetData1 = new SheetData();

                PropertyInfo[] postPropertyInfos = typeof(P).GetProperties();
                PropertyInfo[] commentPropertyInfos = typeof(C).GetProperties();

                // Add header row for post properties
                Row postHeaderRow = new Row();
                for (int i = 0; i < postPropertyInfos.Count(); i++)
                {
                    postHeaderRow.AppendChild(CreateTextCell(postPropertyInfos[i].Name));
                }
                sheetData1.AppendChild(postHeaderRow);
                _logger.LogDebug("Adding post and comment data to the sheet...");


                for (int j = 0; j < post.Count; j++)
                {
                    // Add row for post data
                    sheetData1.AppendChild(CreateDataRow(postPropertyInfos, post[j], 1));

                    // Add header row for comment properties
                    Row commentHeaderRow = new Row();
                    for (int i = 0; i < commentPropertyInfos.Length; i++)
                    {
                        commentHeaderRow.AppendChild(CreateTextCell(commentPropertyInfos[i].Name));
                    }
                    sheetData1.AppendChild(commentHeaderRow);

                    // Add rows for comment data
                    foreach (var comment in comments[j])
                    {
                        sheetData1.AppendChild(CreateDataRow(commentPropertyInfos, comment, 2));
                    }
                    // Add blank row to visually separate posts
                    sheetData1.AppendChild(new Row());
                }

                worksheet1.AppendChild(sheetData1);
                worksheetPart1.Worksheet = worksheet1;

                //Sheet 3
                _logger.LogInformation("Creating Third sheet...");
                sheetId = 3;
                Sheet sheet3 = new Sheet() { Id = workbookPart.GetIdOfPart(workbookPart.AddNewPart<WorksheetPart>()), SheetId = sheetId, Name = "TodoList" };
                sheets.Append(sheet3);

                WorksheetPart worksheetPart2 = (WorksheetPart)workbookPart.GetPartById(sheet3.Id);
                DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet2 = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();
                SheetData sheetData2 = new SheetData();

                PropertyInfo[] propertyInfos2 = typeof(T).GetProperties();
                Row headerRow2 = new Row();
                for (int i = 0; i < propertyInfos2.Count(); i++)
                {
                    headerRow2.AppendChild(CreateTextCell(propertyInfos2[i].Name));
                }
                sheetData2.AppendChild(headerRow2);
                foreach (var todo in todos)
                {
                    sheetData2.AppendChild(CreateDataRow(propertyInfos2, todo));

                }
                worksheet2.AppendChild(sheetData2);
                worksheetPart2.Worksheet = worksheet2;
            }
            return filePath;
        }

        /// <summary>
        /// Create New Text Cell
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Cell CreateTextCell(string value)
        {
            return new Cell() { CellValue = new CellValue(value), DataType = CellValues.String };
        }

        /// <summary>
        /// Create Row in excelsheet
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="classObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Row CreateDataRow<T>(PropertyInfo[] prop, T classObject)
        {
            Row row = new Row();
            foreach (PropertyInfo item in prop)
            {
                object? value = item.GetValue(classObject);
                row.AppendChild(CreateTextCell(value != null ? value.ToString() : ""));
            }
            return row;
        }

        private static Cell CreateTextCell(string value, UInt32 styleIndexValue = 0)
        {
            return new Cell() { CellValue = new CellValue(value), DataType = CellValues.String, StyleIndex = styleIndexValue };
        }

        private static Row CreateDataRow<T>(PropertyInfo[] prop, T classObject, UInt32 styleIndex)
        {
            Row row = new Row();
            foreach (PropertyInfo item in prop)
            {
                object? value = item.GetValue(classObject);
                row.AppendChild(CreateTextCell(value != null ? value.ToString() : "", styleIndex));
            }
            return row;
        }


        /// <summary>
        /// The function generates a stylesheet with various fonts, fills, borders, and cell formats for
        /// use in Excel spreadsheets.
        /// </summary>
        /// <returns>
        /// The method is returning a Stylesheet object.
        /// </returns>
        private Stylesheet GenerateStylesheet()
        {
            Stylesheet styleSheet;

            Fonts fonts = new Fonts
            (
                new Font(new FontSize() { Val = 10 }),
                new Font(new FontSize() { Val = 10 }, new Bold(), new Color() { Rgb = new HexBinaryValue() { Value = "88888888" } })
            );

            Fills fills = new Fills
            (
                new Fill(new PatternFill() { PatternType = PatternValues.None }),
                new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "#000000" } }) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "11110010" } }) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "00002222" } }) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "11113333" } }) { PatternType = PatternValues.Solid })
            );

            Borders borders = new Borders
            (
                new Border(),
                new Border
                (new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder()
                )
            );
            CellFormat defaultCellFormat = new CellFormat { FontId = 0, FillId = 0, BorderId = 0 };
            CellFormat textWrapCellFormat = new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Top, WrapText = true }) { FontId = 1, FillId = 2, BorderId = 1, ApplyFill = true };
            CellFormat blueBackgroundCellFormat = new CellFormat { FontId = 1, FillId = 4, BorderId = 1, ApplyFont = true, ApplyFill = true, ApplyBorder = true };

            CellFormats cellFormat = new CellFormats(defaultCellFormat, textWrapCellFormat, blueBackgroundCellFormat);

            styleSheet = new Stylesheet(fonts, fills, borders, cellFormat);
            return styleSheet;
        }







    }
}

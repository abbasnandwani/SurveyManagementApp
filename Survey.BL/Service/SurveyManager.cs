using Microsoft.EntityFrameworkCore;
using Survey.BL.DTO;
using Survey.BL.EFCoreModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Office2013.Excel;

namespace Survey.BL.Service
{
    public class SurveyManager
    {
        //public int SurveyId { get; private set; }
        private SurveyDbContext Ctx { get; set; }
        Dictionary<string, Dictionary<int, Dictionary<string, string>>> _SurveyUploaderInfo;

        public EFCoreModels.Survey Survey { get; private set; }

        public SurveyManager(SurveyDbContext ctx)
        {
            this.Ctx = ctx;
            _SurveyUploaderInfo = new Dictionary<string, Dictionary<int, Dictionary<string, string>>>();
        }

        public void LoadSurvey(Stream stream)
        {
            using (SpreadsheetDocument xlsxDoc = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart wbPart = xlsxDoc.WorkbookPart;

                Sheets thesheetcollection = wbPart.Workbook.GetFirstChild<Sheets>();

                foreach (Sheet sheet in thesheetcollection)
                {
                    Console.WriteLine(sheet.Name);

                    Worksheet theWorksheet = ((WorksheetPart)wbPart.GetPartById(sheet.Id)).Worksheet;

                    SheetData sheetData = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                    if (sheet.Name == "Survey")
                    {
                        ExtractSurveyInfo(sheet, sheetData, wbPart);
                    }

                    if (sheet.Name == "SectionAndQuestions")
                    {
                        ExtractSectionAndQuestionsInfo(sheet, sheetData, wbPart);

                        //this._SurveyUploaderInfo.Add(sheet.Name, null);

                    }
                }
            }

            //Create and Populate Survey object
            EFCoreModels.Survey survey = null;

            var surveySheet = this._SurveyUploaderInfo["Survey"];

            int surveyId = 0;

            if (surveySheet[1].ContainsKey("B") &&
                int.TryParse(surveySheet[1]["B"], out surveyId))
            {
                survey = Ctx.Surveys
                    .Include(a => a.Sections)
                    .ThenInclude(a => a.SectionQuestions)
                    .Where(s => s.Id == surveyId).FirstOrDefault();
            }
            else
            {
                survey = new EFCoreModels.Survey();
                survey.Id = surveyId;
            }

            //set survey information
            survey.Name = surveySheet[2]["B"];
            survey.Description = surveySheet[3]["B"];

            //sections
            var sectionsAndQues = this._SurveyUploaderInfo["SectionAndQuestions"];

            Dictionary<string, Section> dicSections = new Dictionary<string, Section>();

            foreach (var section in sectionsAndQues)
            {
                int sectionId = 0;
                string sectionName = null;
                int questionId = 0;
                string question = null;
                QuestionTypeEnum questionType = QuestionTypeEnum.TextSingleLine;
                string answerValues = null;

                if (section.Key == 1)
                    continue;

                //sectionId
                if (section.Value.ContainsKey("A") &&
                    int.TryParse(section.Value["A"], out sectionId))
                {

                }

                //sectionName
                if (section.Value.ContainsKey("B"))
                {
                    sectionName = section.Value["B"];
                }

                //questionId
                if (section.Value.ContainsKey("C") &&
                    int.TryParse(section.Value["C"], out questionId))
                {

                }

                //question
                if (section.Value.ContainsKey("D"))
                {
                    question = section.Value["D"];
                }

                //questionType
                if (section.Value.ContainsKey("E"))
                {
                    Enum.TryParse(section.Value["E"], out questionType);
                    //questionType = section.Value["E"];
                }

                //answerValues
                if (section.Value.ContainsKey("F"))
                {
                    answerValues = section.Value["F"];
                }

                Section surveySection = null;

                if (dicSections.ContainsKey(sectionName)) //get existing section from dictionary
                {
                    surveySection = dicSections[sectionName];

                }
                else
                {
                    surveySection = new Section()
                    {
                        Id = sectionId,
                        Name = sectionName
                    };
                }

                surveySection.SectionQuestions.Add(
                    new SectionQuestion
                    {
                        Id = questionId,
                        Question = question,
                        QuestionTypeId = questionType,
                        AnswerValues = answerValues
                    }
                    );


                //add section to dictionary
                dicSections[sectionName] = surveySection;

            }

            foreach (var section in dicSections)
            {
                Section surveySection = null;

                if (section.Value.Id > 0)
                    surveySection = survey.Sections.Where(s => s.Id == section.Value.Id).FirstOrDefault();
                else
                    surveySection = new Section();

                //set survey section info
                surveySection.Name = section.Value.Name;


                //set questions
                foreach (var ques in section.Value.SectionQuestions)
                {
                    SectionQuestion surveyQues = null;

                    if (ques.Id > 0)
                        surveyQues = surveySection.SectionQuestions.FirstOrDefault(a => a.Id == ques.Id);
                    else
                        surveyQues = new SectionQuestion();

                    //set question info
                    surveyQues.Question = ques.Question;
                    surveyQues.QuestionTypeId = ques.QuestionTypeId;
                    surveyQues.AnswerValues = ques.AnswerValues;


                    if (surveyQues.Id == 0)
                        surveySection.SectionQuestions.Add(surveyQues);
                }

                if (surveySection.Id == 0)
                    survey.Sections.Add(surveySection);
            }


            if (survey.Id > 0)
                Ctx.Surveys.Update(survey);
            else
                Ctx.Surveys.Add(survey);

            Ctx.SaveChanges();
        }

        private void ExtractSurveyInfo(Sheet sheet, SheetData sheetData, WorkbookPart wbPart)
        {
            //SheetInfo sheetInfo = new SheetInfo();
            //sheetInfo.Name = sheet.Name;

            Dictionary<int, Dictionary<string, string>> Data = new Dictionary<int, Dictionary<string, string>>();

            foreach (Row row in sheetData.Elements<Row>())
            {
                RowInfo rowInfo = new RowInfo((int)row.RowIndex.Value);
                Dictionary<string, string> cellObj = new Dictionary<string, string>();
                foreach (Cell cell in row.Elements<Cell>())
                {
                    CellInfo cellInfo = new CellInfo();
                    cellInfo.CellReference = Regex.Replace(cell.CellReference, @"[\d-]", string.Empty);
                    cellInfo.Value = GetCellValue(cell, wbPart);
                    rowInfo.Cells.Add(cellInfo.CellReference, cellInfo.Value);


                    cellObj.Add(cellInfo.CellReference, cellInfo.Value);

                    Console.WriteLine(GetCellValue(cell, wbPart));
                }

                //sheetInfo.Rows.Add((int)row.RowIndex.Value, rowInfo);
                Data.Add((int)row.RowIndex.Value, cellObj);
            }

            this._SurveyUploaderInfo.Add(sheet.Name, Data);
        }

        private void ExtractSectionAndQuestionsInfo(Sheet sheet, SheetData sheetData, WorkbookPart wbPart)
        {

            Dictionary<int, Dictionary<string, string>> Data = new Dictionary<int, Dictionary<string, string>>();

            foreach (Row row in sheetData.Elements<Row>())
            {
                RowInfo rowInfo = new RowInfo((int)row.RowIndex.Value);
                Dictionary<string, string> cellObj = new Dictionary<string, string>();
                foreach (Cell cell in row.Elements<Cell>())
                {
                    CellInfo cellInfo = new CellInfo();
                    cellInfo.CellReference = Regex.Replace(cell.CellReference, @"[\d-]", string.Empty);
                    cellInfo.Value = GetCellValue(cell, wbPart);
                    rowInfo.Cells.Add(cellInfo.CellReference, cellInfo.Value);


                    cellObj.Add(cellInfo.CellReference, cellInfo.Value);

                    Console.WriteLine(GetCellValue(cell, wbPart));
                }

                //sheetInfo.Rows.Add((int)row.RowIndex.Value, rowInfo);
                Data.Add((int)row.RowIndex.Value, cellObj);
            }

            this._SurveyUploaderInfo.Add(sheet.Name, Data);
        }

        private string GetCellValue(Cell theCell, WorkbookPart wbPart)
        {
            string value = null;

            // If the cell does not exist, return an empty string.
            if (theCell.InnerText.Length > 0)
            {
                value = theCell.InnerText;

                // If the cell represents an integer number, you are done. 
                // For dates, this code returns the serialized value that 
                // represents the date. The code handles strings and 
                // Booleans individually. For shared strings, the code 
                // looks up the corresponding value in the shared string 
                // table. For Booleans, the code converts the value into 
                // the words TRUE or FALSE.
                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:

                            // For shared strings, look up the value in the
                            // shared strings table.
                            var stringTable =
                                wbPart.GetPartsOfType<SharedStringTablePart>()
                                .FirstOrDefault();

                            // If the shared string table is missing, something 
                            // is wrong. Return the index that is in
                            // the cell. Otherwise, look up the correct text in 
                            // the table.
                            if (stringTable != null)
                            {
                                value =
                                    stringTable.SharedStringTable
                                    .ElementAt(int.Parse(value)).InnerText;
                            }
                            break;

                        case CellValues.Boolean:
                            switch (value)
                            {
                                case "0":
                                    value = "FALSE";
                                    break;
                                default:
                                    value = "TRUE";
                                    break;
                            }
                            break;
                    }
                }
            }

            return value;
        }

        public MemoryStream DownloadSurvey(int surveyId)
        {
            EFCoreModels.Survey survey = null;

            survey = Ctx.Surveys
                .Include(a => a.Sections)
                .ThenInclude(a => a.SectionQuestions)
                .Where(s => s.Id == surveyId).FirstOrDefault();

            var ms = new MemoryStream();

            ExcelWriter excelWriter = new ExcelWriter(); //excel helper
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

            // Add a WorksheetPart for survey to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            SheetData surveySheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(surveySheetData);

            // Add a WorksheetPart for section to the WorkbookPart.
            WorksheetPart worksheetPart2 = workbookpart.AddNewPart<WorksheetPart>();
            SheetData sectionSheetData = new SheetData();
            worksheetPart2.Worksheet = new Worksheet(sectionSheetData);

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            // Append a survey worksheet and associate it with the workbook.
            Sheet surveySheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.
                GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Survey"
            };

            sheets.Append(surveySheet);

            Row row = null;
            UInt32 rowIdex = 0;
            var cellIdex = 0;

            row = new Row { RowIndex = ++rowIdex };
            surveySheetData.AppendChild(row);
            cellIdex = 0;
            row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++),
                rowIdex, "SurveyID"));
            row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++),
                rowIdex, survey.Id.ToString()));

            row = new Row { RowIndex = ++rowIdex };
            surveySheetData.AppendChild(row);
            cellIdex = 0;
            row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++),
                rowIdex, "Name"));
            row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++),
                rowIdex, survey.Name));

            row = new Row { RowIndex = ++rowIdex };
            surveySheetData.AppendChild(row);
            cellIdex = 0;
            row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++),
                rowIdex, "Description"));
            row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++),
                rowIdex, survey.Description));


            //Append SectionAndQuestions sheet to workbook
            Sheet sectionAndQuestionsSheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.
                GetIdOfPart(worksheetPart2),
                SheetId = 2,
                Name = "SectionAndQuestions"
            };

            sheets.Append(sectionAndQuestionsSheet);


            row = null;
            rowIdex = 0;
            cellIdex = 0;

            //header row
            string[] headers = new string[] { "SectionID","SectionName", "QuestionId", "Question", "QuestionType", "CellReference",
                "AnswerValues"};

            row = new Row { RowIndex = ++rowIdex };
            sectionSheetData.AppendChild(row);

            foreach (var header in headers)
            {
                row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++),
                    rowIdex, header));
            }

            foreach (var section in survey.Sections)
            {
                foreach (var ques in section.SectionQuestions)
                {

                    cellIdex = 0; //reset cell index
                    row = new Row { RowIndex = ++rowIdex }; //creat new row
                    sectionSheetData.AppendChild(row);

                    row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++), rowIdex, section.Id.ToString()));
                    row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++), rowIdex, section.Name));
                    row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++), rowIdex, ques.Id.ToString()));
                    row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++), rowIdex, ques.Question));
                    row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++), rowIdex, ques.QuestionTypeId.ToString()));
                    row.AppendChild(excelWriter.CreateTextCell(excelWriter.ColumnLetter(cellIdex++), rowIdex, ques.AnswerValues));
                }
            }

            //save workbook
            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();

            return ms;
        }

        public List<EFCoreModels.Survey> GetSurveys()
        {
            return Ctx.Surveys.ToList();
        }
    }
    public class SheetInfo
    {
        public SheetInfo()
        {
            //this.Rows = new List<RowInfo>();
            //this.Rows = new Dictionary<int, RowInfo>();
            //this.Cells = new List<CellInfo>();
            this.Data = new Dictionary<int, Dictionary<string, string>>();
        }

        public string Name { get; set; }

        //public List<RowInfo> Rows { get; set; }
        //public List<CellInfo> Cells { get; set; }
        //public Dictionary<int, RowInfo> Rows;
        public Dictionary<int, Dictionary<string, string>> Data;
    }
    public class RowInfo
    {
        public RowInfo(int rowNum)
        {
            this.RowNum = rowNum;
            //this.Cells = new List<CellInfo>();
            //this.Cells = new Dictionary<string, CellInfo>();
            this.Cells = new Dictionary<string, string>();
        }

        public int RowNum { get; set; }

        //public List<CellInfo> Cells { get; set; }
        //public Dictionary<string, CellInfo> Cells { get; set; }
        public Dictionary<string, string> Cells { get; set; }
    }

    public class CellInfo
    {
        public string CellReference { get; set; }
        public string Value { get; set; }
    }
}

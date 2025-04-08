using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using System.Linq;

namespace Exam.Services
{
    public class ExcelService
    {
        public class Question
        {
            public string QuestionText { get; set; }
            public List<string> CorrectAnswers { get; set; } = new List<string>();
        }

        public List<Question> LoadQuestions(string sheetName)
        {
            var questions = new List<Question>();
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exam.xlsx");

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var answers = worksheet.Cells[row, 2].Text
                        .Split(new[] { '/', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim())
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .ToList();

                    var question = new Question
                    {
                        QuestionText = worksheet.Cells[row, 1].Text,
                        CorrectAnswers = answers
                    };
                    questions.Add(question);
                }
            }

            return questions;
        }
    }
} 
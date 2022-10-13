using Microsoft.AspNetCore.Mvc.Rendering;
using Survey.BL.EFCoreModels;
using System.Text.RegularExpressions;

namespace Survey.UI.Models
{
    public class SurveyViewModel
    {
        public SurveyViewModel()
        {
            Sections = new List<SectionViewModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public List<SectionViewModel> Sections { get; set; }
    }

    public class SectionViewModel
    {
        public SectionViewModel()
        {
            SectionQuestions = new List<SectionQuestionViewModel>();
        }

        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string Name { get; set; } = null!;

        //public virtual Survey Survey { get; set; } = null!;
        public List<SectionQuestionViewModel> SectionQuestions { get; set; }
    }

    public class SectionQuestionViewModel
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Question { get; set; } = null!;
        public string AnswerValues { get; set; } = null!;
        public string Answer { get; set; }

        public string[] AnswerList { get; set; }
        public QuestionTypeEnum QuestionTypeId { get; set; }

        public List<SelectListItem> GetAnswerValuesForControl()
        {
            List<SelectListItem> lst = new List<SelectListItem>();

            if ((this.QuestionTypeId == QuestionTypeEnum.DropDown ||
                this.QuestionTypeId == QuestionTypeEnum.Choice || this.QuestionTypeId == QuestionTypeEnum.Checkbox)
                && !string.IsNullOrEmpty(this.AnswerValues))
            {
                var textAndValues = this.AnswerValues.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var txt in textAndValues)
                {
                    var value = txt.Split(new string[] { ":" }, StringSplitOptions.None);

                    SelectListItem item = new SelectListItem();

                    item.Text = value[0];
                    item.Value = (value.Length > 1 && value[1] != null) ? value[1] : value[0];

                    lst.Add(item);

                }
            }

            return lst;
        }

    }
        
}
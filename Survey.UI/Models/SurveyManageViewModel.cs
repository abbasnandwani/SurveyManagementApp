using Microsoft.AspNetCore.Mvc.Rendering;
using Survey.BL.EFCoreModels;
using System.Text.RegularExpressions;

namespace Survey.UI.Models
{
    public class SurveyManageViewModel
    {
        public SurveyManageViewModel()
        {
            Surveys = new List<SurveyViewModel>();
        }

        public IFormFile FileUpload { get; set; }

        public List<SurveyViewModel> Surveys { get; set; }
    }
        
}
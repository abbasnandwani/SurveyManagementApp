using Microsoft.AspNetCore.Mvc;
using Survey.BL;
using Survey.BL.DTO;
using Survey.BL.EFCoreModels;
using Survey.BL.Service;
using Survey.UI.Models;
using System.Diagnostics;

namespace Survey.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SurveyDbContext _dbCtx;

        public HomeController(SurveyDbContext dbCtx, ILogger<HomeController> logger)
        {
            _logger = logger;
            _dbCtx = dbCtx;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewSurvey(int id)
        {
            SurveyProcessor sm = new SurveyProcessor(id, _dbCtx);


            SurveyViewModel model = new SurveyViewModel();

            model.Id = sm.Survey.Id;
            model.Name = sm.Survey.Name;
            model.Description = sm.Survey.Description;

            foreach (var sec in sm.Survey.Sections)
            {
                var secView = new SectionViewModel();
                secView.Id = sec.Id;
                secView.Name = sec.Name;

                foreach (var ques in sec.SectionQuestions)
                {
                    var quesView = new SectionQuestionViewModel();

                    quesView.Id = ques.Id;
                    quesView.SectionId = ques.SectionId;
                    quesView.Question = ques.Question;
                    quesView.QuestionTypeId = ques.QuestionTypeId;
                    quesView.AnswerValues = ques.AnswerValues;

                    secView.SectionQuestions.Add(quesView);
                }

                model.Sections.Add(secView);
            }


            return View(model);
        }

        public IActionResult ThankYouSurvey(int id)
        {
            SurveyProcessor sm = new SurveyProcessor(id, _dbCtx);


            SurveyViewModel model = new SurveyViewModel();

            model.Id = sm.Survey.Id;
            model.Name = sm.Survey.Name;
            model.Description = sm.Survey.Description;

            return View(model);
        }


        [HttpPost]
        public IActionResult SubmitSurvey(SurveyViewModel model)
        {
            SurveyProcessor sm = new SurveyProcessor(model.Id, _dbCtx);

            SubmittedSurveyResponse res = new SubmittedSurveyResponse();

            foreach (var sec in model.Sections)
            {
                foreach (var ques in sec.SectionQuestions)
                {
                    res.Responses.Add(new SurveyResponseDTO
                    {
                        Question = ques.Question,
                        SectionId = sec.Id,
                        SectionQuestionId = ques.Id,
                        SurveyId = model.Id,
                        AnswerResponse = ques.QuestionTypeId == QuestionTypeEnum.Checkbox ? String.Join(",", ques.AnswerList) : ques.Answer
                    });
                }
            }

            sm.CaptureSurveyResponse(res);

            //return RedirectToAction("ViewSurvey", new { id = 1 });
            return RedirectToAction("ThankYouSurvey", new { id = model.Id });
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ManageSurveys()
        {
            SurveyManager sm = new SurveyManager(_dbCtx);

            var surveys = sm.GetSurveys()
                .Select(a => new SurveyViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                }).ToList();

            SurveyManageViewModel model = new SurveyManageViewModel();
            model.Surveys = surveys;


            return View(model);
        }

        public IActionResult DownloadSurvey(int id)
        {
            SurveyManager sm = new SurveyManager(_dbCtx);
            var ms = sm.DownloadSurvey(id);

            var returnMs = new MemoryStream(ms.ToArray());
            return File(returnMs, "APPLICATION/octet-stream", "survey.xlsx");

        }

        [HttpPost]
        public IActionResult SurveyUpload(SurveyManageViewModel model)
        {
            //string wwwPath = this.Environment.WebRootPath;
            //string contentPath = this.Environment.ContentRootPath;

            //string path = Path.Combine(this.Environment.WebRootPath, "Uploads");
            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}

            //List<string> uploadedFiles = new List<string>();
            //foreach (IFormFile postedFile in postedFiles)
            //{
            //    string fileName = Path.GetFileName(postedFile.FileName);
            //    using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            //    {
            //        postedFile.CopyTo(stream);
            //        uploadedFiles.Add(fileName);
            //        ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", fileName);
            //    }
            //}

            SurveyManager sm = new SurveyManager(_dbCtx);

            using (MemoryStream ms = new MemoryStream())
            {
                model.FileUpload.CopyTo(ms);
                sm.LoadSurvey(ms);
            }


            return RedirectToAction("ManageSurveys");
        }

        
    }
}
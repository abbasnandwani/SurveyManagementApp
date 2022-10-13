using Microsoft.EntityFrameworkCore;
using Survey.BL.DTO;
using Survey.BL.EFCoreModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Survey.BL.Service
{
    public class SurveyProcessor
    {
        //public int SurveyId { get; private set; }
        private SurveyDbContext Ctx { get; set; }

        public EFCoreModels.Survey Survey { get; private set; }

        public SurveyProcessor(int surveyId, SurveyDbContext ctx)
        {
            //this.SurveyId = surveyId;
            this.Ctx = ctx;
            LoadSurvey(surveyId);
        }

        private void LoadSurvey(int surveyId)
        {
            var s = this.Ctx.Surveys.Where(s => s.Id == surveyId)
                .Include(a => a.Sections)
                .ThenInclude(s => s.SectionQuestions);

            this.Survey = s.FirstOrDefault();
        }

        public void CaptureSurveyResponse(SubmittedSurveyResponse submittedResponses)
        {
            foreach (var res in submittedResponses.Responses)
            {
                res.SurveyId = this.Survey.Id;

                SurveyResponse surveyResponse = new SurveyResponse();


                surveyResponse.SurveyId =this.Survey.Id;
                surveyResponse.AnswerResponse = res.AnswerResponse;
                surveyResponse.Question = res.Question;
                surveyResponse.SectionQuestionId = res.SectionQuestionId;
                surveyResponse.Question = res.Question;
                surveyResponse.SectionId = res.SectionId;

                this.Ctx.SurveyResponses.Add(surveyResponse);
            }

            //Create json for the submitted response and add to database
            //Serialize json         
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string jsonString = System.Text.Json.JsonSerializer.Serialize(submittedResponses, jsonOptions);
            var surveyRes = new SurveyResponseJson { SurveyId = this.Survey.Id, ResponseInJson = jsonString };

            this.Ctx.SurveyResponseJsons.Add(surveyRes);

            this.Ctx.SaveChanges();
        }
    }
}

using System;
using System.Collections.Generic;

namespace Survey.BL.EFCoreModels
{
    public partial class SurveyResponse
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public int SectionId { get; set; }
        public int SectionQuestionId { get; set; }
        public string Question { get; set; } = null!;
        public string AnswerResponse { get; set; } = null!;
    }
}

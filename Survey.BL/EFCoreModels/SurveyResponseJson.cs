using System;
using System.Collections.Generic;

namespace Survey.BL.EFCoreModels
{
    public partial class SurveyResponseJson
    {
        public int SurveyId { get; set; }
        public string ResponseInJson { get; set; } = null!;

        public virtual Survey Survey { get; set; } = null!;
    }
}

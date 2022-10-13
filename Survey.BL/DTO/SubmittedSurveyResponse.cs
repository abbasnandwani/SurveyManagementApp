using Survey.BL.EFCoreModels;
using System;
using System.Collections.Generic;

namespace Survey.BL.DTO
{
    public class SubmittedSurveyResponse
    {
        public SubmittedSurveyResponse()
        {
            Responses = new List<SurveyResponseDTO>();
        }
        public List<SurveyResponseDTO> Responses { get; set; }
    }
}

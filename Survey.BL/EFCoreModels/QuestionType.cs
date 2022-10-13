using System;
using System.Collections.Generic;

namespace Survey.BL.EFCoreModels
{
    public partial class QuestionType
    {
        public QuestionTypeEnum Id { get; set; }
        public string TypeName { get; set; } = null!;
    }
}

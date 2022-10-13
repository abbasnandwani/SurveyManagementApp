using System;
using System.Collections.Generic;

namespace Survey.BL.EFCoreModels
{
    public partial class SectionQuestion
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Question { get; set; } = null!;
        public QuestionTypeEnum QuestionTypeId { get; set; }
        public string AnswerValues { get; set; } = null!;

        public virtual Section Section { get; set; } = null!;
    }
}

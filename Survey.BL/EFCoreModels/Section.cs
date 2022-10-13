using System;
using System.Collections.Generic;

namespace Survey.BL.EFCoreModels
{
    public partial class Section
    {
        public Section()
        {
            SectionQuestions = new HashSet<SectionQuestion>();
        }

        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string Name { get; set; } = null!;

        public virtual Survey Survey { get; set; } = null!;
        public virtual ICollection<SectionQuestion> SectionQuestions { get; set; }
    }
}

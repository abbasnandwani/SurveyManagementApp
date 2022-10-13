using System;
using System.Collections.Generic;

namespace Survey.BL.EFCoreModels
{
    public partial class Survey
    {
        public Survey()
        {
            Sections = new HashSet<Section>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public virtual SurveyResponseJson? SurveyResponseJson { get; set; }

        public virtual ICollection<Section> Sections { get; set; }
    }
}

namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Assign")]
    public partial class Assign
    {
        public long Id { get; set; }

        public DateTime Date { get; set; }

        public long InstructorId { get; set; }

        public long? AssistantId { get; set; }

        public long? ClassId { get; set; }

        public long? CampId { get; set; }

        public long? BirthdayId { get; set; }

        public long? WorkshopId { get; set; }

        public virtual Birthday Birthday { get; set; }

        public virtual Camp Camp { get; set; }

        public virtual Class Class { get; set; }

        public virtual Instructor Instructor { get; set; }

        public virtual Instructor Instructor1 { get; set; }

        public virtual Workshop Workshop { get; set; }
    }
}

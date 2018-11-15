namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Workshop")]
    public partial class Workshop
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Workshop()
        {
            Assigns = new HashSet<Assign>();
            Bookings = new HashSet<Booking>();
        }

        public long Id { get; set; }

        public long LocationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public long GradeGroupId { get; set; }

        public TimeSpan TimeStart { get; set; }

        public TimeSpan TimeEnd { get; set; }

        public long InstructorId { get; set; }

        public long? AssistantId { get; set; }

        public decimal Cost { get; set; }

        public string Notes { get; set; }

        public DateTime? InputDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assign> Assigns { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Booking> Bookings { get; set; }

        public virtual GradeGroup GradeGroup { get; set; }

        public virtual Instructor Instructor { get; set; }

        public virtual Instructor Instructor1 { get; set; }

        public virtual Location Location { get; set; }
    }
}

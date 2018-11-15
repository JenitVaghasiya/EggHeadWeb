namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("Class")]
    public partial class Class
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Class()
        {
            Assigns = new HashSet<Assign>();
            Bookings = new HashSet<Booking>();
        }

        public long Id { get; set; }

        public long LocationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string OnlineName { get; set; }

        public long GradeGroupId { get; set; }

        public TimeSpan TimeStart { get; set; }

        public TimeSpan TimeEnd { get; set; }

        public long InstructorId { get; set; }

        public long? AssistantId { get; set; }

        public bool CanRegistOnline { get; set; }

        public bool IsOpen { get; set; }

        public DateTime? DisplayUntil { get; set; }

        public int MaxEnroll { get; set; }

        public decimal Cost { get; set; }

        public string OnlineDescription { get; set; }

        public string Notes { get; set; }

        public int? Enrolled { get; set; }

        [Column(TypeName = "date")]
        public DateTime? InputDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assign> Assigns { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Booking> Bookings { get; set; }

        public virtual GradeGroup GradeGroup { get; set; }

        public virtual Instructor Instructor { get; set; }

        public virtual Instructor Instructor1 { get; set; }

        public virtual Location Location { get; set; }


        public decimal? NCost
        {
            get;
            set;
        }

        public DateTime? NDisplayUntil
        {
            get;
            set;
        }

        public int? NMaxEnroll
        {
            get;
            set;
        }

        public string Dates
        {
            get;
            set;
        }

        public List<Assign> AssignList
        {
            get;
            set;
        }

        public List<int> GradeIds
        {
            get;
            set;
        }

        public void UpdateCustomProperties()
        {
            AssignList = (
                from t in Assigns
                orderby t.Date
                select t).ToList<Assign>();
            AssignList.ForEach((Assign t) => t.NDate = new DateTime?(t.Date));
            GradeIds = GradeGroup.Grades.Select(t => (int)t.Id).ToList();
            Dates = string.Join(", ", (
                from a in Assigns
                select a.Date.ToShortDateString()).ToArray<string>());
            NDisplayUntil = DisplayUntil;
            NMaxEnroll = new int?(MaxEnroll);
            NCost = new decimal?(Cost);
        }
    }
}

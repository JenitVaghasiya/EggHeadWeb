namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("Birthday")]
    public partial class Birthday
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Birthday()
        {
            Assigns = new HashSet<Assign>();
            Bookings = new HashSet<Booking>();
        }

        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ParentName { get; set; }

        [Required]
        [StringLength(50)]
        public string ContactNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(50)]
        public string ChildName { get; set; }

        public int Age { get; set; }

        public DateTime PartyDate { get; set; }

        public TimeSpan PartyTime { get; set; }

        [Required]
        public string Notes { get; set; }

        public long AreaId { get; set; }

        public DateTime? InputDate { get; set; }

        public virtual Area Area { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assign> Assigns { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Booking> Bookings { get; set; }

        public long InstructorId { get; set; }

        public long? AssistantId { get; set; }
        public void UpdateCustomProperties()
        {
            InstructorId = Assigns.First().InstructorId;
            AssistantId = Assigns.First().AssistantId;
        }
    }
}

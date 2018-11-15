namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AdminTask")]
    public partial class AdminTask
    {
        public long Id { get; set; }

        public long AdminId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        public byte Priority { get; set; }

        public byte Status { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }

        [Column(TypeName = "date")]
        public DateTime CreateDate { get; set; }

        public virtual Admin Admin { get; set; }
    }
}

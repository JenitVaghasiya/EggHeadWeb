namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AdminEmailTemplate")]
    public partial class AdminEmailTemplate
    {
        public int Id { get; set; }

        public long AdminId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string MailSubject { get; set; }

        [Required]
        public string MailBody { get; set; }

        public virtual Admin Admin { get; set; }
    }
}

namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PrivateMessage")]
    public partial class PrivateMessage
    {
        public long Id { get; set; }

        public long? FromAdminId { get; set; }

        public long? ToAdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string MessageSubject { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string MessageContent { get; set; }

        public DateTime SendDate { get; set; }

        public bool Unread { get; set; }

        [StringLength(50)]
        public string FromAdminName { get; set; }

        [StringLength(50)]
        public string ToAdminName { get; set; }

        public virtual Admin Admin { get; set; }

        public virtual Admin Admin1 { get; set; }
    }
}

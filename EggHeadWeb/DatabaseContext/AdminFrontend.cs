namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AdminFrontend")]
    public partial class AdminFrontend
    {
        public int Id { get; set; }

        public long AdminId { get; set; }

        public int? FrontendId { get; set; }

        [StringLength(50)]
        public string MenuName { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string OverridePageContent { get; set; }

        public virtual Admin Admin { get; set; }

        public virtual Frontend Frontend { get; set; }
    }
}

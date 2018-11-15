namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("APVariable")]
    public partial class APVariable
    {
        [Key]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }
    }
}

namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AdminPaymentInfo")]
    public partial class AdminPaymentInfo
    {
        public long Id { get; set; }

        public long AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string APILoginID { get; set; }

        [Required]
        [StringLength(100)]
        public string TransactionKey { get; set; }

        [StringLength(100)]
        public string MD5HashPhrase { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public virtual Admin Admin { get; set; }
    }
}

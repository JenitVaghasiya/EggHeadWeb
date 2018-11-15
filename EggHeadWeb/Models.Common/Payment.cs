namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Payment")]
    public partial class Payment
    {
        public long Id { get; set; }

        public long? AdminId { get; set; }

        public long ParentId { get; set; }

        public decimal Amount { get; set; }

        [StringLength(50)]
        public string AuthCode { get; set; }

        [StringLength(50)]
        public string TransactionID { get; set; }

        [StringLength(100)]
        public string ServiceName { get; set; }

        [Column(TypeName = "text")]
        public string PaymentMessage { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(50)]
        public string Bill_FirstName { get; set; }

        [StringLength(50)]
        public string Bill_LastName { get; set; }

        [StringLength(100)]
        public string Bill_Address { get; set; }

        [StringLength(100)]
        public string Bill_City { get; set; }

        [StringLength(50)]
        public string Bill_Zip { get; set; }

        [StringLength(50)]
        public string Bill_State { get; set; }

        [StringLength(50)]
        public string Bill_Email { get; set; }
    }
}

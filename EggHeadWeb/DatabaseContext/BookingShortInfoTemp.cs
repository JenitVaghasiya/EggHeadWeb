namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BookingShortInfoTemp")]
    public partial class BookingShortInfoTemp
    {
        public long Id { get; set; }

        public long? StudentId { get; set; }

        public long? classId { get; set; }

        [StringLength(50)]
        public string ServiceType { get; set; }

        [StringLength(250)]
        public string Name { get; set; }

        public decimal? Cost { get; set; }

        [StringLength(500)]
        public string Dates { get; set; }

        public long? CouponId { get; set; }
    }
}

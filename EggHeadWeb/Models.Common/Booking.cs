namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Booking")]
    public partial class Booking
    {
        public long Id { get; set; }

        public long? CampId { get; set; }

        public long? ClassId { get; set; }

        public long? WorkshopId { get; set; }

        public long? BirthdayId { get; set; }

        public long StudentId { get; set; }

        public DateTime BookDate { get; set; }

        public long? CouponId { get; set; }

        public virtual Birthday Birthday { get; set; }

        public virtual Camp Camp { get; set; }

        public virtual Class Class { get; set; }

        public virtual Coupon Coupon { get; set; }

        public virtual Student Student { get; set; }

        public virtual Workshop Workshop { get; set; }
    }
}

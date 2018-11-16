namespace EggHeadWeb.DatabaseContext
{
    using EggheadWeb.Models.Common;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Assign")]
    public partial class Assign
    {
        public long Id { get; set; }

        public DateTime Date { get; set; }

        public long InstructorId { get; set; }

        public long? AssistantId { get; set; }

        public long? ClassId { get; set; }

        public long? CampId { get; set; }

        public long? BirthdayId { get; set; }

        public long? WorkshopId { get; set; }

        public virtual Birthday Birthday { get; set; }

        public virtual Camp Camp { get; set; }

        public virtual Class Class { get; set; }

        public virtual Instructor Instructor { get; set; }

        public virtual Instructor Instructor1 { get; set; }

        public virtual Workshop Workshop { get; set; }

        public DateTime? NDate { get; set; }

        public TimeSpan TimeEnd
        {
            get
            {
                switch (Type)
                {
                    case ServiceType.Class:
                        {
                            if (Class == null)
                            {
                                return new TimeSpan();
                            }
                            return Class.TimeEnd;
                        }
                    case ServiceType.Camp:
                        {
                            if (Camp == null)
                            {
                                return new TimeSpan();
                            }
                            return Camp.TimeEnd;
                        }
                    case ServiceType.Workshop:
                        {
                            if (Workshop == null)
                            {
                                return new TimeSpan();
                            }
                            return Workshop.TimeEnd;
                        }
                    case ServiceType.Birthday:
                        {
                            if (Birthday == null)
                            {
                                return new TimeSpan();
                            }
                            TimeSpan partyTime = Birthday.PartyTime;
                            return partyTime.Add(new TimeSpan(1, 0, 0));
                        }
                }
                return new TimeSpan();
            }
        }

        public TimeSpan TimeStart
        {
            get
            {
                switch (Type)
                {
                    case ServiceType.Class:
                        {
                            if (Class == null)
                            {
                                return new TimeSpan();
                            }
                            return Class.TimeStart;
                        }
                    case ServiceType.Camp:
                        {
                            if (Camp == null)
                            {
                                return new TimeSpan();
                            }
                            return Camp.TimeStart;
                        }
                    case ServiceType.Workshop:
                        {
                            if (Workshop == null)
                            {
                                return new TimeSpan();
                            }
                            return Workshop.TimeStart;
                        }
                    case ServiceType.Birthday:
                        {
                            if (Birthday == null)
                            {
                                return new TimeSpan();
                            }
                            return Birthday.PartyTime;
                        }
                }
                return new TimeSpan();
            }
        }

        public ServiceType Type
        {
            get
            {
                if (ClassId.HasValue)
                {
                    return ServiceType.Class;
                }
                if (CampId.HasValue)
                {
                    return ServiceType.Camp;
                }
                if (BirthdayId.HasValue)
                {
                    return ServiceType.Birthday;
                }
                if (WorkshopId.HasValue)
                {
                    return ServiceType.Workshop;
                }
                return ServiceType.Birthday;
            }
        }

        public string Name { get; set; }
    }
}

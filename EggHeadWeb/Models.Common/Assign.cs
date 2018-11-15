using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(AssignValidator))]
	public class Assign
	{
		public long? AssistantId
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Birthday Birthday
		{
			get;
			set;
		}

		public long? BirthdayId
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Camp Camp
		{
			get;
			set;
		}

		public long? CampId
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Class Class
		{
			get;
			set;
		}

		public long? ClassId
		{
			get;
			set;
		}

		public DateTime Date
		{
			get;
			set;
		}

		public DateTime DateTimeEnd
		{
			get
			{
				return this.Date.Date.Add(this.TimeEnd);
			}
		}

		public DateTime DateTimeStart
		{
			get
			{
				return this.Date.Date.Add(this.TimeStart);
			}
		}

		public long Id
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Instructor Instructor
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Instructor Instructor1
		{
			get;
			set;
		}

		public long InstructorId
		{
			get;
			set;
		}

		public string Name
		{
			get
			{
				switch (this.Type)
				{
					case ServiceType.Class:
					{
						return this.Class.Name;
					}
					case ServiceType.Camp:
					{
						return this.Camp.Name;
					}
					case ServiceType.Workshop:
					{
						return this.Workshop.Name;
					}
					case ServiceType.Birthday:
					{
						return this.Birthday.ChildName;
					}
				}
				return string.Empty;
			}
		}

		public DateTime? NDate
		{
			get;
			set;
		}

		public TimeSpan TimeEnd
		{
			get
			{
				switch (this.Type)
				{
					case ServiceType.Class:
					{
						if (this.Class == null)
						{
							return new TimeSpan();
						}
						return this.Class.TimeEnd;
					}
					case ServiceType.Camp:
					{
						if (this.Camp == null)
						{
							return new TimeSpan();
						}
						return this.Camp.TimeEnd;
					}
					case ServiceType.Workshop:
					{
						if (this.Workshop == null)
						{
							return new TimeSpan();
						}
						return this.Workshop.TimeEnd;
					}
					case ServiceType.Birthday:
					{
						if (this.Birthday == null)
						{
							return new TimeSpan();
						}
						TimeSpan partyTime = this.Birthday.PartyTime;
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
				switch (this.Type)
				{
					case ServiceType.Class:
					{
						if (this.Class == null)
						{
							return new TimeSpan();
						}
						return this.Class.TimeStart;
					}
					case ServiceType.Camp:
					{
						if (this.Camp == null)
						{
							return new TimeSpan();
						}
						return this.Camp.TimeStart;
					}
					case ServiceType.Workshop:
					{
						if (this.Workshop == null)
						{
							return new TimeSpan();
						}
						return this.Workshop.TimeStart;
					}
					case ServiceType.Birthday:
					{
						if (this.Birthday == null)
						{
							return new TimeSpan();
						}
						return this.Birthday.PartyTime;
					}
				}
				return new TimeSpan();
			}
		}

		public ServiceType Type
		{
			get
			{
				if (this.ClassId.HasValue)
				{
					return ServiceType.Class;
				}
				if (this.CampId.HasValue)
				{
					return ServiceType.Camp;
				}
				if (this.BirthdayId.HasValue)
				{
					return ServiceType.Birthday;
				}
				if (this.WorkshopId.HasValue)
				{
					return ServiceType.Workshop;
				}
				return ServiceType.Birthday;
			}
		}

		public virtual EggheadWeb.Models.Common.Workshop Workshop
		{
			get;
			set;
		}

		public long? WorkshopId
		{
			get;
			set;
		}

		public Assign()
		{
		}
	}
}
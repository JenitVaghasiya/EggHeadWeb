using Google.GData.Calendar;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Contacts;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using EggHeadWeb.DatabaseContext;

namespace EggheadWeb.Utility
{
	public class GoogleCalendarHelper
	{
		private const string APPLICATION_NAME = "EggheadWeb";

		private string EGGHEAD_CALENDAR_ENTRY_NAME = "Egghead Academy";

		private CalendarService calendarService;

		private Uri calendarUri = new Uri("https://www.google.com/calendar/feeds/default/owncalendars/full");

		private readonly static ILog log;

		static GoogleCalendarHelper()
		{
			GoogleCalendarHelper.log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		}

		public GoogleCalendarHelper(string userName, string password)
		{
			this.calendarService = new CalendarService("EggheadWeb");
			this.calendarService.setUserCredentials(userName, password);
		}

		private void DeleteEventEntryForClass(string calendarId, string uniqueName, string id)
		{
			EventQuery query = new EventQuery(string.Format("https://www.google.com/calendar/feeds/{0}/private/full", calendarId));
			EventFeed feed = this.calendarService.Query(query);
			AtomFeed batchFeed = new AtomFeed(feed);
			foreach (EventEntry entry in feed.Entries)
			{
				if (!this.IsEventBelongToClass(entry, uniqueName, id))
				{
					continue;
				}
				EventEntry toDelete = entry;
				toDelete.Id = new AtomId(toDelete.EditUri.ToString());
				toDelete.BatchData = new GDataBatchEntryData("A", GDataBatchOperationType.delete);
				batchFeed.Entries.Add(toDelete);
			}
			EventFeed batchResultFeed = (EventFeed)this.calendarService.Batch(batchFeed, new Uri(feed.Batch));
			bool success = true;
			foreach (EventEntry entry in batchResultFeed.Entries)
			{
				if (entry.BatchData.Status.Code == 200 || entry.BatchData.Status.Code == 201)
				{
					continue;
				}
				success = false;
			}
			if (!success)
			{
				throw new HttpException("Get exception when connect Goolge Contact API ");
			}
			Console.WriteLine("All batch operations successful!");
		}

		public void DeleteScheduleEvents(string klassId, string campId, string birhdayId, string workshopId)
		{
			try
			{
				CalendarEntry eggheadCalendarEntry = null;
				CalendarQuery query = new CalendarQuery()
				{
					Uri = this.calendarUri
				};
				foreach (CalendarEntry calendar in this.calendarService.Query(query).Entries)
				{
					if (calendar.Title.Text != this.EGGHEAD_CALENDAR_ENTRY_NAME)
					{
						continue;
					}
					eggheadCalendarEntry = calendar;
					break;
				}
				if (eggheadCalendarEntry != null)
				{
					string calendarID = eggheadCalendarEntry.Id.Uri.Content.Substring(eggheadCalendarEntry.Id.Uri.Content.LastIndexOf("/") + 1);
					if (!string.IsNullOrEmpty(klassId))
					{
						this.DeleteEventEntryForClass(calendarID, "Egghead_ClassID", klassId);
					}
					if (!string.IsNullOrEmpty(campId))
					{
						this.DeleteEventEntryForClass(calendarID, "Egghead_CampID", campId);
					}
					if (!string.IsNullOrEmpty(birhdayId))
					{
						this.DeleteEventEntryForClass(calendarID, "Egghead_BirhdayID", birhdayId);
					}
					if (!string.IsNullOrEmpty(workshopId))
					{
						this.DeleteEventEntryForClass(calendarID, "Egghead_workshopID", workshopId);
					}
				}
			}
			catch (Exception exception)
			{
				GoogleCalendarHelper.log.Error(exception);
			}
		}

		private bool IsEventBelongToClass(EventEntry eventEntry, string uniqueName, string uniqueId)
		{
			ExtendedProperty extenProperty = eventEntry.ExtensionElements.OfType<ExtendedProperty>().FirstOrDefault<ExtendedProperty>();
			if (extenProperty == null)
			{
				return false;
			}
			if (extenProperty.Name != uniqueName)
			{
				return false;
			}
			return extenProperty.Value == uniqueId;
		}

		public void SynsScheduleEvents(Class klass, Camp camp, EggHeadWeb.DatabaseContext.Birthday birhday, Workshop workshop)
		{
			try
			{
				bool calendarExist = false;
				CalendarEntry eggheadCalendarEntry = null;
				CalendarQuery query = new CalendarQuery()
				{
					Uri = this.calendarUri
				};
				foreach (CalendarEntry calendar in this.calendarService.Query(query).Entries)
				{
					if (calendar.Title.Text != this.EGGHEAD_CALENDAR_ENTRY_NAME)
					{
						continue;
					}
					eggheadCalendarEntry = calendar;
					calendarExist = true;
					break;
				}
				if (!calendarExist)
				{
					eggheadCalendarEntry = new CalendarEntry();
					eggheadCalendarEntry.Title.Text = this.EGGHEAD_CALENDAR_ENTRY_NAME;
					eggheadCalendarEntry.TimeZone = System.TimeZone.CurrentTimeZone.ToString();
					eggheadCalendarEntry.Hidden = false;
					eggheadCalendarEntry.Color = "#2952A3";
					this.calendarService.Insert<CalendarEntry>(this.calendarUri, eggheadCalendarEntry);
				}
				string calendarID = eggheadCalendarEntry.Id.Uri.Content.Substring(eggheadCalendarEntry.Id.Uri.Content.LastIndexOf("/") + 1);
				if (klass != null)
				{
					long id = klass.Id;
					this.UpdateEventEntryForClass(calendarID, "Egghead_ClassID", id.ToString(), klass.Name, klass.TimeStart, klass.TimeEnd, klass.Location.DisplayName, klass.Assigns.ToList<Assign>());
				}
				if (camp != null)
				{
					long num = camp.Id;
					this.UpdateEventEntryForClass(calendarID, "Egghead_CampID", num.ToString(), camp.Name, camp.TimeStart, camp.TimeEnd, camp.Location.DisplayName, camp.Assigns.ToList<Assign>());
				}
				if (birhday != null)
				{
					string str = birhday.Id.ToString();
					string parentName = birhday.ParentName;
					TimeSpan partyTime = birhday.PartyTime;
					TimeSpan timeSpan = birhday.PartyTime;
					this.UpdateEventEntryForClass(calendarID, "Egghead_BirhdayID", str, parentName, partyTime, timeSpan.Add(new TimeSpan(1, 0, 0)), birhday.Address, birhday.Assigns.ToList());
				}
				if (workshop != null)
				{
					long id1 = workshop.Id;
					this.UpdateEventEntryForClass(calendarID, "Egghead_workshopID", id1.ToString(), workshop.Name, workshop.TimeStart, workshop.TimeEnd, workshop.Location.DisplayName, workshop.Assigns.ToList<Assign>());
				}
			}
			catch (Exception exception)
			{
				GoogleCalendarHelper.log.Error(exception);
			}
		}

		private void UpdateEventEntryForClass(string calendarId, string uniqueName, string id, string className, TimeSpan startTime, TimeSpan endTime, string location, List<Assign> assigns)
		{
			EventQuery query = new EventQuery(string.Format("https://www.google.com/calendar/feeds/{0}/private/full", calendarId));
			EventFeed feed = this.calendarService.Query(query);
			AtomFeed batchFeed = new AtomFeed(feed);
			foreach (EventEntry entry in feed.Entries)
			{
				if (!this.IsEventBelongToClass(entry, uniqueName, id))
				{
					continue;
				}
				EventEntry toDelete = entry;
				toDelete.Id = new AtomId(toDelete.EditUri.ToString());
				toDelete.BatchData = new GDataBatchEntryData("A", GDataBatchOperationType.delete);
				batchFeed.Entries.Add(toDelete);
			}
			foreach (Assign assign in assigns)
			{
				EventEntry toAdd = new EventEntry(className);
				ExtensionCollection<When> times = toAdd.Times;
				DateTime dateTime = assign.Date.Add(startTime);
				DateTime date = assign.Date;
				times.Add(new When(dateTime, date.Add(endTime)));
				toAdd.Locations.Add(new Where(string.Empty, string.Empty, location));
				ExtendedProperty extendedProperty = new ExtendedProperty()
				{
					Name = uniqueName,
					Value = id
				};
				toAdd.ExtensionElements.Add(extendedProperty);
				toAdd.BatchData = new GDataBatchEntryData("B", GDataBatchOperationType.insert);
				batchFeed.Entries.Add(toAdd);
			}
			EventFeed batchResultFeed = (EventFeed)this.calendarService.Batch(batchFeed, new Uri(feed.Batch));
			bool success = true;
			foreach (EventEntry entry in batchResultFeed.Entries)
			{
				if (entry.BatchData.Status.Code == 200 || entry.BatchData.Status.Code == 201)
				{
					continue;
				}
				success = false;
			}
			if (!success)
			{
				throw new HttpException("Get exception when connect Goolge Contact API ");
			}
			Console.WriteLine("All batch operations successful!");
		}
	}
}
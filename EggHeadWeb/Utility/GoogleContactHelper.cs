using EggheadWeb.Common;
using EggheadWeb.Models.Common;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;
using Google.GData.Extensions;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Utility
{
	public class GoogleContactHelper
	{
		private const string APPLICATION_NAME = "EggheadWeb";

		private string EGGHEAD_INSTRUCTOR_GROUP_NAME = "Egghead Instructors";

		private string EGGHEAD_LOCATION_GROUP_NAME = "Egghead Locations";

		private ContactsRequest contactRequest;

		private readonly static ILog log;

		static GoogleContactHelper()
		{
			GoogleContactHelper.log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		}

		public GoogleContactHelper(string userName, string password)
		{
			this.contactRequest = new ContactsRequest(new RequestSettings("EggheadWeb", userName, password));
		}

		public void DeleteContactFromInstructor(string instructorId)
		{
			try
			{
				this.DeleteGoogleContact(this.EGGHEAD_INSTRUCTOR_GROUP_NAME, instructorId);
			}
			catch (Exception exception)
			{
				GoogleContactHelper.log.Error(exception);
			}
		}

		public void DeleteContactFromLocation(string locationId)
		{
			try
			{
				this.DeleteGoogleContact(this.EGGHEAD_LOCATION_GROUP_NAME, locationId);
			}
			catch (Exception exception)
			{
				GoogleContactHelper.log.Error(exception);
			}
		}

		private void DeleteGoogleContact(string groupName, string uniqueId)
		{
			Contact deleteContact = null;
			Uri uri = new Uri(ContactsQuery.CreateContactsUri("default"));
			bool groupExist = false;
			Group eggheadInstrsGroup = null;
			foreach (Group g in this.contactRequest.GetGroups().Entries)
			{
				if (g.Title != groupName)
				{
					continue;
				}
				eggheadInstrsGroup = g;
				groupExist = true;
				break;
			}
			if (!groupExist)
			{
				return;
			}
			ContactsQuery query = new ContactsQuery(ContactsQuery.CreateContactsUri("default"))
			{
				Group = eggheadInstrsGroup.Id
			};
			foreach (Contact contact in this.contactRequest.Get<Contact>(query).Entries)
			{
				ExtendedProperty extendPros = contact.ExtendedProperties.FirstOrDefault<ExtendedProperty>((ExtendedProperty t) => t.Name == "Egghead_ID");
				if (extendPros == null || !(extendPros.Value == uniqueId))
				{
					continue;
				}
				deleteContact = contact;
				break;
			}
			if (deleteContact != null)
			{
				this.contactRequest.Delete<Contact>(deleteContact);
			}
		}

		public void SynContactFromInstructor(Instructor instructor)
		{
			try
			{
				Uri contactFeedUri = new Uri(ContactsQuery.CreateContactsUri("default"));
				bool groupExist = false;
				Group eggheadInstrsGroup = null;
				foreach (Group g in this.contactRequest.GetGroups().Entries)
				{
					if (g.Title != this.EGGHEAD_INSTRUCTOR_GROUP_NAME)
					{
						continue;
					}
					eggheadInstrsGroup = g;
					groupExist = true;
					break;
				}
				if (groupExist)
				{
					Contact updateContact = null;
					ContactsQuery query = new ContactsQuery(ContactsQuery.CreateContactsUri("default"))
					{
						Group = eggheadInstrsGroup.Id
					};
					foreach (Contact contact in this.contactRequest.Get<Contact>(query).Entries)
					{
						ExtendedProperty extendPros = contact.ExtendedProperties.FirstOrDefault<ExtendedProperty>((ExtendedProperty t) => t.Name == "Egghead_ID");
						if (extendPros == null || !(extendPros.Value == instructor.Id.ToString()))
						{
							continue;
						}
						updateContact = contact;
						break;
					}
					if (updateContact == null)
					{
						Contact newContact = new Contact();
						this.UpdateContactFromInstructor(instructor, newContact);
						ExtensionCollection<ExtendedProperty> extendedProperties = newContact.ExtendedProperties;
						long id = instructor.Id;
						extendedProperties.Add(new ExtendedProperty(id.ToString(), "Egghead_ID"));
						ExtensionCollection<GroupMembership> groupMembership = newContact.GroupMembership;
						GroupMembership groupMembership1 = new GroupMembership()
						{
							HRef = eggheadInstrsGroup.Id
						};
						groupMembership.Add(groupMembership1);
						this.contactRequest.Insert<Contact>(contactFeedUri, newContact);
					}
					else
					{
						this.UpdateContactFromInstructor(instructor, updateContact);
						this.contactRequest.Update<Contact>(updateContact);
					}
				}
				else
				{
					eggheadInstrsGroup = new Group()
					{
						Title = this.EGGHEAD_INSTRUCTOR_GROUP_NAME
					};
					eggheadInstrsGroup = this.contactRequest.Insert<Group>(new Uri(GroupsQuery.CreateGroupsUri("default")), eggheadInstrsGroup);
					Contact newContact = new Contact();
					this.UpdateContactFromInstructor(instructor, newContact);
					ExtensionCollection<ExtendedProperty> extendedProperties1 = newContact.ExtendedProperties;
					long num = instructor.Id;
					extendedProperties1.Add(new ExtendedProperty(num.ToString(), "Egghead_ID"));
					ExtensionCollection<GroupMembership> groupMemberships = newContact.GroupMembership;
					GroupMembership groupMembership2 = new GroupMembership()
					{
						HRef = eggheadInstrsGroup.Id
					};
					groupMemberships.Add(groupMembership2);
					this.contactRequest.Insert<Contact>(contactFeedUri, newContact);
				}
			}
			catch (Exception exception)
			{
				GoogleContactHelper.log.Error(exception);
			}
		}

		public void SynContactFromLocation(Location location)
		{
			try
			{
				Uri contactFeedUri = new Uri(ContactsQuery.CreateContactsUri("default"));
				bool groupExist = false;
				Group eggheadInstrsGroup = null;
				foreach (Group g in this.contactRequest.GetGroups().Entries)
				{
					if (g.Title != this.EGGHEAD_LOCATION_GROUP_NAME)
					{
						continue;
					}
					eggheadInstrsGroup = g;
					groupExist = true;
					break;
				}
				if (groupExist)
				{
					Contact updateContact = null;
					ContactsQuery query = new ContactsQuery(ContactsQuery.CreateContactsUri("default"))
					{
						Group = eggheadInstrsGroup.Id
					};
					foreach (Contact contact in this.contactRequest.Get<Contact>(query).Entries)
					{
						ExtendedProperty extendPros = contact.ExtendedProperties.FirstOrDefault<ExtendedProperty>((ExtendedProperty t) => t.Name == "Egghead_ID");
						if (extendPros == null || !(extendPros.Value == location.Id.ToString()))
						{
							continue;
						}
						updateContact = contact;
						break;
					}
					if (updateContact == null)
					{
						Contact newContact = new Contact();
						this.UpdateContactFromLocation(location, newContact);
						ExtensionCollection<ExtendedProperty> extendedProperties = newContact.ExtendedProperties;
						long id = location.Id;
						extendedProperties.Add(new ExtendedProperty(id.ToString(), "Egghead_ID"));
						ExtensionCollection<GroupMembership> groupMembership = newContact.GroupMembership;
						GroupMembership groupMembership1 = new GroupMembership()
						{
							HRef = eggheadInstrsGroup.Id
						};
						groupMembership.Add(groupMembership1);
						this.contactRequest.Insert<Contact>(contactFeedUri, newContact);
					}
					else
					{
						this.UpdateContactFromLocation(location, updateContact);
						this.contactRequest.Update<Contact>(updateContact);
					}
				}
				else
				{
					eggheadInstrsGroup = new Group()
					{
						Title = this.EGGHEAD_LOCATION_GROUP_NAME
					};
					eggheadInstrsGroup = this.contactRequest.Insert<Group>(new Uri(GroupsQuery.CreateGroupsUri("default")), eggheadInstrsGroup);
					Contact newContact = new Contact();
					this.UpdateContactFromLocation(location, newContact);
					ExtensionCollection<ExtendedProperty> extendedProperties1 = newContact.ExtendedProperties;
					long num = location.Id;
					extendedProperties1.Add(new ExtendedProperty(num.ToString(), "Egghead_ID"));
					ExtensionCollection<GroupMembership> groupMemberships = newContact.GroupMembership;
					GroupMembership groupMembership2 = new GroupMembership()
					{
						HRef = eggheadInstrsGroup.Id
					};
					groupMemberships.Add(groupMembership2);
					this.contactRequest.Insert<Contact>(contactFeedUri, newContact);
				}
			}
			catch (Exception exception)
			{
				GoogleContactHelper.log.Error(exception);
			}
		}

		private void UpdateContactFromInstructor(Instructor instructor, Contact newContact)
		{
			Name name = new Name()
			{
				FullName = StringUtil.GetFullName(instructor.FirstName, instructor.LastName),
				GivenName = instructor.FirstName,
				FamilyName = instructor.LastName
			};
			newContact.Name = name;
			newContact.Title = StringUtil.GetFullName(instructor.FirstName, instructor.LastName);
			newContact.Content = instructor.Note;
			newContact.Emails.Clear();
			ExtensionCollection<EMail> emails = newContact.Emails;
			EMail eMail = new EMail()
			{
				Address = instructor.Email,
				Primary = true,
				Rel = "http://schemas.google.com/g/2005#work"
			};
			emails.Add(eMail);
			newContact.Phonenumbers.Clear();
			ExtensionCollection<PhoneNumber> phonenumbers = newContact.Phonenumbers;
			PhoneNumber phoneNumber = new PhoneNumber()
			{
				Value = instructor.PhoneNumber,
				Primary = true,
				Rel = "http://schemas.google.com/g/2005#work"
			};
			phonenumbers.Add(phoneNumber);
			newContact.PostalAddresses.Clear();
			ExtensionCollection<StructuredPostalAddress> postalAddresses = newContact.PostalAddresses;
			StructuredPostalAddress structuredPostalAddress = new StructuredPostalAddress()
			{
				Rel = "http://schemas.google.com/g/2005#work",
				Primary = true,
				Street = instructor.Address,
				City = instructor.City,
				Region = instructor.State,
				Postcode = instructor.Zip,
				Country = "United States"
			};
			object[] address = new object[] { instructor.Address, instructor.City, instructor.State, instructor.Zip };
			structuredPostalAddress.FormattedAddress = string.Format("{0} {1} {2} {3}", address);
			postalAddresses.Add(structuredPostalAddress);
		}

		private void UpdateContactFromLocation(Location location, Contact newContact)
		{
			newContact.Name = new Name()
			{
				FullName = location.DisplayName
			};
			newContact.Title = location.DisplayName;
			newContact.Content = string.Concat(string.Format("Contact Person :  {0}", location.ContactPerson), Environment.NewLine, Environment.NewLine, location.Note);
			newContact.Emails.Clear();
			ExtensionCollection<EMail> emails = newContact.Emails;
			EMail eMail = new EMail()
			{
				Address = location.Email,
				Primary = true,
				Rel = "http://schemas.google.com/g/2005#work"
			};
			emails.Add(eMail);
			newContact.Phonenumbers.Clear();
			ExtensionCollection<PhoneNumber> phonenumbers = newContact.Phonenumbers;
			PhoneNumber phoneNumber = new PhoneNumber()
			{
				Value = location.PhoneNumber,
				Primary = true,
				Rel = "http://schemas.google.com/g/2005#work"
			};
			phonenumbers.Add(phoneNumber);
			newContact.PostalAddresses.Clear();
			ExtensionCollection<StructuredPostalAddress> postalAddresses = newContact.PostalAddresses;
			StructuredPostalAddress structuredPostalAddress = new StructuredPostalAddress()
			{
				Rel = "http://schemas.google.com/g/2005#work",
				Primary = true,
				Street = location.Address,
				City = location.City,
				Region = location.State,
				Postcode = location.Zip,
				Country = "United States"
			};
			object[] address = new object[] { location.Address, location.City, location.State, location.Zip };
			structuredPostalAddress.FormattedAddress = string.Format("{0} {1} {2} {3}", address);
			postalAddresses.Add(structuredPostalAddress);
		}
	}
}
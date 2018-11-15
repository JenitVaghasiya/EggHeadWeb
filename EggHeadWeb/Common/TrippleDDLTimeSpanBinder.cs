using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace EggheadWeb.Common
{
	public class TrippleDDLTimeSpanBinder : DefaultModelBinder
	{
		public TrippleDDLTimeSpanBinder()
		{
		}

		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			DateTime date;
			ModelMetadata metadata = bindingContext.ModelMetadata;
			TrippleDDLTimeSpanAttribute timeSpanDdl = metadata.ContainerType.GetProperty(metadata.PropertyName).GetCustomAttributes(typeof(TrippleDDLTimeSpanAttribute), true).FirstOrDefault<object>() as TrippleDDLTimeSpanAttribute;
			if (timeSpanDdl == null)
			{
				MetadataTypeAttribute metaDataType = metadata.ContainerType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).OfType<MetadataTypeAttribute>().FirstOrDefault<MetadataTypeAttribute>();
				if (metaDataType != null && metaDataType.MetadataClassType.GetProperty(metadata.PropertyName) != null)
				{
					timeSpanDdl = metaDataType.MetadataClassType.GetProperty(metadata.PropertyName).GetCustomAttributes(typeof(TrippleDDLTimeSpanAttribute), true).FirstOrDefault<object>() as TrippleDDLTimeSpanAttribute;
				}
				if (timeSpanDdl == null)
				{
					return base.BindModel(controllerContext, bindingContext);
				}
			}
			string prefix = bindingContext.ModelName;
			ValueProviderResult value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			string[] parts = value.RawValue as string[];
			if (parts.All<string>(new Func<string, bool>(string.IsNullOrEmpty)))
			{
				return null;
			}
			bindingContext.ModelState.SetModelValue(prefix, value);
			if (DateTime.TryParseExact(string.Format("{0}:{1} {2}", parts[0], parts[1], parts[2]), "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
			{
				return date.TimeOfDay;
			}
			bindingContext.ModelState.AddModelError(prefix, timeSpanDdl.ErrorMessage);
			return null;
		}
	}
}
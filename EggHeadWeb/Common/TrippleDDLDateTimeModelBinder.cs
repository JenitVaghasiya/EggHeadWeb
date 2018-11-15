using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace EggheadWeb.Common
{
	public class TrippleDDLDateTimeModelBinder : DefaultModelBinder
	{
		public TrippleDDLDateTimeModelBinder()
		{
		}

		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			DateTime date;
			ModelMetadata metadata = bindingContext.ModelMetadata;
			TrippleDDLDateTimeAttribute trippleDdl = metadata.ContainerType.GetProperty(metadata.PropertyName).GetCustomAttributes(typeof(TrippleDDLDateTimeAttribute), true).FirstOrDefault<object>() as TrippleDDLDateTimeAttribute;
			if (trippleDdl == null)
			{
				MetadataTypeAttribute metaDataType = metadata.ContainerType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).OfType<MetadataTypeAttribute>().FirstOrDefault<MetadataTypeAttribute>();
				if (metaDataType != null && metaDataType.MetadataClassType.GetProperty(metadata.PropertyName) != null)
				{
					trippleDdl = metaDataType.MetadataClassType.GetProperty(metadata.PropertyName).GetCustomAttributes(typeof(TrippleDDLDateTimeAttribute), true).FirstOrDefault<object>() as TrippleDDLDateTimeAttribute;
				}
				if (trippleDdl == null)
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
			if (DateTime.TryParseExact(string.Format("{0}-{1}-{2}", parts[0], parts[1], parts[2]), "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
			{
				return date;
			}
			bindingContext.ModelState.AddModelError(prefix, trippleDdl.ErrorMessage);
			return null;
		}
	}
}
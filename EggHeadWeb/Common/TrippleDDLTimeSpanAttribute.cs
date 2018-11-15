using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web.Mvc;

namespace EggheadWeb.Common
{
	public class TrippleDDLTimeSpanAttribute : ValidationAttribute, IMetadataAware, IClientValidatable
	{
		public TrippleDDLTimeSpanAttribute()
		{
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			ModelClientValidationRule modelClientValidationRule = new ModelClientValidationRule();
			modelClientValidationRule.ErrorMessage = ErrorMessage;
			modelClientValidationRule.ValidationType = "timespanddl";
			yield return modelClientValidationRule;
		}

		public override bool IsValid(object value)
		{
			return true;
		}

		public void OnMetadataCreated(ModelMetadata metadata)
		{
			metadata.TemplateHint = "TrippleDDLTimeSpan";
		}
	}
}
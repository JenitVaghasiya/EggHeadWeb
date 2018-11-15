using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[MetadataType(typeof(APVariableAttr))]
	[Validator(typeof(APVariableValidator))]
	public class APVariable
	{
		public string Name
		{
			get;
			set;
		}

		public string Value
		{
			get;
			set;
		}

		public APVariable()
		{
		}
	}
}
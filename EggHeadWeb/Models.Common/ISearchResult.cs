using System;

namespace EggheadWeb.Models.Common
{
	public interface ISearchResult
	{
		int CurrentPage
		{
			get;
		}

		int FirstIndex
		{
			get;
		}

		bool HasNextPage
		{
			get;
		}

		bool HasPrevPage
		{
			get;
		}

		bool IsPaging
		{
			get;
		}

		int ItemsPerPage
		{
			get;
		}

		int LastIndex
		{
			get;
		}

		int TotalItems
		{
			get;
		}

		int TotalPages
		{
			get;
		}
	}
}
using System;
using System.Linq;

namespace EggheadWeb.Models.Common
{
	public static class SearchResult
	{
		public static SearchResult<object, TItem> New<TItem>(IQueryable<TItem> items, int page, int itemsPerPage)
		{
			return SearchResult.New<object, TItem>(null, items, page, itemsPerPage);
		}

		public static SearchResult<object, TItem> New<TItem>(IQueryable<TItem> items, int page)
		{
			return SearchResult.New<object, TItem>(null, items, page);
		}

		public static SearchResult<TForm, TItem> New<TForm, TItem>(TForm form, IQueryable<TItem> item, int page, int itemsPerPage)
		{
			return new SearchResult<TForm, TItem>(form, item, page, itemsPerPage);
		}

		public static SearchResult<TForm, TItem> New<TForm, TItem>(TForm form, IQueryable<TItem> item, int page)
		{
			return new SearchResult<TForm, TItem>(form, item, page);
		}

		public static SearchResult<TForm, TItem> New<TForm, TItem>(TForm form, IQueryable<TItem> item)
		{
			return new SearchResult<TForm, TItem>(form, item);
		}

		public static SearchResult<TForm, TItem> New<TForm, TKey, TItem>(TForm form, IQueryable<TKey> query, int page, int itemsPerPage, Func<TKey, TItem> convert)
		{
			return (new SearchResult<TForm, TKey>(form, query, page, itemsPerPage)).CastAs<TItem>(convert);
		}

		public static SearchResult<TForm, TItem> New<TForm, TKey, TItem>(TForm form, IQueryable<TKey> query, int page, Func<TKey, TItem> convert)
		{
			return (new SearchResult<TForm, TKey>(form, query, page)).CastAs<TItem>(convert);
		}

		public static SearchResult<TForm, TItem> New<TForm, TKey, TItem>(TForm form, IQueryable<TKey> query, Func<TKey, TItem> convert)
		{
			return (new SearchResult<TForm, TKey>(form, query)).CastAs<TItem>(convert);
		}
	}
}
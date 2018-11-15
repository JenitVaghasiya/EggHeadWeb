using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class SearchResult<TForm, TItem> : ISearchResult
	{
		public const int DEFAULT_ITEMS_PER_PAGE = 10;

		public int CurrentPage
		{
			get
			{
				return JustDecompileGenerated_get_CurrentPage();
			}
			set
			{
				JustDecompileGenerated_set_CurrentPage(value);
			}
		}

		private int JustDecompileGenerated_CurrentPage_k__BackingField;

		public int JustDecompileGenerated_get_CurrentPage()
		{
			return this.JustDecompileGenerated_CurrentPage_k__BackingField;
		}

		private void JustDecompileGenerated_set_CurrentPage(int value)
		{
			this.JustDecompileGenerated_CurrentPage_k__BackingField = value;
		}

		public int FirstIndex
		{
			get
			{
				return (this.CurrentPage - 1) * this.ItemsPerPage + 1;
			}
		}

		public TForm Form
		{
			get;
			private set;
		}

		public bool HasNextPage
		{
			get
			{
				return this.CurrentPage < this.TotalPages;
			}
		}

		public bool HasPrevPage
		{
			get
			{
				return this.CurrentPage > 1;
			}
		}

		public bool IsPaging
		{
			get
			{
				return this.TotalItems > this.ItemsPerPage;
			}
		}

		public int ItemsPerPage
		{
			get
			{
				return JustDecompileGenerated_get_ItemsPerPage();
			}
			set
			{
				JustDecompileGenerated_set_ItemsPerPage(value);
			}
		}

		private int JustDecompileGenerated_ItemsPerPage_k__BackingField;

		public int JustDecompileGenerated_get_ItemsPerPage()
		{
			return this.JustDecompileGenerated_ItemsPerPage_k__BackingField;
		}

		private void JustDecompileGenerated_set_ItemsPerPage(int value)
		{
			this.JustDecompileGenerated_ItemsPerPage_k__BackingField = value;
		}

		public int LastIndex
		{
			get
			{
				return this.CurrentPage * this.ItemsPerPage;
			}
		}

		public List<TItem> PageItems
		{
			get;
			private set;
		}

		public int TotalItems
		{
			get
			{
				return JustDecompileGenerated_get_TotalItems();
			}
			set
			{
				JustDecompileGenerated_set_TotalItems(value);
			}
		}

		private int JustDecompileGenerated_TotalItems_k__BackingField;

		public int JustDecompileGenerated_get_TotalItems()
		{
			return this.JustDecompileGenerated_TotalItems_k__BackingField;
		}

		private void JustDecompileGenerated_set_TotalItems(int value)
		{
			this.JustDecompileGenerated_TotalItems_k__BackingField = value;
		}

		public int TotalPages
		{
			get
			{
				return (int)Math.Ceiling(1 * (double)this.TotalItems / (double)this.ItemsPerPage);
			}
		}

		private SearchResult()
		{
		}

		public SearchResult(TForm form, IQueryable<TItem> query, int page, int itemsPerPage)
		{
			this.Form = form;
			this.CurrentPage = page;
			this.ItemsPerPage = itemsPerPage;
			this.TotalItems = query.Count<TItem>();
			this.PageItems = query.Skip<TItem>(this.FirstIndex - 1).Take<TItem>(this.ItemsPerPage).ToList<TItem>();
		}

		public SearchResult(TForm form, IQueryable<TItem> data) : this(form, data, 1, 10)
		{
		}

		public SearchResult(TForm form, IQueryable<TItem> data, int page) : this(form, data, page, 10)
		{
		}

		public SearchResult<TForm, TNewItem> CastAs<TNewItem>(Func<TItem, TNewItem> convert)
		{
			SearchResult<TForm, TNewItem> searchResult = new SearchResult<TForm, TNewItem>()
			{
				TotalItems = this.TotalItems,
				ItemsPerPage = this.ItemsPerPage,
				CurrentPage = this.CurrentPage,
				PageItems = new List<TNewItem>()
			};
			SearchResult<TForm, TNewItem> result = searchResult;
			foreach (TItem keyData in this.PageItems)
			{
				result.PageItems.Add(convert(keyData));
			}
			return result;
		}
	}
}
using EggheadWeb.Models.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace EggheadWeb.DAL
{
	public class GenericRepository<TEntity>
	where TEntity : class
	{
		internal EggheadEntities context;

		internal DbSet<TEntity> dbSet;

		public GenericRepository(EggheadEntities context)
		{
			this.context = context;
			this.dbSet = context.Set<TEntity>();
		}

		public virtual void Delete(object id)
		{
			DbSet<TEntity> tEntities = this.dbSet;
			object[] objArray = new object[] { id };
			this.Delete(tEntities.Find(objArray));
		}

		public virtual void Delete(TEntity entityToDelete)
		{
			if (this.context.Entry<TEntity>(entityToDelete).State == EntityState.Detached)
			{
				this.dbSet.Attach(entityToDelete);
			}
			this.dbSet.Remove(entityToDelete);
		}

		public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
		{
			IQueryable<TEntity> query = this.dbSet;
			if (filter != null)
			{
				query = query.Where<TEntity>(filter);
			}
			char[] chrArray = new char[] { ',' };
			string[] strArrays = includeProperties.Split(chrArray, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				query = query.Include<TEntity>(strArrays[i]);
			}
			if (orderBy == null)
			{
				return query.ToList<TEntity>();
			}
			return orderBy(query).ToList<TEntity>();
		}

		public virtual TEntity GetByID(object id)
		{
			return this.dbSet.Find(new object[] { id });
		}

		public virtual void Insert(TEntity entity)
		{
			this.dbSet.Add(entity);
		}

		public virtual void Update(TEntity entityToUpdate)
		{
			this.dbSet.Attach(entityToUpdate);
			this.context.Entry<TEntity>(entityToUpdate).State = EntityState.Modified;
		}
	}
}
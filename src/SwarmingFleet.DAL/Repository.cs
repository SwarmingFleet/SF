
namespace SwarmingFleet.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class Repository<TDbContext, TKey, TEntity> : IRepository<TKey, TEntity>
        where TDbContext : DbContext
        where TKey : struct
        where TEntity : class, IKeyed<TKey>
    {
        public DbContext Context { get; }

        public Repository(TDbContext context)
        {
            this.Context = context;
        } 

        public TKey Create(TEntity entity)
        {
            this.Context.Set<TEntity>().Add(entity);
            this.Context.SaveChanges();
            return entity.Id;
        }

        public void Update(TEntity entity)
        {
            this.Context.Entry(entity).State = EntityState.Modified;
            this.Context.SaveChanges();
        }

        public bool DeleteById(TKey id)
        {
            var result = this.FindById(id, out var entity);
            if (result)
            {
                this.Context.Set<TEntity>().Remove(entity);
                this.Context.SaveChanges();
            }
            return result;
        }

        public void Delete(TEntity entity)
        {
            this.Context.Set<TEntity>().Remove(entity);
            this.Context.SaveChanges();
        }


        public IQueryable<TEntity> Retrieve(Expression<Func<TEntity, bool>> predicate)
        {
            return this.Context.Set<TEntity>().Where(predicate);
        }

        public bool FindById(TKey id, out TEntity entity)
        {
            return this.Find(x => x.Id.Equals(id), out entity);
        }

        public bool Find(Expression<Func<TEntity, bool>> predicate, out TEntity entity)
        {
            var set = this.Context.Set<TEntity>();
            entity = set.FirstOrDefault(predicate);
            return entity is TEntity;
        }

        public IQueryable<TEntity> Retrieve()
        {
            return this.Context.Set<TEntity>().AsQueryable();
        }

    }
}
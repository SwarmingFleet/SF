using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SwarmingFleet.Broker.DataAccessLayers
{
    public interface IRepository<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IKeyed<TKey>
    {
        DbContext Context { get; }

        TKey Create(TEntity entity);
        void Delete(TEntity entity);
        bool DeleteById(TKey id);
        bool Find(Expression<Func<TEntity, bool>> predicate, out TEntity entity);
        bool FindById(TKey id, out TEntity entity);
        IQueryable<TEntity> Retrieve();
        IQueryable<TEntity> Retrieve(Expression<Func<TEntity, bool>> predicate);
        void Update(TEntity entity);
    }
}
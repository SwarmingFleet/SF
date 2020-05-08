
namespace SwarmingFleet.DAL
{ 
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    using System.Data.Entity;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// 實作Entity Framework Generic Repository 的 Class。
    /// </summary>
    /// <typeparam name="TEntity">EF Model 裡面的Type</typeparam>
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private DbContext _context;

        /// <summary>
        /// 建構EF一個Entity的Repository，需傳入此Entity的Context。
        /// </summary>
        /// <param name="inContext">Entity所在的Context</param>
        public Repository(DbContext inContext)
        {
            _context = inContext;
        }

        /// <summary>
        /// 新增一筆資料到資料庫。
        /// </summary>
        /// <param name="entity">要新增到資料的庫的Entity</param>
        public void Create(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        /// <summary>
        /// 取得第一筆符合條件的內容。如果符合條件有多筆，也只取得第一筆。
        /// </summary>
        /// <param name="predicate">要取得的Where條件。</param>
        /// <returns>取得第一筆符合條件的內容。</returns>
        public TEntity Retrieve(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Where(predicate).FirstOrDefault();
        }

        /// <summary>
        /// 取得Entity全部筆數的IQueryable。
        /// </summary>
        /// <returns>Entity全部筆數的IQueryable。</returns>
        public IQueryable<TEntity> Reads()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        /// <summary>
        /// 更新一筆Entity內容。
        /// </summary>
        /// <param name="entity">要更新的內容</param>
        public void Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// 更新一筆Entity的內容。只更新有指定的Property。
        /// </summary>
        /// <param name="entity">要更新的內容。</param>
        /// <param name="updateProperties">需要更新的欄位。</param>
        public void Update(TEntity entity, Expression<Func<TEntity, object>>[] updateProperties)
        {
            _context.Configuration.ValidateOnSaveEnabled = false;

            _context.Entry(entity).State = EntityState.Unchanged;

            if (updateProperties != null)
            {
                foreach (var property in updateProperties)
                {
                    _context.Entry(entity).Property(property).IsModified = true;
                }
            }
        }

        /// <summary>
        /// 刪除一筆資料內容。
        /// </summary>
        /// <param name="entity">要被刪除的Entity。</param>
        public void Delete(TEntity entity)
        {
            _context.Entry<TEntity>(entity).State = EntityState.Deleted;
        }

        /// <summary>
        /// 儲存異動。
        /// </summary>
        public void SaveChanges()
        {
            _context.SaveChanges();

            // 因為Update 單一model需要先關掉validation，因此重新打開
            if (!_context.Configuration.ValidateOnSaveEnabled)
            {
                _context.Configuration.ValidateOnSaveEnabled = true;
            }
        }
    }
}
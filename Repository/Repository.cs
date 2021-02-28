using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly La3bniContext la3BniContext;
        private readonly DbSet<TEntity> dbSet;

        public Repository(La3bniContext _la3BniContext)
        {
            la3BniContext = _la3BniContext;
            dbSet = la3BniContext.Set<TEntity>();
        }

        public virtual void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public virtual void AddRange(List<TEntity> entities)
        {
            dbSet.AddRange(entities);
        }

        public virtual void Delete(TEntity entity)
        {
            la3BniContext.Entry(entity).State = EntityState.Deleted;
        }

        public void Delete(List<TEntity> entities)
        {
            foreach (TEntity item in entities)
                la3BniContext.Entry(item).State = EntityState.Deleted;
        }

        public virtual async Task<TEntity> Find(Expression<Func<TEntity, bool>> wherePredict)
        {
            return await dbSet.FirstOrDefaultAsync(wherePredict);
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return dbSet;
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            la3BniContext.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}
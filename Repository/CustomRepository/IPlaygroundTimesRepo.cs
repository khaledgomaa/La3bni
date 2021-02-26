using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.CustomRepository
{
    public interface IPlaygroundTimesRepo
    {
        public Task<PlaygroundTimes> FindWithInclude(Expression<Func<PlaygroundTimes, bool>> wherePredict);

        public IQueryable<PlaygroundTimes> GetAllIQueryable();
    }
}
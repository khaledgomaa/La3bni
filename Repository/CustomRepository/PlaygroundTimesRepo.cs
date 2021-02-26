using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.CustomRepository
{
    public class PlaygroundTimesRepo : Repository<PlaygroundTimes>, IPlaygroundTimesRepo
    {
        private readonly La3bniContext la3BniContext;

        public PlaygroundTimesRepo(La3bniContext _la3BniContext) : base(_la3BniContext)
        {
            la3BniContext = _la3BniContext;
        }

        public async Task<PlaygroundTimes> FindWithInclude(Expression<Func<PlaygroundTimes, bool>> wherePredict)
        {
            return await la3BniContext.PlaygroundTimes.Include(b => b.Playground).FirstOrDefaultAsync(wherePredict);
        }

        public IQueryable<PlaygroundTimes> GetAllIQueryable()
        {
            return la3BniContext.PlaygroundTimes;
        }
    }
}
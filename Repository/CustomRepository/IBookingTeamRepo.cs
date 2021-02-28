using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.CustomRepository
{
    public interface IBookingTeamRepo
    {
        public Task<BookingTeam> FindWithInclude(Expression<Func<BookingTeam, bool>> wherePredict);

        public IQueryable<BookingTeam> GetAllIQueryable();

        public IQueryable<BookingTeam> GetAllIQueryableWithInclude();
    }
}
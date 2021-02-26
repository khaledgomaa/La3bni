using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IBookingRepository
{
    public interface IBookingRepository
    {
        public Task<List<Booking>> GetAllWithInclude();

        public Task<Booking> FindWithInclude(Expression<Func<Booking, bool>> wherePredict);
    }
}
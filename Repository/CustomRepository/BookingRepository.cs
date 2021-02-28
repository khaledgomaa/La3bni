using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IBookingRepository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly La3bniContext la3BniContext;

        public BookingRepository(La3bniContext _la3BniContext) : base(_la3BniContext)
        {
            la3BniContext = _la3BniContext;
        }

        public IQueryable<Booking> GetAllWithInclude()
        {
            return la3BniContext.Bookings.Include(b => b.ApplicationUser)
                                        .Include(b => b.Playground)
                                        .Include(b => b.PlaygroundTimes);
        }

        public async Task<Booking> FindWithInclude(Expression<Func<Booking, bool>> wherePredict)
        {
            return await la3BniContext.Bookings.Include(b => b.ApplicationUser)
                                                .Include(b => b.Playground)
                                                 .Include(b => b.PlaygroundTimes)
                                                 .FirstOrDefaultAsync(wherePredict);
        }
    }
}
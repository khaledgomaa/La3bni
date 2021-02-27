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
    public class BookingTeamRepo : Repository<BookingTeam>, IBookingTeamRepo
    {
        private readonly La3bniContext la3BniContext;

        public BookingTeamRepo(La3bniContext _la3BniContext) : base(_la3BniContext)
        {
            la3BniContext = _la3BniContext;
        }

        public Task<BookingTeam> FindWithInclude(Expression<Func<BookingTeam, bool>> wherePredict)
        {
            return la3BniContext.BookingTeams.Include(b => b.ApplicationUser)
                                             .Include(b => b.Booking)
                                             .Include(b => b.Booking.Playground)
                                             .Include(b => b.Booking.PlaygroundTimes)
                                             .FirstOrDefaultAsync(wherePredict);
        }

        public IQueryable<BookingTeam> GetAllIQueryable()
        {
            return la3BniContext.BookingTeams;
        }

        public IQueryable<BookingTeam> GetAllIQueryableWithInclude()
        {
            return la3BniContext.BookingTeams.Include(b => b.Booking)
                                             .Include(b => b.Booking.Playground)
                                             .Include(b => b.Booking.PlaygroundTimes);
        }
    }
}
using Models;
using Repository.CustomRepository;
using Repository.IBookingRepository;

namespace Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly La3bniContext la3BniContext;
        private Repository<Playground> playGroundRepo;
        private BookingRepository bookingRepo;
        private BookingTeamRepo bookingTeamRepo;
        private Repository<FeedBack> feedBackRepo;
        private Repository<Notification> notificationRepo;
        private Repository<PlaygroundRate> playgroundRateRepo;
        private PlaygroundTimesRepo playgroundTimesRepo;
        private Repository<Subscriber> subscriberRepo;

        public UnitOfWork(La3bniContext _la3BniContext)
        {
            la3BniContext = _la3BniContext;
        }

        public Repository<Playground> PlayGroundRepo
        {
            get
            {
                if (playGroundRepo == null)
                    playGroundRepo = new Repository<Playground>(la3BniContext);
                return playGroundRepo;
            }
        }

        public BookingRepository BookingRepo
        {
            get
            {
                if (bookingRepo == null)
                    bookingRepo = new BookingRepository(la3BniContext);
                return bookingRepo;
            }
        }

        public BookingTeamRepo BookingTeamRepo
        {
            get
            {
                if (bookingTeamRepo == null)
                    bookingTeamRepo = new BookingTeamRepo(la3BniContext);
                return bookingTeamRepo;
            }
        }

        public Repository<FeedBack> FeedBackRepo
        {
            get
            {
                if (feedBackRepo == null)
                    feedBackRepo = new Repository<FeedBack>(la3BniContext);
                return feedBackRepo;
            }
        }

        public Repository<Notification> NotificationRepo
        {
            get
            {
                if (notificationRepo == null)
                    notificationRepo = new Repository<Notification>(la3BniContext);
                return notificationRepo;
            }
        }

        public Repository<PlaygroundRate> PlaygroundRateRepo
        {
            get
            {
                if (playgroundRateRepo == null)
                    playgroundRateRepo = new Repository<PlaygroundRate>(la3BniContext);
                return playgroundRateRepo;
            }
        }

        public PlaygroundTimesRepo PlaygroundTimesRepo
        {
            get
            {
                if (playgroundTimesRepo == null)
                    playgroundTimesRepo = new PlaygroundTimesRepo(la3BniContext);
                return playgroundTimesRepo;
            }
        }

        public Repository<Subscriber> SubscriberRepo
        {
            get
            {
                if (subscriberRepo == null)
                    subscriberRepo = new Repository<Subscriber>(la3BniContext);
                return subscriberRepo;
            }
        }

        public void Dispose()
        {
            la3BniContext.Dispose();
        }

        public int Save()
        {
            return la3BniContext.SaveChanges();
        }
    }
}
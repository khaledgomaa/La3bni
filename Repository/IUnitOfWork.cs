using Models;
using Repository.CustomRepository;
using Repository.IBookingRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public interface IUnitOfWork : IDisposable
    {
        public Repository<Playground> PlayGroundRepo { get; }

        public BookingRepository BookingRepo { get; }

        public BookingTeamRepo BookingTeamRepo { get; }

        public Repository<Subscriber> SubscriberRepo { get; }

        public Repository<PlaygroundRate> PlaygroundRateRepo { get; }

        public PlaygroundTimesRepo PlaygroundTimesRepo { get; }

        public Repository<FeedBack> FeedBackRepo { get; }

        public Repository<Notification> NotificationRepo { get; }

        int Save();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.Repository
{
    public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
        }
    }

    public class BookingTeamConfig : EntityConfiguration<BookingTeam>
    {
        public override void Configure(EntityTypeBuilder<BookingTeam> builder)
        {
            builder.HasKey(b => new { b.ApplicationUserId, b.BookingId });
        }
    }

    public class PlaygroundRatesConfig : EntityConfiguration<PlaygroundRate>
    {
        public override void Configure(EntityTypeBuilder<PlaygroundRate> builder)
        {
            builder.HasKey(s => new { s.ApplicationUserId, s.PlaygroundId });
        }
    }

    public class BookingConfig : EntityConfiguration<Booking>
    {
        public override void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasIndex(b => new { b.PlaygroundId, b.BookedDate, b.PlaygroundTimesId }).IsUnique();
        }
    }
}
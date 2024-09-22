using Gpt.Labs.Common;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gpt.Labs.Models
{
    public class DataContext : DbContext
    {
        #region Fields

        public const string ConnectionString = Constants.DatabaseBaseConnectionString;

#if DEBUG
        private static readonly ILoggerFactory DebugLoggerFactory = GetLoggerFactory();
#endif

        #endregion

        #region Properties

        public DbSet<OpenAIChat> Chats { get; set; }

        public DbSet<OpenAISettings> Settings { get; set; }

        public DbSet<OpenAIStop> Stops { get; set; }

        public DbSet<OpenAILogitBias> LogitBias { get; set; }

        public DbSet<OpenAIMessage> Messages { get; set; }

        #endregion

        #region Public Methods

        public void CleanUpDatabase()
        {
            //using (var transaction = this.Database.BeginTransaction())
            //{
            //    try
            //    {
            //        string commandToExecute = @" 
            //            DELETE FROM [Projects];
            //            DELETE FROM [ClassificationGroups];
            //            DELETE FROM [ClassificationGroupPresets];
            //            DELETE FROM [ClassificationCodes];
            //            DELETE FROM [ImageTypes];
            //            DELETE FROM [ImageRows];
            //            DELETE FROM [Comments];
            //            DELETE FROM [ReferenceClassificationCodes];
            //            DELETE FROM [ColorSchemes];
            //            DELETE FROM [Presets];
            //            DELETE FROM [Profiles]";

            //        await this.Database.ExecuteSqlCommandAsync(commandToExecute);

            //        transaction.Commit();
            //    }
            //    catch (Exception)
            //    {
            //        transaction.Rollback();

            //        throw;
            //    }
            //}
            throw new NotImplementedException();
        }

        public override int SaveChanges()
        {
            InitId();
            AddAuditInfo();

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            InitId();
            AddAuditInfo();

            return await base.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Private Methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OpenAIChat>().ToTable("Chats");
            modelBuilder.Entity<OpenAIChat>().HasIndex(p => new { p.Title }).IsUnique(false);
            modelBuilder.Entity<OpenAIChat>().HasIndex(p => new { p.Type }).IsUnique(false);
            modelBuilder.Entity<OpenAIChat>().Property(p => p.Position).HasDefaultValue(0);
            modelBuilder.Entity<OpenAIChat>().Property(p => p.CreatedDate).HasConversion(p => p, p => DateTime.SpecifyKind(p, DateTimeKind.Utc));
            modelBuilder.Entity<OpenAIChat>().Property(p => p.UpdatedDate).HasConversion(p => p, p => p != null ? DateTime.SpecifyKind(p.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<OpenAIChat>().HasOne(p => p.Settings).WithOne(p => p.Chat).IsRequired();

            modelBuilder.Entity<OpenAISettings>().ToTable("Settings")
                .HasDiscriminator(p => p.Type)
                .HasValue<OpenAIChatSettings>(OpenAIChatType.Chat)
                .HasValue<OpenAIImageSettings>(OpenAIChatType.Image);

            modelBuilder.Entity<OpenAISettings>().Property(p => p.N).HasDefaultValue(1);
            modelBuilder.Entity<OpenAISettings>().Property(p => p.CreatedDate).HasConversion(p => p, p => DateTime.SpecifyKind(p, DateTimeKind.Utc));
            modelBuilder.Entity<OpenAISettings>().Property(p => p.UpdatedDate).HasConversion(p => p, p => p != null ? DateTime.SpecifyKind(p.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.ModelId);
            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.N).HasDefaultValue(1);
            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.LastNMessagesToInclude).HasDefaultValue(5);
            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.Temperature).HasDefaultValue(1);
            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.TopP).HasDefaultValue(1);
            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.FrequencyPenalty).HasDefaultValue(0);
            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.PresencePenalty).HasDefaultValue(0);
            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.CreatedDate).HasConversion(p => p, p => DateTime.SpecifyKind(p, DateTimeKind.Utc));
            modelBuilder.Entity<OpenAIChatSettings>().Property(p => p.UpdatedDate).HasConversion(p => p, p => p != null ? DateTime.SpecifyKind(p.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<OpenAIImageSettings>().Property(p => p.ModelId);
            modelBuilder.Entity<OpenAIImageSettings>().Property(p => p.Size).HasDefaultValue(OpenAIImageSize.Large);

            modelBuilder.Entity<OpenAIStop>().ToTable("Stops");
            modelBuilder.Entity<OpenAIStop>().Property(p => p.CreatedDate).HasConversion(p => p, p => DateTime.SpecifyKind(p, DateTimeKind.Utc));
            modelBuilder.Entity<OpenAIStop>().Property(p => p.UpdatedDate).HasConversion(p => p, p => p != null ? DateTime.SpecifyKind(p.Value, DateTimeKind.Utc) : null);
            modelBuilder.Entity<OpenAIStop>().HasOne(p => (OpenAIChatSettings)p.Settings).WithMany(p => p.Stop).HasForeignKey(p => p.SettingsId);

            modelBuilder.Entity<OpenAILogitBias>().ToTable("LogitBias");
            modelBuilder.Entity<OpenAILogitBias>().Property(p => p.CreatedDate).HasConversion(p => p, p => DateTime.SpecifyKind(p, DateTimeKind.Utc));
            modelBuilder.Entity<OpenAILogitBias>().Property(p => p.UpdatedDate).HasConversion(p => p, p => p != null ? DateTime.SpecifyKind(p.Value, DateTimeKind.Utc) : null);
            modelBuilder.Entity<OpenAILogitBias>().HasOne(p => (OpenAIChatSettings)p.Settings).WithMany(p => p.LogitBias).HasForeignKey(p => p.SettingsId);

            modelBuilder.Entity<OpenAIMessage>().ToTable("Messages");
            modelBuilder.Entity<OpenAIMessage>().Property(p => p.CreatedDate).HasConversion(p => p, p => DateTime.SpecifyKind(p, DateTimeKind.Utc));
            modelBuilder.Entity<OpenAIMessage>().Property(p => p.UpdatedDate).HasConversion(p => p, p => p != null ? DateTime.SpecifyKind(p.Value, DateTimeKind.Utc) : null);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.UseLoggerFactory(DebugLoggerFactory);
            //optionsBuilder.EnableSensitiveDataLogging();
#endif
            optionsBuilder.UseSqlite(ConnectionString);
        }


        private static ILoggerFactory GetLoggerFactory()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddDebug().AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information));

            return serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
        }

        private void AddAuditInfo()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is not EntityState.Added and not EntityState.Modified)
                {
                    continue;
                }

                if (entry.Entity is IAuditableEntity entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedDate = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.UpdatedDate = now;
                    }
                }
            }
        }

        private void InitId()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added)
                {
                    continue;
                }


                if (entry.Entity is not IEntity<Guid> entity || entity.Id != default)
                {
                    continue;
                }

                entity.Id = Guid.NewGuid();
            }
        }

        #endregion
    }
}

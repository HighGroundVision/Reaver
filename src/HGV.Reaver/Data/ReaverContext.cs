using HGV.Reaver.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Data
{
    public class ReaverContext : DbContext
    {
        private const string DATABASE_NAME = "reaver";
        private readonly string CONNECTION_STRING;

        public virtual DbSet<UserLinkEntity> UserLinks { get; set; }
        public virtual DbSet<RoleLinkEntity> RoleLinks { get; set; }
        //public virtual DbSet<TeamEntity> Teams { get; set; }

        public ReaverContext(IOptions<ReaverSettings> settings)
        {
            this.CONNECTION_STRING = settings?.Value?.CosmosConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.CosmosConnectionString));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            base.OnConfiguring(options);

            if (options.IsConfigured == false)
            {
                options.UseCosmos(this.CONNECTION_STRING, DATABASE_NAME);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserLinkEntity>().ToContainer("users").HasNoDiscriminator().HasPartitionKey(o => o.GuidId).HasKey(d => d.Id);
            modelBuilder.Entity<UserLinkEntity>().Property(d => d.Id).HasConversion(new GuidToStringConverter()).ToJsonProperty("id");
            modelBuilder.Entity<UserLinkEntity>().Property(d => d.GuidId).HasConversion(new NumberToStringConverter<ulong>()).ToJsonProperty("pk");
            modelBuilder.Entity<UserLinkEntity>().Property(d => d.UserId).HasConversion(new NumberToStringConverter<ulong>()).ToJsonProperty("user");
            modelBuilder.Entity<UserLinkEntity>().Property(d => d.SteamId).HasConversion(new NumberToStringConverter<ulong>()).ToJsonProperty("steam");
            modelBuilder.Entity<UserLinkEntity>().Property(d => d.Email).ToJsonProperty("email");
            modelBuilder.Entity<UserLinkEntity>().Property(d => d.ETag).IsETagConcurrency();

            modelBuilder.Entity<RoleLinkEntity>().ToContainer("roles").HasNoDiscriminator().HasPartitionKey(o => o.GuidId).HasKey(d => d.Id);
            modelBuilder.Entity<RoleLinkEntity>().Property(d => d.Id).HasConversion(new GuidToStringConverter()).ToJsonProperty("id");
            modelBuilder.Entity<RoleLinkEntity>().Property(d => d.GuidId).HasConversion(new NumberToStringConverter<ulong>()).ToJsonProperty("pk");
            modelBuilder.Entity<RoleLinkEntity>().Property(d => d.MessageId).HasConversion(new NumberToStringConverter<ulong>()).ToJsonProperty("msg");
            modelBuilder.Entity<RoleLinkEntity>().Property(d => d.RoleId).HasConversion(new NumberToStringConverter<ulong>()).ToJsonProperty("role");
            modelBuilder.Entity<RoleLinkEntity>().Property(d => d.EmojiName).ToJsonProperty("emoji");
            modelBuilder.Entity<RoleLinkEntity>().Property(d => d.ETag).IsETagConcurrency();

            //modelBuilder.Entity<TeamEntity>().ToContainer("teams").HasNoDiscriminator().HasPartitionKey(o => o.GuidId).HasKey(d => d.Id);
            //modelBuilder.Entity<RoleLinkEntity>().Property(d => d.Id).HasConversion(new GuidToStringConverter()).ToJsonProperty("id");
            //modelBuilder.Entity<TeamEntity>().Property(d => d.GuidId).HasConversion(new NumberToStringConverter<ulong>()).ToJsonProperty("pk");
            //modelBuilder.Entity<TeamEntity>().Property(d => d.ETag).IsETagConcurrency();


            base.OnModelCreating(modelBuilder);
        }
    }
}


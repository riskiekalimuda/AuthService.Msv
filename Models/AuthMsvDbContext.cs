using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace AuthService.Msv.Models
{

    public partial class AuthMsvDbContext : DbContext
    {
        public AuthMsvDbContext()
        {
        }

        public AuthMsvDbContext(DbContextOptions<AuthMsvDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<MstUser> MstUsers { get; set; }    

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Name=ConnectionStrings:AuthMsvDBConnection");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MstUser>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("mst_users_pkey");

                entity.ToTable("mst_users");

                entity.HasIndex(e => e.Username, "mst_users_username_key").IsUnique();

                entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
                entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
                entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'User'::character varying")
                .HasColumnName("role");
                entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
            });

            this.OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

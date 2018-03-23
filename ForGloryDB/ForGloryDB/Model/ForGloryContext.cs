using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForGloryDB.Model
{
    public class ForGloryContext : DbContext
    {
        public ForGloryContext(DbContextOptions<ForGloryContext> options) : base(options) { }
        private DbSet<User> user;
        private DbSet<Character> character;

        public DbSet<User> User { get => user; set => user = value; }
        public DbSet<Character> Character { get => character; set => character = value; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasMaxLength(255);
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(255);
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Character>(entity =>
            {
                entity.ToTable("character");
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255);
                entity.Property(e => e.CharacterType)
                    .IsRequired()
                    .HasColumnName("character_type")
                    .HasMaxLength(255);
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username_user")
                    .HasMaxLength(255);
                entity.Property(e => e.Slot)
                    .IsRequired()
                    .HasColumnName("slot")
                    .HasMaxLength(11);
                entity.Property(e => e.X)
                    .IsRequired()
                    .HasColumnName("x")
                    .HasMaxLength(11);
                entity.Property(e => e.Y)
                    .IsRequired()
                    .HasColumnName("y")
                    .HasMaxLength(11);
                entity.Property(e => e.Z)
                    .IsRequired()
                    .HasColumnName("z")
                    .HasMaxLength(11);

            });
        }
    }
}

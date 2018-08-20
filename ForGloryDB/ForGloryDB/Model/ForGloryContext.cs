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
        private DbSet<Unit> unit;
        private DbSet<Items> items;

        public DbSet<User> User { get => user; set => user = value; }
        public DbSet<Character> Character { get => character; set => character = value; }
        public DbSet<Unit> Unit { get => unit; set => unit = value; }
        public DbSet<Items> Items { get => items; set => items = value; }

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

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("unit");
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasMaxLength(11);
                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasMaxLength(11);
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(255);
                entity.Property(e => e.NameCharacter)
                    .IsRequired()
                    .HasColumnName("name_character")
                    .HasMaxLength(255);
                entity.Property(e => e.Slot)
                    .HasColumnName("slot")
                    .HasMaxLength(4);
                entity.Property(e => e.Progress)
                    .HasColumnName("progress")
                    .HasMaxLength(11);
                
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
                entity.Property(e => e.Str)
                    .HasColumnName("str")
                    .HasMaxLength(11);
                entity.Property(e => e.Cons)
                    .HasColumnName("cons")
                    .HasMaxLength(11);
                entity.Property(e => e.Def)
                    .HasColumnName("def")
                    .HasMaxLength(11);
                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasMaxLength(11);
                entity.Property(e => e.Progress)
                    .HasColumnName("progress")
                    .HasMaxLength(11);
                entity.Property(e => e.Gold)
                    .HasColumnName("gold")
                    .HasMaxLength(11);
            });

            modelBuilder.Entity<Items>(entity =>
            {
                entity.ToTable("items");
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasMaxLength(11);
                entity.Property(e => e.ItemId)
                    .HasColumnName("item_id")
                    .HasMaxLength(11);
                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasMaxLength(11);
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(255);
                entity.Property(e => e.InUse)
                    .HasColumnName("inUse")
                    .HasMaxLength(4);
                entity.Property(e => e.NameCharacter)
                    .HasColumnName("name_character")
                    .HasMaxLength(255);
                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .HasMaxLength(11);

            });
        }
    }
}

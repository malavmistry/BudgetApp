using System;
using BudgetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ItemName> ItemNames { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetItem> BudgetItems { get; set; }
        public DbSet<BudgetItemLink> BudgetItemLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUser(modelBuilder);
            ConfigureCategory(modelBuilder);
            ConfigureItemName(modelBuilder);
            ConfigureBudget(modelBuilder);
            ConfigureBudgetItem(modelBuilder);
            ConfigureBudgetItemLink(modelBuilder);
        }

        private static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(25);
                entity.HasIndex(u => u.Username).IsUnique();
            });
        }

        private static void ConfigureCategory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(25);
                entity.Property(c => c.Description).HasMaxLength(200);
                entity.HasIndex(c => c.Name).IsUnique();
            });
        }

        private static void ConfigureItemName(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemName>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Name).IsRequired().HasMaxLength(25);
                entity.HasIndex(i => i.Name).IsUnique();
            });
        }

        private static void ConfigureBudget(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(25);

                entity.HasOne(b => b.User)
                      .WithMany(u => u.Budgets)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(b => new { b.UserId, b.Name }).IsUnique();
            });
        }

        private static void ConfigureBudgetItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BudgetItem>(entity =>
            {
                entity.HasKey(bi => bi.Id);
                entity.Property(bi => bi.Amount).HasPrecision(18, 2);
                entity.Property(bi => bi.Note).HasMaxLength(500);

                entity.HasOne(bi => bi.Budget)
                      .WithMany(b => b.BudgetItems)
                      .HasForeignKey(bi => bi.BudgetId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bi => bi.ItemName)
                      .WithMany(n => n.BudgetItems)
                      .HasForeignKey(bi => bi.ItemNameId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bi => bi.Category)
                      .WithMany(c => c.BudgetItems)
                      .HasForeignKey(bi => bi.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureBudgetItemLink(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BudgetItemLink>(entity =>
            {
                entity.HasKey(l => l.Id);

                entity.HasOne(l => l.BudgetItem)
                      .WithMany(bi => bi.AdditionalLinks)
                      .HasForeignKey(l => l.BudgetItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.LinkedBudget)
                      .WithMany(b => b.LinkedItems)
                      .HasForeignKey(l => l.LinkedBudgetId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(l => new { l.BudgetItemId, l.LinkedBudgetId }).IsUnique();
            });
        }
    }
}

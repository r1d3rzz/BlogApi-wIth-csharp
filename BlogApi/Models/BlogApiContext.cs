using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Models;

public partial class BlogApiContext : DbContext
{
    public BlogApiContext()
    {
    }

    public BlogApiContext(DbContextOptions<BlogApiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-LMCTDOI;Database=BlogApi;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(60);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Image).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(60);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

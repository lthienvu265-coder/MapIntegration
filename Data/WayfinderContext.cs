using IndoorWayfinder.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IndoorWayfinder.Api.Data;

public class WayfinderContext : DbContext
{
    public WayfinderContext(DbContextOptions<WayfinderContext> options) : base(options)
    {
    }

    public DbSet<Map> Maps => Set<Map>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<Alias> Aliases => Set<Alias>();
    public DbSet<Edge> Edges => Set<Edge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Map>(entity =>
        {
            entity.ToTable("map");
            entity.HasKey(m => m.Id);
            entity.HasMany(m => m.Nodes)
                  .WithOne(n => n.Map)
                  .HasForeignKey(n => n.MapId);
            entity.HasMany(m => m.Edges)
                  .WithOne(e => e.Map)
                  .HasForeignKey(e => e.MapId);
        });

        modelBuilder.Entity<Node>(entity =>
        {
            entity.ToTable("node");
            entity.HasKey(n => n.Id);
            entity.HasMany(n => n.Aliases)
                  .WithOne(a => a.Node)
                  .HasForeignKey(a => a.NodeId);
        });

        modelBuilder.Entity<Alias>(entity =>
        {
            entity.ToTable("alias");
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.NormName);
        });

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.ToTable("edge");
            entity.HasKey(e => e.Id);
        });
    }
}


using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public partial class GLogWareDbContext(DbContextOptions<GLogWareDbContext> options) : DbContext(options)
{
    #region Entity Sets

    #region Tables
    
    #region Topology
    public DbSet<ResourceMode> ResourceModes => Set<ResourceMode>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<PlaceType> PlaceTypes => Set<PlaceType>();
    public DbSet<Place> Places => Set<Place>();
    public DbSet<Route> Routes => Set<Route>();
    #endregion

    #region Sku
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<SkuType> SkuTypes => Set<SkuType>();
    public DbSet<Sku> Skus => Set<Sku>();
    #endregion

    #region Job
    public DbSet<JobStatus> JobStatus => Set<JobStatus>();
    public DbSet<Job> Jobs => Set<Job>();
    #endregion

    #region Logs
    public DbSet<LogErp> LogErps => Set<LogErp>();

    public DbSet<LogPlcCategory> LogPlcCategories => Set<LogPlcCategory>();
    public DbSet<LogPlcDirection> LogPlcDirections => Set<LogPlcDirection>();
    public DbSet<LogPlc> LogPlcs => Set<LogPlc>();

    public DbSet<Protocol> Protocols => Set<Protocol>();

    public DbSet<StatisticCategory> StatisticCategories => Set<StatisticCategory>();
    public DbSet<Statistic> Statistics => Set<Statistic>();
    #endregion

    #region Language
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Dictionary> Dictionaries => Set<Dictionary>();
    #endregion

    #region Parameter
    public DbSet<ParameterType> ParameterTypes => Set<ParameterType>();
    public DbSet<Parameter> Parameters => Set<Parameter>();
    #endregion

    #region User Management
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    #endregion
    
    #endregion

    #region Views
    //public DbSet<VInventory> VInventories => Set<VInventory>();
    #endregion
    #endregion

    #region Overrides
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // apply BaseTracking properties on all entities inhereting from it.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            entityType.SetTableName(DatabaseProviderHelper.ToProviderName(entityType.GetTableName()!));

            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(DatabaseProviderHelper.ToProviderName(property.GetColumnName()));
            }

            foreach (var key in entityType.GetKeys())
            {
                key.SetName(DatabaseProviderHelper.ToProviderName(key.GetName()!));
            }

            foreach (var fk in entityType.GetForeignKeys())
            {
                fk.SetConstraintName(DatabaseProviderHelper.ToProviderName(fk.GetConstraintName()!));
            }

            if (typeof(BaseTracking).IsAssignableFrom(entityType.ClrType))
            {
                var entity = modelBuilder.Entity(entityType.ClrType);

                entity.Property(nameof(BaseTracking.CreatedBy))
                      .HasMaxLength(50)
                      .HasDefaultValueSql("'GÜDEL'")
                      .ValueGeneratedOnAdd()
                      .HasComment("User or process who created the record");

                entity.Property(nameof(BaseTracking.ModifiedBy))
                      .HasMaxLength(50)
                      .HasDefaultValueSql("'GÜDEL'")
                      .ValueGeneratedOnAdd()
                      .HasComment("User or process who created the record");

                entity.Property(nameof(BaseTracking.CreatedAt))
                      .HasDefaultValueSql(DatabaseProviderHelper.GetNowSql())
                      .ValueGeneratedOnAdd()
                      .HasComment("Date/time the record was created");

                entity.Property(nameof(BaseTracking.ModifiedAt))
                      .HasDefaultValueSql(DatabaseProviderHelper.GetNowSql())
                      .ValueGeneratedOnAdd()
                      .HasComment("Date/time the record was updated for the last time");
            }

            switch (DatabaseProviderHelper.GetDatabaseProvider())
            {
                case DatabaseProvider.Postgres:
                    foreach (var property in entityType.GetProperties())
                    {
                        if (property.ClrType == typeof(DateTime?))
                        {
                            property.SetColumnType("timestamp without time zone");
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        // apply properties efined in dedicated Configuration classses for each entity
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GLogWareDbContext).Assembly);

        //Views
        //modelBuilder.Entity<VInventory>().HasNoKey().ToView("VInventory");

        // Seeding data. Data to seed is performed trough the SeedData method of the entity class itself
        var seedableTypes = typeof(Area).Assembly
                .GetTypes()
                .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISeedData<>)))
                .Select(t => new
                {
                    Type = t,
                    Order = t.GetCustomAttribute<SeedOrderAttribute>()?.Order ?? int.MaxValue
                })
                .OrderBy(x => x.Order);

        foreach (var entry in seedableTypes)
        {
            var method = entry.Type.GetMethod("SeedData");
            var data = method!.Invoke(null, null);

            modelBuilder.Entity(entry.Type).HasData((IEnumerable<object>)data!);
        }
    }

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<BaseTracking>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.ModifiedAt).CurrentValue = DateTime.Now;
            }
        }
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseTracking>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.ModifiedAt).CurrentValue = DateTime.Now;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    #endregion

}
using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public partial class GLogWareDbContext: DbContext
{
    public GLogWareDbContext(DbContextOptions<GLogWareDbContext> options) : base(options)
    {
    }

    #region Entity Sets
    #region Tables
    #region Topology
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<PlaceType> PlaceTypes => Set<PlaceType>();
    public DbSet<Place> Places => Set<Place>();
    #endregion

    #region Sku
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Sku> Skus => Set<Sku>();
    #endregion

    #region Job
    public DbSet<JobStatus> JobStatus => Set<JobStatus>();
    public DbSet<Job> Jobs => Set<Job>();
    #endregion

    #region Log
    public DbSet<LogErp> LogErps => Set<LogErp>();
    public DbSet<LogPlc> LogPlcs => Set<LogPlc>();
    public DbSet<Protocol> Protocols => Set<Protocol>();
    #endregion

    #region Language
    public DbSet<Language> Languages => Set<Language>();
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
            entityType.SetTableName(ToProviderName(entityType.GetTableName()!));

            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToProviderName(property.GetColumnName()));
            }

            foreach (var key in entityType.GetKeys())
            {
                key.SetName(ToProviderName(key.GetName()!));
            }

            foreach (var fk in entityType.GetForeignKeys())
            {
                fk.SetConstraintName(ToProviderName(fk.GetConstraintName()!));
            }

            if (typeof(BaseTracking).IsAssignableFrom(entityType.ClrType))
            {
                var entity = modelBuilder.Entity(entityType.ClrType);

                entity.Property(nameof(BaseTracking.CreatedBy))
                      .HasMaxLength(50)
                      .HasDefaultValueSql("'GÜDEL'")
                      .ValueGeneratedOnAdd()
                      .HasComment("User or process who created the record");

                entity.Property(nameof(BaseTracking.LastUpdatedBy))
                      .HasMaxLength(50)
                      .HasDefaultValueSql("'GÜDEL'")
                      .ValueGeneratedOnAdd()
                      .HasComment("User or process who created the record");

                entity.Property(nameof(BaseTracking.CreatedAt))
                      .HasDefaultValueSql(GetNowSql())
                      .ValueGeneratedOnAdd()
                      .HasComment("Date/time the record was created");

                entity.Property(nameof(BaseTracking.LastUpdatedAt))
                      .HasDefaultValueSql(GetNowSql())
                      .ValueGeneratedOnAdd()
                      .HasComment("Date/time the record was updated for the last time");
            }

            switch (GetDatabaseProvider())
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
                entry.Property(e => e.LastUpdatedAt).CurrentValue = DateTime.Now;
            }
        }
        return base.SaveChanges();
    }
    #endregion

    #region Public methods
    public DatabaseProvider GetDatabaseProvider()
    {
        return Database.ProviderName switch
        {
            "Oracle.EntityFrameworkCore" => DatabaseProvider.Oracle,
            "Microsoft.EntityFrameworkCore.SqlServer" => DatabaseProvider.SqlServer,
            "Npgsql.EntityFrameworkCore.PostgreSQL" => DatabaseProvider.Postgres,
            "Pomelo.EntityFrameworkCore.MySql" => DatabaseProvider.MySql,
            _ => DatabaseProvider.Unknown
        };
    }

    public string GetNowSql()
    {
        return GetDatabaseProvider() switch
        {
            DatabaseProvider.Oracle => "SYSTIMESTAMP",
            DatabaseProvider.SqlServer => "GETDATE()",
            DatabaseProvider.Postgres => "LOCALTIMESTAMP",
            DatabaseProvider.MySql => "NOW()",
            _ => string.Empty
        };
    }
    #endregion

    #region Private methods
    private string ToProviderName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // PascalCase → snake_case
        var snake = Regex.Replace(
            name,
            "([a-z0-9])([A-Z])",
            "$1_$2",
            RegexOptions.Compiled);

        return GetDatabaseProvider() switch
        {
            DatabaseProvider.Oracle => snake.ToUpperInvariant(),
            DatabaseProvider.SqlServer => name,
            DatabaseProvider.Postgres => snake.ToLowerInvariant(),
            DatabaseProvider.MySql => name,
            _ => name
        };
    }
    #endregion
}
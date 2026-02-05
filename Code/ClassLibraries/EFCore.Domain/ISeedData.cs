namespace Gudel.GLogWare.EFCore.Domain;

public interface ISeedData<TEntity>
{
    static abstract IEnumerable<TEntity> SeedData();
}

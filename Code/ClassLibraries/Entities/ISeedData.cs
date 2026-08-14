namespace Gudel.GLogWare.Entities;

public interface ISeedData<TEntity>
{
    static abstract IEnumerable<TEntity> SeedData();
}

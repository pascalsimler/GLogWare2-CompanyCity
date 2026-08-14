namespace Gudel.GLogWare.UI.Entities;

public interface ISeedData<TEntity>
{
    static abstract IEnumerable<TEntity> SeedData();
}

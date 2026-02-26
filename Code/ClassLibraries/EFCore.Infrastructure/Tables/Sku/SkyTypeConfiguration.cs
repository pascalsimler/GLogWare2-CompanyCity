using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class SkyTypeConfiguration : IEntityTypeConfiguration<SkuType>
{
    public void Configure(EntityTypeBuilder<SkuType> builder)
    {
    }
}

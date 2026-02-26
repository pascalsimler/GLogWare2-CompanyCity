using Gudel.GLogWare.EFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.EFCore.Infrastructure;

public class StatisticCategoryConfiguration : IEntityTypeConfiguration<StatisticCategory>
{
    public void Configure(EntityTypeBuilder<StatisticCategory> builder)
    {
    }
}
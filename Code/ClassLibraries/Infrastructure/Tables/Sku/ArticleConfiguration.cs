using Gudel.GLogWare.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gudel.GLogWare.Infrastructure;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> entity)
    {
        entity.HasKey(e => e.ArticleNumber);

        entity.Property(e => e.ArticleNumber)
               .HasMaxLength(16)
               .IsUnicode(false)
               .IsRequired()
               .HasComment("Unique number of the article");
    }
}

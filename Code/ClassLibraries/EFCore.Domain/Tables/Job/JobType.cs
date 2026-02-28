namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class JobType : BaseTracking, ISeedData<JobType>
{
    public string Identifier { get; set; } = null!;
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public static IEnumerable<JobType> SeedData()
    {
        return new List<JobType>() {
            new JobType {
                Identifier = nameof(JobTypeIdentifiers.INFEED),
                TranslationKey = $"{nameof(JobType)}.{nameof(JobTypeIdentifiers.INFEED)}",
                Description = "Infeed into the gantry store"
            },
            new JobType {
                Identifier = nameof(JobTypeIdentifiers.OUTFEED),
                TranslationKey = $"{nameof(JobType)}.{nameof(JobTypeIdentifiers.OUTFEED)}",
                Description = "Outfeed from the gantry store"
            },
            new JobType {
                Identifier = nameof(JobTypeIdentifiers.RELOCATION),
                TranslationKey = $"{nameof(JobType)}.{nameof(JobTypeIdentifiers.RELOCATION)}",
                Description = "Relocation inside the gantry store"
            },
            new JobType {
                Identifier = nameof(JobTypeIdentifiers.PALLETIZING),
                TranslationKey = $"{nameof(JobType)}.{nameof(JobTypeIdentifiers.PALLETIZING)}",
                Description = "Palletizing with ZP or Kuka robot"
            },
        };
    }
}

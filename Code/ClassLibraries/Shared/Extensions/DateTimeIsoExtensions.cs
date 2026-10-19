namespace Gudel.GLogWare.Shared;

public static class DateTimeIsoExtensions
{
    public static (int Year, int Week) GetIsoYearWeek(this DateTime dt)
    {
        // ISO day: Monday = 1, Sunday = 7
        int isoDay = dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek;

        // Thursday of the current week
        var thursday = dt.AddDays(4 - isoDay);

        int isoYear = thursday.Year;

        // First Thursday of ISO year
        var firstThursday = new DateTime(isoYear, 1, 1);
        int firstIsoDay = firstThursday.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)firstThursday.DayOfWeek;
        var firstThursdayAdjusted = firstThursday.AddDays(4 - firstIsoDay);

        int isoWeek = (thursday.DayOfYear - firstThursdayAdjusted.DayOfYear) / 7 + 1;

        return (isoYear, isoWeek);
    }
}


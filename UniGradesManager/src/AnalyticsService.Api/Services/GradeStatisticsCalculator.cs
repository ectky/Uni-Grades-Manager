namespace AnalyticsService.Services
{
    public record StatsSummary(
        double Average, double Median, double Mode, double StdDev,
        double Min, double Max, double PassRatePercent);

    /// <summary>
    /// Pure math, deliberately separated from anything that does I/O so it's
    /// trivial to unit test on its own. Mirrors the metrics shown in
    /// Fig. 3.3 / 3.7: среден успех, медиана, мода, стандартно отклонение,
    /// минимум, максимум, процент успеваемост.
    /// </summary>
    public static class GradeStatisticsCalculator
    {
        // Bulgarian grading scale — 3.00 ("Среден") is the lowest passing grade,
        // 2.00 ("Слаб") fails. Matches the range enforced in Parser/Grades Service.
        private const double PassingThreshold = 3.00;

        public static StatsSummary Calculate(IReadOnlyList<double> values)
        {
            if (values.Count == 0)
            {
                return new StatsSummary(0, 0, 0, 0, 0, 0, 0);
            }

            var sorted = values.OrderBy(v => v).ToList();

            var average = values.Average();
            var median = CalculateMedian(sorted);
            var mode = CalculateMode(values);
            var stdDev = CalculateStdDev(values, average);
            var min = sorted[0];
            var max = sorted[^1];
            var passRatePercent = 100.0 * values.Count(v => v >= PassingThreshold) / values.Count;

            return new StatsSummary(
                Math.Round(average, 2),
                Math.Round(median, 2),
                Math.Round(mode, 2),
                Math.Round(stdDev, 2),
                Math.Round(min, 2),
                Math.Round(max, 2),
                Math.Round(passRatePercent, 2));
        }

        /// <summary>Distribution bucketed by whole-number grade (2–6), for the bar chart.</summary>
        public static List<(int Grade, int Count)> CalculateDistribution(IReadOnlyList<double> values)
        {
            return values
                .GroupBy(v => (int)Math.Round(v, MidpointRounding.AwayFromZero))
                .OrderBy(g => g.Key)
                .Select(g => (Grade: g.Key, Count: g.Count()))
                .ToList();
        }

        private static double CalculateMedian(List<double> sorted)
        {
            var mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2.0
                : sorted[mid];
        }

        /// <summary>
        /// Mode over whole-number buckets, not raw decimal values — with
        /// continuous grades like 5.50, an exact-value mode is rarely
        /// meaningful; bucketing matches how the distribution chart groups
        /// grades and is what Fig. 3.3/3.7 actually display.
        /// </summary>
        private static double CalculateMode(IReadOnlyList<double> values)
        {
            return values
                .GroupBy(v => Math.Round(v, MidpointRounding.AwayFromZero))
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key) // stable tie-break: lowest grade wins
                .First()
                .Key;
        }

        private static double CalculateStdDev(IReadOnlyList<double> values, double mean)
        {
            if (values.Count <= 1) return 0;

            var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            // Population standard deviation — this is a full gradebook for the
            // course, not a sample of a larger population.
            return Math.Sqrt(sumOfSquares / values.Count);
        }
    }
}

using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace AutoTricklerGui
{
    class ExportData
    {
        [Name("Gewicht grs")]
        [Index(0)]
        public decimal? ScaleValue { get; set; }

        [Name("")]
        [Index(1)]
        public string seperatorLine { get; set; }

        [Index(2)]
        [Name("Min")]
        public string MinWeight { get; set; }

        [Index(3)]
        [Name("Max")]
        public string MaxWeight { get; set;}

        [Index(4)]
        [Name("ES")]
        public string ExtremeSpread { get; set; }

        [Index(5)]
        [Name("AVG")]
        public string AverageWeight { get; set; }

        [Index(6)]
        [Name("SD")]
        public string StandardDeviation { get; set; }
    }
}

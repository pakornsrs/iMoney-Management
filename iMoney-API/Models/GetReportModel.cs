namespace iMoney_API.Models
{
    public class GetReportModel
    {
        public decimal Income { get; set; }
        public decimal Saving { get; set; }
        public decimal Investment { get; set; }
        public decimal Expense { get; set; }
        public decimal Unclassify { get; set; }
        public decimal Balanced { get; set; }
    }
}

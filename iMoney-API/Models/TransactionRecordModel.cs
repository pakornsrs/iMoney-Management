namespace iMoney_API.Models
{
    public class TransactionRecordModel
    {
        public string TransName { get; set; }
        public string TransType { get; set; }
        //public TimeOnly TransTime { get; set; }
        //public DateTime TransDate { get; set; }
        public string TransNote { get; set; }
        public decimal TransAmount { get; set; }
    }
}

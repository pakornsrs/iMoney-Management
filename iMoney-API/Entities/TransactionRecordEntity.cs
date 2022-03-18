using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iMoney_API.Entities
{
    [Table("TRANSACTION_RECORD")]
    public class TransactionRecordEntity
    {
        [Key]
        public int TRANS_ID_KEY { get; set; }
        public string TRANS_NAME { get; set; }
        public string TRANS_TYPE { get; set; }
        public string TRANS_MAIN_TYPE { get; set; }
        public string TRANS_TIME { get; set; }
        public DateTime TRANS_DATE { get; set; }    
        public string TRANS_NOTE { get; set; }
        public decimal TRANS_AMOUNT { get; set; }
    }
}

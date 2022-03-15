using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iMoney_API.Entities
{
    [Table("CONFIG_TYPE")]
    public class ConfigCodeEntity
    {
        [Key]
        public int ID_KEY { get; set; }
        public string CONFIG_CODE { get; set; }
        public string CONFIG_KEYWORD { get; set; }
    }
}

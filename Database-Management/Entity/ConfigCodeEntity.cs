using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management.Entity
{
    public class ConfigCodeEntity
    {
        [Key]
        [JsonProperty ("Id")]
        public int Id { get; set; }

        [JsonProperty("CONFIG_CODE")]
        public string CONFIG_CODE { get; set; }

        [JsonProperty("CONFIG_KEYWORD")]
        public string CONFIG_KEYWORD { get; set; }

    }
}

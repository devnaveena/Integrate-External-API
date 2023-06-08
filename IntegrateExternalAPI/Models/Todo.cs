using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace IntegrateExternalAPI.Models
{
    public class Todo
    {
        [Key]
        [JsonProperty("id")]

        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [ForeignKey("UserId")]

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("due_on")]
        public DateTime DueOn { get; set; }

    }
}
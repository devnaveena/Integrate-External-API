using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
namespace IntegrateExternalAPI.Models
{
    public class Post
    {
        [Key]
        [JsonProperty("id")]
        public long Id { get; set; }
        [ForeignKey("UserId")]
        [JsonProperty("user_id")]
        public long UserId { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; } = string.Empty;
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

    }
}
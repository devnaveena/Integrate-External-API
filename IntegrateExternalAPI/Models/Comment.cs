using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace IntegrateExternalAPI.Models
{
    public class Comment
    {
        [Key]
        [JsonProperty("id")]
        public long Id { get; set; }
        [ForeignKey("PostId")]
        [JsonProperty("post_id")]
        public long PostId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("body")]
        public string Body { get; set; } = string.Empty;






    }
}
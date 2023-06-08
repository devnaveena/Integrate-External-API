using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace IntegrateExternalAPI.Models
{
    public class User
    {
        [Key]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("gender")]
        public string? Gender { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }


    }
}
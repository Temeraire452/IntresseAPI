using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IntresseAPI.Models
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string Contact { get; set; }
        [JsonIgnore]
        public List<Interest> Interests { get; set; }
        [JsonIgnore]
        public List<Link> Links { get; set; }
    }
}

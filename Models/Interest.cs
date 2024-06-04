using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IntresseAPI.Models
{
    public class Interest
    {
        [Key]
        public int InterestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public List<Person> Persons { get; set; }
        [JsonIgnore]
        public List<Link> Links { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace GptApi.Models
{
    public class User
    {
        [Key]
        public string? UserName { get; set; }
        public string? UserPwd { get; set; }
        public int Cnt { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;

namespace Fish_Manage.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; }
        public Boolean Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }
    }

}

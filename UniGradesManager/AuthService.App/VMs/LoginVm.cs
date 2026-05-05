using System.ComponentModel.DataAnnotations;

namespace AuthService.App.VMs
{
    public class LoginVm
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

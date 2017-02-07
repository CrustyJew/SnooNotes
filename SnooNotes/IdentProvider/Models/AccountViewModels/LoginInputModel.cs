using System.ComponentModel.DataAnnotations;

namespace IdentProvider.Models.AccountViewModels {
    public class LoginInputModel {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}

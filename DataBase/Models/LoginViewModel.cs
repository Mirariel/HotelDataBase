

using System.ComponentModel.DataAnnotations;

namespace DataBase.Models
{
        public class LoginViewModel
        {
            [Required(ErrorMessage = "Вкажіть електронну пошту!")]
            [EmailAddress(ErrorMessage = "Неправильний формат пошти!")]
            public string Email { get; set; }
            [Required(ErrorMessage = "Ввведіть пароль!")]
            public string Password { get; set; }
        }
}

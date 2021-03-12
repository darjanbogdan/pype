using Pype.Requests;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pype.Sandbox.Users
{
    public class CreateUserCommand : IRequest<User>
    {
        [Required, MaxLength(4), RegularExpression("[0-9]")]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Range(18, 100)]
        public int Age { get; set; }
    }
}

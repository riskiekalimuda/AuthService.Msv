using System;
using System.Collections.Generic;


namespace AuthService.Msv.Models
{

    public partial class MstUser
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = null!;
    }
}
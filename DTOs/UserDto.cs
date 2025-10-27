﻿using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class UserDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

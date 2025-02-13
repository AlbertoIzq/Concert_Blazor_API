﻿using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
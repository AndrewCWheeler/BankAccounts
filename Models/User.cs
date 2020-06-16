using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BankAccounts.Models
{
    public class User
    {
        [Key]
        public int UserId { get;set; }
        [Required]
        public string FirstName { get;set; }
        [Required]
        public string LastName { get;set; }
        
        [EmailAddress]
        [Required]
        public string Email { get;set; }

        [DataType(DataType.Password)]
        [Required]
        [MinLength(8, ErrorMessage="Password must be 8 characters or longer!")]
        public string Password { get;set; }

        // Will not be mapped to your users table!
        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string Confirm { get;set; }

        [Range(0, double.MaxValue, ErrorMessage="You cannot withdraw what you don't have!")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; } = 0.00M;
        public List<Transaction> Transactions { get; set; }

        public DateTime CreatedAt { get;set; } = DateTime.Now;
        public DateTime UpdatedAt { get;set; } = DateTime.Now;
    }
}
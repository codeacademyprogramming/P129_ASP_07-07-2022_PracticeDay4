using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P129Allup.ViewModels.AccountViewModels
{
    public class RegisterVM
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        [Required]
        [StringLength(255)]
        public string SurName { get; set; }
        [Required]
        [StringLength(255)]
        public string UserName { get; set; }
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [StringLength(255)]
        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        public string ConfirmPasword { get; set; }
    }
}

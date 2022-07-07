using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace P129Allup.Areas.Manage.ViewModels.AccountViewModels
{
    public class ResetPasswordVM
    {
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

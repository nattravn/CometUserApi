using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CometUserAPI.Model
{
    public class CustomerModel
    {
        [Column("ID")]
        public int Id { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string Name { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string Email { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string Phone { get; set; }

        public int? CreditLimit { get; set; }

        public bool? IsActive { get; set; }

        public string? StatusName { get; set; } = null;
        public string Code { get; set; }
    }
}

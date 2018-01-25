using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Autyan.Identity.Core.Data
{
    public class BaseEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long? Id { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? ModifyAt { get; set; }

        public DateTime? DeleteAt { get; set; }
    }
}

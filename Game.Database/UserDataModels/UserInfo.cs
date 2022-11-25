using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Database.UserDataModels
{
    [PrimaryKey(nameof(Id))]
    [Table("user_infos")]
    public partial class UserInfo
    {
        [Comment("Identificação do usuário")]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        [Comment("Carteira do usuário")]
        public string Wallet { get; set; }

        [Required]
        [MaxLength(256)]
        [Comment("Nome do usuário no jogo")]
        public string NickName { get; set; }

        [Required]
        [MaxLength(256)]
        [Comment("Senha do usuário")]
        public string Password { get; set; }

        [Required]
        [MaxLength(256)]
        [Comment("Endereço de e-email do usuário")]
        public string Email { get; set; }

        [Required]
        [Comment("Data de criação da conta")]
        public DateTime CreateDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApiToko.ModelsEF.Toko;

[Index("TokenHash", Name = "KEY_RevokeTokens_TokenHash", IsUnique = true)]
public partial class RevokeToken
{
    [Key]
    public int Id { get; set; }

    [StringLength(450)]
    public string UserId { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string TokenHash { get; set; } = null!;

    public DateTime RevokeAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}

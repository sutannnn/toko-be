using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApiToko.ModelsEF.Toko;

[Table("RefreshToken")]
public partial class RefreshToken
{
    [Key]
    [StringLength(450)]
    [Unicode(false)]
    public string Token { get; set; } = null!;

    [StringLength(450)]
    [Unicode(false)]
    public string? JwtId { get; set; }

    [StringLength(450)]
    public string? UserId { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool Invalidated { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual AspNetUser? User { get; set; }
}

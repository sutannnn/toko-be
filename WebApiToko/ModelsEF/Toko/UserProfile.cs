using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApiToko.ModelsEF.Toko;

[Index("FirstName", Name = "IDX_Users_FirstName")]
[Index("LastName", Name = "IDX_Users_LastName")]
[Index("Nik", Name = "UK_Users_NIK", IsUnique = true)]
public partial class UserProfile
{
    [Key]
    public int Id { get; set; }

    [StringLength(450)]
    public string? UserId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Address { get; set; }

    [Column("NIK")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Nik { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserProfiles")]
    public virtual AspNetUser? User { get; set; }
}

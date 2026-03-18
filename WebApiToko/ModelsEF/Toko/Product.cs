using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApiToko.ModelsEF.Toko;

[Table("Product")]
[Index("Department", Name = "IDX_Product_Department", AllDescending = true)]
[Index("Name", Name = "IDX_Product_Name", AllDescending = true)]
[Index("Product1", Name = "IDX_Product_Product", AllDescending = true)]
[Index("ProductMaterial", Name = "IDX_Product_ProductMaterial", AllDescending = true)]
public partial class Product
{
    [Key]
    public long Id { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Unicode(false)]
    public string Description { get; set; } = null!;

    public double Price { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Department { get; set; } = null!;

    [Column("Product")]
    [StringLength(100)]
    [Unicode(false)]
    public string Product1 { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string ProductMaterial { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

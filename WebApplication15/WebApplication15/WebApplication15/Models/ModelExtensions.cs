namespace WebApplication15.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    // Add missing properties expected by controllers/views without changing generated files.
    public partial class ChiTietPhieuXuat
    {
        // Unit cost (may be stored in DB in some setups)
        public Nullable<decimal> DonGia { get; set; }

        // Line total (optional, can be computed if null)
        public Nullable<decimal> ThanhTien { get; set; }

        // Note: `SoLuong` likely exists in generated class; do not re-declare if already present.
    }
    
    public partial class PhieuXuat
    {
        [NotMapped]
        public Nullable<decimal> TongTien { get; set; }
    }
}

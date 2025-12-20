using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WebApplication15.Models
{
    // Extension partial class to provide computed properties used in views
    public partial class SanPham
    {
        // Removed HanSuDung and NgaySanXuat computed properties
        // These properties are now managed per import batch in ChiTietPhieuNhap
        // If needed in views, access them directly from ChiTietPhieuNhaps collection
    }
}

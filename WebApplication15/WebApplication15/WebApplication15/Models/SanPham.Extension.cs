using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WebApplication15.Models
{
    // Extension partial class to provide computed properties used in views
    public partial class SanPham
    {
        [NotMapped]
        public DateTime? HanSuDung
        {
            get
            {
                try
                {
                    // Choose the earliest expiration date among available import batches
                    return this.ChiTietPhieuNhaps?
                               .Where(c => c.HanSuDung.HasValue)
                               .OrderBy(c => c.HanSuDung)
                               .Select(c => c.HanSuDung.Value)
                               .FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            }
        }

        [NotMapped]
        public DateTime? NgaySanXuat
        {
            get
            {
                try
                {
                    // Choose the most recent manufacturing date among import batches
                    return this.ChiTietPhieuNhaps?
                               .Where(c => c.NgaySanXuat.HasValue)
                               .OrderByDescending(c => c.NgaySanXuat)
                               .Select(c => c.NgaySanXuat.Value)
                               .FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}

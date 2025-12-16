using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication15.Models
{
    // Partial class to provide a friendly property name used by views without modifying generated code.
    public partial class PhieuXuat
    {
        [NotMapped]
        public string NguoiNhan
        {
            get { return this.LyDoXuat; }
            set { this.LyDoXuat = value; }
        }
    }
}

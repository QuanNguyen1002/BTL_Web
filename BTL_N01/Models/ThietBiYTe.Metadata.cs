using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BTL_N01.Models
{

    [MetadataTypeAttribute(typeof(ThietBiYTeMetadata))]
    public partial class ThietBiYTe
    {
        internal sealed class ThietBiYTeMetadata
        {
            [Display(Name = "Mã Thiết Bị")]
            [Required(ErrorMessage = "Vui lòng nhập dữ liệu cho trường này")]
            public string MaThietBi { get; set; }
            [Display(Name = "Tên Loại Thiết Bị")]
            public string MaLoai { get; set; }
            [Display(Name = "Tên Hạng Thiết Bị")]
            public string MaHang { get; set; }

            [Display(Name = "Tên thiết bị")]
            public string TenThietBi { get; set; }
            [Display(Name = "Giới thiệu")]
            public string GioiThieu { get; set; }
            [Display(Name = "Giá bán")]
            public Nullable<decimal> GiaBan { get; set; }

            [Display(Name = "Url Ảnh")]
            public string Anh { get; set; }
            [Display(Name = "Chi tiết")]
            public string ChiTiet { get; set; }

            [Display(Name = "Ẩn")]
            public Nullable<bool> An { get; set; }
            [Display(Name = "Giảm giá")]
            public Nullable<double> GiamGia { get; set; }

            [Display(Name = "Số lượng")]
            public Nullable<int> SoLuong { get; set; }

        }
    }
}
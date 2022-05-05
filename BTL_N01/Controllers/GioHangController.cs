using BTL_N01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BTL_N01.Controllers
{
    public class GioHangController : Controller
    {
        ThietBiYTeBTLN01 db = new ThietBiYTeBTLN01();
        public List<GioHang> LayGioHang()
        {
            List<GioHang> gioHangs = Session["GioHang"] as List<GioHang>;
            if (gioHangs == null)
            {
                gioHangs = new List<GioHang>();
                Session["GioHang"] = gioHangs;
            }
            return gioHangs;
        }

        public ActionResult ThemGioHang(string MaThietBi, string strUrl, FormCollection f)
        {
            ThietBiYTe sanpham = db.ThietBiYTes.Single(n => n.MaThietBi == MaThietBi);
            if (sanpham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            List<GioHang> gioHangs = LayGioHang();
            GioHang gioHang = gioHangs.Find(n => n.MaThietBi == MaThietBi);
            if (gioHang == null)
            {
                gioHang = new GioHang(MaThietBi);
                int gia = (int)sanpham.GiaBan;
                if(gia > 0)
                    gioHang.DonGia = gia;
                gioHang.SoLuong = int.Parse(f["Soluong"].ToString());
                if(gioHang.SoLuong > 0)
                    gioHangs.Add(gioHang);
                return Redirect(strUrl);
            }
            else
            {
                int soluong = int.Parse(f["Soluong"].ToString());
                if (sanpham.SoLuong + soluong < gioHang.SoLuong)
                    gioHang.SoLuong+=soluong;
                else
                    gioHang.SoLuong = (int)sanpham.SoLuong;
                return Redirect(strUrl);
            }

        }
        public ActionResult CapNhapSoLuong(string MaThietBi, FormCollection f)
        {
            ThietBiYTe sanpham = db.ThietBiYTes.Single(n => n.MaThietBi == MaThietBi);
            if (sanpham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            List<GioHang> listGioHang = LayGioHang();
            GioHang sp = listGioHang.Single(n => n.MaThietBi == MaThietBi);
            if (sp != null)
            {
                sp.SoLuong = int.Parse(f["Soluong"].ToString());
                if(int.Parse(f["Soluong"].ToString()) <= 0)
                {
                    Xoa(MaThietBi);
                }    
            }
            return RedirectToAction("GioHang"); 
        }
        public ActionResult Xoa(string MaThietBi)
        {
            //kiểm tra mã sản phẩm
            MaThietBi = MaThietBi.Split(')')[0];
            ThietBiYTe sanpham = db.ThietBiYTes.Single(n => n.MaThietBi == MaThietBi);
            if (sanpham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.Single(n => n.MaThietBi == MaThietBi);
            if (sp != null)
            { lstGioHang.RemoveAll(n => n.MaThietBi == MaThietBi); }
            if (lstGioHang.Count == 0)
            { RedirectToAction("TrangChu", "BanHang"); }
            return RedirectToAction("GioHang");
        }

        public ActionResult GioHang()
        {
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("TrangChu", "BanHang");
            }
            List<GioHang> gioHangs = Session["GioHang"] as List<GioHang>;
            ViewBag.TongTien = TongTien();
            ViewBag.SoLuong = TongSoLuong();
            return View(gioHangs);

        }
        public int TongSoLuong()
        {
            int t = 0;
            List<GioHang> listGioHang = LayGioHang();
            t = listGioHang.Count;
            return t;
        }
        public double TongTien()
        {
            double t = 0;
            List<GioHang> listGioHang = LayGioHang();
            t = (double)listGioHang.Sum(n => n.ThanhTien);
            return t;
        }
        public ActionResult GioHangPartial()
        {
            if (TongSoLuong() != 0)
            {
                ViewBag.TongSoLuong = TongSoLuong();
            }
            return PartialView();
        }
        public ActionResult SuaGioHang()
        {
            return View();
        }


        #region Đặt hàng
        [HttpGet]
        public ActionResult ThanhToan()
        {
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("TrangChu", "BanHang");
            }
            List<GioHang> gioHangs = Session["GioHang"] as List<GioHang>;
            ViewBag.TongTien = TongTien();
            ViewBag.SoLuong = TongSoLuong();
            return View(gioHangs);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThanhToan(FormCollection f)
        {
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("TrangChu", "BanHang");
            }
            List<GioHang> lstGioHang = LayGioHang();
            foreach (var item in lstGioHang)
            {
                ThietBiYTe t = db.ThietBiYTes.SingleOrDefault(n => n.MaThietBi == item.MaThietBi);
                if (t.SoLuong < item.SoLuong)
                {
                    ViewBag.Loi = item.TenThietBi + " không đủ!";
                    Xoa(item.MaThietBi);
                    return View(lstGioHang);
                }
            }
            //lưu thông tin vào hóa đơn bán
            HoaDonBan hd = new HoaDonBan();
            string name = f["Name"].ToString();
            string phone = f["Phone"].ToString();
            string address = f["Address"].ToString();
            string email = f["Email"].ToString();  
            int check = db.KhachHangs.Where(n => n.TenKH == name && n.Email == email).Count();
            KhachHang khachHang = new KhachHang();
            if (check == 0)
            {
                int Sl = db.KhachHangs.ToList().Count>0? db.KhachHangs.ToList().Count+1: 1;  
                String MaKH = "KH_" + Sl.ToString();
                khachHang.MaKH = MaKH;
                khachHang.TenKH = name;
                khachHang.Email = email;
                khachHang.DienThoai = phone;
                khachHang.DiaChi = address;
                hd.KhachHang = khachHang;
                db.KhachHangs.Add(khachHang);
            }
            else
            {
                khachHang = db.KhachHangs.SingleOrDefault(n => n.TenKH == name && n.Email == email && n.DienThoai == phone);
            }
            hd.MaKH = khachHang.MaKH;
            hd.NgayLap = DateTime.Now;
            hd.SoHDB = "HDB_" + ((db.HoaDonBans.ToList().Count() + 1)>0? (db.HoaDonBans.ToList().Count() + 1): 1).ToString();
            db.HoaDonBans.Add(hd);
            db.SaveChanges();
            foreach (var item in lstGioHang)
            {
                ChiTietHDB cthd = new ChiTietHDB();
                cthd.SoHDB = hd.SoHDB;
                cthd.MaThietBi = item.MaThietBi;
                cthd.SoLuong = item.SoLuong;             
                cthd.ThanhTien = (decimal?)item.ThanhTien;
                db.ThietBiYTes.Find(item.MaThietBi).SoLuong -= item.SoLuong;
                db.ChiTietHDBs.Add(cthd);
            }
            Session["GioHang"] = null;
            db.SaveChanges();
            return RedirectToAction("TrangChu", "BanHang");
        }
        #endregion  
    }
}
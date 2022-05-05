using BTL_N01.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BTL_N01.Controllers
{
   public class AdminController : Controller
    {
        // GET: Admin
        ThietBiYTeBTLN01 db = new ThietBiYTeBTLN01();
        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Dangnhap()
        {
            if (Session["Name"] != null)
            {
                Session["Name"] = null;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Dangnhap(FormCollection f)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                String sTaiKhoan = f["name"].ToString();
                String sMatKhau = GetMd5Hash(md5Hash, f["password"].ToString());
                var NguoiDung = from p in db.TaiKhoans
                                where p.UserName == sTaiKhoan && p.Password == sMatKhau
                                select p;
                if (NguoiDung.Count() == 0)
                {
                    ViewBag.Thongbao = "Tài khoản hoặc mật khẩu sai";
                    return View();
                }
                else
                {
                    string Id = db.TaiKhoans.SingleOrDefault(n => n.UserName == sTaiKhoan).ID;
                    string name = db.ChuQuanLies.SingleOrDefault(n => n.ID == Id).Ten.Split().LastOrDefault();
                    Session["Name"] = name;
                    return RedirectToAction("TrangChu", "Admin");
                }
            }
        }
        
        public ActionResult TrangChu()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Dangnhap", "Admin");
            }
            else
            {
                Session["Name"] = Session["Name"];
            }
            return View();
        }
        
        public ActionResult HoaDon(int? page)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Dangnhap", "Admin");
            }
            else
            {
                Session["Name"] = Session["Name"];
            }
            List<HoaDonBan> listHoaDons= db.HoaDonBans.ToList();
            int pagesize = 20;
            int pagenumber = (page ?? 1)>pagesize? (page ?? 1):1;
            if (listHoaDons.Count == 0)
            {
                ViewBag.ThongBao = "Không tìm thấy hóa đơn";
                return View(db.HoaDonBans.OrderBy(n => n.SoHDB).ToPagedList(pagenumber, pagesize));
            }
            return View(listHoaDons.OrderByDescending(n => n.NgayLap).ToPagedList(pagenumber, pagesize));
        }
        public PartialViewResult PartialHoaDon(string SoHD= "HDB_1")
        {
            if(SoHD == null)
            {
                return PartialView();
            }    
            HoaDonBan hoaDonBan = db.HoaDonBans.SingleOrDefault(n => n.SoHDB == SoHD);
            if(hoaDonBan == null)
            {
                return PartialView();
            }
            List<ChiTietHDB> chiTietHDBs = db.ChiTietHDBs.Where(n => n.SoHDB == SoHD).ToList();
            Decimal? TongTien = 0;
            foreach(ChiTietHDB t in chiTietHDBs)
            {
                TongTien += t.ThanhTien;
            }    
            ViewBag.TongTien = TongTien;
            return PartialView(hoaDonBan);
        }
        public JsonResult ChiTietHoaDon(String SoHD)
        {
            var s = from p in db.ChiTietHDBs
                    where p.SoHDB == SoHD
                    select new
                    {
                        SoHDB  = p.SoHDB,
                        TenThietBi  = p.ThietBiYTe.TenThietBi,
                        AnhThietBi = p.ThietBiYTe.Anh,
                        SoLuong = p.SoLuong,
                        GiaBan = p.ThietBiYTe.GiaBan,
                        ThanhTien = p.ThanhTien
                    };
            return Json(s, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Danhsach(FormCollection f, int? page)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Dangnhap", "Admin");
            }
            else
            {
                Session["Name"] = Session["Name"];
            }
            List<LoaiThietBi> t = new List<LoaiThietBi>();
            t.Add(new LoaiThietBi());
            foreach(LoaiThietBi i in db.LoaiThietBis.ToList().OrderBy(n => n.TenLoai))
            {
                t.Add(i);
            }    
            ViewBag.MaLoai = new SelectList(t, "MaLoai", "TenLoai");
            string MaLoai = f["MaLoai"];
            List<ThietBiYTe> listThietBi = db.ThietBiYTes.Where(n => n.MaLoai == MaLoai).ToList();
            int pagesize = 20;
            int pagenumber = (page ?? 1) > pagesize ? (page ?? 1) : 1;
            if (listThietBi.Count == 0)
            {
                ViewBag.ThongBao = "Không tìm thấy sản phẩm bạn tìm kiếm";
                //nếu không tìm thấy sản phẩm nào thì xuất ra toàn bộ sản phẩm
                return View(db.ThietBiYTes.OrderBy(n => n.TenThietBi).ToPagedList(pagenumber, pagesize));
            }
            ViewBag.Loai = db.LoaiThietBis.SingleOrDefault(n => n.MaLoai == MaLoai).TenLoai;
            ViewBag.ThongBao = "Đã tìm thấy" + listThietBi.Count + "sản phẩm";
            return View(listThietBi.OrderBy(n => n.TenThietBi).ToPagedList(pagenumber, pagesize));
        }
        [HttpGet]
        public ActionResult Danhsach(int? page, string MaLoai)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Dangnhap", "Admin");
            }
            else
            {
                Session["Name"] = Session["Name"];
            }
            List<LoaiThietBi> t = new List<LoaiThietBi>();
            t.Add(new LoaiThietBi());
            foreach(LoaiThietBi i in db.LoaiThietBis.ToList().OrderBy(n => n.TenLoai))
            {
                t.Add(i);
            }    
            ViewBag.MaLoai = new SelectList(t, "MaLoai", "TenLoai");
            List<ThietBiYTe> listThietBi = db.ThietBiYTes.Where(n => n.MaLoai == MaLoai).ToList();
            int pagesize = 20;
            int pagenumber = (page ?? 1) > pagesize ? (page ?? 1) : 1;
            if (listThietBi.Count == 0)
            {
                ViewBag.ThongBao = "Không tìm thấy sản phẩm bạn tìm kiếm";
                //nếu không tìm thấy sản phẩm nào thì xuất ra toàn bộ sản phẩm
                return View(db.ThietBiYTes.OrderBy(n => n.TenThietBi).ToPagedList(pagenumber, pagesize));
            }
            ViewBag.Loai = db.LoaiThietBis.SingleOrDefault(n => n.MaLoai == MaLoai).TenLoai;
            ViewBag.ThongBao = "Đã tìm thấy" + listThietBi.Count + "sản phẩm";
            return View(listThietBi.OrderBy(n => n.TenThietBi).ToPagedList(pagenumber, pagesize));
        }
        public JsonResult Sanphamtungloai(String MaLoai)
        {
            var Thietbis = db.ThietBiYTes
               .Where(n => n.MaLoai == MaLoai)
               .Select(n => new
               {
                   ID = n.MaThietBi,
                   TenThietBi = n.TenThietBi,
                   Gia = n.GiaBan,
                   GioiThieu = n.GioiThieu,
                   Loai = n.LoaiThietBi.TenLoai,
                   Anh = n.Anh
               })
               .OrderBy(n => n.TenThietBi)
               .ToList();
            return Json(Thietbis, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult PartialOpsitionLoai()
        {
            return PartialView(db.LoaiThietBis.ToList());
        }
        public PartialViewResult PartialDanhSach(String MaThietbi = "TB11")
        {
            ThietBiYTe t = db.ThietBiYTes.Single(n => n.MaThietBi == MaThietbi);
            if (t == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            LoaiThietBi loai = db.LoaiThietBis.SingleOrDefault(n => n.MaLoai == t.MaLoai);
            ViewBag.LoaiThietBi = loai.TenLoai;
            return PartialView(t);
        }
        public ActionResult ThemSanPham()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Dangnhap", "Admin");
            }
            else
            {
                Session["Name"] = Session["Name"];
            }
            ViewBag.MaLoai = new SelectList(db.LoaiThietBis.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaHang = new SelectList(db.HangThietBis.ToList().OrderBy(n => n.Ten), "MaHang", "Ten");
            int MaThietBi = db.ThietBiYTes.ToList().Count + 1;
            while(true)
            {
                if(db.ThietBiYTes.SingleOrDefault(n => n.MaThietBi == ("TB" + MaThietBi.ToString())) == null)
                {
                    ViewBag.MaThietBi = "TB" + MaThietBi.ToString() ;
                    break;
                }
                MaThietBi++;
            }    
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSanPham([Bind(Include = "MaThietBi, MaLoai, MaHang, TenThietBi, GioiThieu, GiaBan, Anh, ChiTiet, An, GiamGia, SoLuong, TongSoSao, TongSoDanhGia")] ThietBiYTe thietbi)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Dangnhap", "Admin");
            }
            else
            {
                Session["Name"] = Session["Name"];
            }
            if (ModelState.IsValid)
            {
                thietbi.TongSoSao = 0;
                thietbi.TongSoDanhGia = 0;
                db.ThietBiYTes.Add(thietbi);
                db.SaveChanges();
                return RedirectToAction("TrangChu");
            }
            return View(thietbi);
        }
        [HttpGet]
        public ActionResult SuaSanPham(string MaSP = "1")
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Dangnhap", "Admin");
            }
            else
            {
                Session["Name"] = Session["Name"];
            }
            if (MaSP == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            ThietBiYTe thietbi = db.ThietBiYTes.Find(MaSP);
            if (thietbi == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaLoai = new SelectList(db.LoaiThietBis.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaHang = new SelectList(db.HangThietBis.ToList().OrderBy(n => n.Ten), "MaHang", "Ten");

            return View(thietbi);
        }
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSanPham([Bind(Include = "MaThietBi, MaLoai, MaHang, TenThietBi, GioiThieu, GiaBan, Anh, ChiTiet, An, GiamGia, SoLuong, TongSoSao, TongSoDanhGia")] ThietBiYTe thietbi)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Dangnhap", "Admin");
            }
            else
            {
                Session["Name"] = Session["Name"];
            }
            if (ModelState.IsValid)
            {
                db.Entry(thietbi).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("TrangChu");
        }
    }
}
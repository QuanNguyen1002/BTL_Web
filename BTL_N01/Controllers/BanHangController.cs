using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BTL_N01.Models;
using PagedList;
using PagedList.Mvc;
using System.Data.Entity;

namespace BTL_N01.Controllers
{
    public class BanHangController : Controller
    {
        // GET: Index
        ThietBiYTeBTLN01 db = new ThietBiYTeBTLN01();
        public ActionResult TrangChu()
        {
            return View(db.DanhMucs.ToList());
        }
        public ActionResult DanhMucSP(String MaDanhMuc)
        {
            DanhMuc t = db.DanhMucs.SingleOrDefault(n => n.MaDanhMuc == MaDanhMuc);
            return View(t);
        }
        public ActionResult LoaiSanPham(String MaLoai = "L1")
        {
            LoaiThietBi t = db.LoaiThietBis.SingleOrDefault(n => n.MaLoai== MaLoai);
            return View(t);
        }
        public ViewResult ChiTietSanPham(string MaSP = "TB1")
        {
            ThietBiYTe thietBi = db.ThietBiYTes.SingleOrDefault(n => n.MaThietBi == MaSP);
            if(thietBi.TongSoDanhGia == 0)
            {
                ViewBag.KhongDanhGia = "Thiết bị chưa có đánh giá";
            }    
            else
            {
                double Tsao = (double)thietBi.TongSoSao;
                double TDG = (double)thietBi.TongSoDanhGia;

                //ViewBag.SoDanhGia = (Tsao/TDG).ToString() + " sao ("+ thietBi.TongSoDanhGia + " lượt đánh giá)" ;
                string res = Math.Round(Tsao / TDG, 2).ToString();
                ViewBag.SoDanhGia = res + " sao (" + thietBi.TongSoDanhGia + " lượt đánh giá)";

            }    
                
            return View(thietBi);        
        }
        public ActionResult DanhGiaSanPham(string MaThietBi, string DanhGia)
        {
            MaThietBi = MaThietBi.Split(')')[0];
            ThietBiYTe thietBi = db.ThietBiYTes.SingleOrDefault(n => n.MaThietBi == MaThietBi);
            thietBi.TongSoSao += int.Parse(DanhGia);
            thietBi.TongSoDanhGia++;
            db.SaveChanges();
            return RedirectToAction("ChiTietSanPham", new { MaSP  = MaThietBi});
        }
        public ActionResult Sanphamtheoloai()
        {
            return View();
        }
        public PartialViewResult Danhmuc()
        {       
            return PartialView(db.DanhMucs.ToList());
            
        }
        public PartialViewResult LoaiDanhmuc(String MaDanhMuc = "DM1", int sosp1loai = 0)
        {
            List<LoaiThietBi> listLoai = db.LoaiThietBis.Where(n => n.MaDanhMuc == MaDanhMuc).OrderBy(n => n.TenLoai).ToList();
            KeyValuePair<List<LoaiThietBi>, int> keyValuePair = new KeyValuePair<List<LoaiThietBi>, int>(listLoai, sosp1loai);
            return PartialView(keyValuePair);
        }
        public PartialViewResult Loai(String MaDanhmuc)
        {

            List<LoaiThietBi> listLoai = db.LoaiThietBis.Where(n => n.MaDanhMuc == MaDanhmuc).OrderBy(n => n.TenLoai).ToList();
            
            return PartialView(listLoai);
        }
        public PartialViewResult LoaiSP(String MaLoaiSP = "L1")
        {
            LoaiThietBi loai = db.LoaiThietBis.SingleOrDefault(n => n.MaLoai == MaLoaiSP);
            return PartialView(loai);

        }
        public PartialViewResult Sanphamtungloai(String MaLoai, int Sosp = 0)
        {
            List<ThietBiYTe> listSP = db.ThietBiYTes.Where(n => n.MaLoai == MaLoai).OrderBy(n => n.TenThietBi).ToList();
            if(Sosp != 0)
            {
                listSP = (from s in db.ThietBiYTes
                          where s.MaLoai == MaLoai
                          select s).Take(Sosp).ToList();
            }    
            return PartialView(listSP);
        }
        public ActionResult TimKiem(FormCollection f)
        {
            string t = f["timkiem"];
            List<ThietBiYTe> thietBiYTes = db.ThietBiYTes.Where(n => n.TenThietBi.Contains(t)).ToList();
            return View(thietBiYTes);
        }

        [HttpPost]
        public ActionResult SearchResults(FormCollection f, int? page)
        {
            string searchkey = f["timkiem"].ToString();
            List<ThietBiYTe> lstSearchResults = db.ThietBiYTes.Where(n => n.TenThietBi.Contains(searchkey)).ToList();
            int pagenumber = (page ?? 1);
            int pagesize = 20;
            if (lstSearchResults.Count == 0)
            {
                ViewBag.ThongBao = "Không tìm thấy sản phẩm bạn tìm kiếm";
                //nếu không tìm thấy sản phẩm nào thì xuất ra toàn bộ sản phẩm
                return View( new List<ThietBiYTe>().ToPagedList(pagenumber, pagesize));
                //return View(db.ThietBiYTes.OrderBy(n => n.TenThietBi).ToPagedList(pagenumber, pagesize));
            }
            ViewBag.keyword = searchkey;
            ViewBag.ThongBao = "Đã tìm thấy " + lstSearchResults.Count + " sản phẩm";
            return View(lstSearchResults.OrderBy(n => n.TenThietBi).ToPagedList(pagenumber, pagesize));
        }
        [HttpGet]
        public ActionResult SearchResults(int? page, string searchkey)
        {
            ViewBag.keyword = searchkey;
            List<ThietBiYTe> lstSearchResults = db.ThietBiYTes.Where(n => n.TenThietBi.Contains(searchkey)).ToList();
            int pagenumber = (page ?? 1);
            int pagesize = 20;
            if (lstSearchResults.Count == 0)
            {
                ViewBag.ThongBao = "Không tìm thấy sản phẩm bạn tìm kiếm";
                //nếu không tìm thấy sản phẩm nào thì xuất ra toàn bộ sản phẩm
                return View(db.ThietBiYTes.OrderBy(n => n.TenThietBi).ToPagedList(pagenumber, pagesize));
            }
            ViewBag.ThongBao = "Đã tìm thấy " + lstSearchResults.Count + "  sản phẩm";
            return View(lstSearchResults.OrderBy(n => n.TenThietBi).ToPagedList(pagenumber, pagesize));
        }

    }
}
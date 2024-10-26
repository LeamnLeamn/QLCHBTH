using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CHBHTH.Models;
using iText.Kernel.Font;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CHBHTH.Areas.Admin.Controllers
{
    public class DonHangsController : Controller
    {
        private QLbanhang db = new QLbanhang();

        // GET: DonHangs
        public ActionResult Index()
        {
            var donHangs = db.DonHangs.Include(d => d.TaiKhoan);
            var u = Session["use"] as CHBHTH.Models.TaiKhoan;
            if (u.PhanQuyen.TenQuyen == "Adminstrator")
            {
                return View(donHangs.OrderByDescending(s => s.MaDon).ToList());
            }
            return RedirectPermanent("~/Home/Index");
            
        }

        // GET: DonHangs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonHang donHang = db.DonHangs.Find(id);
            if (donHang == null)
            {
                return HttpNotFound();
            }
            return View(donHang);
        }
        public ActionResult ExportPdf(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DonHang donHang = db.DonHangs.Include(o => o.ChiTietDonHangs)
                .Include(u => u.TaiKhoan)
                .FirstOrDefault(o => o.MaDon == id);
            if (donHang == null)
            {
                return HttpNotFound();
            }

            MemoryStream ms = new MemoryStream();
            Document document = new Document(PageSize.A4, 25f, 25f, 25f, 25f);

            try
            {
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                string fontPath = Server.MapPath("~/Fonts/NotoSans-Regular.ttf");

                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                Font font = new Font(baseFont, 12);

                Paragraph header = new Paragraph("Chi Tiet Hoa Don", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18f));
                header.Alignment = Element.ALIGN_CENTER;
                document.Add(header);

                var hinhThucThanhToan = donHang.ThanhToan == 1 ? "Thanh toán bằng tiền mặt" : "Thanh toán online";
                var tinhTrang = donHang.TinhTrang.ToString();
                if (tinhTrang.Equals("1"))
                {
                    tinhTrang = "Đã xác nhận";
                }
                else if (string.IsNullOrEmpty(tinhTrang))
                {
                    tinhTrang = "Đang chờ xác nhận";
                }

                document.Add(new Paragraph($"Mã thanh toán: {donHang.MaDon}", font));
                document.Add(new Paragraph($"Tên khách hàng: {donHang.TaiKhoan.HoTen}", font));
                document.Add(new Paragraph($"Trạng thái đơn hàng: {tinhTrang}", font));
                document.Add(new Paragraph($"Ngày thanh toán: {donHang.NgayDat}", font));
                document.Add(new Paragraph($"Hình thức thanh toán: {hinhThucThanhToan}", font));
                document.Add(new Paragraph($"Địa chỉ nhận hàng: {donHang.DiaChiNhanHang}", font));
                document.Add(new Paragraph($"Tổng tiền: {donHang.TongTien}", font));
                document.Add(new Paragraph(" "));


                PdfPTable table = new PdfPTable(3);
                table.WidthPercentage = 100;

                PdfPCell cell1 = new PdfPCell(new Phrase("Sản phẩm", font));
                cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell1);

                PdfPCell cell2 = new PdfPCell(new Phrase("Số lượng", font));
                cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell2);

                PdfPCell cell3 = new PdfPCell(new Phrase("Giá", font));
                cell3.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell3);

                foreach (var item in donHang.ChiTietDonHangs)
                {
                    table.AddCell(new Phrase(item.SanPham.TenSP, font));
                    table.AddCell(new Phrase(item.SoLuong.ToString(), font));
                    table.AddCell(new Phrase(item.DonGia.ToString(), font));
                }

                document.Add(table);

                document.Close();

                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=Order.pdf");
                Response.Buffer = true;
                Response.Clear();
                Response.OutputStream.Write(ms.GetBuffer(), 0, ms.GetBuffer().Length);
                Response.OutputStream.Flush();
                Response.End();
            }
            finally
            {
                document.Close();
            }

            return View(donHang);
        }

        public ActionResult CTDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonHang donhang = db.DonHangs.Find(id);
            var chitiet = db.ChiTietDonHangs.Include(d => d.SanPham).Where(d => d.MaDon == id).ToList();
            if (donhang == null)
            {
                return HttpNotFound();
            }
            return View(chitiet);
        }

        // GET: DonHangs/Create
        public ActionResult Create()
        {
            ViewBag.MaNguoiDung = new SelectList(db.TaiKhoans, "MaNguoiDung", "HoTen");
            return View();
        }

        // POST: DonHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Madon,NgayDat,TinhTrang,ThanhToan,DiaChiNhanHang,MaNguoiDung,TongTien")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                db.DonHangs.Add(donHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaNguoiDung = new SelectList(db.TaiKhoans, "MaNguoiDung", "HoTen", donHang.MaNguoiDung);
            return View(donHang);
        }

        // GET: DonHangs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonHang donHang = db.DonHangs.Find(id);
            if (donHang == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaNguoiDung = new SelectList(db.TaiKhoans, "MaNguoiDung", "HoTen", donHang.MaNguoiDung);
            return View(donHang);
        }

        // POST: DonHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Madon,NgayDat,TinhTrang,ThanhToan,DiaChiNhanHang,MaNguoiDung,TongTien")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(donHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaNguoiDung = new SelectList(db.TaiKhoans, "MaNguoiDung", "HoTen", donHang.MaNguoiDung);
            return View(donHang);
        }

        // GET: DonHangs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonHang donHang = db.DonHangs.Find(id);
            if (donHang == null)
            {
                return HttpNotFound();
            }
            return View(donHang);
        }

        // POST: DonHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DonHang donHang = db.DonHangs.Find(id);
            db.DonHangs.Remove(donHang);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Xacnhan(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonHang donhang = db.DonHangs.Find(id);
            donhang.TinhTrang = 1;
            db.SaveChanges();
            if (donhang == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

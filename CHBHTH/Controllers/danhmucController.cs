using CHBHTH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CHBHTH.Controllers
{
    public class danhmucController : Controller
    {
        // GET: Danhmuc
        QLbanhang db = new QLbanhang();
        // GET: Danhmuc
        public ActionResult danhmucpartial()
        {
            //var danhmuc = db.LoaiHangs.ToList();
           //return PartialView(danhmuc);

            var danhmuc = db.LoaiHangs.Where(n => n.TenLoai != "Thùng rác").Take(6).ToList();
            return PartialView(danhmuc);
        }

    }
}
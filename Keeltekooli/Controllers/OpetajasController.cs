using Keeltekooli.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Keeltekooli.Controllers
{

    public class OpetajasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Opetajas

        public ActionResult Index()
        {
            var opetajad = db.Opetaja
        .Select(o => new OpetajaViewModel
        {
            Id = o.Id,
            Nimi = o.Nimi,
            Kvalifikatsioon = o.Kvalifikatsioon,
            FotoPath = o.FotoPath,
            Email = o.User.Email,
        })
        .ToList();

            return View(opetajad);
        }

        // GET: Opetajas/Details/5
        public ActionResult Details(int? id)
        {
    if (id == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

    var opetaja = db.Opetaja
        .Where(o => o.Id == id)
        .Select(o => new OpetajaViewModel
        {
            Id = o.Id,
            Nimi = o.Nimi,
            Kvalifikatsioon = o.Kvalifikatsioon,
            FotoPath = o.FotoPath,
            Email = o.User.Email
        })
        .FirstOrDefault();

    if (opetaja == null)
        return HttpNotFound();

    return View(opetaja);
        }

        // GET: Opetajas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Opetajas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OpetajaViewModel model, HttpPostedFileBase foto)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if (userManager.FindByEmail(model.Email) != null)
            {
                ModelState.AddModelError("", "Kasutaja selle e-posti aadressiga juba olemas!");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = userManager.Create(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);
                return View(model);
            }

            userManager.AddToRole(user.Id, "Opetaja");

            string fileName = "default.png";
            if (foto != null && foto.ContentLength > 0)
            {
                var ext = Path.GetExtension(foto.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Ainult JPG ja PNG.");
                    return View(model);
                }

                fileName = Guid.NewGuid() + ext;
                var path = Server.MapPath("~/Images/");
                Directory.CreateDirectory(path);
                foto.SaveAs(Path.Combine(path, fileName));
            }


            var opetaja = new Opetaja
            {
                Nimi = model.Nimi,
                Kvalifikatsioon = model.Kvalifikatsioon,
                FotoPath = fileName,
                ApplicationUserId = user.Id
            };

            db.Opetaja.Add(opetaja);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Opetajas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var opetaja = db.Opetaja
                .Where(o => o.Id == id)
                .Select(o => new OpetajaViewModel
                {
                    Id = o.Id,
                    Nimi = o.Nimi,
                    Kvalifikatsioon = o.Kvalifikatsioon,
                    FotoPath = o.FotoPath,
                    Email = o.User.Email
                })
                .FirstOrDefault();

            if (opetaja == null)
                return HttpNotFound();

            return View(opetaja);
        }

        // POST: Opetajas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OpetajaViewModel model, HttpPostedFileBase foto)
        {
            if (!ModelState.IsValid)
                return View(model);

            var opetaja = db.Opetaja
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == model.Id);

            if (opetaja == null)
                return HttpNotFound();

            opetaja.Nimi = model.Nimi;
            opetaja.Kvalifikatsioon = model.Kvalifikatsioon;

            // ===== Фото загрузка =====
            if (foto != null && foto.ContentLength > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(foto.FileName).ToLower();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Ainult JPG ja PNG.");
                    return View(model);
                }

                string uploadsFolder = Server.MapPath("~/Images/");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + ext;
                string filePath = Path.Combine(uploadsFolder, fileName);

                foto.SaveAs(filePath);

                // 🔥 Обновляем путь картинки
                opetaja.FotoPath = fileName;
            }

            // ===== Identity update =====
            var userManager = HttpContext.GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            var user = userManager.FindById(opetaja.ApplicationUserId);

            if (user != null)
            {
                user.Email = model.Email;
                user.UserName = model.Email;

                userManager.Update(user);
            }

            // ===== Password change =====
            if (!string.IsNullOrEmpty(model.Password))
            {
                var token = userManager.GeneratePasswordResetToken(opetaja.ApplicationUserId);
                userManager.ResetPassword(opetaja.ApplicationUserId, token, model.Password);
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Opetaja opetaja = db.Opetaja.Find(id);
            if (opetaja == null)
            {
                return HttpNotFound();
            }
            return View(opetaja);
        }

        // POST: Opetajas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Opetaja opetaja = db.Opetaja.Find(id);
            db.Opetaja.Remove(opetaja);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        //// GET: Opetajas/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        //    var model = db.Opetaja
        //        .Where(o => o.Id == id)
        //        .Select(o => new OpetajaViewModel
        //        {
        //            Id = o.Id,
        //            Nimi = o.Nimi,
        //            Kvalifikatsioon = o.Kvalifikatsioon,
        //            FotoPath = o.FotoPath,
        //            Email = o.User.Email
        //        })
        //        .FirstOrDefault();

        //    if (model == null)
        //        return HttpNotFound();

        //    return View(model);
        //}
        //// POST: Opetajas/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    var opetaja = db.Opetaja
        //        .Include(o => o.User)
        //        .FirstOrDefault(o => o.Id == id);

        //    if (opetaja == null)
        //        return HttpNotFound();

        //    // Удаляем сначала Identity пользователя
        //    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    var user = userManager.FindById(opetaja.ApplicationUserId);

        //    if (user != null)
        //    {
        //        userManager.Delete(user);
        //    }

        //    // Потом удаляем запись Opetaja
        //    db.Opetaja.Remove(opetaja);
        //    db.SaveChanges();

        //    return RedirectToAction("Index");
        //}
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult MinuKoolitused()
        {
            string userId = User.Identity.GetUserId();

            // 1️⃣ Находим профиль учителя
            var opetaja = db.Opetaja
                .FirstOrDefault(o => o.ApplicationUserId == userId);

            if (opetaja == null)
                return HttpNotFound("Õpetaja profiili ei leitud");

            // 2️⃣ Загружаем ТОЛЬКО ЕГО курсы / группы
            var minuKoolitused = db.Koolitus
                .Include(k => k.Keelekursus)
                .Include(k => k.Registreerimised)
                .Where(k => k.OpetajaId == opetaja.Id)
                .ToList();

            return View(minuKoolitused);
        }
    }
}

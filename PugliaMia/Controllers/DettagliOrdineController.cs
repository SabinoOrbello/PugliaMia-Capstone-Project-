using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PugliaMia.Models;

namespace PugliaMia.Controllers
{
    public class DettagliOrdineController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: DettagliOrdine
        public ActionResult Index()
        {
            var dettagliOrdine = db.DettagliOrdine.Include(d => d.Ordini).Include(d => d.Prodotti);
            return View(dettagliOrdine.ToList());
        }

        // GET: DettagliOrdine/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DettagliOrdine dettagliOrdine = db.DettagliOrdine.Find(id);
            if (dettagliOrdine == null)
            {
                return HttpNotFound();
            }
            return View(dettagliOrdine);
        }

        // GET: DettagliOrdine/Create
        public ActionResult Create()
        {
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine");
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome");
            return View();
        }

        // POST: DettagliOrdine/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DettaglioID,OrdineID,ProdottoID,Quantita,Prezzo")] DettagliOrdine dettagliOrdine)
        {
            if (ModelState.IsValid)
            {
                db.DettagliOrdine.Add(dettagliOrdine);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", dettagliOrdine.OrdineID);
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome", dettagliOrdine.ProdottoID);
            return View(dettagliOrdine);
        }

        // GET: DettagliOrdine/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DettagliOrdine dettagliOrdine = db.DettagliOrdine.Find(id);
            if (dettagliOrdine == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", dettagliOrdine.OrdineID);
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome", dettagliOrdine.ProdottoID);
            return View(dettagliOrdine);
        }

        // POST: DettagliOrdine/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DettaglioID,OrdineID,ProdottoID,Quantita,Prezzo")] DettagliOrdine dettagliOrdine)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dettagliOrdine).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", dettagliOrdine.OrdineID);
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome", dettagliOrdine.ProdottoID);
            return View(dettagliOrdine);
        }

        // GET: DettagliOrdine/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DettagliOrdine dettagliOrdine = db.DettagliOrdine.Find(id);
            if (dettagliOrdine == null)
            {
                return HttpNotFound();
            }
            return View(dettagliOrdine);
        }

        // POST: DettagliOrdine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DettagliOrdine dettagliOrdine = db.DettagliOrdine.Find(id);
            db.DettagliOrdine.Remove(dettagliOrdine);
            db.SaveChanges();
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

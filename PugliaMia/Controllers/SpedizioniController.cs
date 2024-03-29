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
    public class SpedizioniController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Spedizioni
        public ActionResult Index()
        {
            var spedizioni = db.Spedizioni.Include(s => s.Ordini);
            return View(spedizioni.ToList());
        }

        // GET: Spedizioni/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Spedizioni spedizioni = db.Spedizioni.Find(id);
            if (spedizioni == null)
            {
                return HttpNotFound();
            }
            return View(spedizioni);
        }

        // GET: Spedizioni/Create
        public ActionResult Create()
        {
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine");
            return View();
        }

        // POST: Spedizioni/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SpedizioneID,OrdineID,IndirizzoSpedizione,Corriere,DataSpedizione,NumeroTracciamento,StatoSpedizione")] Spedizioni spedizioni)
        {
            if (ModelState.IsValid)
            {
                db.Spedizioni.Add(spedizioni);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", spedizioni.OrdineID);
            return View(spedizioni);
        }

        // GET: Spedizioni/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Spedizioni spedizioni = db.Spedizioni.Find(id);
            if (spedizioni == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", spedizioni.OrdineID);
            return View(spedizioni);
        }

        // POST: Spedizioni/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SpedizioneID,OrdineID,IndirizzoSpedizione,Corriere,DataSpedizione,NumeroTracciamento,StatoSpedizione")] Spedizioni spedizioni)
        {
            if (ModelState.IsValid)
            {
                db.Entry(spedizioni).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", spedizioni.OrdineID);
            return View(spedizioni);
        }

        // GET: Spedizioni/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Spedizioni spedizioni = db.Spedizioni.Find(id);
            if (spedizioni == null)
            {
                return HttpNotFound();
            }
            return View(spedizioni);
        }

        // POST: Spedizioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Spedizioni spedizioni = db.Spedizioni.Find(id);
            db.Spedizioni.Remove(spedizioni);
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

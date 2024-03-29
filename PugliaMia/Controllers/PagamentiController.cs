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
    public class PagamentiController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Pagamenti
        public ActionResult Index()
        {
            var pagamenti = db.Pagamenti.Include(p => p.Ordini);
            return View(pagamenti.ToList());
        }

        // GET: Pagamenti/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagamenti pagamenti = db.Pagamenti.Find(id);
            if (pagamenti == null)
            {
                return HttpNotFound();
            }
            return View(pagamenti);
        }

        // GET: Pagamenti/Create
        public ActionResult Create()
        {
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine");
            return View();
        }

        // POST: Pagamenti/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PagamentoID,OrdineID,MetodoPagamento,TotalePagato,DataPagamento,StatoPagamento")] Pagamenti pagamenti)
        {
            if (ModelState.IsValid)
            {
                db.Pagamenti.Add(pagamenti);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", pagamenti.OrdineID);
            return View(pagamenti);
        }

        // GET: Pagamenti/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagamenti pagamenti = db.Pagamenti.Find(id);
            if (pagamenti == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", pagamenti.OrdineID);
            return View(pagamenti);
        }

        // POST: Pagamenti/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PagamentoID,OrdineID,MetodoPagamento,TotalePagato,DataPagamento,StatoPagamento")] Pagamenti pagamenti)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pagamenti).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrdineID = new SelectList(db.Ordini, "OrdineID", "StatoOrdine", pagamenti.OrdineID);
            return View(pagamenti);
        }

        // GET: Pagamenti/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagamenti pagamenti = db.Pagamenti.Find(id);
            if (pagamenti == null)
            {
                return HttpNotFound();
            }
            return View(pagamenti);
        }

        // POST: Pagamenti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pagamenti pagamenti = db.Pagamenti.Find(id);
            db.Pagamenti.Remove(pagamenti);
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

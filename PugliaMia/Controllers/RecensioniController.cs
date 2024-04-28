using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PugliaMia.Models;

namespace PugliaMia.Controllers
{
    public class RecensioniController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Recensioni
        public ActionResult Index(int? id)
        {
            var recensioni = db.Recensioni.Include(r => r.Prodotti).Include(r => r.Utenti);

            if (id.HasValue)
            {
                recensioni = recensioni.Where(r => r.ProdottoID == id);
            }

            // Ordina le recensioni per data in ordine decrescente
            recensioni = recensioni.OrderByDescending(r => r.DataRecensione);

            // Recupera l'ID dell'utente autenticato
            string currentUsername = User.Identity.Name;
            Utenti currentUser = db.Utenti.FirstOrDefault(u => u.Nome == currentUsername);

            ViewBag.CurrentUserID = currentUser?.UserID;

            return PartialView(recensioni.ToList());
        }





        // GET: Recensioni/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recensioni recensioni = db.Recensioni.Find(id);
            if (recensioni == null)
            {
                return HttpNotFound();
            }
            return View(recensioni);
        }

        // GET: Recensioni/Create
        public ActionResult Create(int? prodottoId)
        {
            if (prodottoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.ProdottoID = prodottoId;
            return View();
        }


        // POST: Recensioni/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Punteggio,Commento,ProdottoID")] Recensioni recensioni)
        {
            if (ModelState.IsValid)
            {
                // Recupera l'ID dell'utente autenticato
                string currentUsername = User.Identity.Name;
                Utenti currentUser = await db.Utenti.FirstOrDefaultAsync(u => u.Nome == currentUsername);
                if (currentUser != null)
                {
                    recensioni.UserID = currentUser.UserID;
                }

                recensioni.DataRecensione = DateTime.Today;
                db.Recensioni.Add(recensioni);
                await db.SaveChangesAsync();
                return RedirectToAction("Details", "Prodotti", new { id = recensioni.ProdottoID });
            }

            // Se il modello non è valido, restituisci la vista Create con il modello invalido
            return View(recensioni);
        }



        // GET: Recensioni/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recensioni recensioni = db.Recensioni.Find(id);
            if (recensioni == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome", recensioni.ProdottoID);
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome", recensioni.UserID);
            return View(recensioni);
        }

        // POST: Recensioni/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RecensioneID,ProdottoID,UserID,Punteggio,Commento")] Recensioni recensioni)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recensioni).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Prodotti", new { id = recensioni.ProdottoID });
            }
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome", recensioni.ProdottoID);
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome", recensioni.UserID);
            return View(recensioni);
        }

        // GET: Recensioni/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recensioni recensioni = db.Recensioni.Find(id);
            if (recensioni == null)
            {
                return HttpNotFound();
            }
            return View(recensioni);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Recensioni recensioni = db.Recensioni.Find(id);
            if (recensioni == null)
            {
                return HttpNotFound(); // Gestisci il caso in cui la recensione non esista
            }

            // Recupera l'ID dell'utente autenticato
            string currentUsername = User.Identity.Name;
            Utenti currentUser = db.Utenti.FirstOrDefault(u => u.Nome == currentUsername);

            // Verifica se l'utente corrente è lo stesso che ha creato la recensione
            if (currentUser.UserID != recensioni.UserID)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden); // Restituisci un errore 403 se l'utente non è autorizzato
            }

            int prodottoId = (int)recensioni.ProdottoID;

            db.Recensioni.Remove(recensioni);
            db.SaveChanges();
            return RedirectToAction("Details", "Prodotti", new { id = prodottoId });
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

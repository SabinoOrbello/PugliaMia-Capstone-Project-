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
    [Authorize]
    public class CarrelloController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Carrelloe
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Ottieni l'utente autenticato dal database
                string currentUsername = User.Identity.Name;
                Utenti currentUser = db.Utenti.FirstOrDefault(u => u.Nome == currentUsername);

                if (currentUser != null)
                {
                    // Ottieni il carrello dell'utente autenticato
                    var carrello = db.Carrello.Where(c => c.UserID == currentUser.UserID).ToList();

                    // Ottieni i prodotti nel carrello
                    var prodottoIDs = carrello.Select(c => c.ProdottoID).ToList();
                    var prodottiInCarrello = db.Prodotti.Where(p => prodottoIDs.Contains(p.ProdottoID)).ToList();

                    // Restituisci la vista del carrello con i prodotti
                    return View(prodottiInCarrello);
                }
            }

            // Se l'utente non è autenticato o se non ci sono prodotti nel carrello, restituisci la vista del carrello vuota
            return View(new List<Prodotti>());
        }
        [HttpGet]
        public ActionResult Aggiungi(int? prodottoId)
        {
            // Verifica se l'utente è autenticato
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    // Ottieni l'ID dell'utente autenticato
                    string currentUsername = User.Identity.Name;
                    Utenti currentUser = db.Utenti.FirstOrDefault(u => u.Nome == currentUsername);

                    // Verifica se l'utente esiste nel database
                    if (currentUser == null)
                    {
                        // Se l'utente non esiste, gestisci questo caso in modo appropriato
                        return RedirectToAction("Error", "Home");
                    }

                    // Recupera il prodotto dal database
                    var prodotto = db.Prodotti.Find(prodottoId);

                    // Verifica se il prodotto esiste
                    if (prodotto == null)
                    {
                        // Se il prodotto non esiste, restituisci un errore 404
                        return HttpNotFound();
                    }

                    // Crea un nuovo elemento di carrello per l'utente autenticato
                    var carrello = new Carrello { UserID = currentUser.UserID, ProdottoID = prodottoId };

                    // Aggiungi il prodotto al carrello dell'utente
                    db.Carrello.Add(carrello);
                    db.SaveChanges();

                    // Reindirizza alla pagina dei prodotti o al carrello
                    return RedirectToAction("Index", "Carrello");
                }
                catch (Exception ex)
                {
                    // Gestione delle eccezioni
                    return RedirectToAction("Error", "Home"); // Reindirizza a una pagina di errore
                }
            }
            else
            {
                // L'utente non è autenticato, reindirizza alla pagina di accesso
                return RedirectToAction("Login", "Utenti");
            }
        }


        [HttpPost]
        public ActionResult Aggiungi(int prodottoId)
        {
            // Verifica se l'utente è autenticato
            if (User.Identity.IsAuthenticated)
            {
                // Ottieni l'utente autenticato dal database
                string currentUsername = User.Identity.Name;
                Utenti currentUser = db.Utenti.FirstOrDefault(u => u.Nome == currentUsername);

                // Verifica se l'utente esiste nel database
                if (currentUser == null)
                {
                    // Se l'utente non esiste, gestisci questo caso in modo appropriato
                    return RedirectToAction("Error", "Home");
                }

                // Verifica se il prodotto esiste già nel carrello dell'utente
                Carrello carrelloItem = db.Carrello.FirstOrDefault(c => c.UserID == currentUser.UserID && c.ProdottoID == prodottoId);

                if (carrelloItem != null)
                {
                    // Il prodotto esiste già nel carrello, aggiorna la quantità
                    carrelloItem.Quantita++;
                }
                else
                {
                    // Il prodotto non esiste nel carrello, aggiungi un nuovo elemento
                    Carrello nuovoItem = new Carrello
                    {
                        UserID = currentUser.UserID,
                        ProdottoID = prodottoId,
                        Quantita = 1
                    };

                    db.Carrello.Add(nuovoItem);
                }

                // Salva i cambiamenti nel database
                db.SaveChanges();

                // Reindirizza alla pagina del carrello
                return RedirectToAction("Aggiungi");
            }
            else
            {
                // L'utente non è autenticato, reindirizza alla pagina di accesso
                return RedirectToAction("Login", "Utenti");
            }
        }




        // GET: Carrelloe/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carrello carrello = db.Carrello.Find(id);
            if (carrello == null)
            {
                return HttpNotFound();
            }
            return View(carrello);
        }

        // GET: Carrelloe/Create
        public ActionResult Create()
        {
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome");
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome");
            return View();
        }

        // POST: Carrelloe/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CarrelloID,UserID,ProdottoID,Quantita")] Carrello carrello)
        {
            if (ModelState.IsValid)
            {
                db.Carrello.Add(carrello);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome", carrello.ProdottoID);
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome", carrello.UserID);
            return View(carrello);
        }

        // GET: Carrelloe/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carrello carrello = db.Carrello.Find(id);
            if (carrello == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome", carrello.ProdottoID);
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome", carrello.UserID);
            return View(carrello);
        }

        // POST: Carrelloe/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CarrelloID,UserID,ProdottoID,Quantita")] Carrello carrello)
        {
            if (ModelState.IsValid)
            {
                db.Entry(carrello).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProdottoID = new SelectList(db.Prodotti, "ProdottoID", "Nome", carrello.ProdottoID);
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome", carrello.UserID);
            return View(carrello);
        }

        // GET: Carrelloe/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carrello carrello = db.Carrello.Find(id);
            if (carrello == null)
            {
                return HttpNotFound();
            }
            return View(carrello);
        }

        // POST: Carrelloe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Carrello carrello = db.Carrello.Find(id);
            db.Carrello.Remove(carrello);
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

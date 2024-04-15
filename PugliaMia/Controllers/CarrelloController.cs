using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
        public async Task<ActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Ottieni l'utente autenticato dal database
                string currentUsername = User.Identity.Name;
                Utenti currentUser = await db.Utenti.FirstOrDefaultAsync(u => u.Nome == currentUsername);

                if (currentUser != null)
                {
                    // Ottieni il carrello dell'utente autenticato in modo asincrono
                    var carrello = await db.Carrello.Where(c => c.UserID == currentUser.UserID).ToListAsync();

                    // Ottieni i prodotti nel carrello in modo asincrono
                    var prodottoIDs = carrello.Select(c => c.ProdottoID).ToList();
                    var prodottiInCarrello = await db.Prodotti.Where(p => prodottoIDs.Contains(p.ProdottoID)).ToListAsync();

                    // Calcola il totale della spesa considerando la quantità dei prodotti nel carrello
                    decimal totaleSpesa = 0;
                    foreach (var prodotto in prodottiInCarrello)
                    {
                        var carrelloProdotto = carrello.FirstOrDefault(c => c.ProdottoID == prodotto.ProdottoID);
                        if (carrelloProdotto != null && carrelloProdotto.Quantita.HasValue)
                        {
                            // Aggiungi al totale il prezzo del prodotto moltiplicato per la quantità nel carrello
                            totaleSpesa += (decimal)prodotto.Prezzo * (decimal)carrelloProdotto.Quantita.Value;
                        }
                    }

                    // Ottieni i prodotti correlati (i primi 5 prodotti delle stesse categorie)
                    var categorieProdotto = prodottiInCarrello.Select(p => p.CategoriaID).Distinct();
                    var prodottiCorrelati = await db.Prodotti
                        .Where(p => categorieProdotto.Contains(p.CategoriaID))
                        .Take(5)
                        .ToListAsync();

                    // Passa i prodotti correlati e il totale della spesa alla vista
                    ViewBag.ProdottiCorrelati = prodottiCorrelati;
                    ViewBag.TotaleSpesa = totaleSpesa;
                    return View(prodottiInCarrello);
                }
            }

            // Se l'utente non è autenticato o se non ci sono prodotti nel carrello, restituisci la vista del carrello vuota
            return View(new List<Prodotti>());
        }




        [HttpGet]
        public async Task<ActionResult> Aggiungi(int? prodottoId)
        {
            if (prodottoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Prodotti prodotto = await db.Prodotti.FindAsync(prodottoId);

            if (prodotto == null)
            {
                return HttpNotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                string currentUsername = User.Identity.Name;
                Utenti currentUser = await db.Utenti.FirstOrDefaultAsync(u => u.Nome == currentUsername);

                if (currentUser == null)
                {
                    return RedirectToAction("Error", "Home");
                }

                Carrello carrelloItem = await db.Carrello.FirstOrDefaultAsync(c => c.UserID == currentUser.UserID && c.ProdottoID == prodottoId);

                if (carrelloItem != null)
                {
                    carrelloItem.Quantita++;
                }
                else
                {
                    Carrello nuovoItem = new Carrello
                    {
                        UserID = currentUser.UserID,
                        ProdottoID = prodottoId,
                        Quantita = 1
                    };

                    db.Carrello.Add(nuovoItem);
                }

                await db.SaveChangesAsync();

                var prodottoCorrente = await db.Prodotti.FirstOrDefaultAsync(p => p.ProdottoID == prodottoId);
                var prodottiCorrelati = await db.Prodotti
                    .Where(p => p.CategoriaID == prodottoCorrente.CategoriaID && p.ProdottoID != prodottoId)
                    .Take(5)
                    .ToListAsync();

                // Aggiungi i prodotti correlati alla ViewBag
                ViewBag.ProdottiCorrelati = prodottiCorrelati;

                // Reindirizza alla pagina del carrello
                return RedirectToAction("Index", "Carrello");


            }
            else
            {
                return RedirectToAction("Login", "Utenti");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Aggiungi(int prodottoId)
        {
            // Verifica se l'utente è autenticato
            if (User.Identity.IsAuthenticated)
            {
                // Ottieni l'utente autenticato dal database
                string currentUsername = User.Identity.Name;
                Utenti currentUser = await db.Utenti.FirstOrDefaultAsync(u => u.Nome == currentUsername);

                // Verifica se l'utente esiste nel database
                if (currentUser == null)
                {
                    // Se l'utente non esiste, gestisci questo caso in modo appropriato
                    return RedirectToAction("Error", "Home");
                }

                // Verifica se il prodotto esiste già nel carrello dell'utente
                Carrello carrelloItem = await db.Carrello.FirstOrDefaultAsync(c => c.UserID == currentUser.UserID && c.ProdottoID == prodottoId);

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

                // Salva i cambiamenti nel database in modo asincrono
                await db.SaveChangesAsync();

                // Reindirizza alla pagina del carrello
                return RedirectToAction("Index", "Carrello");
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


        [HttpPost]
        public async Task<ActionResult> Rimuovi(int prodottoId)
        {
            // Verifica se l'utente è autenticato
            if (User.Identity.IsAuthenticated)
            {
                // Ottieni l'utente autenticato dal database
                string currentUsername = User.Identity.Name;
                Utenti currentUser = await db.Utenti.FirstOrDefaultAsync(u => u.Nome == currentUsername);

                // Verifica se l'utente esiste nel database
                if (currentUser != null)
                {
                    // Trova il prodotto nel carrello dell'utente
                    Carrello carrelloItem = await db.Carrello.FirstOrDefaultAsync(c => c.UserID == currentUser.UserID && c.ProdottoID == prodottoId);

                    if (carrelloItem != null)
                    {
                        // Rimuovi il prodotto dal carrello
                        db.Carrello.Remove(carrelloItem);
                        await db.SaveChangesAsync();
                    }
                }
            }

            // Reindirizza alla pagina del carrello dopo aver rimosso il prodotto
            return RedirectToAction("Index", "Carrello");
        }




    }
}

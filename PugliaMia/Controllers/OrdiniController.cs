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
    public class OrdiniController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Ordini
        public ActionResult Index()
        {
            var ordini = db.Ordini.Include(o => o.Utenti);
            return View(ordini.ToList());
        }

        public ActionResult OrdiniUtente()
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

                // Ottieni gli ordini dell'utente
                var ordini = db.Ordini.Where(o => o.UserID == currentUser.UserID);

                // Passa gli ordini alla vista
                return View(ordini.ToList());
            }
            else
            {
                // L'utente non è autenticato, reindirizza alla pagina di accesso
                return RedirectToAction("Login", "Utenti");
            }
        }

        public ActionResult ConfermaOrdine()
        {
            return View();
        }

        public ActionResult CreaOrdine()
        {
            // Verifica se l'utente è autenticato
            if (User.Identity.IsAuthenticated)
            {
                // Ritorna la vista per il modulo di creazione dell'ordine
                return View("ConfermaOrdine");
            }
            else
            {
                // L'utente non è autenticato, reindirizza alla pagina di accesso
                return RedirectToAction("Login", "Utenti");
            }
        }


        [HttpPost]
        public async Task<ActionResult> CreaOrdine(string indirizzoSpedizione, string metodoPagamento, string corriere)
        {
            // Verifica se l'utente è autenticato
            if (User.Identity.IsAuthenticated)
            {
                try
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

                    // Crea un nuovo ordine
                    Ordini ordine = new Ordini
                    {
                        UserID = currentUser.UserID,
                        DataOrdine = DateTime.Now,
                        StatoOrdine = "In elaborazione"
                    };

                    // Aggiungi l'ordine al database
                    db.Ordini.Add(ordine);
                    await db.SaveChangesAsync();

                    // Crea una nuova spedizione associata all'ordine
                    Spedizioni spedizione = new Spedizioni
                    {
                        IndirizzoSpedizione = indirizzoSpedizione,
                        Corriere = corriere,
                        DataSpedizione = DateTime.Now,
                        StatoSpedizione = "In transito",
                        OrdineID = ordine.OrdineID
                    };

                    // Genera un numero di tracciamento casuale
                    Random random = new Random();
                    int numeroTracciamento = random.Next(100000, 999999); // Genera un numero casuale tra 100000 e 999999

                    // Assegna il numero di tracciamento alla spedizione
                    spedizione.NumeroTracciamento = numeroTracciamento.ToString();

                    // Crea un nuovo pagamento associato all'ordine
                    Pagamenti pagamento = new Pagamenti
                    {
                        MetodoPagamento = metodoPagamento,
                        DataPagamento = DateTime.Now,
                        StatoPagamento = "Confermato",
                        OrdineID = ordine.OrdineID
                    };

                    // Calcola il totale del pagamento
                    decimal totalePagamento = 0;

                    var elementiCarrello = db.Carrello.Where(c => c.UserID == currentUser.UserID);

                    foreach (var elemento in elementiCarrello)
                    {
                        var prodotto = db.Prodotti.FirstOrDefault(p => p.ProdottoID == elemento.ProdottoID);
                        if (prodotto != null)
                        {
                            var quantita = int.Parse(Request.Form["quantita_" + prodotto.ProdottoID]);
                            decimal prezzoProdotto = (decimal)(prodotto.Prezzo * quantita); // Calcola il prezzo totale del prodotto considerando la quantità
                            totalePagamento += prezzoProdotto;

                            // Crea un nuovo DettagliOrdine
                            DettagliOrdine dettaglio = new DettagliOrdine
                            {
                                OrdineID = ordine.OrdineID,
                                ProdottoID = prodotto.ProdottoID,
                                Quantita = quantita,
                                Prezzo = prezzoProdotto // Utilizza il prezzo totale del prodotto calcolato sopra
                            };

                            // Aggiungi il dettaglio all'ordine
                            db.DettagliOrdine.Add(dettaglio);
                        }
                    }

                    // Assegna il totale del pagamento al record dei pagamenti
                    pagamento.TotalePagato = totalePagamento;

                    // Assegna il totale del pagamento all'ordine
                    ordine.Totale = totalePagamento;

                    // Aggiungi la spedizione e il pagamento al database
                    db.Spedizioni.Add(spedizione);
                    db.Pagamenti.Add(pagamento);

                    // Rimuovi tutti gli elementi dal carrello dell'utente
                    var carrelloItems = db.Carrello.Where(c => c.UserID == currentUser.UserID);
                    db.Carrello.RemoveRange(carrelloItems);

                    // Salva i cambiamenti nel database
                    await db.SaveChangesAsync();

                    // Reindirizza alla pagina dei prodotti o al carrello
                    return RedirectToAction("RiepilogoOrdine", new { ordineId = ordine.OrdineID });
                }
                catch (Exception ex)
                {
                    // Gestione delle eccezioni
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return RedirectToAction("Error", "Home"); // Reindirizza a una pagina di errore
                }
            }
            else
            {
                // L'utente non è autenticato, reindirizza alla pagina di accesso
                return RedirectToAction("Login", "Utenti");
            }
        }




        public ActionResult RiepilogoOrdine(int ordineId)
        {
            // Recupera l'ordine
            Ordini ordine = db.Ordini
    .Include(o => o.DettagliOrdine.Select(d => d.Prodotti))
    .FirstOrDefault(o => o.OrdineID == ordineId);

            // Verifica se l'ordine esiste
            if (ordine == null)
            {
                return HttpNotFound();
            }

            // Recupera la spedizione associata all'ordine
            Spedizioni spedizione = db.Spedizioni.FirstOrDefault(s => s.OrdineID == ordineId);

            // Recupera il pagamento associato all'ordine
            Pagamenti pagamento = db.Pagamenti.FirstOrDefault(p => p.OrdineID == ordineId);

            // Recupera i dettagli dell'ordine (i prodotti associati all'ordine)
            List<DettagliOrdine> dettagliOrdine = db.DettagliOrdine.Include(d => d.Prodotti).Where(d => d.OrdineID == ordineId).ToList();

            // Popola un view model con i dati dell'ordine
            var viewModel = new RiepilogoOrdineViewModel
            {
                Ordine = ordine,
                Spedizione = spedizione,
                Pagamento = pagamento,
                DettagliOrdine = dettagliOrdine
            };

            return View(viewModel);
        }






        // GET: Ordini/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ordini ordini = db.Ordini.Find(id);
            if (ordini == null)
            {
                return HttpNotFound();
            }
            return View(ordini);
        }

        // GET: Ordini/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome");
            return View();
        }

        // POST: Ordini/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrdineID,UserID,DataOrdine,StatoOrdine,Totale")] Ordini ordini)
        {
            if (ModelState.IsValid)
            {
                db.Ordini.Add(ordini);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome", ordini.UserID);
            return View(ordini);
        }

        // GET: Ordini/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ordini ordini = db.Ordini.Find(id);
            if (ordini == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome", ordini.UserID);
            return View(ordini);
        }

        // POST: Ordini/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrdineID,UserID,DataOrdine,StatoOrdine,Totale")] Ordini ordini)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ordini).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Utenti, "UserID", "Nome", ordini.UserID);
            return View(ordini);
        }

        // GET: Ordini/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ordini ordini = db.Ordini.Find(id);
            if (ordini == null)
            {
                return HttpNotFound();
            }
            return View(ordini);
        }

        // POST: Ordini/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ordini ordini = db.Ordini.Find(id);

            var pagamenti = db.Pagamenti.Where(p => p.OrdineID == id);
            db.Pagamenti.RemoveRange(pagamenti);

            var spedizioni = db.Spedizioni.Where(s => s.OrdineID == id);
            db.Spedizioni.RemoveRange(spedizioni);

            // Add this code to delete the DettagliOrdine records
            var dettagliOrdine = db.DettagliOrdine.Where(d => d.OrdineID == id);
            db.DettagliOrdine.RemoveRange(dettagliOrdine);

            db.Ordini.Remove(ordini);
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

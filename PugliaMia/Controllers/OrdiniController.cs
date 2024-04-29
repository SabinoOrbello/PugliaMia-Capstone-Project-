using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PugliaMia.Models;
using Stripe;
using Stripe.Forwarding;

namespace PugliaMia.Controllers
{
    public class OrdiniController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Ordini
        public ActionResult Index()
        {
            var ordiniElaborazione = db.Ordini
                                      .Where(o => o.StatoOrdine == "In elaborazione")
                                      .OrderBy(o => o.DataOrdine)
                                      .ToList();

            return View(ordiniElaborazione);
        }


        public ActionResult IndexConfermato()
        {
            var ordiniElaborazione = db.Ordini
                                      .Where(o => o.StatoOrdine == "Confermato")
                                      .OrderByDescending(o => o.DataOrdine)
                                      .ToList();

            return View(ordiniElaborazione);
        }



        // Azione per la conferma dell'ordine tramite metodo GET
        [HttpGet]
        public ActionResult ConfermaOrdine(int? id)
        {
            // Puoi implementare qui la logica per mostrare una vista di conferma
            // o semplicemente reindirizzare all'azione Index
            return RedirectToAction("Index");
        }

        // Azione per la conferma dell'ordine tramite metodo POST
        [HttpPost]
        public ActionResult ConfermaOrdine(int id)
        {
            var ordine = db.Ordini.Find(id);
            if (ordine != null)
            {
                ordine.StatoOrdine = "Confermato";
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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

                // Ottieni gli ordini dell'utente ordinati per data in ordine decrescente
                var ordini = db.Ordini
                    .Where(o => o.UserID == currentUser.UserID)
                    .OrderByDescending(o => o.DataOrdine);

                // Passa gli ordini ordinati alla vista
                return View(ordini.ToList());
            }
            else
            {
                // L'utente non è autenticato, reindirizza alla pagina di accesso
                return RedirectToAction("Login", "Utenti");
            }
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
        public async Task<ActionResult> CreaOrdine(string indirizzoSpedizione, string citta, string regione, string provincia, string metodoPagamento, string corriere, string paymentMethodId, FormCollection form)
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

                    // Inizializza Stripe con la tua chiave segreta
                    StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];

                    // Calcola il totale del pagamento e il costo di spedizione
                    decimal totalePagamento = 0;
                    decimal costoSpedizione = 0;

                    var elementiCarrello = db.Carrello.Where(c => c.UserID == currentUser.UserID);

                    foreach (var elemento in elementiCarrello)
                    {
                        var prodotto = db.Prodotti.FirstOrDefault(p => p.ProdottoID == elemento.ProdottoID);
                        if (prodotto != null)
                        {
                            var quantita = int.Parse(Request.Form["quantita_" + prodotto.ProdottoID]);
                            decimal prezzoProdotto = (decimal)(prodotto.Prezzo * quantita); // Calcola il prezzo totale del prodotto considerando la quantità
                            totalePagamento += prezzoProdotto;
                            costoSpedizione += CalcolaCostoSpedizione((decimal)(prodotto.Peso * quantita));
                        }
                    }

                    // Esegui il pagamento con Stripe
                    var options = new PaymentIntentCreateOptions
                    {
                        PaymentMethod = paymentMethodId,
                        Amount = (long)(totalePagamento + costoSpedizione) * 100, // L'importo deve essere in centesimi
                        Currency = "eur", // Valuta
                        PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                        Confirm = true,
                        ReturnUrl = Url.Action("ConfermaOrdine", "Ordini", null, Request.Url.Scheme),
                    };

                    var service = new PaymentIntentService();
                    var paymentIntent = await service.CreateAsync(options);

                    // Controlla se il pagamento è andato a buon fine
                    if (paymentIntent.Status != "succeeded")
                    {
                        // Se il pagamento non è andato a buon fine, restituisci un errore all'utente
                        ModelState.AddModelError("", "Il pagamento non è andato a buon fine. Riprova.");
                        return View(); // Sostituisci con la tua vista di errore
                    }

                    // Salva l'ID del pagamento di Stripe nel database
                    string stripePaymentIntentId = paymentIntent.Id;

                    // Crea un nuovo ordine
                    Ordini ordine = new Ordini
                    {
                        UserID = currentUser.UserID,
                        DataOrdine = DateTime.Now,
                        StatoOrdine = "In elaborazione",
                        Totale = totalePagamento + costoSpedizione
                    };

                    // Aggiungi l'ordine al database
                    db.Ordini.Add(ordine);
                    await db.SaveChangesAsync();

                    // Crea i dettagli dell'ordine per ogni prodotto nel carrello
                    foreach (var elemento in elementiCarrello)
                    {
                        var prodotto = db.Prodotti.FirstOrDefault(p => p.ProdottoID == elemento.ProdottoID);
                        if (prodotto != null)
                        {
                            var quantita = int.Parse(Request.Form["quantita_" + prodotto.ProdottoID]);
                            DettagliOrdine dettaglioOrdine = new DettagliOrdine
                            {
                                OrdineID = ordine.OrdineID,
                                ProdottoID = prodotto.ProdottoID,
                                Quantita = quantita,
                                Prezzo = prodotto.Prezzo
                            };
                            db.DettagliOrdine.Add(dettaglioOrdine);
                        }
                    }

                    await db.SaveChangesAsync();

                    // Crea una nuova spedizione associata all'ordine
                    Spedizioni spedizione = new Spedizioni
                    {
                        IndirizzoSpedizione = indirizzoSpedizione,
                        Citta = citta,
                        Regione = regione,
                        Provincia = provincia,
                        Corriere = corriere,
                        DataSpedizione = DateTime.Now,
                        StatoSpedizione = "In transito",
                        NumeroTracciamento = GenerateRandomTrackingNumber(), // Genera un numero di tracciamento casuale
                        OrdineID = ordine.OrdineID
                    };

                    // Aggiungi la spedizione al database
                    db.Spedizioni.Add(spedizione);

                    // Crea un nuovo pagamento associato all'ordine
                    Pagamenti pagamento = new Pagamenti
                    {
                        MetodoPagamento = metodoPagamento,
                        DataPagamento = DateTime.Now,
                        StatoPagamento = "Confermato",
                        TotalePagato = totalePagamento + costoSpedizione,
                        StripePaymentIntentId = stripePaymentIntentId,
                        StripePaymentStatus = "Confermato",
                        OrdineID = ordine.OrdineID
                    };

                    // Aggiungi il pagamento al database
                    db.Pagamenti.Add(pagamento);

                    // Rimuovi tutti gli elementi dal carrello dell'utente
                    var carrelloItems = db.Carrello.Where(c => c.UserID == currentUser.UserID);
                    db.Carrello.RemoveRange(carrelloItems);

                    // Salva i cambiamenti nel database
                    await db.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Ordine confermato con successo!";

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



        // Metodo per generare un numero di tracciamento casuale
        private string GenerateRandomTrackingNumber()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }


        public async Task<ActionResult> RiepilogoOrdine(int ordineId)
        {
            // Recupera l'ordine dal database
            Ordini ordine = await db.Ordini
                .Include(o => o.DettagliOrdine.Select(d => d.Prodotti))
                .FirstOrDefaultAsync(o => o.OrdineID == ordineId);

            // Verifica se l'ordine esiste
            if (ordine == null)
            {
                return HttpNotFound();
            }

            // Recupera la spedizione associata all'ordine
            Spedizioni spedizione = await db.Spedizioni.FirstOrDefaultAsync(s => s.OrdineID == ordineId);

            // Recupera il pagamento associato all'ordine
            Pagamenti pagamento = await db.Pagamenti.FirstOrDefaultAsync(p => p.OrdineID == ordineId);

            // Calcola il peso totale degli articoli nell'ordine
            decimal pesoTotaleOrdine = 0;
            foreach (var dettaglio in ordine.DettagliOrdine)
            {
                // Recupera il prodotto associato al dettaglio dell'ordine
                Prodotti prodotto = await db.Prodotti.FirstOrDefaultAsync(p => p.ProdottoID == dettaglio.ProdottoID);
                if (prodotto != null)
                {
                    pesoTotaleOrdine += (decimal)prodotto.Peso * (decimal)dettaglio.Quantita;
                }
            }

            // Calcola il costo di spedizione basato sul peso totale dell'ordine
            decimal costoSpedizione = CalcolaCostoSpedizione(pesoTotaleOrdine);

            // Calcola la data di consegna prevista in base alla data dell'ordine e a un intervallo di tempo stimato
            // Ad esempio, possiamo aggiungere 3 giorni lavorativi alla data dell'ordine per ottenere la data di consegna prevista
            DateTime dataConsegnaPrevista = ordine.DataOrdine.AddDays(3);


            // Inizializza il totale dell'ordine
            decimal totaleOrdine = 0;

            // Calcola il totale dell'ordine includendo il costo di spedizione totale
            foreach (var dettaglio in ordine.DettagliOrdine)
            {
                // Aggiungi al totale il prezzo del prodotto moltiplicato per la quantità nel dettaglio dell'ordine
                totaleOrdine += (decimal)dettaglio.Prezzo * (decimal)dettaglio.Quantita;
            }

            // Aggiungi il costo di spedizione al totale dell'ordine
            totaleOrdine += costoSpedizione;

            // Popola un view model con i dati dell'ordine, la spedizione, il pagamento, la data di consegna prevista e il costo di spedizione totale
            var viewModel = new RiepilogoOrdineViewModel
            {
                Ordine = ordine,
                Spedizione = spedizione,
                Pagamento = pagamento,
                DettagliOrdine = ordine.DettagliOrdine.ToList(),
                TotaleOrdine = totaleOrdine,
                CostoSpedizioneTotale = costoSpedizione,
                DataConsegnaPrevista = dataConsegnaPrevista
            };

            return View(viewModel);
        }






        // Metodo per calcolare il costo di spedizione in base al peso totale
        private decimal CalcolaCostoSpedizione(decimal pesoTotale)
        {
            decimal costoSpedizione = 0;

            // Definisci i tuoi intervalli di peso e le relative tariffe di spedizione
            decimal[] intervalliPeso = { 5, 10, 20, 400 }; // Pesi in kg
            decimal[] tariffeSpedizione = { 5, 7, 10, 0 }; // Tariffe di spedizione in euro

            // Controlla in quale intervallo di peso rientra il peso totale
            for (int i = 0; i < intervalliPeso.Length; i++)
            {
                if (pesoTotale <= intervalliPeso[i])
                {
                    costoSpedizione = tariffeSpedizione[i];
                    break; // Esci dal ciclo una volta trovato l'intervallo corretto
                }
            }

            if (intervalliPeso.Contains(pesoTotale) && intervalliPeso.ToList().IndexOf(pesoTotale) < tariffeSpedizione.Length - 1)
            {
                costoSpedizione = tariffeSpedizione[intervalliPeso.ToList().IndexOf(pesoTotale) + 1];
            }


            return costoSpedizione;
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
        public ActionResult Edit([Bind(Include = "OrdineID,UserID,DataOrdine,StatoOrdine,Totale,")] Ordini ordini)
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

        public async Task<ActionResult> ModificaOrdine(int ordineId)
        {
            // Recupera l'ordine dal database
            Ordini ordine = await db.Ordini.FindAsync(ordineId);

            // Assicurati che l'ordine esista
            if (ordine == null)
            {
                return HttpNotFound(); // O un'altra azione appropriata
            }

            // Carica la spedizione associata a questo ordine
            Spedizioni spedizione = await db.Spedizioni.FirstOrDefaultAsync(s => s.OrdineID == ordineId);

            // Carica il pagamento associato a questo ordine
            Pagamenti pagamento = await db.Pagamenti.FirstOrDefaultAsync(p => p.OrdineID == ordineId);

            // Carica i dettagli dell'ordine associati a questo ordine
            List<DettagliOrdine> dettagliOrdine = await db.DettagliOrdine.Where(d => d.OrdineID == ordineId).ToListAsync();

            // Crea un ViewModel per passare i dati alla vista
            ModificaOrdineViewModel viewModel = new ModificaOrdineViewModel
            {
                Ordine = ordine,
                Spedizione = spedizione,
                Pagamento = pagamento,
                DettagliOrdine = dettagliOrdine
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<ActionResult> ModificaOrdine(ModificaOrdineViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Recupera l'ordine dal database
                    Ordini ordine = await db.Ordini.FindAsync(viewModel.Ordine.OrdineID);

                    // Assicurati che l'ordine esista
                    if (ordine == null)
                    {
                        return HttpNotFound(); // O un'altra azione appropriata
                    }

                    // Aggiorna i dati dell'ordine con quelli provenienti dal viewModel
                    ordine.DataOrdine = viewModel.Ordine.DataOrdine;
                    ordine.StatoOrdine = viewModel.Ordine.StatoOrdine;
                    ordine.Totale = viewModel.Ordine.Totale;

                    // Aggiorna la spedizione
                    Spedizioni spedizione = await db.Spedizioni.FirstOrDefaultAsync(s => s.OrdineID == viewModel.Ordine.OrdineID);
                    if (spedizione != null)
                    {
                        spedizione.IndirizzoSpedizione = viewModel.Spedizione.IndirizzoSpedizione;
                        spedizione.Corriere = viewModel.Spedizione.Corriere;
                        spedizione.DataSpedizione = viewModel.Spedizione.DataSpedizione;
                        spedizione.StatoSpedizione = viewModel.Spedizione.StatoSpedizione;
                    }

                    // Aggiorna il pagamento
                    Pagamenti pagamento = await db.Pagamenti.FirstOrDefaultAsync(p => p.OrdineID == viewModel.Ordine.OrdineID);
                    if (pagamento != null)
                    {
                        pagamento.MetodoPagamento = viewModel.Pagamento.MetodoPagamento;
                        pagamento.DataPagamento = viewModel.Pagamento.DataPagamento;
                        pagamento.StatoPagamento = viewModel.Pagamento.StatoPagamento;
                        pagamento.TotalePagato = viewModel.Pagamento.TotalePagato;
                    }

                    // Salva tutte le modifiche nel database
                    await db.SaveChangesAsync();

                    // Reindirizza l'utente a una pagina di conferma o a una vista appropriata
                    return RedirectToAction("RiepilogoOrdine", new { ordineId = ordine.OrdineID });
                }
                catch (Exception ex)
                {
                    // Gestione delle eccezioni
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return RedirectToAction("Error", "Home"); // Reindirizza a una pagina di errore
                }
            }

            // Se ModelState non è valido, torna alla stessa vista per correggere gli errori
            return View(viewModel);
        }



















    }

    internal class StripeChargeCreateOptions
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public object CustomerId { get; set; }
    }

    internal class StripeCustomerCreateOptions
    {
        public string Email { get; set; }
        public string SourceToken { get; set; }
    }

    internal class StripeChargeService
    {
        public StripeChargeService()
        {
        }

        internal object Create(StripeChargeCreateOptions stripeChargeCreateOptions)
        {
            throw new NotImplementedException();
        }
    }
}
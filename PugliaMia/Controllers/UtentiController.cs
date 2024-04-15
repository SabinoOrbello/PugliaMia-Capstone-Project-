using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PugliaMia.Models;

namespace PugliaMia.Controllers
{
    public class UtentiController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Utenti
        public ActionResult Index()
        {
            return View(db.Utenti.ToList());
        }

        // GET: Utenti/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Utenti utenti = db.Utenti.Find(id);
            if (utenti == null)
            {
                return HttpNotFound();
            }
            return View(utenti);
        }

        // GET: Utenti/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Utenti/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserID,Nome,Email,Password,IndirizzoSpedizione,Role")] Utenti utenti)
        {
            if (ModelState.IsValid)
            {
                db.Utenti.Add(utenti);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(utenti);
        }

        // GET: Utenti/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Utenti utenti = db.Utenti.Find(id);
            if (utenti == null)
            {
                return HttpNotFound();
            }
            return View(utenti);
        }

        // POST: Utenti/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,Nome,Email,Password,IndirizzoSpedizione,Role")] Utenti utenti)
        {
            if (ModelState.IsValid)
            {
                db.Entry(utenti).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(utenti);
        }

        // GET: Utenti/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Utenti utenti = db.Utenti.Find(id);
            if (utenti == null)
            {
                return HttpNotFound();
            }
            return View(utenti);
        }

        // POST: Utenti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Utenti utenti = db.Utenti.Find(id);
            db.Utenti.Remove(utenti);
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

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Nome,Password,Email")] Utenti model)
        {
            if (ModelState.IsValid)
            {
                // Verifica se l'utente esiste già
                var existingUser = db.Utenti.FirstOrDefault(u => u.Nome == model.Nome);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Nome", "Nome already in use");
                    return View(model);
                }
                model.Role = "Cliente"; // Imposta il ruolo predefinito su "Cliente

                // Crea un nuovo utente
                db.Utenti.Add(model);
                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            //  significa che qualcosa non ha funzionato, quindi visualizza nuovamente il modulo
            return View(model);
        }


        [HttpGet]
        public ActionResult Login()
        {


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Utenti model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Utenti.FirstOrDefault(u => u.Nome == model.Nome && u.Password == model.Password);
                if (user != null)
                {
                    // Utente autenticato
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                        1,
                        user.Nome,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(30),
                        true,
                        user.Role,
                        FormsAuthentication.FormsCookiePath);

                    string encTicket = FormsAuthentication.Encrypt(ticket);
                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                    // Recupera l'ID dell'utente autenticato
                    int userId = user.UserID;

                    // Controlla se l'utente ha già un carrello
                    var existingCart = db.Carrello.FirstOrDefault(c => c.UserID == userId);

                    if (existingCart == null)
                    {
                        // L'utente non ha ancora un carrello, quindi crea un nuovo carrello per l'utente
                        var nuovoCarrello = new Carrello
                        {
                            UserID = userId
                        };
                        db.Carrello.Add(nuovoCarrello);
                        db.SaveChanges();
                    }

                    // Reindirizza l'utente alla pagina principale
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Nome utente o password non validi");
                }
            }

            return View(model);
        }



        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Utenti");
        }
    }
}

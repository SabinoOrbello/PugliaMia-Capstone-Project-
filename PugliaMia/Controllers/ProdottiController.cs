using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PugliaMia.Models;


namespace PugliaMia.Controllers
{
    public class ProdottiController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Prodotti
        public async Task<ActionResult> Index()
        {
            var prodotti = await db.Prodotti.Include(p => p.Categorie).ToListAsync();
            return PartialView(prodotti);
        }






        public ActionResult IndexAdmin()
        {
            var prodotti = db.Prodotti.Include(p => p.Categorie);
            return View(prodotti.ToList());
        }
        // GET: Prodotti/Details/5
        public ActionResult Details(int id)
        {
            var prodotto = db.Prodotti.Find(id);
            if (prodotto == null)
            {
                return HttpNotFound();
            }

            // Ottieni il punteggio medio delle recensioni associate al prodotto
            double? punteggioMedio = db.Recensioni.Where(r => r.ProdottoID == id).Average(r => (double?)r.Punteggio);

            ViewBag.PunteggioMedio = punteggioMedio;

            return View(prodotto);
        }


        // GET: Prodotti/Create
        public ActionResult Create()
        {
            ViewBag.CategoriaID = new SelectList(db.Categorie, "CategoriaID", "NomeCategoria");
            return View();
        }

        // POST: Prodotti/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProdottoID,Nome,Descrizione,Prezzo, Ingredienti, Peso, Immagine,CategoriaID,Disponibilita")] Prodotti prodotti, HttpPostedFileBase Immagine)
        {
            if (Immagine != null && Immagine.ContentLength > 0)
            {
                string nomeFile = Path.GetFileName(Immagine.FileName);
                string pathToSave = Path.Combine(Server.MapPath("~/Content/Images/"), nomeFile);
                Immagine.SaveAs(pathToSave);
                prodotti.Immagine = "~/Content/Images/" + nomeFile;

            }
            if (ModelState.IsValid)
            {
                db.Prodotti.Add(prodotti);
                db.SaveChanges();
                return RedirectToAction("IndexAdmin");
            }

            ViewBag.CategoriaID = new SelectList(db.Categorie, "CategoriaID", "NomeCategoria", prodotti.CategoriaID);
            return View(prodotti);
        }

        // GET: Prodotti/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Prodotti prodotti = db.Prodotti.Find(id);
            if (prodotti == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoriaID = new SelectList(db.Categorie, "CategoriaID", "NomeCategoria", prodotti.CategoriaID);
            return View(prodotti);
        }

        // POST: Prodotti/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProdottoID,Nome,Descrizione,Prezzo, Ingredienti, Peso, Immagine,CategoriaID,Disponibilita")] Prodotti prodotti, HttpPostedFileBase Immagine)
        {
            if (ModelState.IsValid)
            {
                if (Immagine != null && Immagine.ContentLength > 0)
                {
                    string nomeFile = Path.GetFileName(Immagine.FileName);
                    string pathToSave = Path.Combine(Server.MapPath("~/Content/Images/"), nomeFile);
                    Immagine.SaveAs(pathToSave);
                    prodotti.Immagine = "~/Content/Images/" + nomeFile;
                }
                db.Entry(prodotti).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("IndexAdmin");
            }
            ViewBag.CategoriaID = new SelectList(db.Categorie, "CategoriaID", "NomeCategoria", prodotti.CategoriaID);
            return View(prodotti);
        }

        // GET: Prodotti/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Prodotti prodotti = db.Prodotti.Find(id);
            if (prodotti == null)
            {
                return HttpNotFound();
            }
            return View(prodotti);
        }

        // POST: Prodotti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Prodotti prodotti = db.Prodotti.Find(id);
            db.Prodotti.Remove(prodotti);
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

        public async Task<ActionResult> GetProdottiByCategoria(int categoriaId)
        {
            var prodotti = await db.Prodotti.Where(p => p.CategoriaID == categoriaId).ToListAsync();
            return PartialView("indexProdotti", prodotti);
        }

        public async Task<ActionResult> Search(string searchText)
        {
            var prodotti = await db.Prodotti
                .Where(p => p.Nome.Contains(searchText)) // Filtra i prodotti il cui nome contiene il testo di ricerca
                .Include(p => p.Categorie)
                .ToListAsync();

            return PartialView("indexProdotti", prodotti);
        }



    }
}
using PugliaMia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PugliaMia.Controllers
{
    public class HomeController : Controller
    {
        private ModelDbContext db = new ModelDbContext();
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult ChiSiamo()
        {
            ViewBag.Title = "Chi Siamo";

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult BackOffice()
        {
            ViewBag.Title = "Back Office";

            return View();
        }

        public ActionResult ProdottiPiuVenduti()
        {
            using (var db = new ModelDbContext())
            {
                var prodottiPiuVenduti = db.DettagliOrdine
                    .GroupBy(d => d.ProdottoID)
                    .Select(g => new
                    {
                        ProdottoID = g.Key,
                        QuantitaVenduta = g.Sum(d => d.Quantita)
                    })
                    .OrderByDescending(g => g.QuantitaVenduta)
                    .ToList();

                var prodottiPiuVendutiIDs = prodottiPiuVenduti.Select(ppv => ppv.ProdottoID).ToList();

                var prodotti = db.Prodotti
                    .Where(p => prodottiPiuVendutiIDs.Contains(p.ProdottoID))
                    .ToList() // Aggiungi questa linea per eseguire la query prima di utilizzare il metodo 'First'
                    .Select(p => new ProdottoVendutoViewModel
                    {
                        ProdottoID = p.ProdottoID,
                        Nome = p.Nome,
                        QuantitaVenduta = (int)prodottiPiuVenduti.First(ppv => ppv.ProdottoID == p.ProdottoID).QuantitaVenduta
                    })
                    .OrderByDescending(p => p.QuantitaVenduta)
                    .ToList();

                return View(prodotti);
            }
        }





    }
}

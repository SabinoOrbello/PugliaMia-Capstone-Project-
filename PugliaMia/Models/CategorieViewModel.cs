using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PugliaMia.Models
{
    public class CategorieViewModel
    {
        public List<Categorie> CategorieList { get; set; }
        public int PageCount { get; set; }
    }
}
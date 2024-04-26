using PugliaMia.Models;
using System.Collections.Generic;

namespace PugliaMia.Controllers
{
    internal class StaticPagedList
    {
        private List<Prodotti> prodotti;
        private int pageNumber;
        private int pageSize;
        private int totalCount;

        public StaticPagedList(List<Prodotti> prodotti, int pageNumber, int pageSize, int totalCount)
        {
            this.prodotti = prodotti;
            this.pageNumber = pageNumber;
            this.pageSize = pageSize;
            this.totalCount = totalCount;
        }
    }
}
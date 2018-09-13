using Mlinq;
using Mlinq.Core.IServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            EFContext db = new EFContext();
            var list = db.Categories.Where(o => o.CategoryID == 1).Select(o => new { o.CategoryID, o.CategoryName }).ToList();
        }
    }

    public class Categories
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; }
    }

    public class EFContext : DbContext
    {
        public EFContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Categories> Categories { get; set; }
    }
}

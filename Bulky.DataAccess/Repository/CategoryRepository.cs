using Bulky.Models;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            //var objFromDb = _db.Categories.FirstOrDefault(s => s.Id == category.Id);
            //if (objFromDb != null)
            //{
            //    objFromDb.Name = category.Name;
            //    objFromDb.DisplayOrder = category.DisplayOrder;
            //}
            dbSet.Update(category);
        }
    }

}

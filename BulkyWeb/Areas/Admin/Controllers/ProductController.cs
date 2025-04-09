
using Bulky.Models;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models.ViewModels;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _UnitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> productList = _UnitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(productList);
        }

        public IActionResult Upsert(int? id)
        {

            IEnumerable<SelectListItem> CategoryList = _UnitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            ProductVM productVM = new ProductVM
            {
                CategoryList = CategoryList,
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create
            return View(productVM);

            }
            else
            {
                //update
                productVM.Product = _UnitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl)) { 
                        //delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath,obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath)) {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (obj.Product.Id == 0) {
                _UnitOfWork.Product.Add(obj.Product);

                }
                else
                {
                    _UnitOfWork.Product.Update(obj.Product);
                }
                _UnitOfWork.Save();
                TempData["success"] = "Product Added successfull";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                obj.CategoryList = _UnitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            return View(obj);
            }
        }
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        TempData["error"] = "Must pass the product ID!";
        //        return RedirectToAction("Index", "Product");
        //    }
        //    Product product = _UnitOfWork.Product.Get(u => u.Id == id);
        //    if (product == null)
        //    {
        //        TempData["error"] = "Product not found!";
        //        return RedirectToAction("Index", "Product");
        //    }
        //    return View(product);
        //}
        //[HttpPost]
        //public IActionResult Edit(Product obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _UnitOfWork.Product.Update(obj);
        //        _UnitOfWork.Save();
        //        TempData["success"] = "Product edited successfull";
        //        return RedirectToAction("Index", "Product");
        //    }
        //    return View();
        //}
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        TempData["error"] = "Must pass the product ID!";
        //        return RedirectToAction("Index", "Product");
        //    }
        //    Product? product = _UnitOfWork.Product.Get(u => u.Id == id);
        //    //Category category1 = _db.Categories.FirstOrDefault(u => u.Id == id);
        //    //Category category2 = _db.Categories.Where(u => u.Id == id).FirstOrDefault();
        //    if (product == null)
        //    {
        //        TempData["error"] = "Product Not Found!";
        //        return RedirectToAction("Index", "Product");
        //    }
        //    return View(product);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{
        //    Product? obj = _UnitOfWork.Product.Get(u => u.Id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _UnitOfWork.Product.Remove(obj);
        //    _UnitOfWork.Save();
        //    TempData["success"] = "Product deleted successfull";
        //    return RedirectToAction("Index", "Product");

        //}


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> productList = _UnitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _UnitOfWork.Product.Get(u => u.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
            {
                //delete the old image
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _UnitOfWork.Product.Remove(productToBeDeleted);
            _UnitOfWork.Save();
            return Json(new {success = true, message = "Delete Successful" });
        }
        #endregion
    }
}

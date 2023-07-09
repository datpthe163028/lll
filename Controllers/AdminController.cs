using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using PayPal.Api;
using Project.Data;
using Project.Models;
using WebApplication6.Service;

namespace Project.Controllers
{
    public class AdminController : Controller
    {
		private readonly ShopContext _shopContext;
        private readonly ICloudinaryService _cloudinaryService;
        public AdminController(ShopContext shopContext, ICloudinaryService temp)
		{
			_shopContext = shopContext;
            _cloudinaryService = temp;
        }
        public IActionResult Index()
        {
            DateTime now = DateTime.Now;
            var x = _shopContext.Bills.Where(p => p.PurchaseDate.Year == now.Year).Where(p => p.BillStatus.Equals("3")).ToList();
            double total = 0;
            double totalMonth = 0;
            double totalDay = 0;
            SortedDictionary<int, double> myDictionary = new SortedDictionary<int, double>();

            foreach (var l in x)
            {
                if (myDictionary.ContainsKey(l.PurchaseDate.Month))
                {
                    myDictionary[l.PurchaseDate.Month] += l.TotalPrice;
                }
                else
                {
                    myDictionary.Add(l.PurchaseDate.Month, l.TotalPrice);
                }
                total += l.TotalPrice;
                if (l.PurchaseDate.Month == now.Month)
                {
                    totalMonth += l.TotalPrice;
                    if (l.PurchaseDate.Day == now.Day)
                        totalDay += l.TotalPrice;
                }
            }

            ViewData["total"] = total;
            ViewData["totalMonth"] = totalMonth;
            ViewData["totaday"] = totalDay;
            return View(myDictionary);
        }

        public IActionResult cfFeedback()
		{
			var feedbacks = _shopContext.Feedbacks.ToList();

			return View(feedbacks);
		}

		public IActionResult confirmFeedback(int feedbackId)
		{
			var feedback = _shopContext.Feedbacks.FirstOrDefault(f => f.FeedbackId == feedbackId);
			if (feedback != null)
			{
				feedback.FeedbackStatus = "1";
				_shopContext.SaveChanges();
			}

			return RedirectToAction("cfFeedback", "admin");
		}
		public IActionResult deleteFb(int feedbackId)
		{
			var feedback = _shopContext.Feedbacks.FirstOrDefault(f => f.FeedbackId == feedbackId);
			if (feedback != null)
			{
				_shopContext.Feedbacks.Remove(feedback);
				_shopContext.SaveChanges();
			}
			return RedirectToAction("cfFeedback", "admin");
		}

        //display product list
        public IActionResult DashProduct()
        {
            LoadRoleUser();
            List<Product> products = _shopContext.Products.ToList();
            return View(products);
        }


        //Delete product
        public IActionResult delProd(string productId)
        {
            LoadRoleUser();

            var product = _shopContext.Products.FirstOrDefault(p => p.ProductId == Int32.Parse(productId));
            if (product != null)
            {
                _shopContext.Products.Remove(product);
                _shopContext.SaveChanges();
                return Redirect("DashProduct");
            }
            return Redirect("Index");
        }


        //change product's Home Status
        public IActionResult changeHomeStatus(string pid)
        {
            LoadRoleUser();

            var product = _shopContext.Products.FirstOrDefault(p => p.ProductId == Int32.Parse(pid));
            if (product != null)
            {
                if (product.HomeStatus == true)
                {
                    product.HomeStatus = false;
                    _shopContext.Update(product);
                    _shopContext.SaveChanges();
                    return Redirect("DashProduct");
                }
                else
                {
                    product.HomeStatus = true;
                    _shopContext.Update(product);
                    _shopContext.SaveChanges();
                    return Redirect("DashProduct");
                }
            }

            return Redirect("Index");

        }

        //create product
        public IActionResult CreateProduct()
        {
            LoadRoleUser();

            List<SubCategory> subcate = _shopContext.SubCategory.ToList();

            return View(subcate);
        }

        [HttpPost]
        public IActionResult CreateProduct(IFormFile ImageUrl, Product product)
        {
            LoadRoleUser();

            var imageURL = _cloudinaryService.UploadImage(ImageUrl, "MainImageProduct");

            product.ImageMain = imageURL;
            _shopContext.Add(product);
            _shopContext.SaveChanges();

            return Redirect("DashProduct");
        }

        public IActionResult ViewDetailProduct(int productId)
        {
            Product product = _shopContext.Products.FirstOrDefault(x => x.ProductId == productId);
            if (product != null)
            {
                return View(product);
            }
            return Redirect("DashProduct");
        }
        public IActionResult UpdateProduct(int productId)
        {
            List<SubCategory> subcate = _shopContext.SubCategory.ToList();
            Product product = _shopContext.Products.FirstOrDefault(x => x.ProductId == productId);


            if (product != null)
            {
                ViewBag.Product = product;

                return View(subcate);
            }
            return Redirect("DashProduct");
        }

        [HttpPost]
        public IActionResult UpdateProduct(IFormFile ImageUrl, Product updateProd)
        {
            LoadRoleUser();
            Product product = _shopContext.Products.FirstOrDefault(x => x.ProductId == updateProd.ProductId);

            if (product != null)

            {
                product.ProductName = updateProd.ProductName;
                product.ProductDescription = updateProd.ProductDescription;
                product.SubCategoryID = updateProd.SubCategoryID;
                product.ImportDate = updateProd.ImportDate;
                product.ProductPrice = updateProd.ProductPrice;
                product.Discount = updateProd.Discount;
                product.HomeStatus = updateProd.HomeStatus;
                product.IsAvailble = updateProd.IsAvailble;
                if (ImageUrl != null)
                {
                    product.ImageMain = _cloudinaryService.UploadImage(ImageUrl, "MainImageProduct");
                }

                _shopContext.Update(product);
                _shopContext.SaveChanges();
                return Redirect("DashProduct");
            }





            return Redirect("DashProduct");
        }


        //Detail Product
        public IActionResult ViewDetailProd(int productId)
        {
            Product product = _shopContext.Products.FirstOrDefault(x =>x.ProductId == productId);
            if (product != null)
            {
                List<ProductDetails> pr = _shopContext.productdetails.Where(x => x.productId == productId).ToList();
                ViewBag.ProductDetails = pr;
                List<ImageProduct> imgProd = _shopContext.ImageProducts.Where(x => x.ProductId == productId).ToList();
                ViewBag.ImageProducts = imgProd;
			}
            return View(product);
        }

        //check product detail is exist
        private Boolean checkDetailExist(ProductDetails detail)
        {
            if (detail != null)
            {
                List<ProductDetails> pr = _shopContext.productdetails.Where(x => x.productId == detail.productId).ToList();
                foreach (ProductDetails a in pr)
                {
                    if (a.size == detail.size && a.color.Equals(detail.color))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        [HttpPost]
        public IActionResult CreateProductDetail(ProductDetails details)
        {
            LoadRoleUser();
            Boolean test = checkDetailExist(details);
            string mess;
            if (test)
            {
                _shopContext.Add(details);
                _shopContext.SaveChanges();
                mess = "create successfully";
            }
            else
            {
                mess = "create failed";
            }
            TempData["mess"] = mess;
            return Redirect($"ViewDetailProd?productId={details.productId}");
        }

        //delete detail product
        public IActionResult DelDetailProduct(int productDetailId)
        {
            var detailProd =_shopContext.productdetails.FirstOrDefault(x => x.productDetailId == productDetailId);
            int prodId = detailProd.productId;
            _shopContext.Remove(detailProd);
            _shopContext.SaveChanges();
            TempData["mess"] = " delete sucessfully";
            return Redirect($"ViewDetailProd?productId={prodId}");
        }


        [HttpPost]
        public IActionResult CreateImageProduct(IFormFile ImageUrl, ImageProduct imageProduct)
        {
            LoadRoleUser();

            //return Redirect($"ViewDetailProd?productId={imageProduct.ProductId}");


            var imageURL = _cloudinaryService.UploadImage(ImageUrl,"ImageProduct");

            imageProduct.ImageURL = imageURL;
            _shopContext.Add(imageProduct);
            _shopContext.SaveChanges();
            TempData["mess"] = " create sucessfully";
            return Redirect($"ViewDetailProd?productId={imageProduct.ProductId}");
        }

        //delete Image Product
        public IActionResult DelImageProduct(int ImageProductId)
        {
            ImageProduct img = _shopContext.ImageProducts.FirstOrDefault(x => x.ImageProductId == ImageProductId);
            int prodId = img.ProductId;
            _shopContext.Remove(img);
            _shopContext.SaveChanges();

            TempData["mess"] = "delete sucessfully";
            return Redirect($"ViewDetailProd?productId={prodId}");
        }

        private void LoadRoleUser()
        {
            var user = HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (user.IsInRole("Admin"))
                {
                    ViewBag.ShowAdminButton = true;
                }
                else
                {
                    ViewBag.ShowAdminButton = false;
                }

                if (user.IsInRole("Marketing"))
                {
                    ViewBag.ShowMarketingButton = true;
                }
                else
                {
                    ViewBag.ShowMarketingButton = false;
                }

                if (user.IsInRole("Seller"))
                {
                    ViewBag.ShowSellerButton = true;
                }
                else
                {
                    ViewBag.ShowSellerButton = false;
                }
            }
            else
            {
                ViewBag.ShowAdminButton = false;
                ViewBag.ShowMarketingButton = false;
                ViewBag.ShowSellerButton = false;
            }
        }

    }
}

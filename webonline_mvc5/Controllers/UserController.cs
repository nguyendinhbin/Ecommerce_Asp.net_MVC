using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using webonline_mvc5.Models;

namespace webonline_mvc3.Controllers
{
    public class UserController : Controller
    {
        private webonline_mvc5Entities1 db = new webonline_mvc5Entities1();


        // GET: User
        public ActionResult Index(int? page)
        {
            int pageSize = 9, pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.categories.Where(x => x.cat_status == 1).OrderByDescending(x => x.cat_id).ToList();
            IPagedList<category> cate = list.ToPagedList(pageIndex, pageSize);

            // Truy vấn các sản phẩm có id là 1, 2, và 3
            var featuredProducts = db.products.Where(x => x.pro_id == 8 || x.pro_id == 9 || x.pro_id == 10 || x.pro_id == 11 || x.pro_id == 12).ToList();

            // Gán danh sách sản phẩm vào ViewBag
            ViewBag.FeaturedProducts = featuredProducts;

            return View(cate);
        }





        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(tbl_user us, HttpPostedFileBase imgfile)
        {
            string path = UploadImage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "Image could not be uploaded";
                return View();
            }

            try
            {
                tbl_user u = new tbl_user();
                u.u_name = us.u_name;
                u.u_password = us.u_password;
                u.u_phone = us.u_phone;
                u.u_email = us.u_email;
                u.u_image = path;

                object value = db.tbl_user.Add(u);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.error = "An error occurred while registering. Please try again.";
                return View();
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(tbl_user svm)
        {
            tbl_user ad = db.tbl_user.FirstOrDefault(x => x.u_email == svm.u_email && x.u_password == svm.u_password);

            if (ad != null)
            {
                Session["u_id"] = ad.u_id.ToString();
                Session["user"] = ad.u_name;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.error = "Invalid Email or Password";
            }
            return View();
        }




        [HttpGet]
        public ActionResult CreateAdd()
        {
            List<category> li = db.categories.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");

            return View();
        }

        [HttpPost]
        public ActionResult CreateAdd(product p, HttpPostedFileBase imgfile)
        {
            List<category> li = db.categories.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");

            string path = UploadImage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "Image could not be uploaded";
                return View();
            }

            else
            {
                product pr = new product();
                pr.pro_name = p.pro_name;
                pr.pro_price = p.pro_price;
                pr.pro_image = path;
                pr.cat_id_fk = p.pro_user_id_fk;
                pr.pro_desc = p.pro_desc;

                // Check if "u_id" session variable is set
                if (Session["u_id"] != null)
                {
                    pr.pro_user_id_fk = Convert.ToInt32(Session["u_id"].ToString());
                    db.products.Add(pr);
                    db.SaveChanges();

                    Response.Redirect("Index");
                }

                return View();
            }
        }


        [HttpPost]
        public ActionResult DisplayAdd(int? id, int? page)
        {
            int pageSize = 9, pageIndex = page ?? 1;

            var query = db.products.AsQueryable();

            // Filter products by category ID if provided
            if (id.HasValue)
            {
                query = query.Where(x => x.cat_id_fk == id);
                // Set ViewBag to retain CategoryId for pagination links
                ViewBag.CategoryId = id;
            }

            // Order products by ID or other criteria as needed
            query = query.OrderByDescending(x => x.pro_id);

            // Paginate the results
            IPagedList<product> cate = (IPagedList<product>)query.ToPagedList(pageIndex, pageSize);
            return View(cate);
        }

        [HttpGet]
        public ActionResult DisplayAdd(int? id, int? page, string search)
        {
            int pageSize = 9, pageIndex = page ?? 1;

            var query = db.products.AsQueryable();

            // Filter products by category ID if provided
            if (id.HasValue)
            {
                query = query.Where(x => x.cat_id_fk == id);
                // Set ViewBag to retain CategoryId for pagination links
                ViewBag.CategoryId = id;
            }

            // Filter products by search query if provided
            if (!string.IsNullOrEmpty(search))
            {
                int productId;
                // Check if search string is a valid integer (product id)
                if (int.TryParse(search, out productId))
                {
                    // Redirect to the route with product id as parameter
                    return RedirectToAction("ViewAdds", new { id = productId });
                }
                else
                {
                    // If not a valid integer, search by product name
                    query = query.Where(x => x.pro_name.Contains(search));
                }
            }

            // Order products by ID or other criteria as needed
            query = query.OrderByDescending(x => x.pro_id);

            // Paginate the results
            IPagedList<product> cate = (IPagedList<product>)query.ToPagedList(pageIndex, pageSize);
            return View(cate);
        }




            // Image upload
            public string UploadImage(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                Random r = new Random();
                int random = r.Next();

                string path = "-1";
                string extension = Path.GetExtension(file.FileName);

                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {
                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as required
                        ViewBag.error = "An error occurred while uploading the image.";
                    }
                }
                else
                {
                    // Unsupported file format
                    ViewBag.error = "Only jpg, jpeg, or png formats are acceptable.";
                }

                return path;
            }
            else
            {
                // No file selected
                ViewBag.error = "Please select a file.";
                return "-1";
            }
        }


        //end

        public ActionResult SignOut()
        {
            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("Login", "User");
        }

      
        public ActionResult Add_Delete(int? id)

        {
            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            db.products.Remove(p);
            db.SaveChanges();

            return RedirectToAction("Index", "User");
        }
        public ActionResult ViewAdds(int? id)
        {
            ad_view_model adm = new ad_view_model();

            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            if (p != null)
            {
                adm.pro_id = p.pro_id;
                adm.pro_name = p.pro_name;
                adm.pro_image = p.pro_image;
                adm.pro_price = p.pro_price;
                adm.pro_desc = p.pro_desc;

                category cat = db.categories.Where(x => x.cat_id == p.cat_id_fk).SingleOrDefault();
                if (cat != null)
                {
                    adm.cat_name = cat.cat_name; // Set the Category name in the Model
                }

                tbl_user u = db.tbl_user.Where(x => x.u_id == p.pro_user_id_fk).SingleOrDefault();
                if (u != null)
                {
                    adm.u_name = u.u_name;
                    adm.u_image = u.u_image;
                    adm.u_phone = u.u_phone;
                    adm.pro_user_id_fk = u.u_id;

                    // Check if the logged-in user is the owner of the product
                    if (Session["u_id"] != null && Convert.ToInt32(Session["u_id"]) == adm.pro_user_id_fk)
                    {
                        ViewBag.IsOwner = true; // Set this condition to ViewBag to use it in the View
                    }
                    else
                    {
                        ViewBag.IsOwner = false; // Set to false if the logged-in user is not the owner
                    }
                }
                return View(adm);
            }
            // Return a default view if the product is not found
            return View("NotFound");
        }


        public ActionResult Ad_tocart(int? id)
        {
            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            return View(p);

        }




        List<cart> li = new List<cart>();

        [HttpPost]

        public ActionResult Ad_tocart(product pr, string qty, int id)
        {
            // Retrieve the existing cart items from TempData
            List<cart> li = TempData["cart"] as List<cart> ?? new List<cart>();

            // Find the product by id
            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();

            if (p != null)
            {
                cart c = new cart();
                c.pro_id = p.pro_id;
                c.pro_name = p.pro_name;
                c.pro_price = p.pro_price;
                c.o_qty = Convert.ToInt32(qty);
                c.o_bill = c.pro_price * c.o_qty;
                if (TempData["cart"] == null)
                {


                    // Add the new item to the cart
                    li.Add(c);

                    // Store the updated cart items back into TempData
                    TempData["cart"] = li;
                }
                else
                {
                    List<cart> li2 = TempData["cart"] as List<cart>;
                    int flag = 0;
                    foreach (var item in li2)
                    {
                        if (item.pro_id == c.pro_id)
                        {
                            item.o_qty += c.o_qty;
                            item.o_bill += c.o_bill;
                            flag = 1;
                        }
                    }
                    if (flag == 0)
                    {
                        li.Add(c);
                        // item is new.....
                    }
                    TempData["cart"] = li2;
                }
                TempData.Keep();
                // Update the count of items in the cart
                TempData["cartCount"] = li.Count;
            }
            else
            {
                // Handle the case where the product with the specified id is not found
                ViewBag.Error = "Product not found.";
                return View();
            }

            return RedirectToAction("Index", "User");
        }


        public ActionResult remove(int? id)
        {
            List<cart> li2 = TempData["cart"] as List<cart>;
            cart c =li2.Where(x => x.pro_id == id).SingleOrDefault();
            li2.Remove(c);
            int h = 0;
            foreach (var item in li2)
            {
                h += (Convert.ToInt32(item.o_bill));
            }
            TempData["total"] = h;


            return RedirectToAction("checkout");
            
        }



        public ActionResult checkout()
        {
            // Lấy danh sách sản phẩm từ TempData
            List<cart> cartList = TempData["cart"] as List<cart>;
            if (cartList != null && cartList.Any())
            {
                // Lưu giữ TempData
                TempData.Keep("cart");

                return View(cartList);
            }
            else
            {
                ViewBag.ErrorMessage = "Cart is empty";
                return View();
            }
        }

        [HttpPost]
        public ActionResult checkout(order_table O)
        {
            // Retrieve the cart items from TempData
            List<cart> cartItems = TempData["cart"] as List<cart>;

            // Check if cartItems is null or empty
            if (cartItems == null || !cartItems.Any())
            {
                // Redirect to checkout page with an error message
                TempData["msg"] = "Error: Cart is empty.";
                return RedirectToAction("Checkout");
            }

            // Calculate total bill
            decimal totalBill = (decimal)cartItems.Sum(item => item.o_bill);

            // Check if totalBill is greater than zero
            if (totalBill > 0)
            {
                // Create a new invoice instance
                tbl_invoice invoice = new tbl_invoice();
                invoice.in_date = DateTime.Now;
                invoice.in_fk_user = Convert.ToInt32(Session["u_id"].ToString());
                invoice.in_totalbill = (double?)totalBill;
                db.tbl_invoice.Add(invoice);
                db.SaveChanges();

                // Loop through the items in the cart and create order_table entries
                foreach (var cartItem in cartItems)
                {
                    // Check if the product exists in the database
                    product product = db.products.FirstOrDefault(p => p.pro_id == cartItem.pro_id);
                    if (product != null)
                    {
                        // Create order_table entry
                        order_table order = new order_table();
                        order.o_fk_pro = cartItem.pro_id;
                        order.o_fk_invoice = invoice.in_id;
                        order.o_date = DateTime.Now;
                        order.o_qty = cartItem.o_qty;
                        order.o_unitprice = cartItem.pro_price;
                        order.o_bill = cartItem.o_bill;
                        order.o_fk_user = Convert.ToInt32(Session["u_id"].ToString());
                        db.order_table.Add(order);
                    }
                    else
                    {
                        // Handle the case where the product with the specified id is not found
                        ViewBag.Error = "Product with ID " + cartItem.pro_id + " not found.";
                        return RedirectToAction("Index", "User");
                    }
                }
                db.SaveChanges();

                TempData["total"] = (double)totalBill; // Convert totalBill to double for TempData
                TempData["msg"] = "Transaction Successfully Completed.....";
            }
            else
            {
                // Redirect to checkout page with an error message
                TempData["msg"] = "Error: Total is not set.";
                return RedirectToAction("Checkout");
            }

            TempData.Keep(); // Keep TempData after this point

            return RedirectToAction("Checkout");
        }


      
    }
}
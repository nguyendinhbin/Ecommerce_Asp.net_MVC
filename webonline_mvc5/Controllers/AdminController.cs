using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using webonline_mvc5.Models;
using PagedList.Mvc;
using PagedList;
using System.Data.Entity;

namespace webonline_mvc5.Controllers
{
    public class AdminController : Controller
    {
        webonline_mvc5Entities1 db = new webonline_mvc5Entities1();


        [HttpGet]
        // GET: Admin
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(admin adm)
        {
            admin ad = db.admins.Where(x => x.ad_name == adm.ad_name && x.ad_password == adm.ad_password).SingleOrDefault();
            if (ad != null)
            {
                Session["ad_id"] = ad.ad_id.ToString();
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.error = "Invalid User Name or Password";

            }
            return View();
        }



        // Action method to list users
        public ActionResult ListUsers()
        {
            var users = db.tbl_user.ToList();
            return View(users);
        }

        // Action method to add a new user
        [HttpGet]
        public ActionResult AddUser()
        {
            return View();
        }

       [HttpPost]
public ActionResult AddUser(tbl_user user)
{
    if (ModelState.IsValid)
    {
        // Add user to the database
        tbl_user newUser = db.tbl_user.Add(user);
        // Save changes to the database
        db.SaveChanges();

        return RedirectToAction("ListUsers");
    }
    return View(user);
}


        // Action method to edit a user
        [HttpGet]
        public ActionResult EditUser(string id) 
        {
            var user = db.tbl_user.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Edituser(Applicationtuser user)
        {
            if (ModelState.IsValid)
            {
                // Update user in the database
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListUsers");
            }
            return View(user);
        }

        // Action method to delete a user
        public ActionResult DeleteUser(string id)
        {
            var user = db.tbl_user.Find(id);
            if (user != null)
            {
                // Delete user from the database
                db.tbl_user.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("ListUsers");
        }
    

    public ActionResult Category()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Category(category cat, HttpPostedFileBase imgfile)
        {


            admin ad = new admin();

            string path = UploadImage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "Image could not be uploaded";
            }
            else
            {
                category ca = new category();
                ca.cat_name = cat.cat_name;
                ca.cat_image = path;
                ca.cat_status = 1;
                ca.ad_id_fk = Convert.ToInt32(Session["ad_id"].ToString());
                db.categories.Add(ca);
                db.SaveChanges();

                return RedirectToAction("ViewCategory");
            }

            return View();
        }

        public ActionResult ViewCategory(int? page)
        {
            int pageSize = 9, pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.categories.Where(x => x.cat_status == 1).OrderByDescending(x => x.cat_id).ToList();
            IPagedList<category> cate = list.ToPagedList(pageIndex, pageSize);

            return View(cate);
        }

        public ActionResult DeleteCategory(int id)
        {
            // Tìm danh mục cần xóa từ id
            category cat = db.categories.Find(id);
            if (cat != null)
            {
                // Xóa danh mục khỏi cơ sở dữ liệu
                db.categories.Remove(cat);
                db.SaveChanges();
            }
            // Chuyển hướng về trang danh sách danh mục
            return RedirectToAction("ViewCategory");
        }

        [HttpGet]
        public ActionResult EditCategory(int id)
        {
            // Tìm danh mục cần chỉnh sửa từ id
            category cat = db.categories.Find(id);
            if (cat != null)
            {
                // Trả về view với thông tin của danh mục để chỉnh sửa
                return View(cat);
            }
            // Nếu không tìm thấy danh mục, chuyển hướng về trang danh sách danh mục
            return RedirectToAction("ViewCategory");
        }

        [HttpPost]
        public ActionResult EditCategory(category cat, HttpPostedFileBase imgfile)
        {
            // Tìm danh mục cần chỉnh sửa từ id
            category existingCat = db.categories.Find(cat.cat_id);
            if (existingCat != null)
            {
                // Cập nhật thông tin của danh mục với thông tin mới
                existingCat.cat_name = cat.cat_name;

                // Kiểm tra xem có tệp tin hình ảnh mới được chọn hay không
                if (imgfile != null && imgfile.ContentLength > 0)
                {
                    // Nếu có, tải lên và cập nhật đường dẫn hình ảnh mới
                    string path = UploadImage(imgfile);
                    if (!path.Equals("-1"))
                    {
                        existingCat.cat_image = path;
                    }
                    else
                    {
                        ViewBag.error = "Image could not be uploaded";
                        return View(cat);
                    }
                }
                // Lưu các thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
            }
            // Chuyển hướng về trang danh sách danh mục
            return RedirectToAction("ViewCategory");
        }

        public ActionResult ViewProducts()
        {
            // Retrieve the list of products from the database
            var products = db.products.ToList();

            // Pass the list of products to the ViewProducts view for display
            return View(products);
        }


        public ActionResult Index()
        {
            // Truy vấn danh sách sản phẩm từ cơ sở dữ liệu
            var products = db.products.ToList();

            // Truyền danh sách sản phẩm đến view để hiển thị
            return View(products);
        }


        [HttpGet]
        public ActionResult AddProduct()
        {
            // Populate ViewBag with categories for dropdown list
            ViewBag.CategoryList = new SelectList(db.categories, "cat_id", "cat_name");
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(product product, HttpPostedFileBase imgfile)
        {
            // Validate ModelState
            if (ModelState.IsValid)
            {
                // Upload image and save path
                string imagePath = UploadImage(imgfile);

                // Check if image uploaded successfully
                if (!imagePath.Equals("-1"))
                {
                    // Set product image path
                    product.pro_image = imagePath;

                    // Check if Session["ad_id"] is null
                    if (Session["ad_id"] != null)
                    {
                        // Set pro_user_id_fk from session
                        int adminId = Convert.ToInt32(Session["ad_id"].ToString());
                        product.pro_user_id_fk = adminId;

                        // Add product to database
                        db.products.Add(product);
                        db.SaveChanges();

                        // Redirect to product list view
                        return RedirectToAction("ViewProducts");
                    }
                    else
                    {
                        // Handle the case where Session["ad_id"] is null
                        ViewBag.Error = "User not logged in";
                        return RedirectToAction("Login"); // Redirect to login page or handle it appropriately
                    }
                }
                else
                {
                    ViewBag.Error = "Image could not be uploaded";
                }
            }

            // If ModelState is not valid, return the form with validation errors
            ViewBag.CategoryList = new SelectList(db.categories, "cat_id", "cat_name", product.cat_id_fk);
            return View(product);
        }



        public ActionResult EditProduct(int id)
        {
            // Tìm sản phẩm cần sửa
            product p = db.products.Find(id);
            if (p != null)
            {
                // Lấy danh sách category từ database và chuyển đổi sang SelectList
                var categories = db.categories.Select(c => new SelectListItem
                {
                    Value = c.cat_id.ToString(),
                    Text = c.cat_name
                }).ToList();

                ViewBag.CategoryList = categories;

                // Trả về view với thông tin của sản phẩm để chỉnh sửa
                return View(p);
            }
            // Nếu không tìm thấy sản phẩm, chuyển hướng về trang danh sách sản phẩm
            return RedirectToAction("ViewProducts");
        }



        [HttpPost]
        public ActionResult EditProduct(product p, HttpPostedFileBase imgfile)
        {
            // Tìm sản phẩm cần sửa trong cơ sở dữ liệu
            product existingProduct = db.products.Find(p.pro_id);
            if (existingProduct != null)
            {
                // Cập nhật thông tin của sản phẩm với thông tin mới
                existingProduct.pro_name = p.pro_name;
                existingProduct.pro_price = p.pro_price;
                existingProduct.cat_id_fk = p.cat_id_fk; // Chỉnh sửa tương ứng với thuộc tính trong model của bạn
                existingProduct.pro_desc = p.pro_desc;

                // Kiểm tra xem có hình ảnh mới được chọn không
                if (imgfile != null && imgfile.ContentLength > 0)
                {
                    // Upload hình ảnh mới và cập nhật đường dẫn hình ảnh
                    string path = UploadImage(imgfile);
                    if (!path.Equals("-1"))
                    {
                        existingProduct.pro_image = path;
                    }
                    else
                    {
                        ViewBag.error = "Image could not be uploaded";
                        return View(p);
                    }
                }
                // Lưu các thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
            }
            // Chuyển hướng về trang danh sách sản phẩm
            return RedirectToAction("ViewProducts");
        }

        public ActionResult DeleteProduct(int id)
        {
            // Find the product to delete
            product p = db.products.Find(id);
            if (p != null)
            {
                // Find and delete related records in order_table
                var relatedOrders = db.order_table.Where(o => o.o_fk_pro == id).ToList();
                db.order_table.RemoveRange(relatedOrders);

                // Now delete the product
                db.products.Remove(p);

                // Save changes
                db.SaveChanges();
            }

            // Redirect to the product list view
            return RedirectToAction("ViewProducts");
        }

        public ActionResult Ad_tocart(int? id)
        {
            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            return View(p);

        }


        // GET: Admin
        public ActionResult Dashboard()
        {
            // Đảm bảo rằng chỉ admin mới có thể truy cập trang Dashboard
            if (Session["ad_id"] == null)
            {
                // Nếu không phải admin, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login");
            }
            // Nếu là admin, trả về view cho trang Dashboard
            return View();
        }



        public ActionResult Total_Bill(order_table O)
        {
            return View(db.order_table.ToList());
        }



        public string UploadImage(HttpPostedFileBase file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {
                        path = Path.Combine(Server.MapPath("~/Content/upload/"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);

                        // ViewBag.Message = "File uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        path = "-1";
                    }
                }
                else
                {
                    ViewBag.error = "Only jpg, jpeg, or png formats are acceptable.";
                }
            }
            else
            {
                ViewBag.error = "Please select a file.";
            }
            return path;
        }
    }
}

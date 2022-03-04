using API.Dtos;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using  Microsoft.AspNetCore.Http;

namespace API.Controllers
{
    public class SellerController
    {
        private readonly StoreContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGenericRepository<SellerProductlist> _sellerproductrepo;

        public SellerController(IGenericRepository<SellerProductlist> sellerproductrepo, StoreContext context, IWebHostEnvironment webHostEnvironment)
        {
            _sellerproductrepo = sellerproductrepo;
            _webHostEnvironment = webHostEnvironment;
            _context = context;

        }
        [HttpPost("SellerProduct")]
        public async Task<List<Product>> ProductUpload(IFormFile file, List<IFormFile> files,string sellername)
        {
            var list=new List<Product>();
            using(var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using(var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet= package.Workbook.Worksheets[0];
                    var rowcount = worksheet.Dimension.Rows;
                    for(int row = 2;row<rowcount;row++)
                    {
                        list.Add(new Product{
                            //Id=Convert.ToInt32(worksheet.Cells[row,0].Value),
                            Name = worksheet.Cells[row,1].Value.ToString().Trim(),
                            Description =worksheet.Cells[row,2].Value.ToString().Trim(),
                            Price=Convert.ToDecimal(worksheet.Cells[row,3].Value),
                            RentedPrice = Convert.ToDecimal(worksheet.Cells[row,4].Value),
                            PictureUrl =worksheet.Cells[row,5].Value.ToString().Trim(),
                            ProductTypeId=Convert.ToInt32(worksheet.Cells[row,6].Value),
                            ProductBrandId=Convert.ToInt32(worksheet.Cells[row,7].Value),
                            CategoryId=Convert.ToInt32(worksheet.Cells[row,8].Value),

                            
                        });
                    }
                }
            }
            foreach(var x in list)
            {
              //var email = HttpContext.User.RetrieveEmailFromPrincipal();
              
              await  _context.Products.AddAsync(x);
              await _context.SaveChangesAsync();
              var seller=new SellerProductlist();
              seller.Id=x.Id;
              seller.productid=x.Id;
              seller.sellername=sellername;
              await _context.productlists.AddAsync(seller);
              await _context.SaveChangesAsync();
            }

            foreach(var product in files)
            {

                 string wwwRootPath = _webHostEnvironment.WebRootPath;

                string fileName = Path.GetFileNameWithoutExtension(product.FileName);
                string extension = Path.GetExtension(product.FileName);

                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                string path = Path.Combine(wwwRootPath + "/images/products/", fileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {

                    await product.CopyToAsync(fileStream);
                }
            }

            return list;
        }

        /*[HttpDelete("selllerproductdelete")]
        public async Task<string> DeleteSellerProduct(int id, string Sellername)
        {

            var remove = _context.SProductlist;
            var obj = new SellerProductlist();
            bool flag = false;
            foreach (var x in remove)
            {
                if (x.Id == id)
                {
                    if (x.sellername == Sellername)
                    {
                        obj = x;
                        flag = true;
                    }
                }
            }

            if (flag)
            {
                _context.Products.Remove(await _context.Products.FindAsync(obj.productid));
                await _context.SaveChangesAsync();
                _context.SProductlist.Remove(obj);
                await _context.SaveChangesAsync();
                return "Product Deleted succcessfully";
            }

            return "Product does not exist";

        }*/
      /*  [HttpGet("sellerproductslist")]
        public async Task<ActionResult<List<ProductToReturnDto>>> GetProductdatabyuser(string username, [FromQuery] ProductSpecParams specParams)
        {
            //Task<IList<ProductToReturnDto>> 
            var sellerproductlist = await _sellerproductrepo.ListAllAsync();
            var x = new List<ProductToReturnDto>();
            foreach (var element in sellerproductlist)
            {
                if (element.sellername == username)
                {
                    var product = await _context.Products.FindAsync(element.productid);
                    // product.ProductBrand.Name= _context.ProductBrands.Find(product.ProductBrandId).Name;
                    var product1 = new ProductToReturnDto();
                    product1.Id = product.Id;
                    product1.Name = product.Name;
                    product1.Description = product.Description;
                    product1.Price = product.Price;
                    product1.RentedPrice = product.RentedPrice;
                    product1.PictureUrl = product.PictureUrl;
                    product1.ProductType = _context.ProductTypes.Find(product.ProductTypeId).Name;
                    product1.ProductBrand = _context.ProductBrands.Find(product.ProductBrandId).Name;
                    product1.Category = _context.Categories.Find(product.CategoryId).Name;
                    x.Add(product1);
                }
            }

            return x;


        }*/
        /*[HttpPost("upload Products")]
        public async Task<string> UploadSellerProduct([FromForm] SellerProduct product)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            string fileName = Path.GetFileNameWithoutExtension(product.PictureUrl.FileName);
            string extension = Path.GetExtension(product.PictureUrl.FileName);

            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

            string path = Path.Combine(wwwRootPath + "/images/products/", fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {

                await product.PictureUrl.CopyToAsync(fileStream);
            }

            Product product1 = new Product();
            product1.Name = product.Name;
            product1.Description = product.Description;
            product1.Price = product.Price;
            product1.RentedPrice = product.RentedPrice;
            product1.ProductTypeId = product.ProductTypeId;
            product1.ProductBrandId = product.ProductBrandId;
            product1.CategoryId = product.CategoryId;
            product1.ProductBrand = _context.ProductBrands.Find(product1.ProductBrandId);
            product1.PictureUrl = "images/products/" + fileName;
            // _productsRepo.Add(product1);
            _context.Products.Add(product1);
            await _context.SaveChangesAsync();

            SellerProductlist sellerProductlist = new SellerProductlist();
            sellerProductlist.sellername = "Ram";
            sellerProductlist.productid = product1.Id;
            // _sellerproductrepo.Add(sellerProductlist);
            _context.SProductlist.Add(sellerProductlist);
            await _context.SaveChangesAsync();
            return "Data upload successfully";
        }*/
    }
}
using _66033872_Yossayut_rutatip.Models.db;

namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class ProductCreateViewModel
    {
        public ushort CategoryId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int StockQty { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Description { get; set; }
    }

    public class MenuProductsViewModel
    {
        public List<Product> ProductList { get; set; } = new List<Product>();
        public List<Category> CategoryList { get; set; } = new List<Category>();
        public ushort? SelectedCategoryId { get; set; }
        public string? SelectedStatus { get; set; }
        public string? Keyword { get; set; }
        public string SortBy { get; set; } = "newest";
        public ProductCreateViewModel ProductForm { get; set; } = new ProductCreateViewModel();
        public ProductEditViewModel EditProductForm { get; set; } = new ProductEditViewModel();
        public CategoryCreateViewModel CategoryForm { get; set; } = new CategoryCreateViewModel();
    }

    public class ProductEditViewModel
    {
        public ulong ProductId { get; set; }
        public ushort CategoryId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int StockQty { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }

    public class CategoryCreateViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
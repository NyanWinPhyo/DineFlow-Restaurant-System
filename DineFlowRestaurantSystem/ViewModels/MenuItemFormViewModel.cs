using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class MenuItemFormViewModel
    {
        public int? MenuItemID { get; set; }

        [Required]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryID { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Food Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImagePath { get; set; }

        public bool RemoveImage { get; set; }

        public List<MenuCategoryOptionViewModel> Categories { get; set; } = new();
    }
}
using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class MenuCategoryFormViewModel
    {
        public int? CategoryID { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
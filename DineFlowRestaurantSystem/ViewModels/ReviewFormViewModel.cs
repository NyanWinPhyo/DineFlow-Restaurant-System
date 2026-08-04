using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class ReviewFormViewModel
    {
        public int OrderID { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Display(Name = "Comment")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }
}
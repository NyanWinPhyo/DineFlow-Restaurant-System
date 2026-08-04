using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class ReviewResponseViewModel
    {
        public int FeedbackID { get; set; }

        [Display(Name = "Management Response")]
        [StringLength(1000, ErrorMessage = "Response cannot exceed 1000 characters.")]
        public string? AdminResponse { get; set; }

        [Display(Name = "Mark as Reviewed")]
        public bool IsReviewed { get; set; }
    }
}
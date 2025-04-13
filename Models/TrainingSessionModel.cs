using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BeFit.Models
{
    public class TrainingSession
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
        [Display(Name = "Data rozpoczęcia")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana")]
        [Display(Name = "Data zakończenia")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [StringLength(200, ErrorMessage = "Notatki nie mogą przekraczać 200 znaków")]
        [Display(Name = "Notatki")]
        public string Notes { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public virtual IdentityUser User { get; set; }

        public ICollection<CompletedExercise> CompletedExercises { get; set; } = new List<CompletedExercise>();
    }
}
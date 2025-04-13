using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BeFit.Models
{
    public class CompletedExercise
    {
        public int Id { get; set; }

        [Required]
        public int TrainingSessionId { get; set; }

        [Required]
        public int ExerciseTypeId { get; set; }

        [Required(ErrorMessage = "Liczba serii jest wymagana")]
        [Range(1, 100, ErrorMessage = "Liczba serii musi być pomiędzy 1 a 100")]
        [Display(Name = "Liczba serii")]
        public int Sets { get; set; }

        [Required(ErrorMessage = "Liczba powtórzeń jest wymagana")]
        [Range(1, 1000, ErrorMessage = "Liczba powtórzeń musi być pomiędzy 1 a 1000")]
        [Display(Name = "Powtórzenia w serii")]
        public int Reps { get; set; }

        [Required(ErrorMessage = "Obciążenie jest wymagane")]
        [Range(0, 1000, ErrorMessage = "Obciążenie musi być pomiędzy 0 a 1000 kg")]
        [Display(Name = "Obciążenie (kg)")]
        public double Weight { get; set; }

        [StringLength(200, ErrorMessage = "Notatki nie mogą przekraczać 200 znaków")]
        [Display(Name = "Notatki")]
        public string Notes { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public virtual IdentityUser User { get; set; }

        [ForeignKey("TrainingSessionId")]
        [ValidateNever]
        public virtual TrainingSession TrainingSession { get; set; }

        [ForeignKey("ExerciseTypeId")]
        [ValidateNever]
        public virtual ExerciseType ExerciseType { get; set; }
    }
}
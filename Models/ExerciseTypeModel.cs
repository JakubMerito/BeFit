using System.ComponentModel.DataAnnotations;

namespace BeFit.Models
{
    public class ExerciseType
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nazwa musi mieć od 3 do 100 znaków")]
        [Display(Name = "Nazwa ćwiczenia")]
        public string Name { get; set; }


        [StringLength(500, ErrorMessage = "Opis może mieć maksymalnie 500 znaków")]
        [Display(Name = "Opis")]
        public string? Description { get; set; }


        [Display(Name = "Kategoria")]
        [Required(ErrorMessage = "Kategoria jest wymagana")]
        [StringLength(50, ErrorMessage = "Kategoria nie może przekraczać 50 znaków")]
        public string Category { get; set; }


        public ICollection<CompletedExercise> CompletedExercises { get; set; } = new List<CompletedExercise>();
    }
}
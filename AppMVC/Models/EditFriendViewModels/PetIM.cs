using System.ComponentModel.DataAnnotations;
using Models.DTO;
using Models.Interfaces;
using Models;

namespace AppMVC.Models.EditFriendViewModels
{
    public class PetIM
    {
        public StatusIM StatusIM { get; set; }
        public Guid PetId { get; set; }

        [Required(ErrorMessage = "You must provide a pet name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "You must select an animal kind")]
        public AnimalKind Kind { get; set; }

        [Required(ErrorMessage = "You must select an animal mood")]
        public AnimalMood Mood { get; set; }

        [Required(ErrorMessage = "You must provide a pet name")]
        public string editName { get; set; }

        [Required(ErrorMessage = "You must select an animal kind")]
        public AnimalKind editKind { get; set; }

        [Required(ErrorMessage = "You must select an animal mood")]
        public AnimalMood editMood { get; set; }

        public PetIM() { }

        public PetIM(PetIM original)
        {
            StatusIM = original.StatusIM;
            PetId = original.PetId;
            Name = original.Name;
            Kind = original.Kind;
            Mood = original.Mood;
            editName = original.editName;
            editKind = original.editKind;
            editMood = original.editMood;
        }

        public PetIM(IPet model)
        {
            StatusIM = StatusIM.Unchanged;
            PetId = model.PetId;
            Name = editName = model.Name;
            Kind = editKind = model.Kind;
            Mood = editMood = model.Mood;
        }

        public IPet UpdateModel(IPet model)
        {
            model.PetId = this.PetId;
            model.Name = this.Name;
            model.Kind = this.Kind;
            model.Mood = this.Mood;
            return model;
        }

        public PetCuDto CreateCUdto() => new PetCuDto()
        {
            PetId = null,
            Name = this.Name,
            Kind = this.Kind,
            Mood = this.Mood
        };
    }
}
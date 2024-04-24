using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class MaterialValidator : FluentValidation.AbstractValidator<Material>
    {
        public MaterialValidator()
        {

            this.RuleFor(x => x.NameMaterial)
                .Must(x => x.ToUpper().First() == x.First())
                .WithMessage("Первая буква названия материала должна быть заглавной!")
                .MinimumLength(4)
                .WithMessage("Название материала не должно быть меньше 4 символов")
                .MaximumLength(100)
                .WithMessage("Название материала не должно превышать 100 символов");

            this.RuleFor(x => x.DisciplineId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите дисциплину!");

            this.RuleFor(x => x.TypeMaterialId)
               .Must(x => !x.Equals(0))
               .WithMessage("Выберите тип материала!");



        }

    }
}

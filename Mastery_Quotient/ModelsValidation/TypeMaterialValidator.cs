using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class TypeMaterialValidator : FluentValidation.AbstractValidator<TypeMaterial>
    {
        public TypeMaterialValidator() 
        {
            this.RuleFor(x => x.NameTypeMaterial)
                .Must(x => x.ToUpper().First() == x.First())
                .WithMessage("Первая буква типа материала должна быть заглавной!")
                .MaximumLength(50)
                .WithMessage("Тип материала не должен превышать 50 символов!");
        }
    }
}

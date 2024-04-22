using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class DisciplineValidator : FluentValidation.AbstractValidator<Discipline>
    {
        public DisciplineValidator() 
        {

            this.RuleFor(x => x.NameDiscipline)
                .Must(x => x.ToUpper().First() == x.First())
                .WithMessage("Первая буква названия дисциплины должна быть заглавной!")
                .MinimumLength(5)
                .WithMessage("Название дисциплины не должно быть меньше 5 символов");


            this.RuleFor(x => x.CourseId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите курс!");
        }
    }
}

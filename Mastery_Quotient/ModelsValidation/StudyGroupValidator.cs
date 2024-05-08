using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class StudyGroupValidator : FluentValidation.AbstractValidator<StudyGroup>
    {
        public StudyGroupValidator()
        {
            this.RuleFor(x => x.NameStudyGroup)
            .Matches(@".*-\d{2}$")
            .WithMessage("Название учебной группы не соответствует формату!")
            .Must(x => x.ToUpper().First() == x.First())
            .WithMessage("Первая буква названия учебной группы должна быть заглавной!")
            .MaximumLength(15)
            .WithMessage("Название учебной группы не должно превышать 15 символов!")
            .MinimumLength(5)
            .WithMessage("Название учебной группы не должно быть меньше 5 символов!");

            this.RuleFor(x => x.CourseId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите курс!");

        }
    }
}

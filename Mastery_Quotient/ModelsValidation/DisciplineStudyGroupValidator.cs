using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class DisciplineStudyGroupValidator : FluentValidation.AbstractValidator<DisciplineOfTheStudyGroup>
    {
        public DisciplineStudyGroupValidator()
        {
            this.RuleFor(x => x.DisciplineId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите дисциплину!");

            this.RuleFor(x => x.StudyGroupId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите учебную группу!");
        }
    }
}

using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class DisciplineEmployeeValidator : FluentValidation.AbstractValidator<DisciplineEmployee>
    {
        public DisciplineEmployeeValidator()
        {
            this.RuleFor(x => x.DisciplineId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите дисциплину!");

            this.RuleFor(x => x.EmployeeId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите сотрудника!");
        }
    }
}

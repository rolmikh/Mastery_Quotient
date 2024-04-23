using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class StudyGroupEmployeeValidator : FluentValidation.AbstractValidator<EmployeeStudyGroup>
    {
        public StudyGroupEmployeeValidator() 
        {
            this.RuleFor(x => x.EmployeeId)
                 .Must(x => !x.Equals(0))
                 .WithMessage("Выберите сотрудника!");

            this.RuleFor(x => x.StudyGroupId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите учебную группу!");
        }
    }
}

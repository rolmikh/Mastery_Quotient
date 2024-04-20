using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class EmployeeValidator :FluentValidation.AbstractValidator<Employee>
    {
        public EmployeeValidator() 
        {
            this.RuleFor(x => x.EmailEmployee)
                .EmailAddress().WithMessage("Некорректный формат адреса электронной почты")
                .Matches(@"^[a-zA-Z0-9._%+-]+@mpt.ru$").WithMessage("Некорректный формат адреса электронной почты. Электронная почта должна принадлежать домену МПТ");

            
        }
    }
}

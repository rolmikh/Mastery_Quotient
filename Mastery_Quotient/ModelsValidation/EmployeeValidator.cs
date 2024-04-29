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

            this.RuleFor(x => x.SurnameEmployee)
                .Must(x => string.IsNullOrEmpty(x) || x.ToUpper().First() == x.First())
                .WithMessage("Первая буква фамилии должна быть заглавной!");

            this.RuleFor(x => x.NameEmployee)
                .Must(x => string.IsNullOrEmpty(x) || x.ToUpper().First() == x.First())
                .WithMessage("Первая буква имени должна быть заглавной!");

            this.RuleFor(x => x.MiddleNameEmployee)
                .Must(x => string.IsNullOrEmpty(x) || x.ToUpper().First() == x.First())
                .WithMessage("Первая буква отчества должна быть заглавной!");

            this.RuleFor(x => x.PasswordEmployee)
                .MinimumLength(8)
                .WithMessage("Пароль должен содержать больше 8 символов!");

            this.RuleFor(x => x.RoleId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите роль сотрудника!");
        }
    }
}

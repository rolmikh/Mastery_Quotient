﻿using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class EmployeeValidator :FluentValidation.AbstractValidator<Employee>
    {
        public EmployeeValidator() 
        {
            this.RuleFor(x => x.EmailEmployee)
                .NotEmpty().WithMessage("Электронная почта не должна быть пустой")
                .EmailAddress().WithMessage("Некорректный формат адреса электронной почты")
                .Matches(@"^[a-zA-Z0-9._%+-]+@mpt.ru$").WithMessage("Некорректный формат адреса электронной почты. Электронная почта должна принадлежать домену МПТ");

            this.RuleFor(x => x.SurnameEmployee)
                .NotEmpty().WithMessage("Фамилия не должна быть пустой")
                .Must(x => string.IsNullOrEmpty(x) || x != null && !string.IsNullOrWhiteSpace(x) && x.ToUpper().First() == x.First())
                .WithMessage("Первая буква фамилии должна быть заглавной!");

            this.RuleFor(x => x.NameEmployee)
                .NotEmpty().WithMessage("Имя не должно быть пустым")
                .Must(x => string.IsNullOrEmpty(x) || x != null && !string.IsNullOrWhiteSpace(x) && x.ToUpper().First() == x.First())
                .WithMessage("Первая буква имени должна быть заглавной!");

            this.RuleFor(x => x.MiddleNameEmployee)
                .Must(x => string.IsNullOrEmpty(x) || x != null && !string.IsNullOrWhiteSpace(x) && x.ToUpper().First() == x.First())
                .WithMessage("Первая буква отчества должна быть заглавной!");

            this.RuleFor(x => x.PasswordEmployee)
                .NotEmpty().WithMessage("Пароль не должен быть пустым")
                .MinimumLength(8)
                .WithMessage("Пароль должен содержать больше 8 символов!");

            this.RuleFor(x => x.RoleId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите роль сотрудника!");
        }
    }
}

﻿using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class StudentValidator : FluentValidation.AbstractValidator<Student>
    {

        public StudentValidator() 
        {
            this.RuleFor(x => x.EmailStudent)
                .EmailAddress().WithMessage("Некорректный формат адреса электронной почты")
                .Matches(@"^[a-zA-Z0-9._%+-]+@mpt.ru$").WithMessage("Некорректный формат адреса электронной почты. Электронная почта должна принадлежать домену МПТ");

            this.RuleFor(x => x.SurnameStudent)
                .Must(x => string.IsNullOrEmpty(x) || x.ToUpper().First() == x.First())
                .WithMessage("Первая буква фамилии должна быть заглавной!");

            this.RuleFor(x => x.NameStudent)
                .Must(x => string.IsNullOrEmpty(x) || x.ToUpper().First() == x.First())
                .WithMessage("Первая буква имени должна быть заглавной!");

            this.RuleFor(x => x.MiddleNameStudent)
                .Must(x => string.IsNullOrEmpty(x) || x.ToUpper().First() == x.First())
                .WithMessage("Первая буква отчества должна быть заглавной!");

            this.RuleFor(x => x.PasswordStudent)
                .MinimumLength(8)
                .WithMessage("Пароль должен содержать больше 8 символов!");

            this.RuleFor(x => x.StudyGroupId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите учебную группу!");

        }
    }
}

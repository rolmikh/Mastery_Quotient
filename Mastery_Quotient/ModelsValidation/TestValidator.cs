using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class TestValidator : FluentValidation.AbstractValidator<Test>
    {
        public TestValidator() 
        {
            this.RuleFor(x => x.NameTest)
                .Must(x => x.ToUpper().First() == x.First())
                .WithMessage("Первая буква названия тестирования должна быть заглавной!")
                .MinimumLength(4)
                .WithMessage("Название тестирования не должно быть меньше 4 символов")
                .MaximumLength(100)
                .WithMessage("Название тестирования не должно превышать 100 символов");

            this.RuleFor(x => x.DisciplineId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите дисциплину!");

            this.RuleFor(x => x.TestParameterId)
               .Must(x => !x.Equals(0))
               .WithMessage("Выберите параметр тестирования!");

        }

    }
}

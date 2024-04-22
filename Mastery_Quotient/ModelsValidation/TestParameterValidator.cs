using FluentValidation;
using Mastery_Quotient.Models;

namespace Mastery_Quotient.ModelsValidation
{
    public class TestParameterValidator : FluentValidation.AbstractValidator<TestParameter>
    {
        public TestParameterValidator() 
        { 
        
            this.RuleFor(x => x.NameParameter)
                .Must(x => x.ToUpper().First() == x.First())
                .WithMessage("Первая буква названия параметра тестирования должна быть заглавной!")
                .MaximumLength(50)
                .WithMessage("Название параметра тестирования не должен превышать 50 символов!");

            this.RuleFor(x => x.ValueParameter)
                .Must(x => !string.IsNullOrEmpty(x) && x[0] != '-')
                .WithMessage("Первый символ не может быть минус!")
                .MaximumLength(50)
                .WithMessage("Параметр тестирования не должен превышать 50 символов!");
        }
    }
}

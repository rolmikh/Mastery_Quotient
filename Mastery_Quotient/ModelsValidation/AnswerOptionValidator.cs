using Mastery_Quotient.Models;
using FluentValidation;

namespace Mastery_Quotient.ModelsValidation
{
    public class AnswerOptionValidator : FluentValidation.AbstractValidator<AnswerOption>
    {
        public AnswerOptionValidator() 
        {
            this.RuleFor(x => x.ContentAnswer)
                .NotEmpty()
                .WithMessage("Вариант ответ не должен быть пустым");
        
        }

    }
}

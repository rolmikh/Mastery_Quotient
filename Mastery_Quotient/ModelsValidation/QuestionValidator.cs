using Mastery_Quotient.Models;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;

namespace Mastery_Quotient.ModelsValidation
{
    public class QuestionValidator : FluentValidation.AbstractValidator<Question>
    {
        public QuestionValidator() 
        {
            this.RuleFor(x => x.NumberQuestion)
                .GreaterThan(0)
                .WithMessage("Значение должно быть больше нуля!")
                .NotEmpty()
                .WithMessage("Номер вопроса не должно быть пустым");


            this.RuleFor(x => x.NameQuestion)
                .NotEmpty()
                .WithMessage("Вопрос не должен быть пустым")
                .Must(x => x.ToUpper().First() == x.First())
                .WithMessage("Первая буква вопроса должна быть заглавной!");


            this.RuleFor(x => x.TypeQuestionId)
                .Must(x => !x.Equals(0))
                .WithMessage("Выберите тип вопроса!");
        }


    }
}

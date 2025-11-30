using FluentValidation;
using Lottery.Properties.Langs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery.Helpers
{
    internal class CodeValidator : AbstractValidator<string>
    {
        public CodeValidator()
        {
            RuleFor(code => code)
                .NotEmpty().WithMessage(Lang.CodeRequiredMessage)
                .Length(6).WithMessage(Lang.CodeLengthMessage)
                .Matches(@"^[0-9]{6}$").WithMessage(Lang.CodeFormatMessage);
        }
    }
}

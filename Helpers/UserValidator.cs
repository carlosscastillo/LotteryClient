using FluentValidation;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery.Helpers
{
    internal class UserValidator : AbstractValidator<UserDto>
    {
        public UserValidator() { 
        }
        private void ValidateNickname()
        {
            RuleFor(u => u.Nickname)
                .NotEmpty().WithMessage(Lang.NicknameRequiredMessage)
                .MinimumLength(4).WithMessage(Lang.NicknameMinLengthMessage)
                .MaximumLength(20).WithMessage(Lang.NicknameMaxLengthMessage)
                .Matches(@"^[a-zA-Z0-9._\-@]+$")
                .WithMessage(Lang.NicknameInvalidCharactersMessage);
        }
        private void ValidatePassword()
        {
            RuleFor(u => u.Password)
                .NotEmpty().WithMessage(Lang.PasswordRequiredMessage)
                .MinimumLength(8).WithMessage(Lang.PasswordMinLengthMessage)
                .Matches(@"[A-Z]").WithMessage(Lang.PasswordRequireUppercaseMessage)
                .Matches(@"[a-z]").WithMessage(Lang.PasswordRequireLowercaseMessage)
                .Matches(@"\d").WithMessage(Lang.PasswordRequireNumberMessage)
                .Matches(@"[!@#$%&*_\-+=]").WithMessage(Lang.PasswordRequireSpecialCharMessage)
                .Matches(@"^[a-zA-Z0-9!@#$%&*_\-+=]+$")
                .WithMessage(Lang.PasswordInvalidCharactersMessage);
        }
        public UserValidator ValidatePasswordOnly()
        {
            ValidatePassword();
            return this;
        }
        public UserValidator ValidateLogin()
        {
            ValidateNickname();
            ValidatePassword();

            return this;
        }
        private void ValidateEmail()
        {
            RuleFor(u => u.Email)
                .NotEmpty().WithMessage(Lang.EmailRequiredMessage)
                .EmailAddress().WithMessage(Lang.EmailInvalidFormatMessage)
                .Matches(@"^[a-zA-Z0-9._\-]+@[a-zA-Z0-9._\-]+(\.[a-zA-Z]{2,})+$")
                .WithMessage(Lang.EmailInvalidDomainOrCharsMessage);            
        }
        public UserValidator ValidateEmailOnly()
        {
            ValidateEmail();
            return this;
        }

        public UserValidator ValidateRegister()
        {
            ValidateNickname();
            ValidatePassword();
            ValidateEmail();

            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage(Lang.FirstNameRequiredMessage)
                .MaximumLength(30).WithMessage(Lang.FirstNameMaxLengthMessage)
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")
                .WithMessage(Lang.FirstNameInvalidCharactersMessage);

            RuleFor(u => u.PaternalLastName)
                .NotEmpty().WithMessage(Lang.PaternalLastNameRequiredMessage)
                .MaximumLength(30).WithMessage(Lang.PaternalLastNameMaxLengthMessage)
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")
                .WithMessage(Lang.PaternalLastNameInvalidCharactersMessage);

            RuleFor(u => u.MaternalLastName)
                .MaximumLength(30).WithMessage(Lang.MaternalLastNameMaxLengthMessage)
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$")
                .WithMessage(Lang.MaternalLastNameInvalidCharactersMessage);

            return this;
        }
    }
}
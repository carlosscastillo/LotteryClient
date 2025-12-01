using FluentValidation;
using Lottery.LotteryServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery.Helpers
{
    internal class SocialMediaValidator : AbstractValidator<SocialMediaDto>
    {
        public SocialMediaValidator()
        {

        }

        private void ValidateTwitter()
        {
            RuleFor(s => s.Twitter)
                .MaximumLength(15).WithMessage("El usuario de Twitter/X debe tener menos de 16 caracteres.")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("El usuario de Twitter/X solo puede contener letras, números y guiones bajos.")
                .When(s => !string.IsNullOrWhiteSpace(s.Twitter));
        }

        private void ValidateInstagram()
        {
            RuleFor(s => s.Instagram)
                .MaximumLength(30).WithMessage("El usuario de Instagram debe tener menos de 31 caracteres.")
                .Matches(@"^[a-zA-Z0-9._]+$")
                .WithMessage("El usuario de Instagram solo puede contener letras, números, puntos y guiones bajos.")
                .When(s => !string.IsNullOrWhiteSpace(s.Instagram));
        }

        private void ValidateTikTok()
        {
            RuleFor(s => s.TikTok)
                .MinimumLength(2).WithMessage("El usuario de TikTok debe tener al menos 2 caracteres.")
                .MaximumLength(24).WithMessage("El usuario de TikTok debe tener menos de 25 caracteres.")
                .Matches(@"^[a-zA-Z0-9._]+$")
                .WithMessage("El usuario de TikTok solo puede contener letras, números, puntos y guiones bajos.")
                .When(s => !string.IsNullOrWhiteSpace(s.TikTok));
        }

        private void ValidateFacebook()
        {
            RuleFor(s => s.Facebook)
                .MaximumLength(50).WithMessage("La ruta de Facebook debe tener menos de 51 caracteres.")
                .Matches(@"^[a-zA-Z0-9._\-]+$")
                .WithMessage("La ruta de Facebook contiene caracteres inválidos.")
                .When(s => !string.IsNullOrWhiteSpace(s.Facebook));
        }

        public SocialMediaValidator ValidateAll()
        {
            ValidateTwitter();
            ValidateInstagram();
            ValidateTikTok();
            ValidateFacebook();
            return this;
        }
    }
}
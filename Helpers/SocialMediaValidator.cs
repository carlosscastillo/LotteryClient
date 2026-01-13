using Contracts.DTOs;
using FluentValidation;
using Lottery.Properties.Langs;
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
                .MaximumLength(15)
                .WithMessage(Lang.ValidationTwitterMaxLength)
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage(Lang.ValidationTwitterFormat)
                .When(s => !string.IsNullOrWhiteSpace(s.Twitter));
        }

        private void ValidateInstagram()
        {
            RuleFor(s => s.Instagram)
                .MaximumLength(30)
                .WithMessage(Lang.ValidationInstagramMaxLength)
                .Matches(@"^[a-zA-Z0-9._]+$")
                .WithMessage(Lang.ValidationInstagramFormat)
                .When(s => !string.IsNullOrWhiteSpace(s.Instagram));
        }

        private void ValidateTikTok()
        {
            RuleFor(s => s.TikTok)
                .MinimumLength(2)
                .WithMessage(Lang.Validation_TikTok_MinLength)
                .MaximumLength(24)
                .WithMessage(Lang.ValidationTikTokMaxLength)
                .Matches(@"^[a-zA-Z0-9._]+$")
                .WithMessage(Lang.ValidationTikTokFormat)
                .When(s => !string.IsNullOrWhiteSpace(s.TikTok));
        }

        private void ValidateFacebook()
        {
            RuleFor(s => s.Facebook)
                .MaximumLength(50)
                .WithMessage(Lang.ValidationFacebookMaxLength)
                .Matches(@"^[a-zA-Z0-9._\-]+$")
                .WithMessage(Lang.Validation_Facebook_Format)
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
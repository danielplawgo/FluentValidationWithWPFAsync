using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentValidationWithWPF
{
    public class MainWindowViewModelValidator : AbstractValidator<MainWindowViewModel>
    {
        public MainWindowViewModelValidator()
        {
            RuleFor(u => u.Email)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .EmailAddress()
                .MustAsync((email, token) => ValidateEmail(email, token))
                .WithMessage("Email must be unique");
                
        }

        private async Task<bool> ValidateEmail(string email, CancellationToken token)
        {
            await Task.Delay(2000);

            return email != "daniel@plawgo.pl";
        }
    }
}

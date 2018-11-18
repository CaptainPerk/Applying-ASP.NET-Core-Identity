using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.Models;

namespace Users.Infrastructure
{
    public class CustomUserValidator : UserValidator<AppUser>
    {
        public override async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            var identityResult = await base.ValidateAsync(manager, user);
            var identityErrors = identityResult.Succeeded ? new List<IdentityError>() : identityResult.Errors.ToList();

            if (!user.Email.ToLower().EndsWith("@example.com"))
            {
                identityErrors.Add(new IdentityError
                {
                    Code = "EmailDomainError",
                    Description = "Only example.com is an allowable email address"
                });
            }

            return identityErrors.Any() ? IdentityResult.Failed(identityErrors.ToArray()) : IdentityResult.Success;
        }
    }
}

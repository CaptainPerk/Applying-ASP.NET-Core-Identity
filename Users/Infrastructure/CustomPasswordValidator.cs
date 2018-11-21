using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.Models;

namespace Users.Infrastructure
{
    public class CustomPasswordValidator : PasswordValidator<AppUser>
    {
        public override async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            var result = await base.ValidateAsync(manager, user, password);
            var identityErrors = result.Succeeded ? new List<IdentityError>() : result.Errors.ToList();

            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                identityErrors.Add(new IdentityError{ Code = "PasswordContainsUserName", Description = "Password cannot contain your user name"});
            }

            if (password.Contains("12345"))
            {
                identityErrors.Add(new IdentityError{ Code = "PasswordContainsSequesnce", Description = "Password cannot contain numeric sequence"});
            }

            return identityErrors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(identityErrors.ToArray());
        }
    }
}

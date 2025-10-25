using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Domain.Dtos
{
    public record CreateUserDto
    (
        string FullName,
        string UserName,
        string Email,
        string Password,
        IList<string>SelectedRoles
    );
}

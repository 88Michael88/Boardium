using Boardium.Areas.Admin.Models;
using Boardium.Models.Auth;
using Riok.Mapperly.Abstractions;

namespace Boardium.Areas.Admin.Mappers;

[Mapper]
public partial class UserMapper
{
    public partial EditUserViewModel ToViewModel(ApplicationUser user);
    public partial void UpdateUser(EditUserViewModel model, ApplicationUser user);
}
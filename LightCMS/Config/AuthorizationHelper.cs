using LightCMS.Components.Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace LightCMS.Config
{
    public class AuthorizationHelper
    {
        public static Role GetUserRole(ClaimsPrincipal identity, CMSDBContext db)
        {            
            foreach(var claim in identity.Claims)
            {
                if(claim.Type == ClaimTypes.Role)
                {
                    return db.Roles.SingleOrDefault(role => role.Name.Equals(claim.Value));
                }
            }
            return null;
        }

        public static bool IsAuthorized(Role userRole, Role resourceRole)
        {
            if (!resourceRole.Name.Equals("Public") && !resourceRole.Name.Equals(userRole.Name))
                return false;
            return true;
        }
    }
}

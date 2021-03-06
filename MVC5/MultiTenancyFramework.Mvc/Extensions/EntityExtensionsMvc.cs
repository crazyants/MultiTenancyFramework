﻿using Microsoft.AspNet.Identity;
using MultiTenancyFramework.Data;
using MultiTenancyFramework.Data.Queries;
using MultiTenancyFramework.Entities;
using MultiTenancyFramework.Mvc;
using MultiTenancyFramework.Mvc.Logic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiTenancyFramework
{
    public static class EntityExtensionsMvc
    {
        public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this IdentityUser user, UserManager<IdentityUser, long> manager)
        {
            //// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            //var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            //// Add custom user claims here
            //userIdentity.AddClaim(new Claim("InstitutionCode", WebUtils.WebUtilities.InstitutionCode));
            //return userIdentity;

            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie, ClaimTypes.Name, ClaimTypes.Role);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToStringLookup(), "http://www.w3.org/2001/XMLSchema#string"));
            claimsIdentity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"));

            var instCode = WebUtilities.InstitutionCode;

            if (!string.IsNullOrWhiteSpace(instCode))
            {
                var queryProcessor = Utilities.QueryProcessor;
                var query = new GetInstitutionByCodeQuery
                {
                    Code = instCode,
                };
                user.InstitutionCode = instCode;
                user.InstitutionShortName = queryProcessor.Process(query)?.ShortName;
            }
            else
            {
                instCode = Utilities.INST_DEFAULT_CODE;
            }
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, $"{user.UserName}:{instCode}", "http://www.w3.org/2001/XMLSchema#string"));
            
            if (manager.SupportsUserSecurityStamp && !string.IsNullOrWhiteSpace(user.SecurityStamp))
            {
                claimsIdentity.AddClaim(new Claim(Constants.DefaultSecurityStampClaimType, user.SecurityStamp, "http://www.w3.org/2001/XMLSchema#string"));
            }

            SetLoggedInUsersPrivileges(user);

            if (manager.SupportsUserClaim)
            {
                claimsIdentity.AddClaims(await manager.GetClaimsAsync(user.Id));
            }
            WebUtilities.SetCurrentlyLoggedInUser(user);
            return claimsIdentity;
        }

        public static HashSet<ActionAccessPrivilege> SetLoggedInUsersPrivileges(this AppUser user)
        {
            HashSet<ActionAccessPrivilege> list = new HashSet<ActionAccessPrivilege>();
            var _userRoleEngine = MyServiceLocator.GetInstance<ICoreDAO<UserRole>>();
            string instCode = user.InstitutionCode;
            _userRoleEngine.InstitutionCode = instCode;

            var theUserRoles = _userRoleEngine.RetrieveByIDs(user.UserRoleIDs.ToArray());

            var privilegesInDb = DataCacheMVC.AllPrivileges;
            if (theUserRoles != null && theUserRoles.Count > 0)
            {
                ActionAccessPrivilege privilege;
                string roleNames = string.Empty;
                foreach (var userRole in theUserRoles)
                {
                    roleNames += $"{userRole.Name} / ";
                    foreach (var priv in userRole.PrivilegeIDs)
                    {
                        if (privilegesInDb.TryGetValue(priv, out privilege) && privilege != null)
                        {
                            list.Add(privilege);
                        }
                    }
                }
                user.RoleNames = roleNames.Substring(0, roleNames.Length - 3);
            }
            var defaultPrivs = privilegesInDb.Values.Where(x => x.IsDefault);
            if (!string.IsNullOrWhiteSpace(instCode)) // If Tenant
            {
                defaultPrivs = defaultPrivs.Where(x => x.Scope != AccessScope.CentralOnly);
            }
            else
            {
                defaultPrivs = defaultPrivs.Where(x => x.Scope != AccessScope.TenantsOnly);
            }
            foreach (var priv in defaultPrivs)
            {
                list.Add(priv);
            }

            WebUtilities.LoggedInUsersPrivilegesDict = list.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            return list;
        }

        /// <summary>
        /// Deserializes user.Data and returns it
        /// </summary>
        public static AuditLog SetTrailItems(this AuditLog auditTrail)
        {
            var data = auditTrail.AuditData;
            if (string.IsNullOrWhiteSpace(data)) return auditTrail;

            data = data.DecompressString();
            auditTrail.TrailItems = JsonConvert.DeserializeObject<List<TrailItem>>(data);
            if (auditTrail.TrailItems == null || auditTrail.TrailItems.Count == 0) return auditTrail;

            //Any TODO?
            return auditTrail;
        }

    }
}

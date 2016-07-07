﻿using MultiTenancyFramework.Entities;

namespace MultiTenancyFramework.NHibernate.Maps
{
    public sealed class InstitutionMap : InstitutionMap<Institution>
    {

    }

    public class InstitutionMap<T> : EntityMap<T> where T : Institution
    {
        public InstitutionMap()
        {
            Map(x => x.Name);
            Map(x => x.ShortName);
            Map(x => x.Code);
            Map(x => x.Email);
            Map(x => x.Phone);
            Map(x => x.TempUserName);
            Map(x => x.TempPassword);
            Map(x => x.DatabaseConnectionId);
            Map(x => x.LastModificationDoneByUs);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Database
{
    public static class ExceptionHanders
    {
        public static void ProcessEntityException(DbEntityValidationException e)
        {
            foreach (var eve in e.EntityValidationErrors)
            {
                PandaLogger.Log(ChatColor.red, "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);

                foreach (var ve in eve.ValidationErrors)
                    PandaLogger.Log(ChatColor.red, "- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
            }
        }
    }
}

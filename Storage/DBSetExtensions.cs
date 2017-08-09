using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Office365GmailMigratorChecker.Model
{
    static class DBSetExtensions
    {
        public static void AddOrUpdate(this DbSet<MyMessage> dbSet, MyMessage data)
        {
            var keyVal = data.Office365Id;
            var dbVal = dbSet.AsNoTracking().SingleOrDefault(m => m.Office365Id == keyVal);
            if (dbVal != null)
            {
                dbSet.Update(data);
                return;
            }
            dbSet.Add(data);
        }
        public static void AddOrUpdate<T>(this DbSet<T> dbSet, T data) where T : class
        {
            var t = typeof(T);
            PropertyInfo keyField = null;
            foreach (var propt in t.GetProperties())
            {
                var keyAttr = propt.GetCustomAttribute<KeyAttribute>();
                if (keyAttr != null)
                {
                    keyField = propt;
                    break; // assume no composite keys
                }
            }
            if (keyField == null)
            {
                throw new Exception($"{t.FullName} does not have a KeyAttribute field. Unable to exec AddOrUpdate call.");
            }
            var keyVal = keyField.GetValue(data);            
            var dbVal = dbSet.AsNoTracking().SingleOrDefault(m => keyField.GetValue(m) == keyVal);
            if (dbVal != null)
            {
                dbSet.Update(data);
                return;
            }
            dbSet.Add(data);
        }
    }    
}

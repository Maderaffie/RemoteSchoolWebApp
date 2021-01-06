using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RemoteSchoolWebApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new SchoolContext(serviceProvider.GetRequiredService<DbContextOptions<SchoolContext>>()))
            {
                //Look for any classes.
                if (context.Classes.Any())
                {
                    return;
                }

                context.Classes.AddRange(
                    new Class
                    {
                        Name = "Ia"
                    },

                    new Class
                    {
                        Name = "Ib"
                    },

                    new Class
                    {
                        Name = "Ic"
                    },

                    new Class
                    {
                        Name = "Id"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace mvcAuth.Identity.Data
{
    public class IdentityServiceDbContextFactory : IDbContextFactory<IdentityServiceDbContext>
    {
        public IdentityServiceDbContext Create(string[] args) =>
            Program.BuildWebHost(args).Services.GetRequiredService<IdentityServiceDbContext>();
    }
}

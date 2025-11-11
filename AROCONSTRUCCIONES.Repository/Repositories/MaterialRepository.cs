using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Repository.Repositories
{
    public class MaterialRepository : RepositoryBase<Material>, IMaterialRepository
    {
        private readonly ApplicationDbContext context;

        public MaterialRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}

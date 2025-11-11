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
    public class AlmacenRepository : RepositoryBase<Almacen>, IAlmacenRepository
    {
        private readonly ApplicationDbContext context;

        public AlmacenRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}

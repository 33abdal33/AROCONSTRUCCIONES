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
    public class DetalleOrdenCompraRepository : RepositoryBase<DetalleOrdenCompra>, IDetalleOrdenCompraRepository
    {
        private readonly ApplicationDbContext context;

        public DetalleOrdenCompraRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        
    }
}

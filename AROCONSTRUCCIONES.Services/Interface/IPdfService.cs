using AROCONSTRUCCIONES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IPdfService
    {
        Task<string> GenerarPdfOrdenCompra(OrdenCompra ordenCompra);
        Task<string> GenerarPdfSolicitudPago(SolicitudPago solicitudPago);
    }
}

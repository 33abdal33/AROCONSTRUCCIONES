using AROCONSTRUCCIONES.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface ILogisticaDashboardService
    {
        Task<LogisticaDashboardDto> GetSummaryAsync();
    }
}

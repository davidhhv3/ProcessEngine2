using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace MVM.ProcessEngine.Extension.Service
{
    class IndicatorAffectation:Indicator
    {
        private AffectationRepository _sqlRepository;
        public IndicatorAffectation(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new AffectationRepository(tenant);
        }
        public void GetAffectations(string indicatorType)
        {
            var affectation = _sqlRepository.GetAffectation(EndDate, ActiveCodes, indicatorType);
            _sqlRepository.DeleteData(Tables.AffectationsTimesTableName);
            _sqlRepository.AffectationsInsert(affectation, Tables.AffectationsTimesTableName);
        }

        public void GetSecurityGeneration()
        {
            var secGen = _sqlRepository.GetGenSec(EndDate );
            _sqlRepository.DeleteData(Tables.SecurityGenerationTableName);
            _sqlRepository.SecurityGenerationInsert(secGen, Tables.SecurityGenerationTableName);
        }

    }
}

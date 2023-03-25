using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.EnergySuite
{
    /// <summary>
    /// Type of Value on Aditional Variable
    /// </summary>
    public enum TypeValueAditionalVariable
    {
        Decimal,
        Text,
        Date
    }

    /// <summary>
    /// Load Type H,M,L
    /// </summary>
    public enum LoadType
    {
        High,
        Medium,
        Low,
        Specific
    }

    /// <summary>
    /// TypeDay selected by User
    /// </summary>
    public enum SelectedTypeDay
    {
        ALL_Normal_Holiday,
        ALL_Normal,
        ALL_Holiday,
        Specific
    }
}

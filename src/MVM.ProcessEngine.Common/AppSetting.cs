using Newtonsoft.Json;
using MVM.ProcessEngine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Common
{
    public class AppSetting
    {
        //  public string Tenant { get; set; }
        //  public IEnumerable<Key> Keys { get; set; }

        public Dictionary<string, IEnumerable<Key>> Metadata { get; set; } = new Dictionary<string, IEnumerable<Key>>();

    }


    public class Key
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}

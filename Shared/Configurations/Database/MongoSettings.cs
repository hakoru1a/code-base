using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Configurations.Database
{
    public class MongoSettings : DatabaseSettings
    {
        public string DatabaseName { get; set; }
    }
}

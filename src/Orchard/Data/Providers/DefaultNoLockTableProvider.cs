using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Data.Providers {
    public class DefaultNoLockTableProvider : INoLockTableProvider {

        public DefaultNoLockTableProvider() {

            // We may use AutoFac to override the default tables:
            /*
             <component instance-scope="per-lifetime-scope"
                       type="Orchard.Data.Providers.DefaultNoLockTableProvider, Orchard.Framework"
                       service="Orchard..Data.Providers.INoLockTableProvider">
                <properties>
                    <property name="TableName" value="Table_Name_1, Table_Name_2" />
                </properties>
            </component>
             */
            TableNames = "Orchard_Framework_ContentItemVersionRecord, Title_TitlePartRecord, Orchard_Framework_ContentItemRecord";
        }

        public string TableNames { get; set; }

        private IEnumerable<string> _tableNames;

        public IEnumerable<string> GetTableNames() {
            if (_tableNames == null) {
                _tableNames = new List<string>(TableNames
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
            return _tableNames;
        }
    }
}

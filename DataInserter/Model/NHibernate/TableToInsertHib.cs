using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model.NHibernate
{
    class TableToInsertHib
    {
        public virtual int Id { get; set; }

        public virtual int IntValue { get; set; }

        public virtual string StringValue { get; set; }

    }
}

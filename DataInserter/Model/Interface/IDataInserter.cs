using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model.Interface
{
    public interface IDataInserter
    {
        void LoadDataFromFile(int fileSize);

        bool SaveDataToDataBase();

    }
}

using DataInserter.BaseClasses;
using DataInserter.Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model.EntityFramework
{
    public class EFDataInserter : BaseDataInserter, IDataInserter
    {
        public void LoadDataFromFile(int fileSize)
        {
            LoadData(fileSize);
        }

        public bool SaveDataToDataBase()
        {
            using (Entities context = new Entities())
            {
                foreach (var line in DataFromFile)
                {
                    context.TableToInsert.Add(new TableToInsert
                    {
                        IntValue = line.IntValue,
                        StringValue = line.StringValue
                    });
                }
                context.SaveChanges();
            }
            return true;
        }
    }
}

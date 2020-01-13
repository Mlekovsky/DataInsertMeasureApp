using DataInserter.BaseClasses;
using DataInserter.Model.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model.PlainSql
{
    public class PlainSqlDataInserter : BaseDataInserter, IDataInserter
    {
        public void LoadDataFromFile(int fileSize)
        {
            LoadData(fileSize);
        }

        public bool SaveDataToDataBase()
        {
            SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString);

            string query = $@"insert into [TableToInsert] (IntValue, StringValue) values(@intVal, @strVal)";

            for (int i = 0; i < DataFromFile.Length; i++)
            {
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlCommand.Parameters.AddWithValue("intVal", DataFromFile[i].IntValue);
                sqlCommand.Parameters.AddWithValue("strVal", DataFromFile[i].StringValue);

                sqlConnection.Open();

                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();
            }
         
            return true;
        }
        public PlainSqlDataInserter()
        {

        }
    }
}

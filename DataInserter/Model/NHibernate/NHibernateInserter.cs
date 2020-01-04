using DataInserter.BaseClasses;
using DataInserter.Model.Interface;
using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model.NHibernate
{
    public class NHibernateInserter : BaseDataInserter, IDataInserter
    {
        private Configuration myConfiguration;
        private ISessionFactory mySessionFactory;
        private ISession mySession;

        public void LoadDataFromFile(int fileSize)
        {
            LoadData(fileSize);
        }

        public bool SaveDataToDataBase()
        {
            using (mySession.BeginTransaction())
            {
                foreach (var line in DataFromFile)
                {
                    mySession.Save(new TableToInsertHib { IntValue = line.IntValue, StringValue = line.StringValue });
                }
                mySession.Transaction.Commit();
            }

            return true;
        }

        public void InitialConfiguration()
        {
            if (mySession != null && mySession.IsOpen)
            {
                mySession.Close();
            }
            if (mySessionFactory != null && !mySessionFactory.IsClosed)
            {
                mySessionFactory.Close();
            }

            myConfiguration = new Configuration();
            myConfiguration.Configure();
            mySessionFactory = myConfiguration.BuildSessionFactory();
            mySession = mySessionFactory.OpenSession();
        }

        public NHibernateInserter()
        {
            InitialConfiguration();
        }
    }
}

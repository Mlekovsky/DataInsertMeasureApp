using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.BaseClasses
{
    public class BaseDataInserter
    {
        protected Line[] DataFromFile;

        private const string path = @"../../Resources";
        private const string fileName = @"DataFile";

        public void LoadData(int fileSize)
        {
            //Let's assume that our previous validations went correct, and we are having beautiful file to read from right now
            string[] lines = System.IO.File.ReadAllLines($@"{path}/{fileName}.txt");

            DataFromFile = new Line[fileSize];
            string[] tmpLine;
            int index = 0;
            foreach (var line in lines)
            {
                tmpLine = line.Split(' ');
                DataFromFile[index++] = new Line { IntValue = Int32.Parse(tmpLine[0]), StringValue = tmpLine[1] };
            }
        }
    }
}


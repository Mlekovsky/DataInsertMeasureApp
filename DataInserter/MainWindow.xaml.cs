using DataInserter.Model.EntityFramework;
using DataInserter.Model.Interface;
using DataInserter.Model.NHibernate;
using DataInserter.Model.PlainSql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataInserter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string path = @"../../Resources";
        private const string fileName = @"DataFile";
        private const string invalidName = @"Invalid";
        private const string basicTimeLog = @"BasicTimeLog";
        private const string debugInfoFile = @"DebugInfo";
        private bool refreshDatabaseAfterTest = false;
        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;

        private static Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            CheckDbSize();
        }

        public void WriteText2(object sender, EventArgs e)
        {
            //To lazy to fix because of 9 references, writes comment instead. Whole me :(
        }

        private void GenerateFileButton_Click(object sender, RoutedEventArgs e)
        {
            int fileSize = GetFileSizeFromForm();

            if (fileSize == 0)
                MessageBox.Show("Please select file size");
            else
                GenerateFileWithRandomData(fileSize);
        }

        private int GetFileSizeFromForm()
        {
            int fileSize = 0;

            if (size10.IsChecked == true)
                fileSize = 10;
            else if (size100.IsChecked == true)
                fileSize = 100;
            else if (size1000.IsChecked == true)
                fileSize = 1000;
            else if (size10000.IsChecked == true)
                fileSize = 10000;
            else if (size100000.IsChecked == true)
                fileSize = 100000;

            return fileSize;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void GenerateFileWithRandomData(int fileSize)
        {
            if (FileExists())
            {
                int currentFileSize = GetFileSize();
                if (currentFileSize != fileSize)
                {
                    MessageBoxResult result = MessageBox.Show($"You are going to override existing file.\nCurrect size: {currentFileSize} \nNew size: {fileSize}", "Confirm your choice", MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.Cancel)
                        return;
                }
            }
            //If we got here, let's finally generate our file
            using (StreamWriter file = new StreamWriter($@"{path}\{fileName}.txt", false))
            {
                for (int i = 0; i < fileSize; i++)
                {
                    file.WriteLine($@"{i + 1} {RandomString(random.Next(1, 12))}");
                }
            }

            MessageBox.Show($@"File with size {fileSize} has been generated successfully");
        }

        private int GetFileSize()
        {
            var lineCount = 0;
            using (var reader = File.OpenText($@"{path}/{fileName}.txt"))
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }
            return lineCount;
        }

        private bool FileExists()
        {
            return File.Exists($@"{path}/{fileName}.txt");
        }

        private void CheckFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileExists())
            {
                int fileSize = GetFileSize();
                int fileSizeFromForm = GetFileSizeFromForm();
                MessageBox.Show($@"Actual file contains {fileSize} lines. Selected file size is {fileSizeFromForm} lines");
            }
            else
            {
                MessageBox.Show("File does not exist");
            }
        }

        private void RunTestButton_Click(object sender, RoutedEventArgs e)
        {
            string InsertMethodName = GetInsertMethodName();

            if (InsertMethodName == invalidName)
            {
                MessageBox.Show("Please select data insert method");
                return;
            }

            int destFileSize = GetFileSizeFromForm();

            if (destFileSize == 0)
            {
                MessageBox.Show("Please select file size");
                return;
            }

            int numberOfTests = GetNumberOfTests();

            if (numberOfTests == 0)
            {
                MessageBox.Show("Please enter a valid number");
                return;
            }

            SetCleanDBFlag();

            if (FileExists())
            {
                int currFileSize = GetFileSize();
                if (currFileSize == destFileSize)
                {
                    bool success = StartTest(currFileSize, InsertMethodName, numberOfTests);
                    MessageBox.Show($"{numberOfTests} test(s) completed, Success: {success}");

                }
                else if (currFileSize == 0)
                {
                    MessageBox.Show("File size is 0. Please generate file first.");
                }
                else
                {
                    MessageBox.Show($@"Selected file size is different from actual file size. Selected {destFileSize}, expected {currFileSize}");
                }
            }
            else
            {
                MessageBox.Show("File does not exist");
            }
        }

        private void SetCleanDBFlag()
        {
            refreshDatabaseAfterTest = CleanDBCheckbox.IsChecked.Value;
        }

        private int GetNumberOfTests()
        {
            int number = 1;

            Int32.TryParse(NumberOfTests.Text, out number);

            return number;
        }

        private bool StartTest(int fileSize, string insertMethodName, int numberOfTests = 1)
        {
            Stopwatch sw = new Stopwatch();
            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            //ramCounter = new PerformanceCounter("Process", "Private Bytes", Process.GetCurrentProcess().ProcessName);
            //cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
            //Process.GetCurrentProcess().ProcessName
            List<float> ramMeasureData = new List<float>();
            List<float> cpuMeasureData = new List<float>();

            IDataInserter dataInserter = CreateDataInserterClassInstance(insertMethodName);

            dataInserter.LoadDataFromFile(fileSize);

            bool success = true;

            string errorMsg = "---";

            sw.Start();

            ramMeasureData.Add(getAvailableRAM()); //Get first default value
            cpuMeasureData.Add(getCurrentCpuUsage()); //Get first default value

            try
            {
                for (int i = 0; i < numberOfTests; i++)
                {
                    dataInserter.SaveDataToDataBase();

                    ramMeasureData.Add(getAvailableRAM());
                    cpuMeasureData.Add(getCurrentCpuUsage());

                    if (refreshDatabaseAfterTest)
                        CleanDataBase();
                }
            }
            catch (Exception ex)
            {
                success = false;
                errorMsg = ex.ToString();
            }

            sw.Stop();

            using (StreamWriter file = new StreamWriter($@"{path}\{basicTimeLog}.txt", true))
            {
                file.WriteLine($@"LogEntry from {DateTime.Now}
[
------Basic data---------
Selected method: {insertMethodName}
Selected size: {fileSize}
Number of tests: {numberOfTests}

------Measure Data-------
Elapsed time: {sw.Elapsed.Minutes}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms
CPU:
Minimum CPU Usage: {cpuMeasureData.Min()}
Maximum CPU Usage: {cpuMeasureData.Max()}
Average CPU Usage: {cpuMeasureData.Average()}
Memory:
Minimum Memory Usage: {ramMeasureData.Min()}
Maximum Memory Usage: {ramMeasureData.Max()}
Average Memory Usage: {ramMeasureData.Average()}

----------Stauts---------
Save succeed: {success}
Error: {errorMsg}
]

");
            }

            CheckDbSize();

            return success;
        }

        private IDataInserter CreateDataInserterClassInstance(string insertMethodName)
        {
            switch (insertMethodName)
            {
                case "PSql":
                    return new PlainSqlDataInserter();
                case "NHib":
                    return new NHibernateInserter();
                case "EF":
                    return new EFDataInserter();
                default:
                    return null;
            }
        }

        private string GetInsertMethodName()
        {
            if (PSql.IsChecked == true)
                return PSql.Name;
            if (EF.IsChecked == true)
                return EF.Name;
            if (NHib.IsChecked == true)
                return NHib.Name;

            return invalidName;
        }

        public void CleanDataBase()
        {
            SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString);

            string query = $@"delete from [TableToInsert]";

            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

            sqlConnection.Open();

            sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();

            CheckDbSize();
        }

        private void CleanDBButton_Click(object sender, RoutedEventArgs e)
        {
            CleanDataBase();
        }

        public void CheckDbSize()
        {
            int size = 0;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT count(*) FROM [TableToInsert]", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                size = Convert.ToInt32(reader[0]);
                            }
                        }
                    }
                }
            }

            DBSizeText.Text = $@"Database size : {size}";
        }

        protected void SaveDataFromDataBaseToFile()
        {
            List<BaseClasses.Line> lines = new List<BaseClasses.Line>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT * FROM [TableToInsert]", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                lines.Add(new BaseClasses.Line { IntValue = Convert.ToInt32(reader["IntValue"]), StringValue = reader["StringValue"].ToString() });
                            }
                        }
                    }
                }
            }

            using (StreamWriter file = new StreamWriter($@"{path}\{debugInfoFile}.txt", true))
            {
                int index = 0;
                file.WriteLine($@"Log from {DateTime.Now}
[");
                foreach (var line in lines)
                    file.WriteLine($@"From db: No: {index++} IntVal: {line.IntValue} StrVal: {line.StringValue}");
                file.WriteLine($@"]");
            }
        }

        private void SaveDBButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDataFromDataBaseToFile();
            MessageBox.Show("Save complete");
        }

        public float getCurrentCpuUsage()
        {
            return cpuCounter.NextValue();
        }

        public float getAvailableRAM()
        {
            return ramCounter.NextValue();
        }
    }
}

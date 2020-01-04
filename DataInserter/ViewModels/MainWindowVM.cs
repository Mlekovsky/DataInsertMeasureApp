using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DataInserter.BaseClasses;
namespace DataInserter.ViewModels
{
    public class MainWindowVM : PropertyBase
    {
        public string Title { get; set; }

        public MainWindowVM()
        {
            Title = "Data inserter";
        }
    }
}

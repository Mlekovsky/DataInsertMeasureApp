using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.BaseClasses
{
    public class PropertyBase : INotifyPropertyChanged
    {
        protected PropertyChangedEventHandler propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this.propertyChanged += value;
            }

            remove
            {
                this.propertyChanged -= value;
            }
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (this.propertyChanged != null)
            {
                this.propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void OnPropertyChanged(params string[] ParamNames)
        {
            if (propertyChanged != null)
            {
                foreach (string Param in ParamNames)
                {
                    propertyChanged(this, new PropertyChangedEventArgs(Param));
                }
            }
        }
    }
}

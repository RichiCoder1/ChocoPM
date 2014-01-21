using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ChocoPM.Extensions
{
    internal static class DataGridExtensions
    {
        public static string GetSortMemberPath(this DataGridColumn column)
        {
            // find the sortmemberpath
            string sortPropertyName = column.SortMemberPath;
            if (string.IsNullOrEmpty(sortPropertyName))
            {
                DataGridBoundColumn boundColumn = column as DataGridBoundColumn;
                if (boundColumn != null)
                {
                    Binding binding = boundColumn.Binding as Binding;
                    if (binding != null)
                    {
                        if (!string.IsNullOrEmpty(binding.XPath))
                        {
                            sortPropertyName = binding.XPath;
                        }
                        else if (binding.Path != null)
                        {
                            sortPropertyName = binding.Path.Path;
                        }
                    }
                }
            }

            return sortPropertyName;
        }

        public static int FindSortDescription(this SortDescriptionCollection sortDescriptions, string sortPropertyName)
        {
            int index = -1;
            int i = 0;
            foreach (SortDescription sortDesc in sortDescriptions)
            {
                if (string.Compare(sortDesc.PropertyName, sortPropertyName) == 0)
                {
                    index = i;
                    break;
                }
                i++;
            }

            return index;
        }
    }
    
}

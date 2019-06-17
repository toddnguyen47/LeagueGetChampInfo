using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LeagueChampName
{
    class CustomListViewItem : ListViewItem, IComparable<ListViewItem>, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public int CompareTo(ListViewItem other)
        {
            String thisContent = this.Content.ToString();
            String otherContent = other.Content.ToString();

            return thisContent.CompareTo(otherContent);
        }
    }
}

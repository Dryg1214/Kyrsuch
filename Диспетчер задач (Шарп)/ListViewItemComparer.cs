using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskManager
{
    class ListViewItemComparer : IComparer
    {
        private int _columnIndex;

        public int ColumnIndex
        {
            get
            {
                return _columnIndex;
            }
            set
            {
                _columnIndex = value;
            }
        }
        // SortOrder - направление сортировки
        // .Ascending - возрастание
        // .Descending - убывание
        // .None - не сортируются
        private SortOrder _sortDirection;

        public SortOrder SortDirection
        {
            get
            {
                return _sortDirection;
            }
            set
            {
                _sortDirection = value;
            }
        }

        public ListViewItemComparer()
        {
            _sortDirection = SortOrder.None;
        }

        public int Compare(object x, object y)
        {
            ListViewItem listViewItemX = x as ListViewItem;
            ListViewItem listViewItemY = y as ListViewItem;
            int rezult;

            switch (_columnIndex)
            {
                case 0:
                    rezult = string.Compare(listViewItemX.SubItems[_columnIndex].Text,
                        listViewItemY.SubItems[_columnIndex].Text, false); //false для учета регистра
                    break;
                case 1:
                    double valueX = Convert.ToDouble(listViewItemX.SubItems[_columnIndex].Text);
                    double valueY = Convert.ToDouble(listViewItemY.SubItems[_columnIndex].Text);
                    rezult = valueY.CompareTo(valueX);
                    break;
                case 2:
                    double valuEX = Convert.ToDouble(listViewItemX.SubItems[_columnIndex].Text);
                    double valuEY = Convert.ToDouble(listViewItemY.SubItems[_columnIndex].Text);
                    rezult = valuEY.CompareTo(valuEX); 
                    break;
                case 3:
                    double valUEX = Convert.ToDouble(listViewItemX.SubItems[_columnIndex].Text);
                    double valUEY = Convert.ToDouble(listViewItemY.SubItems[_columnIndex].Text);
                    rezult = valUEY.CompareTo(valUEX);
                    break;
                default:
                    rezult = string.Compare(listViewItemX.SubItems[_columnIndex].Text,
                        listViewItemY.SubItems[_columnIndex].Text, false);
                    break;
                    //case 3:
            }

            if (_sortDirection == SortOrder.Descending)
            {
                return -rezult;
            }
            else
            {
                return rezult;
            }
        }
    }
}

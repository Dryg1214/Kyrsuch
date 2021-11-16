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
            //x
        }
    }
}

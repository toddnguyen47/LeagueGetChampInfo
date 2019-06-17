using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class SettingsJsonObject
    {
        /// <summary>
        /// To store the 5 most recent champions
        /// </summary>
        public string[] fiveRecentChampions { get; set; }

        /// <summary>
        /// To store the sites selected.
        /// </summary>
        public bool[] sitesSelected { get; set; }
    }
}

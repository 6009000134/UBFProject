using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctus.CustomBE
{
    [Serializable]
    public class MesReturnData
    {
        public List<DataArray1> data { get; set; }

    }
    [Serializable]
    public class DataArray1
    {
        public int CompleteQty { get; set; }
    }
}

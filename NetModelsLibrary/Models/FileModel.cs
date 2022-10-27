using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class FileModel : BusTypeModel
    {
        public byte[] Data { get; set; }
        public int DataSize { get; set; }
        public int PackageIndex { get; set; }
    }
}

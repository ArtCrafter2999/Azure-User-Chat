using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class FileInfoModel : BusTypeModel
    {
        public string Name { get; set; }
        public string Format { get; set; }
        public long DataSize { get; set; }
        public int PackageCount { get; set; }
    }
}

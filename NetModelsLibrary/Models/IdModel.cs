using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class IdModel : BusTypeModel
    {
        public int Id { get; set; }
        public IdModel(int id)
        {
            Id = id;
        }
        public IdModel(){}
    }
}

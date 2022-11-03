using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class ChatCreationModel 
    {
        public string? Title { get; set; }
        public List<IdModel> Users { get; set; }
    }
}

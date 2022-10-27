using NetModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserApp.ViewModels
{
    public abstract class EndpointResoulter
    {
        public event Action<ResoultModel>? Success;
        public event Action<ResoultModel>? Failure;
        public void Invoke(ResoultModel model)
        {
            if (model.Success)
            {
                Success?.Invoke(model);
            }
            else
            {
                Failure?.Invoke(model);
            }
        }
    }
}

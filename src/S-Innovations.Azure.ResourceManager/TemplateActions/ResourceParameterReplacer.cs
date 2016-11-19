using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.Azure.ResourceManager.TemplateActions
{
    public class ResourceParameterReplacer : ResourceParamterConstant
    {

        public ResourceParameterReplacer(string oldParameterName, string newParameterName) : base(oldParameterName, $"parameters('{newParameterName}')")
        {

        }

    }
}

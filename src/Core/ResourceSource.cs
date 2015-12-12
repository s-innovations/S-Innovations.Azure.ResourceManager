using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SInnovations.Azure.ResourceManager
{
  
    
    
   
    
    public class JObjectResourceSource : ResourceSource
    {

    }
    public class ResourceSource : IEnumerable
    {
        private List<ITemplateAction> _actions = new List<ITemplateAction>();
        public ResourceSource()
        {

        }
        public IEnumerable<ITemplateAction> Actions { get { return _actions; } }
        public ResourceSource(string path) : this(path,typeof(ResourceSource).Assembly)
        {
        }
        public ResourceSource(string path, Assembly assembly)
        {
            this.Path = path;
            this.Assembly = assembly;
        }
        public void Add(ITemplateAction action)
        {
            this._actions.Add(action);
        }
        public string Path { get; set; }
        public Assembly Assembly { get; set; }

        public static implicit operator JObject(ResourceSource source)
        {
            return TemplateHelper.ReadDataAsync(source).GetAwaiter().GetResult();
        }
        public static implicit operator ResourceSource(string source)
        {
            return new ResourceSource
            {
                Path = source,
                Assembly = typeof(ResourceSource).Assembly
            };
        }

        public IEnumerator<ITemplateAction> GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _actions.GetEnumerator();
        }
    }
}

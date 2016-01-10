using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SInnovations.Azure.ResourceManager
{
    public class ResourceSourceCollection : List<ResourceSource>
    {
        public ResourceSourceCollection()
        {

        }
        public ResourceSourceCollection(IEnumerable<ResourceSource> sources) : base(sources)
        {
            
        }
        public static implicit operator ResourceSourceCollection(ResourceSource source)
        {
            return new ResourceSourceCollection { source };
        }
        public static implicit operator ResourceSourceCollection(ResourceSource[] sources)
        {
            return new ResourceSourceCollection(sources);
        }
        public static implicit operator ResourceSourceCollection(string source)
        {
            return (ResourceSource)source;
        }
        public static implicit operator ResourceSourceCollection(string[] source)
        {
            var rsc = new ResourceSourceCollection();
            rsc.AddRange(source.Select(s => (ResourceSource)s));
            return rsc;
        }

        public static implicit operator JObject(ResourceSourceCollection collection)
        {
            return new JObject(collection.SelectMany(s => ((JObject)s).Properties()));
        }
    }
}

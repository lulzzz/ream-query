namespace ReamQuery.Server.Services
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using ReamQuery.Core;

    public class ReferenceProvider
    {
        Lazy<IEnumerable<MetadataReference>> _references = new Lazy<IEnumerable<MetadataReference>>(() => {
            var list = new List<MetadataReference>();
            foreach(var stream in ReamQuery.Resource.Resources.Metadata())
            {
                list.Add(MetadataReference.CreateFromStream(stream));
            }
            return list;
        });

        public IEnumerable<MetadataReference> GetReferences()
        {
            var list = new List<MetadataReference>();
            foreach(var stream in ReamQuery.Resource.Resources.Metadata())
            {
                list.Add(MetadataReference.CreateFromStream(stream));
            }
            var coreAsm = new Uri(typeof(IGenerated).Assembly.CodeBase).LocalPath;
            var serverAsm = new Uri(typeof(Emitter).Assembly.CodeBase).LocalPath;
            list.Add(MetadataReference.CreateFromFile(coreAsm));
            list.Add(MetadataReference.CreateFromFile(serverAsm));
            return list;
            // return _references.Value;
        }
    }
}
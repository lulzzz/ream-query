namespace ReamQuery.Server.Services
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

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
            return list;
            // return _references.Value;
        }
    }
}
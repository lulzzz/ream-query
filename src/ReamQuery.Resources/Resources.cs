namespace ReamQuery.Resource
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Newtonsoft.Json;

    public static class Resources
    {
        static Assembly _assembly;
        static IEnumerable<string> _names;

        const string MetadataPrefix = "metadata-ref";

        static Resources()
        {
            _assembly = typeof(Resources).Assembly;
            _names = _assembly.GetManifestResourceNames();
        }

        public static IEnumerable<Stream> Metadata()
        {
            foreach(var name in _names)
            {
                if (name.StartsWith(MetadataPrefix))
                {
                    yield return _assembly.GetManifestResourceStream(name);
                }
            }
        }

        public static IEnumerable<string> List()
        {
            var stream = _assembly.GetManifestResourceStream(Validate("ref-list.json"));
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                var json = Encoding.Default.GetString(ms.ToArray());
                return JsonConvert.DeserializeObject<IEnumerable<string>>(json);
            }
        }

        static string Validate(string name)
        {
            if (!_names.Contains(name))
            {
                throw new ArgumentException("Invalid name");
            }
            return name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using disfr.Doc;

namespace disfr.Writer
{
    public class DefaultColumnDesc : IColumnDesc
    {
        public string Header { get; private set; }

        public string Path { get; private set; }

        private readonly Func<ITransPair, object> ContentHandler;

        private DefaultColumnDesc(string header, string path, Func<ITransPair, object> content_handler)
        {
            Header = header;
            Path = path;
            ContentHandler = content_handler;
        }

        public object GetContent(ITransPair pair)
        {
            return ContentHandler(pair);
        }

        public static IColumnDesc Create(string header, string path = null, IList<PropInfo> props = null)
        {
            if (header == null) throw new ArgumentNullException("header");
            if (path == null) path = header;

            switch (path)
            {
                case "Serial": return new DefaultColumnDesc(header, path, pair => pair.Serial);
                case "Id": return new DefaultColumnDesc(header, path, pair => pair.Id);
                case "Source": return new DefaultColumnDesc(header, path, pair => pair.Source);
                case "Target": return new DefaultColumnDesc(header, path, pair => pair.Target);
                case "SourceLang": return new DefaultColumnDesc(header, path, pair => pair.SourceLang);
                case "TargetLang": return new DefaultColumnDesc(header, path, pair => pair.TargetLang);
                case "Notes": return new DefaultColumnDesc(header, path, pair => pair.Notes);
            }

            if (path.StartsWith("[") && path.EndsWith("]"))
            {
                var index = int.Parse(path.Substring(1, path.Length - 2));
                return new DefaultColumnDesc(header, path, pair => pair[index]);
            }

            for (int i = 0; i < props.Count; i++)
            {
                if (path == props[i].Key)
                {
                    return new DefaultColumnDesc(header, "[" + i + "]", pair => pair[i]);
                }
            }

            throw new ArgumentOutOfRangeException("path", path, "Invalid property name.");
        }

        public static IColumnDesc[] DefaultDescs
        {
            get
            {
                return new[]
                {
                    Create("Serial"),
                    Create("Id"),
                    Create("Source"),
                    Create("Target"),
                    Create("SourceLang"),
                    Create("TargetLang"),
                    Create("Notes"),
                };
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;
using disfr.Writer;

namespace disfr.UI
{
    public abstract class RowDataColumnDesc : IColumnDesc
    {
        protected RowDataColumnDesc(string header, string path)
        {
            Header = header;
            Path = path;
        }

        public string Header { get; private set; }

        public string Path { get; private set; }

        public abstract object GetContent(ITransPair pair);

        private class SourceColumnDesc : RowDataColumnDesc
        {
            public SourceColumnDesc(string header, string path) : base(header, path) { }

            public override object GetContent(ITransPair pair)
            {
                return pair.Source;
            }
        }

        private class TargetColumnDesc : RowDataColumnDesc
        {
            public TargetColumnDesc(string header, string path) : base(header, path) { }

            public override object GetContent(ITransPair pair)
            {
                return pair.Target;
            }
        }

        private class Target2ColumnDesc : RowDataColumnDesc
        {
            public Target2ColumnDesc(string header, string path) : base(header, path) { }

            public override object GetContent(ITransPair pair)
            {
                throw new NotImplementedException();
            }
        }

        private class PropertyColumnDesc : RowDataColumnDesc
        {
            private readonly PropertyInfo PropertyInfo;

            public PropertyColumnDesc(string header, string path) : base(header, path)
            {
                PropertyInfo = typeof(IRowData).GetProperty(path) ?? typeof(ITransPair).GetProperty(path);
            }

            public override object GetContent(ITransPair pair)
            {
                return PropertyInfo.GetValue((IRowData)pair);
            }
        }

        private class IndexColumnDesc : RowDataColumnDesc
        {
            private readonly int PropIndex;

            public IndexColumnDesc(string header, string path) : base(header, path)
            {
                PropIndex = int.Parse(Path.Substring(1, Path.Length - 2));
            }

            public override object GetContent(ITransPair pair)
            {
                return pair[PropIndex];
            }
        }

        public static IColumnDesc Create(string header, string path)
        {
            switch (path)
            {
                case "Source": return new SourceColumnDesc(header, path);
                case "Target": return new TargetColumnDesc(header, path);
                case "Target2": return new Target2ColumnDesc(header, path);
                default:
                    break;
            }

            if (path.StartsWith("["))
            {
                return new IndexColumnDesc(header, path);
            }
            else
            {
                return new PropertyColumnDesc(header, path);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public class SdltmReader : IAssetReader
    {
        private static readonly string[] _FIlterString = { "Studio Translation Memory|*.sdltm" };

        public IList<string> FilterString {  get { return _FIlterString; } }

        public string Name { get { return "SdltmReader"; } }

        public int Priority { get { return 10; } }

        private class TmMeta
        {
            public string Name;
            public string SourceLang;
            public string TargetLang;
        };

        public IEnumerable<IAsset> Read(string filename, int filterindex)
        {
            using (var s = File.OpenRead(filename))
            {
                if (s.ReadByte() != 'S' || s.ReadByte() != 'Q' ||
                    s.ReadByte() != 'L' || s.ReadByte() != 'i' ||
                    s.ReadByte() != 't' || s.ReadByte() != 'e' ||
                    s.ReadByte() != ' ' || s.ReadByte() != 'f' ||
                    s.ReadByte() != 'o' || s.ReadByte() != 'r' ||
                    s.ReadByte() != 'm' || s.ReadByte() != 'a' ||
                    s.ReadByte() != 't' || s.ReadByte() != ' ' ||
                    s.ReadByte() != '3' || s.ReadByte() != '\0') return null;
            }

            SQLiteConnection connection = null;
            SQLiteDataReader reader = null;
            SQLiteCommand cmd = null;
            try
            {
                var b = new SQLiteConnectionStringBuilder()
                {
                    DataSource = filename,
                };
                try
                {
                    connection = new SQLiteConnection(b.ConnectionString);
                    connection.Open();
                }
                catch (Exception e)
                {
                    return null;
                }

                cmd = connection.CreateCommand();

                cmd.CommandText = @"SELECT value FROM parameters WHERE name = 'VERSION'";
                if ((cmd.ExecuteScalar() as string)?.StartsWith("8.") != true)
                {
                    return null;
                }

                cmd.CommandText = @"SELECT min(id) FROM translation_memories";
                var min = cmd.ExecuteScalar() as int?;
                cmd.CommandText = @"SELECT max(id) FROM translation_memories";
                var max = cmd.ExecuteScalar() as int?;
                if (!(max - min < 1000)) return null;

                var min_tm_id = (int)min;
                var tm_meta = new TmMeta[(int)max - min_tm_id + 1];

                cmd.CommandText = @"SELECT id, name, source_language, target_language FROM translation_memories";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var meta = new TmMeta()
                    {
                        Name = reader.GetString(1),
                        SourceLang = reader.GetString(2),
                        TargetLang = reader.GetString(3),
                    };
                    tm_meta[id - min_tm_id] = meta;
                }
                reader.Dispose();

                throw new NotImplementedException();

            }
            finally
            {
                cmd?.Dispose();
                reader?.Dispose();
                connection?.Dispose();
            }
        }
    }
}

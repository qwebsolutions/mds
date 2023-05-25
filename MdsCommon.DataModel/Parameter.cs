using System.Collections.Generic;
using System.Linq;
using System;

namespace MdsCommon
{
    public static partial class Parameter
    {
        public class DbServer
        {
            public string Server { get; set; } = string.Empty;
            public string Db { get; set; } = string.Empty;

            public override string ToString()
            {
                return $"server: {Server}, db: {Db}";
            }
        }

        public static DbServer ParseConnectionString(string dbConnection)
        {
            var dbServerKeys = new List<string>() { "data source", "server", "address", "addr", "network address" };
            var dbCatalogKeys = new List<string>() { "initial catalog", "database" };

            string dbServer = string.Empty;
            string dbCatalog = string.Empty;

            var segments = dbConnection.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                if (dbServerKeys.Any(x => segment.ToLower().Contains(x)))
                {
                    var keyValue = segment.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (keyValue.Count() == 2)
                    {
                        dbServer = keyValue.Last();
                    }
                }
                else
                {
                    if (dbCatalogKeys.Any(x => segment.ToLower().Contains(x)))
                    {
                        var keyValue = segment.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                        if (keyValue.Count() == 2)
                        {
                            dbCatalog = keyValue.Last();
                        }
                    }
                };
            }

            return new DbServer()
            {
                Db = dbCatalog,
                Server = dbServer
            };
        }
    }

}

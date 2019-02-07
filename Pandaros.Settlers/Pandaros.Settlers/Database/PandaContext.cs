using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Database
{
    public class PandaContext : DbContext
    {
            public PandaContext() :
                base(new SQLiteConnection()
                {
                    ConnectionString = new SQLiteConnectionStringBuilder() { DataSource = GameLoader.SAVE_LOC + "pandaros.settlers.sqlite", ForeignKeys = true }.ConnectionString
                }, true)
            {
            }
    }
}

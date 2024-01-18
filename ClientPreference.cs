using Dapper;
using Microsoft.Data.Sqlite;

namespace ClientPreference
{
    public class ClientPreference
    {
        private SqliteConnection Database = null!;

        public void RegDatabase(string modulepath, string modulename)
        {
            Database = new SqliteConnection($"Data Source={Path.Join(modulepath, modulename + ".db")}");
            Database.Open();
        }

        public async Task CreateTable(string tablename)
        {
            await Database.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS @tablename (`SteamID` VARCHAR(64), `Value` VARCHAR(256), PRIMARY KEY (`SteamID`));", tablename);
        }

        [Obsolete]
        public async Task RegClientData(string tablename, string steamid, string value)
        {
            if (Database == null)
            {
                return;
            }

            await Database.ExecuteAsync("BEGIN IF NOT EXIST (SELECT * FROM @DBName WHERE `SteamID` = @SteamId) BEGIN INSERT INTO @DatabaseName (`SteamID`, `Value`) VALUES(@steamid, @Values) END END)",
                new
                {
                    DBName = tablename,
                    SteamId = steamid,
                    Values = value
                });
        }

        public async Task<string> GetClientData(string tablename, string steamid)
        {
            if (Database == null)
            {
                return null!;
            }

            var value = await Database.QueryFirstOrDefaultAsync<string>(@"SELECT `Value` From @DBName WHERE `SteamID` = @SteamId",
                new
                {
                    DBName = tablename,
                    SteamId = steamid
                });

            return value!;
        }

        public async Task SetClientData(string tablename, string steamid, string value)
        {
            if (Database == null)
            {
                return;
            }

            await Database.ExecuteAsync("INSERT INTO @DBName (`SteamID`, `Value`) VALUES (@SteamId, @Values) ON CONFLICT(`SteamID`) DO UPDATE SET `Value` = @Values",
                new
                {
                    DBName = tablename,
                    SteamId = steamid,
                    Values = value
                });
        }
    }
}

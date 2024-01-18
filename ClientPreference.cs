using Dapper;
using Microsoft.Data.Sqlite;

namespace ClientPreference
{
    public class ClientPreference
    {
        private SqliteConnection Database = null!;

        string DatabaseName = null!;

        public async Task RegDatabase(string modulepath, string modulename, string databasename)
        {
            Database = new SqliteConnection($"Data Source={Path.Join(modulepath, modulename + ".db")}");
            Database.Open();

            DatabaseName = databasename;

            await Database.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS @DatabaseName (`SteamID` VARCHAR(64), `Value` VARCHAR(256), PRIMARY KEY (`SteamID`));", DatabaseName);
        }

        [Obsolete]
        public async Task RegClientData(string steamid, string value)
        {
            if (Database == null)
            {
                return;
            }

            await Database.ExecuteAsync("BEGIN IF NOT EXIST (SELECT * FROM @DBName WHERE `SteamID` = @SteamId) BEGIN INSERT INTO @DatabaseName (`SteamID`, `Value`) VALUES(@steamid, @Values) END END)",
                new
                {
                    DBName = DatabaseName,
                    SteamId = steamid,
                    Values = value
                });
        }

        public async Task<string> GetClientData(string steamid)
        {
            if (Database == null)
            {
                return null!;
            }

            var value = await Database.QueryFirstOrDefaultAsync<string>(@"SELECT `Value` From @DBName WHERE `SteamID` = @SteamId",
                new
                {
                    DBName = DatabaseName,
                    SteamId = steamid
                });

            return value!;
        }

        public async Task SetClientData(string steamid, string value)
        {
            if (Database == null)
            {
                return;
            }

            await Database.ExecuteAsync("INSERT INTO @DBName (`SteamID`, `Value`) VALUES (@SteamId, @Values) ON CONFLICT(`SteamID`) DO UPDATE SET `Value` = @Values",
                new
                {
                    SteamId = steamid,
                    Values = value
                });
        }
    }
}

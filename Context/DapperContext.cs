using System.Data;
using Dapper;
namespace netback.Context{
    public class DapperContext{
        private readonly IConfiguration _config;
        public List<String> tables;
        public Dictionary<string, List<dynamic>> tableDefinitions;

        public DapperContext(IConfiguration config){
            _config = config;
            var db = CreateConnection(); 
            tables = db.Query("show tables").Select(x=>(string)x.Tables_in_myDb).ToList();
            tableDefinitions = new Dictionary<string, List<dynamic>>{};
            Console.WriteLine("dapper connection");
        }
        public IDbConnection CreateConnection()=>
            new MySql.Data.MySqlClient.MySqlConnection(_config.GetConnectionString("DefaultConnection"));
        public List<dynamic> ColumnsGet(string tablename, Boolean forceReload=false){
            if(!tableDefinitions.ContainsKey(tablename)){
                var db = CreateConnection();
                tableDefinitions.Add(tablename,  db.Query($"DESCRIBE {tablename}").ToList());
            } 
            return tableDefinitions[tablename];
        }
    }
}
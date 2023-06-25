using System.Dynamic;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using netback.Context;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Data.Common;


/*
    All tables              Tables

    1 table, all data       {tablename}/All
    
*/


namespace netback.ControllerTables{

[Route("/api/v1")]
[ApiController]
public class Tables : ControllerBase
{
    private readonly DapperContext _dctx;

    public Tables(DapperContext dctx ){
        _dctx = dctx;
        Console.WriteLine("working");
    }
    [Authorize (Roles="admin") ]
    [HttpGet("Tables")   ] 
     public ActionResult<JsonContent>  ListDb(){
            var u = User.FindAll(ClaimTypes.Role)?.Select(s=>s.Value).ToList();
            Console.WriteLine("roles");
            Console.WriteLine(string.Join(", ", u ));
            return Ok( _dctx.tables);
    }
    [HttpGet("{tablename}/All")   ]
     public ActionResult<JsonContent>  TableAll(string tablename){
        if(_dctx.tables.Find(x=>x==tablename) == null)
            return NotFound("Unknown table name given");
        using(var db = _dctx.CreateConnection()){
            var result = db.Query($"Select * from {tablename}").ToList();

            return Ok( result);
        }
    }
    [HttpGet("{tablename}")]
     public ActionResult<JsonContent>  TableWhere(string tablename, string? where, string? orderby){
        if(_dctx.tables.Find(x=>x==tablename) == null)
            return NotFound("Unknown table name given");
        var qBuilder = new SqlBuilder(); 
        var qtemplate = qBuilder.AddTemplate($"select * from {tablename} /**where**/");
        var q = Request.Query;
        if(where!=null){            
            Console.WriteLine("where:", where);
            var wheres = where.Split("|");
            var indx = 0;
            foreach(var w in  wheres){
                indx++;
                var key = "a"+indx; 
                var ws = w.Split(",");
                if(ws.Length!=3){
                    Console.WriteLine("problem with where clause", w);
                    continue;
                }
                var nobj = new DynamicParameters();

                switch(ws[1]){
                    case "=":   case "eq":
                        nobj.Add(key, ws[2]); 
                        qBuilder.Where($"{ws[0]} = @{key}", nobj);//new {$"{key}" = ws[2]});
                        break;
                    case ">":   
                        nobj.Add(key, ws[2]); 
                        qBuilder.Where($"{ws[0]} > @{key}", nobj  );
                        break;
                    case "<":   
                        nobj.Add(key, ws[2]); 
                        qBuilder.Where($"{ws[0]} < @{key}", nobj  );
                        break;
                    case "in":   
                        nobj.Add(key, ws[2].Split(";"));
                        qBuilder.Where($"{ws[0]} in  @{key}", nobj); 
                        break;
                    
                    case "%":   case "like":
                        nobj.Add(key, ws[2]);
                        qBuilder.Where($"{ws[0]} like @{key}", nobj  );
                        break;
                    default:
                        Console.WriteLine("problem with where", ws);
                        break;
                }
            }

        }
        if(q.ContainsKey("orderby")){
            Console.WriteLine(q["where"]);
        }

        using(var db = _dctx.CreateConnection()){ 
            Console.WriteLine(qtemplate.RawSql);
            var result = db.Query(qtemplate.RawSql, qtemplate.Parameters).ToList();
            return Ok( result);
        }
        return NotFound();
    }
    [HttpGet("{tablename}/{id}")]
     public ActionResult<JsonContent>  TableOne(string tablename, int  id){
        if(_dctx.tables.Find(x=>x==tablename) == null)
            return NotFound("Unknown table name given");
        using(var db = _dctx.CreateConnection()){
            var result = db.Query($"select * from {tablename} where id={id}").ToList();
            return Ok( result);
        }
        return NotFound();
    }

    [HttpPost ("{tablename}")]
    public async Task<ActionResult<JsonContent>> Save(string tablename, [FromBody] JsonObject dataObject){
        if(_dctx.tables.Find(x=>x==tablename) == null)
            return NotFound("Unknown table name given"); 
        
        var form = dataObject; 
        var verb = "insert";
        var DBTableColumns  = _dctx.ColumnsGet(tablename);
        var datasToSave     =   new DynamicParameters();
        var unusedParams = new List<string>{};
        var usedParams = new List<string>{};
        foreach(var column in DBTableColumns){
            var fieldName = (string)column.Field;
            Console.WriteLine($"fieldname : {fieldName}");
            string value;
            if(! form.ContainsKey(fieldName) ){ 
                unusedParams.Add(fieldName);
                continue;
            }
                usedParams.Add(fieldName);
            switch(fieldName.ToLower()){
                case "id":
                    verb ="update";
                    break;
                case "updatedAt":
                    value =  new DateTime().ToString();
                    break;
            } 
            value =  form[fieldName]?.ToString(); 
            datasToSave.Add(fieldName, value)  ;
        }
         

        var template            = "";
        if(verb == "insert"){
            var fieldNames =  String.Join(", ",  datasToSave.ParameterNames);
            var fieldPlaceholders = String.Join(", ",  datasToSave.ParameterNames.Select(k=>"@"+k)); 
            template        = $" insert into {tablename}( {fieldNames} ) values ( {fieldPlaceholders} )  ";
        }else{
             var setValues = string.Join(", ", datasToSave.ParameterNames.Select(p=>$"{p}=@{p}"));
            template = $" update {tablename}  set {setValues}  where id= @id  ";
        }
        Console.WriteLine(template); 
        IEnumerable<dynamic> result;
        using(var db =  _dctx.CreateConnection()){
            try{
                result = db.Query(template, datasToSave);
            }catch(DbException  e){
                return Problem(e.Message );
            }
 
        }

        return Ok(new {
            usedParams=usedParams, 
            unusedParams=unusedParams, 
            result=result});
    }
    [HttpDelete("{tablename}/{id}") ]
    [Authorize (Roles="admin,delete") ]
    public ActionResult<JsonContent> Delete(string tablename, int id){
        
        IEnumerable<dynamic> result;
        using(var db = _dctx.CreateConnection()){
            try{
                result = db.Query($"delete from {tablename} where id = {id}");
            }catch(DbException e){
                return Problem(e.Message);
            }
        }
        return Ok(result );
    }
    
}

}
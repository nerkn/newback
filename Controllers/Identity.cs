
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using netback.Context;
using Dapper;


public class loginDTO{
    public string email  {get; set;}
    public string password {get; set;} 
}

namespace netback.ControllerTables{

[Route("/auth/v1")]
[ApiController]
public class Identity:ControllerBase{ 
    private readonly IConfiguration _config;
    private readonly DapperContext _dctx;

    public Identity(IConfiguration config, DapperContext dctx ){
            _config = config;
            _dctx = dctx;

            Console.WriteLine("Identitiy constructor");
    }
    private string generateToken(int userid, string username, List<string> roles){
        var key = _config.GetSection("JwtSettings")["Key"];
        var claims = new List<Claim>{
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("userid", userid.ToString()),
        };
        claims.AddRange(roles.Select(role=>new Claim(ClaimTypes.Role, role ) ));
        var signingCredentals = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:Key"])),
            SecurityAlgorithms.HmacSha256 
        );
        var token = new JwtSecurityToken(
            "issuer",
            "audience",
            claims,
            null,
            DateTime.UtcNow.AddHours(8),
            signingCredentals
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    [HttpGet("token")]
    public ActionResult glglg(){
        Console.WriteLine("'hello get'");
        return Ok("Tabi");
    }
    [HttpPost("token") ]
    public ActionResult GenerateToken(
        [FromBody] loginDTO login
    ){ 
        Console.WriteLine("login.email");
        Console.WriteLine(login);
        Console.WriteLine(login.email);
        Console.WriteLine(login.password);
        using(var db = _dctx.CreateConnection()){
            var result = db.Query($"select * from User where email=@email ",
                new {email=login.email, password=login.password } ).ToList();
            if(result.Count<1){
                return Problem("Problem with login");
            }
            return Ok(generateToken(result.First().id, result.First().username, new List<string>{"admin"} ));
        }
        return Ok();
    }
 
}
}
using Microsoft.AspNetCore.Mvc;
using Promul.Relay.Server.Models.Sessions;
using Promul.Relay.Server.Relay;

using System.Security.Cryptography;

namespace Promul.Relay.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    private readonly ILogger<SessionController> _logger;
    private readonly RelayServer _relay;

    public SessionController(ILogger<SessionController> logger, RelayServer server)
    {
        _logger = logger;
        _relay = server;
    }

    [HttpPut("Create")]
    public SessionInfo CreateSession()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int joinCodeLength = 6;

        if (System.Environment.GetEnvironmentVariable("JOIN_CODE_LENGTH") != null)
        {
            if (Int32.TryParse(System.Environment.GetEnvironmentVariable("JOIN_CODE_LENGTH"), out int length))
            {
                if (length <= 0) {
                    _logger.LogInformation("Not using JOIN_CODE_LENGTH enviroment variable value: `{}`, below zero",
                        length);
                }

                joinCodeLength = length;
            } 
            else
            {
                _logger.LogInformation("Not using JOIN_CODE_LENGTH enviroment variable: `{}`, not an integer",
                    System.Environment.GetEnvironmentVariable("JOIN_CODE_LENGTH"));
            }
        }

        var joinCode = new string(Enumerable.Repeat(chars, joinCodeLength).Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
        //var joinCode = "TEST";

        // Verify Join Code is unique
        // Not the best implementation, but would only cause real theoretical problems with around 26^6/6 (51 million) lobbies
        // If that's somehow an issue, just expand join code length using env var JOIN_CODE_LENGTH
        while (_relay.GetSession(joinCode) != null) 
        {
            joinCode = new string(Enumerable.Repeat(chars, joinCodeLength).Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
        }

        _relay.CreateSession(joinCode);

        int relayPort = 15593;
        if (
            System.Environment.GetEnvironmentVariable("RELAY_PORT") != null && 
            Int32.TryParse(System.Environment.GetEnvironmentVariable("RELAY_PORT"), out int port)
        )
        {
            relayPort = port;
        }
        var sci = new SessionInfo
        {
            JoinCode = joinCode,
            RelayAddress = System.Environment.GetEnvironmentVariable("RELAY_ADDRESS") ?? "aus628.relays.net.fireworkeyes.com",
            RelayPort = relayPort
        };

        _logger.LogInformation("User {}:{} created session with join code {}",
            HttpContext.Connection.RemoteIpAddress,
            HttpContext.Connection.RemotePort,
            sci.JoinCode);

        return sci;
    }

    [HttpPut("Join")]
    public ActionResult<SessionInfo> JoinSession([FromBody] SessionRequestJoinInfo joinCode)
    {
        var session = _relay.GetSession(joinCode.JoinCode);
        if (session == null) return NotFound();

        int relayPort = 15593;
        if (
            System.Environment.GetEnvironmentVariable("RELAY_PORT") != null && 
            Int32.TryParse(System.Environment.GetEnvironmentVariable("RELAY_PORT"), out int port)
        )
        {
            relayPort = port;
        }
        return new SessionInfo
        {
            JoinCode = session.JoinCode,
            RelayAddress = System.Environment.GetEnvironmentVariable("RELAY_ADDRESS") ?? "aus628.relays.net.fireworkeyes.com",
            RelayPort = relayPort
        };
    }

    [HttpDelete("Destroy")]
    public ActionResult<SessionInfo> DestroySession([FromBody] SessionRequestJoinInfo joinCode)
    {
        // For backwards compatability and safety if API public
        if (System.Environment.GetEnvironmentVariable("ENABLE_DESTROY_API") != null && !(
            System.Environment.GetEnvironmentVariable("ENABLE_DESTROY_API") == "true" || 
            System.Environment.GetEnvironmentVariable("ENABLE_DESTROY_API") == "t" ||
            System.Environment.GetEnvironmentVariable("ENABLE_DESTROY_API") == "1" )
        )
        {
            return NotFound();
        }

        var session = _relay.GetSession(joinCode.JoinCode);
        if (session == null) return NotFound();

        _logger.LogInformation("User {}:{} destroyed session with join code {}",
            HttpContext.Connection.RemoteIpAddress,
            HttpContext.Connection.RemotePort,
            joinCode.JoinCode);

        _relay.DestroySession(session);
        
        return Ok();
    }

    public struct SessionRequestJoinInfo
    {
        public string JoinCode { get; set; }
    }
}
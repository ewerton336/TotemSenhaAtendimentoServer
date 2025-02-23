using Microsoft.AspNetCore.Mvc;
using TotemSenhaAtendimentoServer.Domain.Senhas.Dtos;
using TotemSenhaAtendimentoServer.Domain.Senhas.Entities;
using TotemSenhaAtendimentoServer.Domain.Senhas.Services;

namespace TotemSenhaAtendimentoServer.Host.Controllers
{
    [ApiController]
    [Route("api/senhas")]
    public class SenhaController : ControllerBase
    {
        private readonly ISenhaService _senhaService;

        public SenhaController(ISenhaService senhaService)
        {
            _senhaService = senhaService;
        }

        [HttpPost]
        public IActionResult GerarSenha([FromBody] SenhaRequest request)
        {
            if (request == null)
            {
                return BadRequest("Dados inválidos.");
            }

            Senha senha = _senhaService.GerarSenha(request);

            return Ok(senha);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using TotemSenhaAtendimentoServer.Domain.Filas.Dtos;
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
        public async Task<IActionResult> GerarSenha([FromBody] SenhaRequest request)
        {
            if (request == null)
            {
                return BadRequest("Dados inválidos.");
            }

            string queueName = request.Prioritario ? "fila_senhas_prioritaria" : "fila_senhas_normal";
            Senha senha = await _senhaService.GerarSenha(request, queueName);

            return Ok(senha);
        }

        [HttpGet("fila")]
        public async Task<FilaSenhasResponse> GetFila()
        {
            return await _senhaService.ObterFila();
        }



        [HttpPost("chamar")]
        public async Task<Senha?> ChamarProximaSenha(bool prioritario)
        {
            string queueName = prioritario ? "fila_senhas_prioritaria" : "fila_senhas_normal";
            var senha = await _senhaService.ChamarProximaSenha(queueName);
            return senha;
        }
    }
}

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
        public async Task<IActionResult> GetFila()
        {
            var filaNormal = await _senhaService.ObterFila("fila_senhas_normal");
            var filaPrioritaria = await _senhaService.ObterFila("fila_senhas_prioritaria");

            return Ok(new
            {
                Normal = filaNormal,
                Prioritaria = filaPrioritaria
            });
        }



        [HttpPost("chamar/{tipo}")]
        public IActionResult ChamarProximaSenha(string tipo)
        {
            string queueName = tipo.ToLower() == "prioritaria" ? "fila_senhas_prioritaria" : "fila_senhas_normal";
            var senha = _senhaService.ChamarProximaSenha(queueName);

            if (senha == null)
            {
                return NotFound("Nenhuma senha na fila.");
            }

            return Ok(senha);
        }
    }
}

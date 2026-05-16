using Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RespuestasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RespuestasController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL: api/Respuestas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Respuesta>>> GetRespuestas()
        {
            return await _context.Respuestas
                .Include(r => r.Peticion) // Incluir la petición relacionada
                .ToListAsync();
        }

        // 2. GET BY ID: api/Respuestas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Respuesta>> GetRespuesta(int id)
        {
            var respuesta = await _context.Respuestas.FindAsync(id);

            if (respuesta == null)
            {
                return NotFound(new { mensaje = $"No se encontró la respuesta con ID {id}" });
            }

            return respuesta;
        }

        // 3. POST (Crear): api/Respuestas
        [HttpPost]
        public async Task<ActionResult<Respuesta>> PostRespuesta(Respuesta respuesta)
        {
            _context.Respuestas.Add(respuesta);
            await _context.SaveChangesAsync();

            // Esto devuelve un estatus 201 Created y la ruta para consultar el objeto creado
            return CreatedAtAction(nameof(GetRespuesta), new { id = respuesta.Id }, respuesta);
        }

       
        // 4. DELETE: api/Respuestas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRespuesta(int id)
        {
            var respuesta = await _context.Respuestas.FindAsync(id);
            if (respuesta == null)
            {
                return NotFound(new { mensaje = $"No se encontró la respuesta con ID {id} para eliminar" });
            }

            _context.Respuestas.Remove(respuesta);
            await _context.SaveChangesAsync();

            return NoContent(); // Estatus 204: Eliminado con éxito
        }

      
        
    }
}
using Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncargadosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EncargadosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL: api/Encargados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Encargados>>> GetEncargados()
        {
            var lista = await _context.Encargados
                .Include(e => e.Laboratorio.Nombre_Laboratorio) // Incluir el laboratorio relacionado
                .ToListAsync();
            return Ok(lista);
            //return await _context.Encargados.ToListAsync();
        }

        // 2. GET BY ID: api/Encargados/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Encargados>> GetEncargado(int id)
        {
            var lista = await _context.Encargados
                .Include(e => e.Laboratorio.Nombre_Laboratorio) // Incluir el laboratorio relacionado
                .FirstOrDefaultAsync(e => e.Id == id);

            if (lista == null)
            {
                return NotFound(new { mensaje = $"No se encontró la Encargado con ID {id}" });
            }

            return Ok(lista);
        }

        // 3. POST (Crear): api/Encargados
        [HttpPost]
        public async Task<ActionResult<Encargados>> PostEncargado(Encargados Encargado)
        {
            _context.Encargados.Add(Encargado);
            await _context.SaveChangesAsync();

            // Esto devuelve un estatus 201 Created y la ruta para consultar el objeto creado
            return CreatedAtAction(nameof(GetEncargado), new { id = Encargado.Id }, Encargado);
        }

        // 4. PUT (Actualizar): api/Encargados/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEncargado(int id, Encargados Encargado)
        {
            if (id != Encargado.Id)
            {
                return BadRequest(new { mensaje = "El ID del parámetro no coincide con el ID del objeto" });
            }

            _context.Entry(Encargado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EncargadoExists(id))
                {
                    return NotFound(new { mensaje = $"La Encargado con ID {id} ya no existe" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Estatus 204: Actualizado con éxito, sin contenido que devolver
        }

        // 5. DELETE: api/Encargados/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEncargado(int id)
        {
            var Encargado = await _context.Encargados.FindAsync(id);
            if (Encargado == null)
            {
                return NotFound(new { mensaje = $"No se encontró la Encargado con ID {id} para eliminar" });
            }

            _context.Encargados.Remove(Encargado);
            await _context.SaveChangesAsync();

            return NoContent(); // Estatus 204: Eliminado con éxito
        }

        // Método de soporte para verificar si el registro existe
        private bool EncargadoExists(int id)
        {
            return _context.Encargados.Any(e => e.Id == id);
        }
    }
}

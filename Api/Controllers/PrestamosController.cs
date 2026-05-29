using Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamosController : ControllerBase
    {

        private readonly AppDbContext _context;

        public PrestamosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL: api/Peticiones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prestamos>>> GetPeticiones
            ()
        {
            var lista = await _context.Prestamos
                .Include(l => l.Laboratorios.Nombre_Laboratorio) // Incluir el laboratorio relacionado
                .ToListAsync();
            return Ok(lista);
        }

        // 2. GET BY ID: api/Peticiones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Prestamos>> GetPeticione(int id)
        {
            var Peticione = await _context.Prestamos.FindAsync(id);

            if (Peticione == null)
            {
                return NotFound(new { mensaje = $"No se encontró la Peticione con ID {id}" });
            }

            return Peticione;
        }

        // 3. POST (Crear): api/Peticiones
        [HttpPost]
        public async Task<ActionResult<Prestamos>> PostPeticione(Prestamos Peticione)
        {
            _context.Prestamos.Add(Peticione);
            await _context.SaveChangesAsync();

            // Esto devuelve un estatus 201 Created y la ruta para consultar el objeto creado
            return CreatedAtAction(nameof(GetPeticione), new { id = Peticione.Id }, Peticione);
        }
  
        // 4. PUT (Actualizar): api/Peticiones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPeticione(int id, Prestamos Peticione)
        {
            if (id != Peticione.Id)
            {
                return BadRequest(new { mensaje = "El ID del parámetro no coincide con el ID del objeto" });
            }

            _context.Entry(Peticione).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PeticioneExists(id))
                {
                    return NotFound(new { mensaje = $"La Peticione con ID {id} ya no existe" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Estatus 204: Actualizado con éxito, sin contenido que devolver
        }

        // 5. DELETE: api/Peticiones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeticione(int id)
        {
            var Peticione = await _context.Prestamos.FindAsync(id);
            if (Peticione == null)
            {
                return NotFound(new { mensaje = $"No se encontró la Peticione con ID {id} para eliminar" });
            }

            _context.Prestamos.Remove(Peticione);
            await _context.SaveChangesAsync();

            return NoContent(); // Estatus 204: Eliminado con éxito
        }

        // Método de soporte para verificar si el registro existe
        private bool PeticioneExists(int id)
        {
            return _context.Prestamos.Any(e => e.Id == id);
        }
    }

}
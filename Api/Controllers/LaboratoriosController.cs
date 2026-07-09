using Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaboratoriosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LaboratoriosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL: api/Laboratorios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Laboratorios>>> GetLaboratorios()
        {
            var lista = await _context.Laboratorios
                .Include(l => l.Prestamos)  // Incluir peticiones relacionadas
                .ToListAsync();
            return Ok(lista);
            //return await _context.Laboratorios.ToListAsync();
        }

        // 2. GET BY ID: api/Laboratorios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Laboratorios>> GetLaboratorio(int id)
        {
            var lista = await _context.Laboratorios
                .Include(l => l.Prestamos)  // Incluir peticiones relacionadas
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lista == null)
            
                return NotFound(new { mensaje = $"No se encontró la Laboratorio con ID {id}" });
            

            return Ok(lista);
        }

        // 3. POST (Crear): api/Laboratorios
        [HttpPost]
        public async Task<ActionResult<Laboratorios>> PostLaboratorio(Laboratorios NuevoLaboratorio)
        {
            _context.Laboratorios.Add(NuevoLaboratorio);
            await _context.SaveChangesAsync();

            // Esto devuelve un estatus 201 Created y la ruta para consultar el objeto creado
            return CreatedAtAction(nameof(GetLaboratorio), new { id = NuevoLaboratorio.Id }, NuevoLaboratorio);
        }

        // 4. PUT (Actualizar): api/Laboratorios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLaboratorio(int id, Laboratorios Laboratorio)
        {
            if (id != Laboratorio.Id)
            {
                return BadRequest(new { mensaje = "El ID del parámetro no coincide con el ID del objeto" });
            }

            _context.Entry(Laboratorio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LaboratorioExists(id))
                {
                    return NotFound(new { mensaje = $"La Laboratorio con ID {id} ya no existe" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Estatus 204: Actualizado con éxito, sin contenido que devolver
        }

        // 5. DELETE: api/Laboratorios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLaboratorio(int id)
        {
            var Laboratorio = await _context.Laboratorios.FindAsync(id);
            if (Laboratorio == null)
            {
                return NotFound(new { mensaje = $"No se encontró la Laboratorio con ID {id} para eliminar" });
            }

            _context.Laboratorios.Remove(Laboratorio);
            await _context.SaveChangesAsync();

            return NoContent(); // Estatus 204: Eliminado con éxito
        }

        // Método de soporte para verificar si el registro existe
        private bool LaboratorioExists(int id)
        {
            return _context.Laboratorios.Any(e => e.Id == id);
        }
    }
}

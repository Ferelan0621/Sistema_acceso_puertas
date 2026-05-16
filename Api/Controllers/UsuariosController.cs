using Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuarios>>> GetUsuarios()
        {
            //return await _context.Usuarios.ToListAsync();
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol) // Incluir el rol relacionado
                .Include(u => u.Peticiones) // Incluir las peticiones relacionadas
                .ToListAsync();
            return Ok (usuarios);
        }

        // 2. GET BY ID: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuarios>> GetUsuario(int id)
        {
            var Usuario = await _context.Usuarios
                .Include(u => u.Rol) // Incluir el rol relacionado
                .Include(u => u.Peticiones) // Incluir las peticiones relacionadas
                .FirstOrDefaultAsync(u => u.Id == id);


            if (Usuario == null)
            {
                return NotFound(new { mensaje = $"No se encontró la Usuario con ID {id}" });
            }

            return Usuario;
        }

        // 3. POST (Crear): api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuarios>> PostUsuario(Usuarios Usuario)
        {
            _context.Usuarios.Add(Usuario);
            await _context.SaveChangesAsync();

            // Esto devuelve un estatus 201 Created y la ruta para consultar el objeto creado
            return CreatedAtAction(nameof(GetUsuario), new { id = Usuario.Id }, Usuario);
        }

        // 4. PUT (Actualizar): api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuarios Usuario)
        {
            if (id != Usuario.Id)
            {
                return BadRequest(new { mensaje = "El ID del parámetro no coincide con el ID del objeto" });
            }

            _context.Entry(Usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound(new { mensaje = $"La Usuario con ID {id} ya no existe" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Estatus 204: Actualizado con éxito, sin contenido que devolver
        }

        // 5. DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var Usuario = await _context.Usuarios.FindAsync(id);
            if (Usuario == null)
            {
                return NotFound(new { mensaje = $"No se encontró la Usuario con ID {id} para eliminar" });
            }

            _context.Usuarios.Remove(Usuario);
            await _context.SaveChangesAsync();

            return NoContent(); // Estatus 204: Eliminado con éxito
        }

        // Método de soporte para verificar si el registro existe
        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}

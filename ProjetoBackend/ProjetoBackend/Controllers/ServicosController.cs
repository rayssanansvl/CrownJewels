using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoBackend.Data;
using ProjetoBackend.Models;

namespace ProjetoBackend.Controllers
{
    public class ServicosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Servicos
        public async Task<IActionResult> Index()
        {
            var servicos = await _context.Servicos.ToListAsync();
            return View(servicos.OrderBy(s => s.Nome)); // Ordena por Nome antes de exibir
        }

        // GET: Servicos/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var servico = await _context.Servicos
                .FirstOrDefaultAsync(m => m.ServicoId == id);
            if (servico == null) return NotFound();

            return View(servico);
        }

        // GET: Servicos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Servicos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,ValorServico")] Servico servico)
        {
            if (ModelState.IsValid)
            {
                servico.ServicoId = Guid.NewGuid(); // Define o ID como um novo GUID
                _context.Add(servico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(servico);
        }

        // GET: Servicos/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null) return NotFound();

            return View(servico);
        }

        // POST: Servicos/Edit/5
        // POST: Servicos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ServicoId,Nome,ValorServico")] Servico servico)
        {
            if (id != servico.ServicoId)
            {
                return BadRequest("O ID fornecido não corresponde ao serviço a ser editado.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var servicoExistente = await _context.Servicos.FindAsync(id);

                    if (servicoExistente == null)
                    {
                        return NotFound("Serviço não encontrado.");
                    }

                    // Atualiza apenas os campos que podem ser editados
                    servicoExistente.Nome = servico.Nome;
                    servicoExistente.ValorServico = servico.ValorServico;

                    _context.Servicos.Update(servicoExistente); // Atualiza o serviço existente
                    await _context.SaveChangesAsync(); // Salva as alterações
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicoExists(servico.ServicoId))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(servico);
        }


        // GET: Servicos/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var servico = await _context.Servicos
                .FirstOrDefaultAsync(m => m.ServicoId == id);
            if (servico == null) return NotFound();

            return View(servico);
        }

        // POST: Servicos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var servico = await _context.Servicos.FindAsync(id);
            if (servico != null)
            {
                _context.Servicos.Remove(servico);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Método privado para verificar se o serviço existe
        private bool ServicoExists(Guid id)
        {
            return _context.Servicos.Any(e => e.ServicoId == id);
        }
    }
}

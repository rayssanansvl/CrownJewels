using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoBackend.Data;
using ProjetoBackend.Models;

namespace ProjetoBackend.Controllers
{
    public class ItensVendasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItensVendasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ItensVendas
        public async Task<IActionResult> Index(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaItens = await _context.ItensVenda
                .Include(i => i.Produto)
                .Include(i => i.Venda)
                .Where(i => i.VendaId == id)
                .ToListAsync();

            ViewData["idVendaAtual"] = id;
            return View("Index", listaItens);
        }

        // GET: ItensVendas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemVenda = await _context.ItensVenda
                .Include(i => i.Produto)
                .Include(i => i.Venda)
                .FirstOrDefaultAsync(m => m.ItemVendaId == id);
            if (itemVenda == null)
            {
                return NotFound();
            }

            return View(itemVenda);
        }

        // GET: ItensVendas/Create
        public IActionResult Create(Guid? id)
        {
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome");
            ViewData["VendaId"] = id;
            return View();
        }

        // POST: ItensVendas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemVendaId,VendaId,ProdutoId,Quantidade,ValorUnitario,ValorTotal")] ItemVenda itemVenda)
        {
            if (ModelState.IsValid)
            {
                itemVenda.ItemVendaId = Guid.NewGuid();
                _context.Add(itemVenda);
                await _context.SaveChangesAsync();

                // lista de itens da venda
                var listaItens = await _context.ItensVenda
                    .Include(i => i.Produto)
                    .Include(i => i.Venda)
                    .Where(v => v.VendaId == itemVenda.VendaId)
                    .ToListAsync();

                // Calcular o valor total da venda
                var valorTotalVenda = listaItens.Sum(i => i.ValorTotal);

                // Atualizar o valor total da venda correspondente
                var venda = await _context.Vendas.FindAsync(itemVenda.VendaId);
                venda.ValorTotal = valorTotalVenda;

                // Salvar mudança no banco de dados
                await _context.SaveChangesAsync();

                ViewData["idVendaAtual"] = itemVenda.VendaId;
                return View("Index", listaItens);
            }
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome", itemVenda.ProdutoId);
            ViewData["VendaId"] = new SelectList(_context.Vendas, "VendaId", "NotaFiscal", itemVenda.VendaId);
            return View(itemVenda);
        }

        // GET: ItensVendas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemVenda = await _context.ItensVenda.FindAsync(id);
            if (itemVenda == null)
            {
                return NotFound();
            }
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome", itemVenda.ProdutoId);
            ViewData["VendaId"] = new SelectList(_context.Vendas, "VendaId", "NotaFiscal", itemVenda.VendaId);
            return View(itemVenda);
        }

        // POST: ItensVendas/Edit/5
        // POST: ItensVendas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ItemVendaId,VendaId,ProdutoId,Quantidade,ValorUnitario,ValorTotal")] ItemVenda itemVenda)
        {
            if (id != itemVenda.ItemVendaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Atualizar o item da venda no banco de dados
                    _context.Update(itemVenda);
                    await _context.SaveChangesAsync();

                    // Recalcular o valor total do item (ValorUnitario * Quantidade)
                    itemVenda.ValorTotal = itemVenda.ValorUnitario * itemVenda.Quantidade;

                    // Atualizar a lista de itens da venda
                    var listaItens = await _context.ItensVenda
                        .Include(i => i.Produto)
                        .Include(i => i.Venda)
                        .Where(i => i.VendaId == itemVenda.VendaId)
                        .ToListAsync();

                    // Calcular o valor total da venda
                    var valorTotalVenda = listaItens.Sum(i => i.ValorTotal);

                    // Atualizar o valor total da venda no banco de dados
                    var venda = await _context.Vendas.FindAsync(itemVenda.VendaId);
                    if (venda != null)
                    {
                        venda.ValorTotal = valorTotalVenda;
                        _context.Update(venda); // Atualizar a venda com o novo valor total
                        await _context.SaveChangesAsync(); // Salvar as mudanças da venda
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemVendaExists(itemVenda.ItemVendaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redireciona de volta para a lista de itens da venda
                return RedirectToAction(nameof(Index), new { id = itemVenda.VendaId });
            }

            // Caso o modelo não seja válido, carregue os dados para o formulário de edição
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome", itemVenda.ProdutoId);
            ViewData["VendaId"] = new SelectList(_context.Vendas, "VendaId", "NotaFiscal", itemVenda.VendaId);
            return View(itemVenda);
        }


        // GET: ItensVendas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemVenda = await _context.ItensVenda
                .Include(i => i.Produto)
                .Include(i => i.Venda)
                .FirstOrDefaultAsync(m => m.ItemVendaId == id);
            if (itemVenda == null)
            {
                return NotFound();
            }

            return View(itemVenda);
        }

        // POST: ItensVendas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var itemVenda = await _context.ItensVenda.FindAsync(id);

            // Se o item de venda foi encontrado, removê-lo
            if (itemVenda != null)
            {
                var vendaId = itemVenda.VendaId;  // Guardar o ID da venda antes de remover o item
                _context.ItensVenda.Remove(itemVenda);
                await _context.SaveChangesAsync();

                // Agora, vamos atualizar a lista de itens da venda
                var listaItens = await _context.ItensVenda
                    .Include(i => i.Produto)
                    .Include(i => i.Venda)
                    .Where(i => i.VendaId == vendaId)
                    .ToListAsync();

                // Atualizar o valor total da venda após a exclusão
                var valorTotalVenda = listaItens.Sum(i => i.ValorTotal);
                var venda = await _context.Vendas.FindAsync(vendaId);
                venda.ValorTotal = valorTotalVenda;
                await _context.SaveChangesAsync();

                // Redirecionando para a lista de itens da venda (Index)
                return View("Index", listaItens);
            }

            return NotFound();
        }

        private bool ItemVendaExists(Guid id)
        {
            return _context.ItensVenda.Any(e => e.ItemVendaId == id);
        }

        public double PrecoProduto(Guid id)
        {
            var produto = _context.Produtos.Where(p => p.ProdutoId == id).FirstOrDefault();

            if (produto == null)
            {
                return 0; // ou outro valor adequado
            }

            return produto.Preco;
        }
    }
}

(function () {
    const $ = window.jQuery;
    if (!$) return;

    const $input = $('#globalSearch');
    if ($input.length === 0) return;
    const $list = $('#globalSearchList');

    // Detecta en qué pantalla estás (ListProduct establece window.pageScope = 'products')
    const scope = window.pageScope || 'global';
    if (scope === 'products') {
        $input.attr('placeholder', 'Buscar productos…');
    }

    // Debounce helper
    function debounce(fn, ms) { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; }

    // Render de sugerencias (protegido si no hay lista)
    function renderSuggestions(items) {
        if ($list.length) $list.empty();
        if (!items || !items.length) {
            if ($list.length) {
                $list.append('<li class="px-3 py-2 text-sm text-gray-500">Sin resultados</li>');
                $list.removeClass('hidden');
            }
            return;
        }
        if ($list.length) {
            items.forEach(p => {
                $list.append(
                    `<li class="px-3 py-2 cursor-pointer hover:bg-gray-50 text-sm flex justify-between items-center" data-id="${p.id}">
                       <span class="truncate">${p.name}</span>
                       <span class="ml-3 text-gray-500 text-xs">₡${Number(p.price).toFixed(2)} · IVA ${Number(p.taxPercent).toFixed(2)}% · stk ${p.stock}</span>
                     </li>`
                );
            });
            $list.removeClass('hidden');
        }
    }

    // Render de la tabla (solo en ListProduct)
    function renderProductsTable(items) {
        const $tbody = $('#productsTableBody');
        if ($tbody.length === 0) return;
        $tbody.empty();

        if (!items || !items.length) {
            $tbody.append('<tr><td colspan="8" class="px-6 py-6 text-center text-gray-400">No se encontraron productos.</td></tr>');
            return;
        }

        items.forEach(p => {
            const img = p.imageUrl
                ? `<img src="${p.imageUrl}" alt="${p.name}" class="w-12 h-12 rounded-lg object-cover">`
                : `<span class="text-gray-400 italic">No image</span>`;

            const row = `
        <tr class="border-t border-[color:var(--border)] hover:bg-[color:var(--light-bg)] transition-colors">
          <td class="px-6 py-4 text-center">
            <div class="flex items-center justify-center gap-4">
              <a href="/Product/EditProduct/${p.id}" class="text-[color:var(--primary)] hover:text-[color:var(--primary-dark)]">
                <i data-lucide="pencil-line" class="w-5 h-5"></i>
              </a>
              <a href="/Product/DeleteProduct/${p.id}" class="text-[color:var(--error)] hover:text-red-700">
                <i data-lucide="trash-2" class="w-5 h-5"></i>
              </a>
              <a href="/Product/DetailsProduct/${p.id}" class="text-[color:var(--secondary)] hover:text-emerald-600">
                <i data-lucide="eye" class="w-5 h-5"></i>
              </a>
            </div>
          </td>
          <td class="px-6 py-4">${p.name}</td>
          <td class="px-6 py-4">${p.categoryId ?? ''}</td>
          <td class="px-6 py-4 text-right">₡${Number(p.price).toFixed(2)}</td>
          <td class="px-6 py-4 text-right">${Number(p.taxPercent).toFixed(2)}%</td>
          <td class="px-6 py-4 text-right">${p.stock}</td>
          <td class="px-6 py-4">${img}</td>
          <td class="px-6 py-4">
            <span class="px-3 py-1 rounded-full text-xs font-medium bg-emerald-100 text-emerald-700">Active</span>
          </td>
        </tr>`;
            $tbody.append(row);
        });

        if (window.lucide?.createIcons) window.lucide.createIcons();
    }

    // Búsqueda AJAX (solo usamos productos por ahora)
    const doSearch = debounce(function () {
        const q = $input.val().trim();
        if (!q) { if ($list.length) $list.addClass('hidden').empty(); return; }

        $.getJSON('/api/productos/buscar', { q })
            .done(items => {
                renderSuggestions(items);
                if (scope === 'products') renderProductsTable(items);
            })
            .fail(() => { if ($list.length) $list.addClass('hidden').empty(); });
    }, 300);

    // Eventos
    $input.on('input', doSearch);

    // Click en sugerencia -> Detalles
    if ($list.length) {
        $list.on('click', 'li[data-id]', function () {
            const id = $(this).data('id');
            if (id) window.location.assign(`/Product/DetailsProduct/${id}`);
        });
    }

    // Cerrar si clic fuera
    $(document).on('click', e => {
        if ($list.length && !$(e.target).closest('#globalSearchBox').length) $list.addClass('hidden');
    });

    // Atajo "/" para enfocar
    $(document).on('keydown', e => {
        if (e.key === '/' && !$(e.target).is('input,textarea')) {
            e.preventDefault(); $input.trigger('focus');
        }
    });

    // Si la URL trae ?q=..., precarga el input y dispara la búsqueda
    const urlQ = new URLSearchParams(window.location.search).get('q');
    if (urlQ) { $input.val(urlQ); doSearch(); }

    // Fallback a prueba de balas: Enter siempre navega aunque falle algo arriba
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;
        const el = document.activeElement;
        if (!el || el.id !== 'globalSearch') return;
        const q = (el.value || '').trim();
        const base = '/Product/ListProduct';
        const url = q ? (base + '?q=' + encodeURIComponent(q)) : base;
        e.preventDefault();
        window.location.assign(url);
    });
})();

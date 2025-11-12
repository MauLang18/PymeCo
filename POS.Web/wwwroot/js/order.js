(function ($) {
    'use strict';

    function pageActive() { return $('#txtBuscar').length > 0; }

    // ---------- utilidades ----------
    function debounce(fn, ms) {
        let t; return function () { clearTimeout(t); const a = arguments, c = this; t = setTimeout(() => fn.apply(c, a), ms); };
    }
    function antiForgeryHeader() {
        var token = $('input[name="__RequestVerificationToken"]').val();
        return token ? { 'RequestVerificationToken': token } : {};
    }

    // ---------- render ----------
    function renderResultados(items) {
        const $ul = $('#resultados').empty();
        if (!items || !items.length) {
            $ul.append('<li class="list-group-item text-muted">Sin resultados</li>');
            return;
        }
        items.forEach(function (p) {
            const $li = $(`
        <li class="list-group-item list-group-item-action" role="button">
          ${p.name} — ₡${p.price} (Imp ${p.taxPercent}%, Stock ${p.stock})
        </li>`);
            $li.data('producto', p);
            $ul.append($li);
        });
    }

    // ---------- validación ----------
    // Crea/obtiene el validador de la página
    function ensureValidator() {
        var $form = $('#orderForm');
        if (!$form.data('validator')) {
            $form.validate({
                ignore: [], // valida también campos dinámicos
                errorClass: 'is-invalid',
                validClass: 'is-valid',
                errorPlacement: function (err, el) {
                    // muestra el error debajo del input
                    err.addClass('text-danger small');
                    err.insertAfter(el);
                }
            });
        }
        return $form.data('validator');
    }

    // Agrega reglas a inputs dinámicos
    function addRules($row) {
        const $qty = $row.find('td:eq(1) input');
        const $disc = $row.find('td:eq(2) input');

        $qty.rules('add', {
            required: true, min: 1, digits: true, messages: {
                required: 'Requerido', min: '≥ 1', digits: 'Entero'
            }
        });
        $disc.rules('add', {
            required: true, min: 0, max: 100, number: true, messages: {
                required: 'Requerido', min: '≥ 0', max: '≤ 100', number: 'Número'
            }
        });
    }

    // ---------- tabla ----------
    function agregarLinea(p) {
        const $tr = $(`
      <tr>
        <td data-id="${p.id}">${p.name}</td>
        <td><input type="number" class="form-control form-control-sm" min="1" value="1" name="qty[]"></td>
        <td><input type="number" class="form-control form-control-sm" min="0" max="100" value="0" name="disc[]"></td>
        <td><button type="button" class="btn btn-sm btn-outline-danger quitar">Quitar</button></td>
      </tr>
    `);
        $('#lineas tbody').append($tr);

        // agrega reglas de validación a los inputs recién creados
        ensureValidator();
        addRules($tr);

        calcular();
    }

    function construirPayload() {
        const payload = [];
        $('#lineas tbody tr').each(function () {
            const $tr = $(this);
            const productId = Number($tr.find('td:first').data('id'));
            const cantidad = Number($tr.find('td:eq(1) input').val()) || 0;
            const descuento = Number($tr.find('td:eq(2) input').val()) || 0;
            if (productId && cantidad > 0 && descuento >= 0) {
                payload.push({ productId, cantidad, descuento });
            }
        });
        return payload;
    }

    // ---------- cálculo (AJAX) ----------
    function calcular() {
        const validator = ensureValidator();
        // valida todos los inputs visibles (cantidades y descuentos)
        const ok = $('#lineas tbody input').toArray().every(function (el) {
            return validator.element(el) !== false;
        });
        if (!ok) { $('#subtotal,#impuestos,#total').text('0.00'); return; }

        const payload = construirPayload();
        if (!payload.length) { $('#subtotal,#impuestos,#total').text('0.00'); return; }

        $.ajax({
            url: '/api/pedidos/calcular',
            method: 'POST',
            data: JSON.stringify(payload),
            contentType: 'application/json; charset=utf-8',
            headers: antiForgeryHeader()
        })
            .done(function (t) {
                $('#subtotal').text(Number(t.subtotal).toFixed(2));
                $('#impuestos').text(Number(t.impuestos).toFixed(2));
                $('#total').text(Number(t.total).toFixed(2));
            })
            .fail(function (xhr) { console.warn('Error cálculo', xhr.responseText || xhr.statusText); });
    }

    // ---------- eventos ----------
    $(function () {
        if (!pageActive()) return;

        // buscar con debounce
        $('#txtBuscar').on('input', debounce(function () {
            const q = $(this).val().trim();
            $.getJSON('/api/productos/buscar', { q: q })
                .done(renderResultados)
                .fail(function (xhr) { console.warn('Error búsqueda', xhr.statusText); });
        }, 300));

        // elegir resultado
        $('#resultados').on('click', 'li', function () {
            const p = $(this).data('producto'); if (p) agregarLinea(p);
        });

        // cambios en qty/desc recalculan
        $('#lineas').on('input', 'input', debounce(calcular, 250));

        // quitar línea
        $('#lineas').on('click', '.quitar', function () {
            $(this).closest('tr').remove();
            calcular();
        });

        // inicializa validador del formulario
        ensureValidator();
    });

})(jQuery);

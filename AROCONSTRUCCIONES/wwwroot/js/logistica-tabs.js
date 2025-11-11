/*
 * ==========================================================
 * CEREBRO DE PESTAÑAS (TABS) Y TABLAS (DATATABLES)
 * ==========================================================
 * Este script maneja:
 * 1. La carga de contenido (partials) en las pestañas.
 * 2. La inicialización de todas las DataTables.
 * 3. La lógica de filtros para cada tabla.
 * ==========================================================
 */

/**
 * Carga el contenido de una pestaña usando AJAX.
 * Esta función es llamada por los botones x-on:click en el HTML.
 */
function loadTabContent(url, targetSelector) {
    const $targetContainer = $(targetSelector);

    if (url && $targetContainer.length) {

        // Muestra un spinner (con clases de Tailwind)
        $targetContainer.html(`
            <div class="flex justify-center items-center p-10">
                <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
                <span class="ml-4 text-gray-600">Cargando...</span>
            </div>`);

        // Petición AJAX (jQuery)
        $.get(url, function (data) {
            $targetContainer.html(data);

            // ¡IMPORTANTE!
            // Inicializa las DataTables y los filtros DESPUÉS de que el HTML haya cargado
            initializeDataTables();

        }).fail(function () {
            $targetContainer.html('<div class="rounded-md bg-red-50 p-4"><h3 class="text-sm font-medium text-red-800">Error al cargar contenido</h3></div>');
        });
    }
}

/**
 * Inicializa todas las DataTables del módulo de logística.
 */
function initializeDataTables() {

    /**
     * Función auxiliar para inicializar una tabla de forma segura.
     * Comprueba si la librería DataTable está cargada antes de usarla.
     */
    function initTable(selector, options = {}) {
        const $table = $(selector);

        // --- ¡LA CORRECCIÓN ESTÁ AQUÍ! ---
        // Comprueba si la tabla existe Y si la función DataTable está disponible
        if ($table.length && $.fn.DataTable) {
            $table.DataTable({
                destroy: true, // Permite reinicializar la tabla sin errores
                responsive: true,
                language: { url: "//cdn.datatables.net/plug-ins/1.10.25/i18n/Spanish.json" },
                ...options // Combina con opciones específicas (como 'order', 'dom', 'buttons')
            });
        }
    }

    // --- 1. Tabla de Inventario ---
    initTable('#dataTableInventario');
    const tableInventario = $('#dataTableInventario').DataTable();
    if (tableInventario) { // Chequea si la tabla existe antes de asignarle filtros
        $('#inventarioSearchInput').on('keyup', function () { tableInventario.search(this.value).draw(); });
        $('#inventarioStatusFilter').on('change', function () { tableInventario.column(7).search(this.value ? '^' + this.value + '$' : '', true, false).draw(); });
        $('#inventarioCategoryFilter').on('change', function () { tableInventario.column(2).search(this.value ? '^' + this.value + '$' : '', true, false).draw(); });
    }

    // --- 2. Tabla de Movimientos ---
    initTable('#dataTableMovimientos', { order: [[0, 'desc']] });
    const tableMovimientos = $('#dataTableMovimientos').DataTable();
    if (tableMovimientos) { // Chequea si la tabla existe
        // Asigna los eventos de filtro
        $('#inputBuscarMovimiento').on('keyup', function () { tableMovimientos.search(this.value).draw(); });
        $('#filtroTipoMovimiento').on('change', function () { tableMovimientos.column(1).search(this.value ? '^' + this.value + '.*' : '', true, false).draw(); });
        $('#filtroAlmacen').on('change', function () { tableMovimientos.column(4).search(this.value ? '^' + this.value + '$' : '', true, false).draw(); });
    }

    // --- 3. Tabla de Catálogo de Materiales ---
    initTable('#dataTableMateriales');

    // --- 4. Tabla de Órdenes de Compra ---
    initTable('#dataTableOrdenesCompra', { order: [[2, 'desc']] });

    // --- 5. Tabla de Proveedores ---
    initTable('#dataTableProveedores');

    // --- 6. Tabla de Almacenes ---
    initTable('#dataTableAlmacenes');

    // --- 7. Tabla de Reporte Kárdex ---
    initTable('#tablaKardexReporte', {
        order: [[0, 'desc']], // Ordenar por fecha
        dom: 'Bfrtip', // Habilita los botones de exportación
        buttons: [
            'copy', 'csv', 'excel', 'pdf', 'print'
        ]
    });
}
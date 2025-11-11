// --- MANEJADOR GLOBAL DE MODALES (TAILWIND + ALPINE + JQUERY-AJAX) ---
$(document).ready(function () {

    const modalPlaceholder = $('#modal-placeholder');

    // 1. ABRIR MODAL
    //    Cualquier botón/enlace con 'data-modal-url' abrirá un modal.
    //    Usamos 'on' para que funcione en contenido cargado por AJAX (como las pestañas)
    $(document).on('click', '[data-modal-url]', function () {
        const url = $(this).data('modal-url');

        // Petición GET para traer el HTML del formulario
        $.get(url, function (data) {
            modalPlaceholder.html(data);
            const $form = modalPlaceholder.find('form');
            if ($form.length && $.validator && $.validator.unobtrusive) {
                $.validator.unobtrusive.parse($form);
            }
        }).fail(function () {
            Swal.fire("Error", "Error al cargar el formulario.", "error");
        });
    });

    // 2. ENVIAR FORMULARIO DEL MODAL
    //    Cualquier formulario con 'data-form-ajax="true"' se enviará por AJAX.
    $(document).on('submit', 'form[data-form-ajax="true"]', function (e) {
        e.preventDefault(); // Evita que la página se recargue

        const form = $(this);
        if (!form.valid()) {
            return false;
        }
        const url = form.attr('action');
        const data = form.serialize();
        const $submitButton = form.find('button[type="submit"]');
        $submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin mr-2"></i> Guardando...');

        $.post(url, data, function (response) {
            if (response.success) {
                // ¡Éxito! Cierra el modal
                modalPlaceholder.html('');
                Swal.fire({ icon: 'success', title: '¡Guardado!', text: response.message, timer: 1500, showConfirmButton: false });
                // Recarga la pestaña activa
                if (typeof loadTabContent === 'function') {
                    // Busca el botón de la pestaña activa (la que tiene el borde azul)
                    const $activeTabButton = $('button[class*="border-blue-500"]');
                    if ($activeTabButton.length) {
                        loadTabContent($activeTabButton.data('tab-url'), '#tab-content-container');
                    } else {
                        location.reload(); // Fallback
                    }
                } else {
                    location.reload(); // Fallback
                }

            } else {
                // Validación fallida o error de negocio
                // El 'response' es el HTML del formulario con los mensajes de error
                modalPlaceholder.html(response);

                // Volvemos a activar la validación en el formulario que se recargó
                const $formRecargado = modalPlaceholder.find('form');
                if ($formRecargado.length && $.validator && $.validator.unobtrusive) {
                    $.validator.unobtrusive.parse($formRecargado);
                }
            }
        }).fail(function () {
            Swal.fire({ icon: 'error', title: 'Error de Red', text: 'No se pudo conectar con el servidor.' });
            $submitButton.prop('disabled', false).html('Guardar'); // (Restaura el texto original)
        });
    });
});

// Este script debe cargarse DESPUÉS de Alpine.js
document.addEventListener('alpine:init', () => {
    Alpine.data('movimientoModal', (listaMateriales) => ({
        open: true,
        activeTab: 'ingreso',
        listaMateriales: listaMateriales || [], // Lista completa de materiales

        // Datos del encabezado
        maestro: {
            proveedorId: '',
            almacenId: '',
            fechaEmision: new Date().toISOString().split('T')[0], // Hoy por defecto
            nroFactura: '',
            nroGuia: ''
        },

        // Fila para añadir nuevo item
        newItem: {
            materialId: '',
            cantidad: 1,
            costoUnitario: 0
        },

        // La "tabla" de items añadidos
        detalle: [],

        // Calcula el costo total
        get totalCosto() {
            return this.detalle.reduce((total, item) => {
                return total + (item.cantidad * item.costoUnitario);
            }, 0);
        },

        // Formatea un número como moneda
        formatoMoneda(valor) {
            return new Intl.NumberFormat('es-PE', { style: 'currency', currency: 'PEN' }).format(valor);
        },

        // Añade un material a la tabla
        addMaterial() {
            if (!this.newItem.materialId || this.newItem.cantidad <= 0 || this.newItem.costoUnitario < 0) {
                Swal.fire('Campos incompletos', 'Seleccione un material, cantidad y costo válidos.', 'warning');
                return;
            }

            // Busca el nombre del material en la lista completa
            const materialSeleccionado = this.listaMateriales.find(m => m.Value == this.newItem.materialId);

            this.detalle.push({
                materialId: parseInt(this.newItem.materialId),
                materialNombre: materialSeleccionado ? materialSeleccionado.Text : 'Material no encontrado',
                cantidad: this.newItem.cantidad,
                costoUnitario: this.newItem.costoUnitario
            });

            // Resetea los campos
            this.newItem.materialId = '';
            this.newItem.cantidad = 1;
            this.newItem.costoUnitario = 0;
        },

        // Quita un material de la tabla
        removeMaterial(index) {
            this.detalle.splice(index, 1);
        },

        // Envía el formulario
        submitMovimiento() {
            if (this.activeTab === 'ingreso') {
                this.submitIngreso();
            } else {
                // ... (lógica para submitSalida) ...
            }
        },

        submitIngreso() {
            // Validaciones
            if (!this.maestro.proveedorId || !this.maestro.almacenId) {
                Swal.fire('Datos incompletos', 'Debe seleccionar un Proveedor y un Almacén.', 'warning');
                return;
            }
            if (this.detalle.length === 0) {
                Swal.fire('Sin materiales', 'Debe añadir al menos un material al ingreso.', 'warning');
                return;
            }

            // Prepara el DTO que se enviará al C#
            const ingresoDto = {
                Maestro: this.maestro,
                Detalles: this.detalle
            };

            // ¡¡IMPORTANTE!!
            // Esto es lo que tu controlador de C# debe recibir
            console.log("Enviando al backend:", ingresoDto);

            // Llamada AJAX para guardar
            $.ajax({
                url: '/MovimientoInventario/RegistrarIngreso', // ¡NECESITAS CREAR ESTA RUTA/ACCIÓN!
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(ingresoDto),
                headers: {
                    // (Necesitarás un AntiForgeryToken si lo requieres)
                },
                success: (response) => {
                    if (response.success) {
                        this.open = false; // Cierra el modal
                        Swal.fire('¡Guardado!', response.message, 'success');
                        // Recarga la pestaña activa
                        loadTabContent($('#movimientos-tab').data('url'), '#tab-content-container');
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                },
                error: () => {
                    Swal.fire('Error', 'No se pudo conectar con el servidor.', 'error');
                }
            });
        }
    }));
});
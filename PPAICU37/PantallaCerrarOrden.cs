using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PPAICU37
{
    public partial class PantallaCerrarOrden : Form
    {
        private ControladorCerrarOrden _controlador;
        private OrdenDeInspeccion _ordenTemporalmenteSeleccionadaEnGrilla; // Para guardar la selecci�n de la grilla antes de confirmar con el bot�n


        public PantallaCerrarOrden()
        {
            InitializeComponent();
            _controlador = new ControladorCerrarOrden();
            ConfigurarEstadoInicialUI();
            
        }

        private void ConfigurarEstadoInicialUI()
        {
            HabilitarSeccionSeleccionOrden(false); // dgvOrdenesInspeccion y btnSeleccionarOrden
            HabilitarSeccionObservacion(false);    // txtObservacionCierre y btnConfirmarObservacion
            HabilitarSeccionMotivos(false);       // cmbTiposMotivo, txtComentarioMotivo, btnAgregarMotivo, dgvMotivosFueraServicio

            btnConfirmar.Enabled = false;
            btnCancelar.Enabled = false; // Se habilita despu�s del login
            btnIniciarSesion.Enabled = true;
        }

        private void HabilitarSeccionSeleccionOrden(bool habilitar)
        {
            grillaOrdenes.Enabled = habilitar;
            // btnSeleccionarOrden se habilita solo cuando hay una fila seleccionada en la grilla
            if (!habilitar) btnSeleccionarOrden.Enabled = false;
        }

        private void HabilitarSeccionObservacion(bool habilitar)
        {
            txtObservacion.Enabled = habilitar;
            btnConfirmarObservacion.Enabled = habilitar;
            if (habilitar)
            {
                // solicitarIngresoObservacion() - Se le da foco al txt
                txtObservacion.Focus();
            }
        }

        private void HabilitarSeccionMotivos(bool habilitar)
        {
            cmbTiposMotivo.Enabled = habilitar;
            txtComentario.Enabled = habilitar;
            btnAgregarMotivo.Enabled = habilitar;
            grillaMotivos.Enabled = habilitar;
        }

        private void opcionCerrarOrden(object sender, EventArgs e)
        {
      //      MessageBox.Show($"Login simulado exitoso para: {_controlador.ResponsableLogueado.NombreUsuario}", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
            HabilitarSeccionSeleccionOrden(true);
            DataTable tablaFiltrada = _controlador.tomarOpcionSeleccionada("CERRAR_ORDEN_INSPECCION");
            MessageBox.Show($"Login simulado exitoso para: {_controlador.ResponsableLogueado.NombreUsuario}", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnCancelar.Enabled = true;
            mostrarOrdenesConAsociados(tablaFiltrada);
            btnIniciarSesion.Enabled = false;

        }

        //private void mostrarOrdenes(List<OrdenDeInspeccion> OrdenesFiltradas)
        //{
        //    grillaOrdenes.DataSource = null;
        //    _ordenTemporalmenteSeleccionadaEnGrilla = null; // Resetear la selecci�n temporal
        //    btnSeleccionarOrden.Enabled = false; // Deshabilitar hasta nueva selecci�n en grilla

        //    if (OrdenesFiltradas != null)
        //    {
        //        // NUMERO ORDEN, FECHA FINALIZACION, ESTACION SISMOLOGICA, ID SISMO.

        //        grillaOrdenes.DataSource = OrdenesFiltradas;
        //    }
        //    else
        //    {
        //        MessageBox.Show("No hay �rdenes de inspecci�n completamente realizadas para mostrar.", "Informaci�n", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    // Deshabilitar las siguientes secciones hasta que se seleccione una orden expl�citamente
        //    HabilitarSeccionObservacion(false);
        //    HabilitarSeccionMotivos(false);
        //    btnConfirmar.Enabled = false;
        //    btnCancelar.Enabled = true; // Habilitar el bot�n cancelar despu�s del login
        //    btnIniciarSesion.Enabled = false;
        //}

        private void mostrarOrdenesConAsociados(DataTable dt)
        {
            // Asigno al DataGridView
            grillaOrdenes.DataSource = dt;

            // (Opcional) afinaciones de UI
            grillaOrdenes.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            grillaOrdenes.ReadOnly = true;
        }

        private void cargarTiposMotivo()
        {
            cmbTiposMotivo.DataSource = null;
            cmbTiposMotivo.DataSource = _controlador.buscarTiposMotivos();
            cmbTiposMotivo.DisplayMember = "Descripcion";
        }

        // Evento cuando cambia la selecci�n en la grilla de �rdenes
        private void grillaOrdenes_SelectionChanged(object sender, EventArgs e)
        {
            // 1) Si no hay fila seleccionada, deshabilito y salgo
            if (grillaOrdenes.CurrentRow == null)
            {
                _ordenTemporalmenteSeleccionadaEnGrilla = null;
                btnSeleccionarOrden.Enabled = false;
                return;
            }

            // 2) Casteo a DataRowView para extraer el campo "N�mero Orden"
            var drv = grillaOrdenes.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null
             || drv["N�mero Orden"] == DBNull.Value
             || !int.TryParse(drv["N�mero Orden"].ToString(), out int nro))
            {
                _ordenTemporalmenteSeleccionadaEnGrilla = null;
                btnSeleccionarOrden.Enabled = false;
                return;
            }

            // 3) Busco la entidad completa por ese n�mero (opcional)
            _ordenTemporalmenteSeleccionadaEnGrilla =
                _controlador.Ordenes
                    .FirstOrDefault(o => o.NumeroOrden == nro);

            // 4) Habilito el bot�n si encontr� algo
            btnSeleccionarOrden.Enabled = _ordenTemporalmenteSeleccionadaEnGrilla != null;
        }

        // NUEVO: Evento para el bot�n "Seleccionar Orden"
        // Este m�todo ahora es el que realmente confirma la selecci�n de la orden en el controlador
        // y habilita la siguiente secci�n (observaciones).
        // Corresponde al m�todo `seleccionarOrden()` del diagrama de boundary [cite: 1]
        private void btnSeleccionarOrden_Click(object sender, EventArgs e)
        {
            if (_ordenTemporalmenteSeleccionadaEnGrilla != null)
            {
                _controlador.tomarOrdenSeleccionada(_ordenTemporalmenteSeleccionadaEnGrilla); // Informa al controlador

                // Limpiar campos para la nueva selecci�n
                txtObservacion.Text = string.Empty;
                txtComentario.Text = string.Empty;
                _controlador.listaMotivosTipoComentario.Clear();
                mostrarMotivosAgregados(_controlador.listaMotivosTipoComentario);

                HabilitarSeccionObservacion(true); // Habilita txtObservacionCierre y btnConfirmarObservacion
                HabilitarSeccionMotivos(false);    // La secci�n de motivos se habilita DESPU�S de confirmar observaci�n
                btnConfirmar.Enabled = false;    // El bot�n final de cierre se habilita despu�s de confirmar motivos

                grillaOrdenes.Enabled = false; // Opcional: deshabilitar la grilla para evitar cambios
                btnSeleccionarOrden.Enabled = false;  // Deshabilitar este bot�n una vez usado
            }
            else
            {
                MessageBox.Show("No hay una orden v�lida seleccionada en la grilla.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // NUEVO: Evento para el bot�n "Confirmar Observaci�n"
        // Corresponde a los m�todos `ingresarObservacion()` y `solicitarConfirmacion()` (parcial) del diagrama [cite: 1]
        private void btnConfirmarObservacion_Click(object sender, EventArgs e)
        {
            string observacion = txtObservacion.Text;
            if (string.IsNullOrWhiteSpace(observacion)) // Validaci�n b�sica
            {
                MessageBox.Show("La observaci�n de cierre no puede estar vac�a.", "Dato Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // `solicitarIngresoObservacion()` - Mantener foco
                txtObservacionCierre.Focus();
                return;
            }

            _controlador.tomarObservacionIngresada(observacion); // `ingresarObservacion()`
            MessageBox.Show("Observaci�n de cierre registrada.", "Confirmado", MessageBoxButtons.OK, MessageBoxIcon.Information);

            txtObservacion.Enabled = false;         // Deshabilitar despu�s de confirmar
            btnConfirmarObservacion.Enabled = false;    // Deshabilitar despu�s de confirmar

            cargarTiposMotivo();

            HabilitarSeccionMotivos(true); // Ahora habilitar la secci�n de motivos
                                           // btnCerrarOrden todav�a no, hasta que se agreguen motivos.
                                           // El m�todo `solicitarConfirmacion()` del diagrama para el cierre final est� en `btnCerrarOrden_Click`
        }

        // Evento cuando cambia el tipo de motivo seleccionado
        private void btnAgregarMotivo_Click(object sender, EventArgs e)
        {
            string comentario = txtComentario.Text;
            if (!(cmbTiposMotivo.SelectedItem is MotivoTipo motivoTipoSeleccionado))
            {
                MessageBox.Show(
                    "Debe seleccionar un tipo de motivo.",
                    "Dato incompleto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (string.IsNullOrWhiteSpace(comentario))
            {
                MessageBox.Show(
                    "Debe escribir un comentario para el motivo.",
                    "Dato incompleto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                txtComentario.Focus();
                return;
            }
            _controlador.tomarMotivoSeleccionado(motivoTipoSeleccionado);
            _controlador.tomarComentarioIngresado(txtComentario.Text);
            List<Tuple<string, MotivoTipo>> lista = _controlador.agregarMotivoALista();

            mostrarMotivosAgregados(lista);

            txtComentario.Clear();
            cmbTiposMotivo.Focus();

            if (_controlador.listaMotivosTipoComentario.Any())
            {
                btnConfirmar.Enabled = true;
            }
        }

        private void seleccionarMotivo(object sender, EventArgs e)
        {
            btnAgregarMotivo.Enabled = cmbTiposMotivo.SelectedItem is MotivoTipo;
        }

        private void mostrarMotivosAgregados(List<Tuple<string, MotivoTipo>> lista)
        {
            grillaMotivos.DataSource = null;
            grillaMotivos.DataSource =
              lista
                .Select(t => new
                {
                    Comentario = t.Item1,
                    Tipo = t.Item2.Descripcion
                })
                .ToList();
        }

        // Evento para el bot�n "Cerrar Orden"
        private void btnCerrarOrden_Click(object sender, EventArgs e)
        {
            if (_controlador.OrdenSeleccionada == null)
            {
                MessageBox.Show("Primero debe seleccionar y confirmar una orden.", "Acci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // La observaci�n ya fue tomada y validada (b�sicamente) por btnConfirmarObservacion_Click
            // pero el controlador tiene su propia validaci�n m�s robusta.
            if (!_controlador.validarObservacion()) // Esta validaci�n usa la observaci�n guardada en el controlador
            {
                MessageBox.Show("La observaci�n de cierre no fue registrada o es inv�lida.", "Error de validaci�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Reactivar secci�n observaci�n si fuera necesario o guiar al usuario.
                // Por ahora, es un estado an�malo si se llega aqu� con observaci�n inv�lida.
                return;
            }
            if (!_controlador.validarMotivoSeleccionado())
            {
                MessageBox.Show("Debe agregar al menos un motivo de fuera de servicio.", "Error de validaci�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HabilitarSeccionMotivos(true); // Re-habilitar para que agregue motivos
                cmbTiposMotivo.Focus();
                return;
            }

            // `solicitarConfirmacion()` del diagrama para el cierre final
            var confirmResult = MessageBox.Show("�Confirma el cierre final de esta orden de inspecci�n?",
                                             "Confirmar Cierre Final", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes) return;

            bool exito = _controlador.tomarConfirmacionRegistrada();
            if (!exito)
            {
                MessageBox.Show(
                    "Error al cerrar la orden. Verifique los datos y el estado del sistema.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            MessageBox.Show(
                 "Orden cerrada. Sism�grafo puesto fuera de servicio.",
                 "Operaci�n Exitosa",
                 MessageBoxButtons.OK,
                 MessageBoxIcon.Information
            );

            _controlador.finCU();
            ConfigurarEstadoInicialUI(); // Volver al estado inicial para una nueva operaci�n

            List<OrdenDeInspeccion> Ordenes = _controlador.Ordenes; // Obtener las �rdenes filtradas del controlador
            DataTable tablaFiltrada = _controlador.buscarOrdenInspeccion(Ordenes);
            mostrarOrdenesConAsociados(tablaFiltrada);

            HabilitarSeccionSeleccionOrden(true); // Permitir seleccionar otra orden
            grillaOrdenes.Enabled = true;
            txtObservacion.Clear();
            mostrarMotivosAgregados(_controlador.listaMotivosTipoComentario); // Limpiar la grilla de motivos

        }

        // Evento para el bot�n "Cancelar"
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            // Reiniciar el estado del CU en el controlador y la UI
            _controlador.finCU(); // Llama a finCU para limpiar el estado del controlador

            // Resetear la UI a un estado similar al inicial despu�s del login
            _ordenTemporalmenteSeleccionadaEnGrilla = null;
            txtObservacion.Clear();
            if (cmbTiposMotivo.Items.Count > 0) cmbTiposMotivo.SelectedIndex = -1;
            txtComentario.Clear();
            // MotivosAgregados se limpian en finCU del controlador, aqu� actualizamos la grilla
            mostrarMotivosAgregados(_controlador.listaMotivosTipoComentario);

            HabilitarSeccionSeleccionOrden(true); // Permitir volver a seleccionar orden
            grillaOrdenes.Enabled = true;  // Asegurarse que la grilla est� activa
            if (grillaOrdenes.Rows.Count > 0) grillaOrdenes.ClearSelection();

            HabilitarSeccionObservacion(false);
            HabilitarSeccionMotivos(false);

            btnConfirmar.Enabled = false;

            //List<OrdenDeInspeccion> OrdenesFiltradas = _controlador.buscarOrdenInspeccion();
            //mostrarOrdenes(OrdenesFiltradas); // Recargar las �rdenes disponibles

            List<OrdenDeInspeccion> Ordenes = _controlador.Ordenes; // Obtener las �rdenes filtradas del controlador
            DataTable tablaFiltrada = _controlador.buscarOrdenInspeccion(Ordenes);
            mostrarOrdenesConAsociados(tablaFiltrada);

            MessageBox.Show("Operaci�n cancelada. Puede seleccionar una nueva orden.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

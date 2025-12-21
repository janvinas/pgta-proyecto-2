using Accord.IO;
using AsterixParser;
using AsterixParser.Utils;
using AsterixViewer.Projecte3;
using ExcelDataReader;
using ExcelDataReader.Log;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Management.Deployment;
using static AsterixViewer.Projecte3.DatosVirajes;
using static AsterixViewer.Projecte3.DistanciasSonometro;
using static AsterixViewer.Projecte3.LecturaArchivos;
using static AsterixViewer.Projecte3.PerdidasSeparacion;
using static AsterixViewer.Projecte3.VelocidadesDespegue;
using static AsterixViewer.Projecte3.CalculosEstereograficos;
using static AsterixViewer.Tabs.Proyecto3;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.ObjectModel;
using Windows.UI.WebUI;

namespace AsterixViewer.Tabs
{
    /// <summary>
    /// Lógica de interacción para UserControl1.xaml
    /// </summary>
    public partial class Proyecto3 : UserControl, INotifyPropertyChanged
    {
        // -------------------------------- DEFINICIÓN DE VARIABLES GLOBALES ----------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------- 

        public bool calculosPreliminaresHechos = false;

        // Listado de msg Asterix en formato filas de variables
        List<List<string>> datosAsterix = new List<List<string>>();

        // Listado de planes de Vuelo
        List<List<string>> listaPV = new List<List<string>>();

        // Datos sobre clasificacion de las aeronaves (Input P3)
        ClasificacionAeronavesLoA clasificacionAeronavesLoA = new ClasificacionAeronavesLoA();

        // Definicion de distintos puntos fijos
        CoordinatesUVH THR_06R = new CoordinatesUVH();
        CoordinatesUVH THR_24L = new CoordinatesUVH();
        CoordinatesUVH sonometro = new CoordinatesUVH();
        CoordinatesUVH DVOR_BCN = new CoordinatesUVH();

        // Clase Vuelo, cada despegue es una instancia, contiene el codigo del avion y la lista de mensajes Asterix que le corresponden
        public class Vuelo
        {
            public string identificadorDeparture = "";
            public string codigoVuelo = "";
            public string horaPV = "";
            public string estela = "";
            public string pistadesp = "";
            public string tipo_aeronave = "";
            public string sid = "";
            public string motorizacion = "";
            public string ATOT = "";
            public string timeDEP_05NM = "";
            public List<List<string>> mensajesVuelo = new List<List<string>>();
            public List<List<string>> mensajesVueloInterpolados = new List<List<string>>();
        }

        // Lista de todos los vuelos ya ordenados y filtrados
        List<Vuelo> vuelosOrdenados = new List<Vuelo>();                

        // Lista de conjuntos de distancias de despegues consecutivos
        List<ConjuntoDespeguesConsecutivos> listaConjuntosDistanciasDespeguesConsecutivos = new List<ConjuntoDespeguesConsecutivos>();

        // Lista de datos de distancias minimas de vuelos respecto sonometro
        List<DistanciaMinimaSonometro> listaDistanciasMinimasSonometro = new List<DistanciaMinimaSonometro>();

        // Lista de datos de velocidades en despegue a distintas altitudes
        List<IASaltitudes> listaVelocidadesIASDespegue = new List<IASaltitudes>();

        // Lista de datos de los virajes de los vuelos y sus radiales respecto al DVOR
        List<DatosViraje> listaVirajes = new List<DatosViraje>();

        //
        List<THRAltitudVelocidad> listaTHRAltitudVelocidad = new List<THRAltitudVelocidad>();

        // Colección para mostrar periodos leídos en la UI
        public ObservableCollection<string> PeriodosAsterix { get; } = new ObservableCollection<string>();

        // Interno: estructura para agrupar intervalos y filenames
        private class PeriodoInfo
        {
            public TimeSpan Start;
            public TimeSpan End;
            public HashSet<string> FileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private readonly List<PeriodoInfo> _periodos = new List<PeriodoInfo>();

        private const double PeriodoItemHeight = 20.0;    // debe coincidir con Height en ItemContainerStyle
        private const double PeriodoVerticalPadding = 8.0;
        private const int PeriodoMaxVisible = 3;

        private double _periodosHeight = PeriodoItemHeight + PeriodoVerticalPadding;
        public double PeriodosHeight
        {
            get => _periodosHeight;
            set
            {
                if (Math.Abs(_periodosHeight - value) > 0.1)
                {
                    _periodosHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        // Controla si el botón de "Cálculos preliminares" está habilitado
        private bool _canRunPreliminares = true;
        public bool CanRunPreliminares
        {
            get => _canRunPreliminares;
            set
            {
                if (_canRunPreliminares != value)
                {
                    _canRunPreliminares = value;
                    OnPropertyChanged();
                }
            }
        }

        // Nueva propiedad que controla los botones de carga auxiliares (Concatenar, Planes, Clasificación)
        private bool _canLoadAuxFiles = false;
        public bool CanLoadAuxFiles
        {
            get => _canLoadAuxFiles;
            set
            {
                if (_canLoadAuxFiles != value)
                {
                    _canLoadAuxFiles = value;
                    OnPropertyChanged();
                }
            }
        }

        private void UpdateButtonStates()
        {
            // Botones de carga auxiliar: solo cuando hay datos ASTERIX y no se hayan hecho (o estén bloqueados) los preliminares
            CanLoadAuxFiles = DatosAsterixCargados && !PreliminaresHechos;

            // Desbloquear "Cálculos preliminares" siempre que los tres ficheros estén cargados.
            // Esto permite reactivar el botón si se vuelven a cargar Datos + PV + Clasificación.
            CanRunPreliminares = DatosAsterixCargados && DatosPVCargados && ClasificacionCargada;

            // Paso3 sigue dependiendo de PreliminaresHechos (ya definido en la propiedad Paso3Permitido)
        }

        public Proyecto3()
        {
            InitializeComponent();
            DataContext = this;

            // Estado inicial: TODO bloqueado excepto el botón "Cargar Datos ASTERIX"
            DatosAsterixCargados = false;
            DatosPVCargados = false;
            ClasificacionCargada = false;
            PreliminaresHechos = false;

            // Forzar actualización de botones (dejarlos bloqueados)
            UpdateButtonStates();

            // Inicializar altura según estado inicial (sin items)
            RefreshPeriodosCollection();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Nuevo: mensaje dinámico para el bloque de Paso 3
        public string Paso3Message
        {
            get
            {
                return PreliminaresHechos
                    ? "Cálculos preliminares hechos, seleccione el cálculo deseado para extraer sus métricas."
                    : "¡¡ Realice primero los cálculos preliminares !!";
            }
        }

        private bool _datosAsterixCargados;
        public bool DatosAsterixCargados
        {
            get => _datosAsterixCargados;
            set
            {
                if (_datosAsterixCargados == value) return;
                _datosAsterixCargados = value;
                OnPropertyChanged(); // Notifica a DatosAsterixCargados
                OnPropertyChanged(nameof(EstadoAsterix)); // Notifica a EstadoAsterix
                OnPropertyChanged(nameof(Paso2Permitido));
                OnPropertyChanged(nameof(InfoPaso2Visibility));
                UpdateButtonStates();
                OnPropertyChanged(nameof(Paso3Message));
            }
        }

        private bool _datosPVCargados;
        public bool DatosPVCargados
        {
            get => _datosPVCargados;
            set
            {
                if (_datosPVCargados == value) return;
                _datosPVCargados = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstadoPV));
                OnPropertyChanged(nameof(Paso2Permitido));
                OnPropertyChanged(nameof(InfoPaso2Visibility));
                UpdateButtonStates();
                OnPropertyChanged(nameof(Paso3Message));
            }
        }

        private bool _clasificacionCargada;
        public bool ClasificacionCargada
        {
            get => _clasificacionCargada;
            set
            {
                if (_clasificacionCargada == value) return;
                _clasificacionCargada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstadoClasificacion));
                OnPropertyChanged(nameof(Paso2Permitido));
                OnPropertyChanged(nameof(InfoPaso2Visibility));
                UpdateButtonStates();
                OnPropertyChanged(nameof(Paso3Message));
            }
        }

        private bool _preliminaresHechos;
        public bool PreliminaresHechos
        {
            get => _preliminaresHechos;
            set
            {
                if (_preliminaresHechos == value) return;
                _preliminaresHechos = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Paso3Permitido));
                UpdateButtonStates();
                OnPropertyChanged(nameof(Paso3Message));
            }
        }

        public bool Paso2Permitido => DatosAsterixCargados && DatosPVCargados && ClasificacionCargada;
        public bool Paso3Permitido => PreliminaresHechos;

        public string EstadoAsterix => DatosAsterixCargados ? "✔ Cargado" : "Pendiente";
        public string EstadoPV => DatosPVCargados ? "✔ Cargado" : "Pendiente";
        public string EstadoClasificacion => ClasificacionCargada ? "✔ Cargada" : "Pendiente";

        public Visibility InfoPaso2Visibility => Paso2Permitido ? Visibility.Collapsed : Visibility.Visible;
        public Visibility InfoPaso3Visibility => Paso3Permitido ? Visibility.Collapsed : Visibility.Visible;

        private void DatosAsterix_Click(object sender, RoutedEventArgs e)
        {
            datosAsterix.Clear();
            _periodos.Clear();
            PeriodosAsterix.Clear();

            // Recalcular altura tras limpiar
            RefreshPeriodosCollection();

            try
            {
                LecturaArchivos lect = new LecturaArchivos();
                var resultado = lect.LeerCsvASTERIX();

                if (resultado.data == null)
                    return;  // ← USUARIO CANCELÓ

                // Asignar nuevos datos ASTERIX
                datosAsterix = resultado.data;

                // Marcamos que ASTERIX está cargado
                DatosAsterixCargados = true;

                // Añadir periodo leído con filename
                if (TryGetTimeRangeFromDatos(resultado.data, out TimeSpan tStart, out TimeSpan tEnd))
                    AddPeriodo(tStart, tEnd, System.IO.Path.GetFileName(resultado.filePath));

                // COMPORTAMIENTO REQUERIDO:
                // Cuando se carga un nuevo fichero ASTERIX queremos forzar que el usuario recargue
                // también Planes de Vuelo y Clasificación para poder ejecutar "Cálculos preliminares".
                // Por tanto reiniciamos esos flags y dejamos habilitados los botones de carga
                // (para que el usuario pueda volver a cargar PV y Clasificación).
                DatosPVCargados = false;
                ClasificacionCargada = false;

                // Asegurarnos de que los preliminares no están marcados
                calculosPreliminaresHechos = false;
                PreliminaresHechos = false;

                // Habilitar botones de carga auxiliar para que el usuario pueda recargar PV / Clasificación
                CanLoadAuxFiles = true;

                // Asegurar que no se puedan ejecutar preliminares hasta que PV y Clasificación se recarguen
                CanRunPreliminares = false;

                // Actualizar estados visuales de botones y textos
                UpdateButtonStates();
                OnPropertyChanged(nameof(InfoPaso2Visibility));
                OnPropertyChanged(nameof(InfoPaso3Visibility));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer datos ASTERIX:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ConcatenarDatosAsterix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LecturaArchivos lect = new LecturaArchivos();
                var resultado = lect.LeerCsvASTERIX();

                if (resultado.data == null)
                    return;  // ← USUARIO CANCELÓ

                // Obtener intervalo del archivo recién leído
                if (!TryGetTimeRangeFromDatos(resultado.data, out TimeSpan sNew, out TimeSpan eNew))
                {
                    MessageBox.Show("No se pudieron extraer las marcas temporales del fichero importado. Concatenación abortada.",
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Si ya hay periodos concatenados, comprobar si el nuevo periodo ya está completamente incluido
                if (_periodos.Count > 0)
                {
                    var contains = _periodos.Any(p => p.Start <= sNew && p.End >= eNew);
                    if (contains)
                    {
                        MessageBox.Show(
                            "El periodo seleccionado ya se encuentra concatenado. No se duplicará su contenido.",
                            "Periodo ya concatenado",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                        return;
                    }

                    var maxExistingEnd = _periodos.Max(p => p.End);
                    // Si el nuevo periodo termina antes de un periodo ya concatenado -> impedir (evita ordenar incorrecto)
                    if (eNew < maxExistingEnd)
                    {
                        MessageBox.Show(
                            "Operación cancelada.\n" +
                            "El periodo que intenta concatenar es anterior a otro ya concatenado.\n" +
                            "Reinicie la carga y concatene los periodos en orden.",
                            "Fallo en concatenación",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        return;
                    }
                }

                // registrar periodo del archivo recién leído (antes de concatenar)
                AddPeriodo(sNew, eNew, System.IO.Path.GetFileName(resultado.filePath));

                // Concatenar datos
                if (datosAsterix != null && datosAsterix.Count > 0)
                {
                    var concatenado = lect.ConcatenarDatosAsterix(datosAsterix, resultado.data);
                    datosAsterix = concatenado;
                }
                else
                {
                    datosAsterix = resultado.data;
                }

                DatosAsterixCargados = true;

                // Actualizar estados de botones tras concatenar
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al concatenar datos:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper: parsear tiempo ASTERIX formato esperado "HH:MM:SS:ms" (ms opcional)
        private bool TryParseAstTime(string s, out TimeSpan ts)
        {
            ts = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var parts = s.Trim().Split(':');
            if (parts.Length < 3) return false;

            if (!int.TryParse(parts[0], out int hh)) return false;
            if (!int.TryParse(parts[1], out int mm)) return false;
            if (!int.TryParse(parts[2], out int ss)) return false;
            int ms = 0;
            if (parts.Length >= 4) int.TryParse(parts[3], out ms);

            try
            {
                long totalMs = hh * 3600L * 1000L + mm * 60L * 1000L + ss * 1000L + ms;
                ts = TimeSpan.FromMilliseconds(totalMs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Nuevo: devuelve start/end si hay times válidos
        private bool TryGetTimeRangeFromDatos(List<List<string>> datos, out TimeSpan start, out TimeSpan end)
        {
            start = default;
            end = default;

            if (datos == null || datos.Count == 0) return false;
            int colAST_time = 3;
            List<TimeSpan> times = new List<TimeSpan>();

            // Si hay cabecera intenta saltarla comprobando contenido de la primera fila
            int startIdx = 0;
            if (datos.Count > 0 && datos[0].Count > colAST_time && !TimeSpan.TryParse(datos[0][colAST_time], out _))
            {
                // probable cabecera -> empezar en 1
                startIdx = 1;
            }

            for (int i = startIdx; i < datos.Count; i++)
            {
                if (datos[i].Count <= colAST_time) continue;
                if (TryParseAstTime(datos[i][colAST_time], out TimeSpan ts)) times.Add(ts);
            }

            if (times.Count == 0) return false;

            start = times.Min();
            end = times.Max();
            return true;
        }

        // Añade un periodo y lo agrupa/mezcla con intervalos contiguos o solapados
        private void AddPeriodo(TimeSpan start, TimeSpan end, string filename)
        {
            if (end < start)
            {
                var tmp = start; start = end; end = tmp;
            }

            // Tolerancia para considerar "contiguo" (1 ms)
            var tol = TimeSpan.FromMilliseconds(1);

            // buscar intervalos que intersecten o sean contiguos
            var toMerge = _periodos.Where(p => !(p.End + tol < start || p.Start - tol > end)).ToList();

            if (toMerge.Count == 0)
            {
                var pi = new PeriodoInfo { Start = start, End = end };
                pi.FileNames.Add(filename);
                _periodos.Add(pi);
            }
            else
            {
                // fusionar con todos los encontrados
                var newStart = toMerge.Min(p => p.Start);
                var newEnd = toMerge.Max(p => p.End);
                newStart = newStart < start ? newStart : start;
                newEnd = newEnd > end ? newEnd : end;

                var mergedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var m in toMerge) foreach (var fn in m.FileNames) mergedNames.Add(fn);
                mergedNames.Add(filename);

                // eliminar antiguos
                foreach (var m in toMerge) _periodos.Remove(m);

                // añadir fusionado
                var merged = new PeriodoInfo { Start = newStart, End = newEnd, FileNames = mergedNames };
                _periodos.Add(merged);
            }

            // mantener orden cronológico
            _periodos.Sort((a, b) => a.Start.CompareTo(b.Start));

            RefreshPeriodosCollection();
        }

        private void RefreshPeriodosCollection()
        {
            PeriodosAsterix.Clear();
            foreach (var p in _periodos)
            {
                var files = string.Join(", ", p.FileNames);
                var display = $"{p.Start.ToString(@"hh\:mm\:ss\.fff")} → {p.End.ToString(@"hh\:mm\:ss\.fff")}   [{files}]";
                PeriodosAsterix.Add(display);
            }

            // Calcular altura basada en número de items visibles (ObservableCollection.Count refleja lo mostrado)
            int count = Math.Max(1, PeriodosAsterix.Count);
            int visible = Math.Min(count, PeriodoMaxVisible);
            PeriodosHeight = visible * PeriodoItemHeight + PeriodoVerticalPadding;
        }

        private void Planvuelo_click(object sender, RoutedEventArgs e)
        {
            listaPV.Clear();

            try
            {
                LecturaArchivos lect = new LecturaArchivos();
                var resultado = lect.LeerExcelPV();

                if (resultado == null)
                    return;  // ← CANCELADO

                listaPV = resultado;
                DatosPVCargados = true;

                // Actualizar estados después de cargar planes de vuelo
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el archivo:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ClasificacionAeronaves_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LecturaArchivos lect = new LecturaArchivos();
                var resultado = lect.LeerClasificacionAeronaves();

                if (resultado == null)
                    return;  // ← CANCELADO

                clasificacionAeronavesLoA = resultado;
                ClasificacionCargada = true;

                // Actualizar estados después de cargar clasificación
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el archivo:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CalculosPreliminares_Click(object sender, RoutedEventArgs e)
        {
            // NO manipular aquí IsEnabled del botón; el binding a CanRunPreliminares controla la UI.

            if (!calculosPreliminaresHechos)
            {
                try
                {
                    CalculosPreliminares calcPre = new CalculosPreliminares();

                    listaPV = calcPre.AcondicionarPV(listaPV);
                    datosAsterix = calcPre.FiltroDeparturesLEBL(datosAsterix,listaPV);

                    THR_24L = calcPre.DefinirPosicionEstereograficaPuntoFijo("41:17:31.99N 02:06:11.81E", 8 * 0.3048);
                    THR_06R = calcPre.DefinirPosicionEstereograficaPuntoFijo("41:16:56.32N 02:04:27.66E", 8 * 0.3048);
                    sonometro = calcPre.DefinirPosicionEstereograficaPuntoFijo("41:16:19.00N 02:02:52.00E", 8 * 0.3048);
                    DVOR_BCN = calcPre.DefinirPosicionEstereograficaPuntoFijo("41:18:25.60N 02:06:28.10E", 8 * 0.3048);

                    CalculosEstereograficos calcPos = new CalculosEstereograficos();
                    datosAsterix = calcPos.CalcularPosicionesEstereograficasMatriz(datosAsterix);

                    vuelosOrdenados = calcPre.ClasificarDistintosVuelos(datosAsterix,listaPV);

                    vuelosOrdenados = calcPre.CalcularTiempo05NMfromTHR(datosAsterix,vuelosOrdenados,THR_24L,THR_06R);

                    vuelosOrdenados = calcPre.AñadirMotorizacion(vuelosOrdenados,clasificacionAeronavesLoA);

                    // Marcar preliminares realizados
                    calculosPreliminaresHechos = true;
                    PreliminaresHechos = true;

                    // BLOQUEAR botones: no se deben poder recargar/concatenar ni volver a ejecutar preliminares
                    // (UpdateButtonStates aplicará las reglas)
                    CanLoadAuxFiles = false;

                    // Si quieres que, al ejecutar los preliminares, los textos vuelvan a "Pendiente",
                    // mantén los flags a false; pero para permitir reactivar el botón al recargar los 3 ficheros,
                    // NO forzamos permanentemente CanRunPreliminares aquí.
                    DatosAsterixCargados = false;
                    DatosPVCargados = false;
                    ClasificacionCargada = false;

                    // Forzar actualización de estados en la UI
                    UpdateButtonStates();

                    OnPropertyChanged(nameof(Paso3Permitido));
                    OnPropertyChanged(nameof(InfoPaso3Visibility));
                }
                catch
                {
                    MessageBox.Show(
                        $"Lectura de archivos previos incorrecta, vuelva a realizarla",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    ResetearEstadosLectura();
                }
            }
        }

        private void ResetearEstadosLectura()
        {
            DatosAsterixCargados = false;
            DatosPVCargados = false;
            ClasificacionCargada = false;

            calculosPreliminaresHechos = false;
            PreliminaresHechos = false;

            datosAsterix.Clear();
            listaPV.Clear();
            vuelosOrdenados.Clear();

            // Restaurar permisos de carga por estado inicial
            CanLoadAuxFiles = false;
            CanRunPreliminares = false;

            // Forzar actualización visual de botones
            UpdateButtonStates();

            OnPropertyChanged(nameof(Paso2Permitido));
            OnPropertyChanged(nameof(Paso3Permitido));
            OnPropertyChanged(nameof(InfoPaso2Visibility));
            OnPropertyChanged(nameof(InfoPaso3Visibility));
        }

        // Todas las llamadas necesarias para calcular las separciones entre despegues consecutivos
        private void CalculoSeparacionesDespegues_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaConjuntosDistanciasDespeguesConsecutivos.Clear();

                PerdidasSeparacion sep = new PerdidasSeparacion();
                listaConjuntosDistanciasDespeguesConsecutivos = sep.CalcularDistanciasDespeguesConsecutivos(vuelosOrdenados,datosAsterix,THR_24L,THR_06R,listaConjuntosDistanciasDespeguesConsecutivos);
                sep.GuardarDistDESPConsecutivos(vuelosOrdenados,clasificacionAeronavesLoA,listaConjuntosDistanciasDespeguesConsecutivos);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void DistanciasMinimasSonometro_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaDistanciasMinimasSonometro.Clear();

                DistanciasSonometro dist = new DistanciasSonometro();
                listaDistanciasMinimasSonometro = dist.CalcularDistanciaMinimaSonometro(datosAsterix,vuelosOrdenados,sonometro,listaDistanciasMinimasSonometro);
                dist.GuardarDistMinSonometro(listaDistanciasMinimasSonometro);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void VelocidadIASDespegue_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaVelocidadesIASDespegue.Clear();

                VelocidadesDespegue vel = new VelocidadesDespegue();
                listaVelocidadesIASDespegue = vel.CalcularVelocidadIASDespegue(vuelosOrdenados,listaVelocidadesIASDespegue);
                vel.GuardarVelocidadIASDespegue(listaVelocidadesIASDespegue);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void PosicionAltitudViraje_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaVirajes.Clear();
                
                DatosVirajes vir = new DatosVirajes();
                listaVirajes = vir.CalcularPosicionAltitudViraje(datosAsterix,vuelosOrdenados,listaVirajes,DVOR_BCN);
                vir.GuardarPosicionAltitudViraje(listaVirajes);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private void AltitudVelocidadTHR_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaTHRAltitudVelocidad.Clear();

                DatosTHR thr = new DatosTHR();
                listaTHRAltitudVelocidad = thr.CalcularAltitudVelocidadTHR(datosAsterix, vuelosOrdenados, listaTHRAltitudVelocidad);
                thr.GuardarAltitudVelocidadTHR(listaTHRAltitudVelocidad);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // -------------------------------- FUNCIONES PARA EXPORTAR DATOS EN FORMATO CSV ----------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------

        private void GuardarMSG_Asterix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Guardar CSV de mensajes ASTERIX Actualizados",
                    Filter = "Archivos CSV (*.csv)|*.csv",
                    FileName = "Asterix_Updated.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        foreach (var fila in datosAsterix)
                        {
                            writer.WriteLine(string.Join(";", fila));
                        }
                    }

                    // ✅ Confirmar al usuario
                    MessageBox.Show(
                        $"Archivo exportado correctamente:\n{filePath}",
                        "Exportación completada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el archivo:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void GuardarMSG_PV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Guardar CSV de Planes de Vuelo Actualizados",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "PV_Updated.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        foreach (var fila in listaPV)
                        {
                            writer.WriteLine(string.Join(";", fila));
                        }
                    }

                    // ✅ Confirmar al usuario
                    MessageBox.Show(
                        $"Archivo exportado correctamente:\n{filePath}",
                        "Exportación completada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el archivo:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}

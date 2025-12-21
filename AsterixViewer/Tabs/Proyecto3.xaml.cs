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
        ClasificacionAeronavesLoA clasificacionAeronavesLoA;

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

                // registrar periodo del archivo recién leído (antes de concatenar)
                if (TryGetTimeRangeFromDatos(resultado.data, out TimeSpan sNew, out TimeSpan eNew))
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
                    AcondicionarPV();
                    FiltroDeparturesLEBL();

                    DefinirPosicionesEstereograficasPuntosfijos();

                    CalculosEstereograficos calc = new CalculosEstereograficos();
                    datosAsterix = calc.CalcularPosicionesEstereograficasMatriz(datosAsterix);

                    ClasificarDistintosVuelos();
                    // OrdenarVuelos();

                    CalcularTiempo05NMfromTHR();

                    AñadirMotorizacion();

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

        // -------------------------------- FUNCIONES UTILIZADAS EN EL CODIGO ---------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------- 
        public void AcondicionarPV()
        {
            int pistadesp = -1;
            int procdesp = -1;
            int rutasacta = -1;
            for (int i = 0;i < listaPV[0].Count;i++) // Buscamos columnas clave
            {
                if (listaPV[0][i] == "PistaDesp") pistadesp = i;
                if (listaPV[0][i] == "ProcDesp") procdesp = i;
                if (listaPV[0][i] == "RutaSACTA") rutasacta = i;
            }

            for (int i = 1;i < listaPV.Count -1;i++) // Solo dejamos las de 24L y 06R
            {
                if (listaPV[i][pistadesp] != "LEBL-24L" && listaPV[i][pistadesp] != "LEBL-06R")
                {
                    listaPV.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 1; i < listaPV.Count -1; i++)
            {
                if (listaPV[i][procdesp] == "-")
                {
                    var puntos = listaPV[i][rutasacta].Split(' ');
                    bool encontrado1 = false;
                    bool encontrado2 = false;
                    int j = 0;
                    int m = 0;

                    // ------------ QUE HACER CON NITBA ------------
                    var puntosdeseados = new HashSet<string> { "OLOXO" , "NATPI" , "MOPAS" , "GRAUS" , "LOBAR" , "MAMUK" , "REBUL" , "VIBOK" , "DUQQI" , "DUNES" , "LARPA" , "LOTOS" , "SENIA" , "DALIN" , "AGENA" , "DIPES"};
                    
                    // Cuando el texto de la SID sale tal cual
                    while (!encontrado1)
                    {
                        if (puntosdeseados.Contains(puntos[puntos.Length - j - 1]))
                        {
                            if (listaPV[i][pistadesp] == "LEBL-24L")
                            {
                                listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1C");
                            }
                            else if (listaPV[i][pistadesp] == "LEBL-06R")
                            {
                                if (puntos[puntos.Length - j - 1] == "OLOXO") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                                else if (puntos[puntos.Length - j - 1] == "NATPI") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}2R");
                                else if (puntos[puntos.Length - j - 1] == "MOPAS") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}3R");
                                else if (puntos[puntos.Length - j - 1] == "GRAUS") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}3R");
                                else if (puntos[puntos.Length - j - 1] == "LOBAR") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "MAMUK") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}I1R");
                                else if (puntos[puntos.Length - j - 1] == "REBUL") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                                else if (puntos[puntos.Length - j - 1] == "VIBOK") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                                else if (puntos[puntos.Length - j - 1] == "DUQQI") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                                else if (puntos[puntos.Length - j - 1] == "DUNES") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "LARPA") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "LOTOS") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}3R");
                                else if (puntos[puntos.Length - j - 1] == "SENIA") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}5R");
                                else if (puntos[puntos.Length - j - 1] == "DALIN") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "AGENA") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "DIPES") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                            }

                            encontrado1 = true;
                            break;
                        }
                        j++;
                        if (j == puntos.Length)
                        {
                            encontrado1 = true;
                            break;
                        }
                    }

                    // Cuando la SID sale entre paréntesis
                    while (!encontrado2)
                    {
                        List<string> puntos2 = new List<string>();

                        int k = 0;
                        while (k < listaPV[i][rutasacta].Length)
                        {
                            List<char> elementos = new List<char>();
                            if (listaPV[i][rutasacta][k] == '(')
                            {
                                int l = 0;
                                while (listaPV[i][rutasacta][k + l] != ')')
                                {
                                    if (listaPV[i][rutasacta][k + l] != '(') elementos.Add(listaPV[i][rutasacta][k + l]);
                                    l++;
                                }
                                puntos2.Add(new string([.. elementos]));
                                elementos.Clear();
                            }
                            k++;
                        }
                        if (puntos2.Count == 0)
                        {
                            encontrado2 = true;
                            break;
                        }
                        if (puntosdeseados.Contains(puntos2[puntos2.Count - m - 1]))
                        {
                            if (listaPV[i][pistadesp] == "LEBL-24L")
                            {
                                listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1C");
                            }
                            else if (listaPV[i][pistadesp] == "LEBL-06R")
                            {
                                if (puntos2[puntos2.Count - m - 1] == "OLOXO") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                                else if (puntos2[puntos2.Count - m - 1] == "NATPI") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}2R");
                                else if (puntos2[puntos2.Count - m - 1] == "MOPAS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}3R");
                                else if (puntos2[puntos2.Count - m - 1] == "GRAUS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}3R");
                                else if (puntos2[puntos2.Count - m - 1] == "LOBAR") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "MAMUK") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}I1R");
                                else if (puntos2[puntos2.Count - m - 1] == "REBUL") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                                else if (puntos2[puntos2.Count - m - 1] == "VIBOK") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                                else if (puntos2[puntos2.Count - m - 1] == "DUQQI") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                                else if (puntos2[puntos2.Count - m - 1] == "DUNES") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "LARPA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "LOTOS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}3R");
                                else if (puntos2[puntos2.Count - m - 1] == "SENIA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}5R");
                                else if (puntos2[puntos2.Count - m - 1] == "DALIN") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "AGENA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "DIPES") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                            }

                            encontrado2 = true;
                            break;
                        }
                        m++;
                        if (m == puntos2.Count)
                        {
                            encontrado2 = true;
                            break;
                        }
                    }
                }
            }
            // MessageBox.Show("Planes de vuelo acondicionados");
        }

        public void FiltroDeparturesLEBL()
        {
            int ti = -1;
            int indicative = -1;
            int rmv = 0;

            for (int i = 0; i < datosAsterix[0].Count; i++)
            {
                if (datosAsterix[0][i] == "TI")
                {
                    ti = i;
                    break;
                }
            }

            for (int i = 0;i < listaPV[0].Count; i++)
            {
                if (listaPV[0][i] == "Indicativo")
                {
                    indicative = i;
                    break;
                }
            }

            for (int i = 0; i < datosAsterix.Count; i++)
            {
                bool pertenece = false;
                for (int j = 0; j < listaPV.Count; j++)
                {
                    if (datosAsterix[i][ti] == listaPV[j][indicative])
                    {
                        pertenece = true;
                        break;
                    }
                }
                if (!pertenece)
                {
                    datosAsterix.RemoveAt(i);
                    rmv++;
                    i--;
                }
            }

            // MessageBox.Show($"Se han eliminado {rmv} filas");
        }

        private void DefinirPosicionesEstereograficasPuntosfijos()
        {
            GeoUtils geo = new GeoUtils();
            CoordinatesWGS84 centro_tma = GeoUtils.LatLonStringBoth2Radians("41:06:56.5600N 01:41:33.0100E", 6368942.808);
            GeoUtils tma = new GeoUtils(Math.Sqrt(geo.E2), geo.A, centro_tma);
            double rt = 6356752.3142;

            // Calcular Estereograficas de THR_24L
            CoordinatesWGS84 coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:17:31.99N 02:06:11.81E", 8 * 0.3048 + rt);
            CoordinatesXYZ coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            CoordinatesXYZ coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            THR_24L = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            // Calcular Estereograficas de THR_06R
            coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:16:56.32N 02:04:27.66E", 8 * 0.3048 + rt);
            coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            THR_06R = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            // Calcular Estereograficas de Sonometro
            coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:16:19.00N 02:02:52.00E", 8 * 0.3048 + rt);
            coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            sonometro = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            // Calcular Estereograficas de DVOR BCN
            coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:18:25.60N 02:06:28.10E", 8 * 0.3048 + rt);
            coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            DVOR_BCN = tma.change_system_cartesian2stereographic(coords_system_cartesian);
        }

        // HAY QUE REDEFINIR ESTA FUNCION ENTERA
        /* IDEA:
         * 1- Iterar lista de PV par crear los nuevos "Aviones"
         * 2- Cada nuevo TI/TA/TrackNum -> nuevo Avion
         * 3- Bucle en datosAsterix desde 0 hasta encontrar ID -> comprobar si ATOT - Time < 30'
         * 4- Introducir todos los mensajes desde ese punto hasta que no haya durante 100 mensajes
         * 5- Introducir Avion en listaAviones
         */
        private void ClasificarDistintosVuelos()
        {
            int colPV_callsign = 1;
            int colPV_ATOT = 10;

            int colAST_time = 3;
            int colAST_callsign = 13;

            int ATOT_ms;
            int ASTtime_ms;

            int counter = 0;

            listaPV.RemoveAt(0);
            foreach (List<string> planVuelo in listaPV)
            {
                Vuelo vuelo = new Vuelo();
                vuelo.codigoVuelo = planVuelo[colPV_callsign];

                vuelo.identificadorDeparture = planVuelo[0];
                vuelo.horaPV = planVuelo[4];
                vuelo.tipo_aeronave = planVuelo[6];
                vuelo.estela = planVuelo[7];
                vuelo.sid = planVuelo[9];
                vuelo.ATOT = planVuelo[10];
                vuelo.pistadesp = planVuelo[11];

                for (int i = 0; i < datosAsterix.Count; i++)
                {
                    if (datosAsterix[i][colAST_callsign] == planVuelo[colPV_callsign])
                    {
                        ATOT_ms = (int)TimeSpan.Parse(planVuelo[colPV_ATOT]).TotalMilliseconds;
                        ASTtime_ms = int.Parse(datosAsterix[i][colAST_time].Split(':')[0]) * 3600 * 1000
                            + int.Parse(datosAsterix[i][colAST_time].Split(':')[1]) * 60 * 1000
                            + int.Parse(datosAsterix[i][colAST_time].Split(':')[2]) * 1000
                            + int.Parse(datosAsterix[i][colAST_time].Split(':')[3]);

                        if (Math.Abs(ATOT_ms - ASTtime_ms) < 0.5 * 3600 * 1000)
                        {
                            for (int j = i; j < datosAsterix.Count; j++)
                            {
                                if (datosAsterix[j][colAST_callsign] == planVuelo[colPV_callsign])
                                {
                                    vuelo.mensajesVuelo.Add(datosAsterix[j]);
                                    counter = 0;
                                }
                                else counter++;

                                if (counter >= 100) break;
                            }
                        }
                    }
                }

                // TAL VEZ ES MEJOR NO ELIMINAR LOS PV SI NO SALEN EN ASTERIX
                if (vuelo.mensajesVuelo.Count > 0) vuelosOrdenados.Add(vuelo);
            }
        }

        private double CalcularDistanciaEntrePuntos(Point punto1, Point punto2)
        {
            double dx = punto2.X - punto1.X;
            double dy = punto2.Y - punto1.Y;

            double distancia = Math.Sqrt(dx * dx + dy * dy);

            return distancia;
        }

        private void CalcularTiempo05NMfromTHR()
        {
            int TIMEcol = 3;
            int Xcol = datosAsterix[1].Count - 2;
            int Ycol = datosAsterix[1].Count - 1;

            Point posTHR_24L = new Point(THR_24L.U, THR_24L.V);
            Point posTHR_06R = new Point(THR_06R.U, THR_06R.V);
            Point posVuelo;
            Point posVuelo_siguienteMSG;

            double distanciaVueloTHR;
            double distanciaVueloTHR_MSGposterior;

            bool condicion05NMvuelo = false;

            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                condicion05NMvuelo = false;

                for (int j = 0; j < vuelo.mensajesVuelo.Count - 1; j++)
                {
                    posVuelo = new Point(Convert.ToDouble(vuelo.mensajesVuelo[j][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[j][Ycol]));
                    posVuelo_siguienteMSG = new Point(Convert.ToDouble(vuelo.mensajesVuelo[j+1][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[j+1][Ycol]));

                    if (vuelo.pistadesp == "LEBL-24L") {
                        distanciaVueloTHR = CalcularDistanciaEntrePuntos(posVuelo, posTHR_06R);
                        distanciaVueloTHR_MSGposterior = CalcularDistanciaEntrePuntos(posVuelo_siguienteMSG, posTHR_06R);

                        condicion05NMvuelo = (distanciaVueloTHR > 1852 / 2) && (distanciaVueloTHR < distanciaVueloTHR_MSGposterior);
                    }
                    else
                    {
                        distanciaVueloTHR = CalcularDistanciaEntrePuntos(posVuelo, posTHR_24L);
                        distanciaVueloTHR_MSGposterior = CalcularDistanciaEntrePuntos(posVuelo_siguienteMSG, posTHR_24L);

                        condicion05NMvuelo = (distanciaVueloTHR > 1852 / 2) && (distanciaVueloTHR < distanciaVueloTHR_MSGposterior);
                    }

                    if (condicion05NMvuelo)
                    {
                        vuelo.timeDEP_05NM = vuelo.mensajesVuelo[j][TIMEcol];
                        break;
                    }
                }
            }
        }


        private void AñadirMotorizacion()
        {
            foreach(Vuelo vuelo in vuelosOrdenados)
            {
                if (clasificacionAeronavesLoA.HP.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "HP";
                else if (clasificacionAeronavesLoA.NR.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "NR";
                else if (clasificacionAeronavesLoA.LP.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "LP";
                else if (clasificacionAeronavesLoA.NRminus.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "NR-";
                else if (clasificacionAeronavesLoA.NRplus.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "NR+";
                else vuelo.motorizacion = "R";
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

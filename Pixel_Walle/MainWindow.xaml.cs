using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Pixel_Walle
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            InitializeCanvas(10, 10); // Dimensiones iniciales del canvas
        }
        public void Execute(object sender, RoutedEventArgs e)
        {
            RunExecute(sender, e);
        }
        private void RunExecute(object sender, RoutedEventArgs e) // Método para ejecutar el código ingresado en el editor
        {
            if (CodeEditor.Text.Length > 0)
            {
                Lexer Tokenization = new Lexer(GetTextBoxLines());

                Parser parsing = new Parser(Tokenization.GetLexer());

                ProgramCompiler ast = parsing.Parsing();

                if (Utils.Errors.Count > 0) // Verifica si se encontraron errores sintácticos durante el parsing
                {
                    StringBuilder errorMessage = new StringBuilder("----Errores Sintácticos Detectados----\n");
                    foreach (var error in Utils.Errors)
                    {
                        errorMessage.AppendLine(error);
                    }
                    MessageBox.Show(errorMessage.ToString(), "Errores Sintácticos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Scope scope = new Scope();
                    if (!ast.CheckSemantic(scope))  // Verifica si se encontraron errores semánticos durante el chequeo semántico
                    {
                        StringBuilder errorMessage = new StringBuilder("----Errores Semánticos Detectados----\n");
                        foreach (var error in Utils.Errors)
                        {
                            errorMessage.AppendLine(error);
                        }
                        MessageBox.Show(errorMessage.ToString(), "Errores Semánticos", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        try
                        {
                            IVisitor visitor = new Visitor();
                            ast.Evaluate(visitor);
                        }
                        catch (Exception ex) // Captura cualquier excepción que ocurra durante la ejecución del código
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Escriba código en el editor antes de ejecutar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Utils.Reset();
        }
        private void InitializeCanvas(int rows, int columns)
        {
            // Limpiar el canvas existente
            CanvasGrid.RowDefinitions.Clear();
            CanvasGrid.ColumnDefinitions.Clear();
            CanvasGrid.Children.Clear();

            // Inicialización de la matriz de celdas
            Utils.cellMatrix = new Border[rows, columns];

            // Crear filas
            for (int i = 0; i < rows; i++)
                CanvasGrid.RowDefinitions.Add(new RowDefinition());

            // Crear columnas
            for (int j = 0; j < columns; j++)
                CanvasGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Crear celdas visibles
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var cell = new Border
                    {
                        Background = Brushes.White, // Color inicial de la celda
                        BorderBrush = Brushes.Gray, // Color del borde
                        BorderThickness = new Thickness(0.0) // Grosor del borde
                    };

                    // Ubicar la celda en la posición correspondiente
                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);

                    // Agregar la celda al canvas
                    CanvasGrid.Children.Add(cell);
                    Utils.cellMatrix[i, j] = cell; // Guardar la referencia de la celda en la matriz
                }
            }
        } // Método para inicializar el canvas con un número específico de filas y columnas
        private void ResizeCanvasButton_Click(object sender, RoutedEventArgs e)
        {
            // Obtener las dimensiones del canvas desde los TextBox
            if (int.TryParse(CanvasWidthInput.Text, out int width) && int.TryParse(CanvasHeightInput.Text, out int height))
                InitializeCanvas(height, width); // Redimensionar el canvas

            else
                MessageBox.Show("Por favor, introduce dimensiones válidas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        } // Método para redimensionar el canvas según las dimensiones ingresadas en los TextBox
        private string[] GetTextBoxLines()
        {
            // Obtener el texto completo del TextBox
            string fullText = CodeEditor.Text;

            // Dividir el texto en líneas y devolverlo como un array de strings
            string[] lines = fullText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            return lines;
        } // Método para obtener las líneas del TextBox como un array de strings
        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Crear un diálogo para guardar archivos
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivos GW (*.gw)|*.gw", // Filtro para archivos .gw
                DefaultExt = ".gw", // Extensión predeterminada
                Title = "Guardar Código"
            };

            // Mostrar el diálogo y verificar si el usuario seleccionó un archivo
            if (saveFileDialog.ShowDialog() == true)
            {
                // Guardar el contenido del TextBox en el archivo seleccionado
                System.IO.File.WriteAllText(saveFileDialog.FileName, CodeEditor.Text);
                MessageBox.Show("Código guardado.", "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        } //Guardar archivo como extension (.gw)
        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Crear un diálogo para abrir archivos
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos GW (*.gw)|*.gw", // Filtro para archivos .gw
                DefaultExt = ".gw", // Extensión predeterminada
                Title = "Cargar Código"
            };

            // Mostrar el diálogo y verificar si el usuario seleccionó un archivo
            if (openFileDialog.ShowDialog() == true)
            {
                // Leer el contenido del archivo seleccionado y cargarlo en el TextBox
                CodeEditor.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
            }
        } //Cargar archivo con extension (.gw)
        private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e) // Método para la actualizar los números de líneas
        {
            UpdateLineNumbers();
        }
        private void CodeEditorScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) // Sincronizar el scroll de los números de línea con el editor de texto
        {
            // Sincronizar el scroll de los números de línea con el editor de texto
            LineNumberScrollViewer.ScrollToVerticalOffset(CodeEditorScrollViewer.VerticalOffset);
        }
        private void UpdateLineNumbers() // Método para actualizar los números de línea en el TextBlock
        {
            // Obtener el número total de líneas en el editor de texto
            int totalLines = CodeEditor.LineCount;

            // Generar los números de línea
            StringBuilder lineNumbers = new StringBuilder();
            for (int i = 1; i <= totalLines; i++)
            {
                lineNumbers.AppendLine(i.ToString());
            }

            // Actualizar el TextBlock con los números de línea
            LineNumbers.Text = lineNumbers.ToString();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e) // Método para cerrar la aplicación
        {
            this.Close();
        }
    }
}
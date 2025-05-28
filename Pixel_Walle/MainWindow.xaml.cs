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

namespace Pixel_Walle
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeCanvas(10, 10); // Dimensiones iniciales del canvas
        }
        private void Execute(object sender, RoutedEventArgs e)
        {
            Lexer Tokenization = new Lexer(GetTextBoxLines());

            Parser parsing = new Parser(Tokenization.GetLexer());

            parsing.Parsing();
            if (Utils.Errors.Count == 0)
                MessageBox.Show("Parsing Complete");
            else
            {
                StringBuilder errorMessage = new StringBuilder("Errors found during parsing:\n");
                foreach (var error in Utils.Errors)
                {
                    errorMessage.AppendLine(error);
                }
                MessageBox.Show(errorMessage.ToString(), "Parsing Errors", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        private void InitializeCanvas(int rows, int columns)
        {
            // Limpiar el canvas existente
            CanvasGrid.RowDefinitions.Clear();
            CanvasGrid.ColumnDefinitions.Clear();
            CanvasGrid.Children.Clear();

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
                        BorderThickness = new Thickness(0.5) // Grosor del borde
                    };

                    // Ubicar la celda en la posición correspondiente
                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);

                    // Agregar la celda al canvas
                    CanvasGrid.Children.Add(cell);
                }
            }
        }

        private void ResizeCanvasButton_Click(object sender, RoutedEventArgs e)
        {
            // Obtener las dimensiones del canvas desde los TextBox
            if (int.TryParse(CanvasWidthInput.Text, out int width) && int.TryParse(CanvasHeightInput.Text, out int height))
                InitializeCanvas(height, width); // Redimensionar el canvas

            else
                MessageBox.Show("Por favor, introduce dimensiones válidas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }
        private string[] GetTextBoxLines()
        {
            // Obtener el texto completo del TextBox
            string fullText = CodeEditor.Text;

            // Dividir el texto en líneas y devolverlo como un array de strings
            string[] lines = fullText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            return lines;
        }

    }
}
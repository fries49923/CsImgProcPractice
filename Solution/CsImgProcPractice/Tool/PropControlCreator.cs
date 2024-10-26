using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace CsImgProcPractice
{
    public class PropControlCreator
    {
        private readonly object obj;

        public PropControlCreator(object obj)
        {
            this.obj = obj
                ?? throw new ArgumentNullException(nameof(obj));
        }

        public FrameworkElement GetPropControls()
        {
            var propInfos = GetParameter();

            if (propInfos.Length == 0)
            {
                //return "Empty" grid
                return CreateEmptyGird();
            }

            return CreateParaGrid(propInfos);
        }

        private PropertyInfo[] GetParameter()
        {
            Type type = obj.GetType();

            return type.GetProperties(
                BindingFlags.Public
                | BindingFlags.Instance);
        }

        private Grid CreateEmptyGird()
        {
            // Create Grid
            var grid = new Grid();
            var label = new TextBlock
            {
                Text = "This Algorithm don't need parameter.",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            grid.Children.Add(label);

            return grid;
        }

        private Grid CreateParaGrid(PropertyInfo[] propInfos)
        {
            // Create Grid
            var grid = new Grid();
            for (int i = 0; i < 3; i++)
            {
                grid.ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                        MinWidth = 30
                    });

                if (i == 1)
                {
                    grid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Auto);
                }
            }

            for (int i = 0; i < propInfos.Length; i++)
            {
                grid.RowDefinitions.Add(
                    new RowDefinition
                    {
                        Height = new GridLength(25, GridUnitType.Pixel)
                    });
            }

            // Create and add GridSplitter
            var splitter = new GridSplitter
            {
                Margin = new Thickness(2, 0, 2, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 2
            };

            Grid.SetColumn(splitter, 1);
            Grid.SetRowSpan(splitter, propInfos.Length);
            grid.Children.Add(splitter);

            // Create control for property and put in Grid
            for (int i = 0; i < propInfos.Length; i++)
            {
                var info = propInfos[i];
                var binding = new Binding(info.Name)
                {
                    Source = obj
                };

                FrameworkElement? element = null;
                if (info.PropertyType == typeof(int)
                    || info.PropertyType == typeof(double))
                {
                    element = new TextBox();
                    BindingOperations.SetBinding(element, TextBox.TextProperty, binding);
                }
                else if (info.PropertyType == typeof(bool))
                {
                    element = new CheckBox();
                    BindingOperations.SetBinding(element, CheckBox.IsCheckedProperty, binding);
                }
                else if (info.PropertyType.IsEnum)
                {
                    element = new ComboBox();

                    // ComboBox.ItemSource => Enum
                    Array enumAry = Enum.GetValues(info.PropertyType);
                    ((ComboBox)element).ItemsSource = enumAry;

                    BindingOperations.SetBinding(element, ComboBox.SelectedItemProperty, binding);
                }

                if (element is not null)
                {
                    FrameworkElement label =
                        new TextBlock
                        {
                            Text = $"{info.Name}:"
                        };

                    Grid.SetColumn(label, 0);
                    Grid.SetColumn(element, 2);
                    Grid.SetRow(label, i);
                    Grid.SetRow(element, i);
                    grid.Children.Add(label);
                    grid.Children.Add(element);
                }
            }

            return grid;
        }
    }
}

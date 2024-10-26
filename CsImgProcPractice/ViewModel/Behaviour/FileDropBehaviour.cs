using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace CsImgProcPractice
{
    public class FileDropBehaviour : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string),
                typeof(FileDropBehaviour), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(FileDropBehaviour), null);

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            // UIElement.PreviewDragOver
            AssociatedObject.PreviewDragOver += AssociatedObject_PreviewDragOver;
            AssociatedObject.PreviewDrop += AssociatedObject_PreviewDrop;
        }

        private void AssociatedObject_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void AssociatedObject_PreviewDrop(object sender, DragEventArgs e)
        {
            try
            {
                var files =
                    (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files is null
                    || files.Length == 0)
                {
                    return;
                }

                FilePath = files[0];

                if (Command is not null
                    && Command.CanExecute(files[0]))
                {
                    Command.Execute(files[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[FileDropBehaviour][PreviewDrop]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                //This is file being dropped not copied so handle it
                e.Handled = true;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewDragOver -= AssociatedObject_PreviewDragOver;
            AssociatedObject.PreviewDrop -= AssociatedObject_PreviewDrop;

            base.OnDetaching();
        }
    }
}

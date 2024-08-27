using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

namespace TransactionQL.DesktopApp.Tests
{
    public class UnitTest1
    {
        [AvaloniaFact]
        public void Test1()
        {
            // Setup controls:
            var textBox = new TextBox();
            var window = new Window { Content = textBox };

            // Open window:
            window.Show();

            // Focus text box:
            textBox.Focus();

            // Simulate text input:
            window.KeyTextInput("Hello World");

            // Assert:
            Assert.Equal("Hello World", textBox.Text);
        }
    }
}
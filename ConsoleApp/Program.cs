namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var reader = new DataReader();
            reader.ProcessImportAndPrintData("data.csv");
        }
    }
}

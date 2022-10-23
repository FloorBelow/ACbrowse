namespace ImGuiNET.SampleProgram.XNA
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var game = new AcBrowserGame()) game.Run();
        }
    }
}
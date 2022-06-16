namespace CC.CodeGenerator.Demo.BlazorServer.Data
{
    [Service]
    public class DemoService1
    {
        public string Name { get; set; } = nameof(DemoService1);
    }

    [Service]
    public class DemoService2
    {
        public string Name { get; set; } = nameof(DemoService2);
    }

    [Service]
    public partial class DemoService3
    {
        [AutoInject]
        public DemoService1 DemoService1 { get; }

        [AutoInject]
        public DemoService2 DemoService2 { get; }

        public void Run()
        {
            Console.WriteLine(DemoService1.Name);
            Console.WriteLine(DemoService2.Name);
        }
    }


}

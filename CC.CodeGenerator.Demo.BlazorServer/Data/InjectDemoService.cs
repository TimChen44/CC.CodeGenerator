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
    [AutoInject(typeof(DemoService1))]
    [AutoInject(typeof(DemoService2),"DS2")]
    public partial class DemoService4
    {

        public void Run()
        {
            Console.WriteLine(DemoService1.Name);
            Console.WriteLine(DS2.Name);

        }
    }
}

namespace CC.CodeGenerator.DemoWebAPI
{
    [Service]
    public class ServicesScoped
    {
        public int Demo(int a)
        {
            return a * a;
        }
    }

    [Service(LifeCycle = ELifeCycle.Transient)]
    public class ServicesTransient
    {
    }
}

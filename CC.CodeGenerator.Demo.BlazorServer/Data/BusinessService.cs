
namespace CC.CodeGenerator.Demo.BlazorServer.Data;

[Service] 
public partial class BusinessService
{
    [AutoInject] 
    public WeatherForecastService Weather { get; }
}

//public partial class BusinessService
//{
//    public BusinessService(WeatherForecastService abc)
//    {
//        Abc = abc;
//    }
//}


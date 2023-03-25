using Microsoft.AspNetCore.Mvc;

namespace CC.CodeGenerator.DemoWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class DemoController : ControllerBase
    {
        ServicesScoped ServicesScoped;
        public DemoController(ServicesScoped servicesScoped)
        {
            ServicesScoped = servicesScoped;
        }

        [HttpGet]
        public int Scoped()
        {
            return ServicesScoped.Demo(new Random().Next(0, 100));
        }
    }
}
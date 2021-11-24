using Microsoft.AspNetCore.Authorization;
using Orleans.WebApi.Abstractions;
using Sample.IGrains;

namespace Sample.Gateway.Controllers
{
    //[Authorize]
    [MapToWebApi(typeof(IHelloGrain))]
    public class OrleansWebApiContext
    {
    }
}

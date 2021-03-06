using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.WebApi.Abstractions;
using Sample.IGrains.Models;
using RouteAttribute = Orleans.WebApi.Abstractions.RouteAttribute;

namespace Sample.IGrains
{
    /// <summary>
    /// 转换为Controller测试
    /// </summary>
    [Route("{grainId}/{keyext}/[controller]")]
    public interface IHelloGrain : IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 单对象返回测试
        /// </summary>
        /// <param name="greeting">参数1</param>
        /// <param name="type">参数2</param>
        /// <returns></returns>
        [HttpGet("say")]
        Task<User> SayHello([FromQuery] string greeting = "netcore", [FromQuery] GreetType type = GreetType.Secret);

        /// <summary>
        /// 测试返回列表的方法
        /// </summary>
        /// <param name="greeting">姓名</param>
        /// <param name="types">类型</param>
        /// <param name="kv"></param>
        /// <returns></returns>
        [HttpGet("list")]
        Task<List<User>> SayHelloList([FromQuery] string[] greeting, [FromQuery] GenericValue<GreetType>[] types, [FromQuery] KeyValuePair<GreetType, int> kv);

        /// <summary>
        /// Post测试
        /// </summary>
        /// <param name="greeting"></param>
        /// <returns></returns>
        [HttpPost("/{grainid}/{keyext}/HelloPost")]
        Task<User> SayHello([FromBody] Greet greeting);

        /// <summary>
        /// 测试httpresult
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(User),200)]
        [HttpGet("httpresult")]
        Task<HttpResult> Get([FromQuery]int code);
    }
}

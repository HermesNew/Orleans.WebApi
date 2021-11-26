using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.WebApi.Abstractions;
using Sample.IGrains;
using Sample.IGrains.Models;
using System.Net;

namespace Sample.Grains
{
    public class HelloGrain : Grain, IHelloGrain
    {
        public Task<HttpResult<User>> Get()
        {
            throw new NotImplementedException();
        }

        public Task<HttpResult> Get([FromQuery] int code)
        {
            var user = new User
            {
                Name = $"Hello, {code}!",
                Sex = 18,
                ExtensionValues = new Dictionary<string, object>
            {
                { "1",11},
                { "2",new User{ Name="hello",Sex=99} },
                { "3","33"}
            }
            };
            var result = new HttpResult
            {
                StatusCode = (HttpStatusCode)code,
                Body = user
            };
            if (code == 200)
            {
                result.ResponseHeaders = new Dictionary<string, string> { { "Test-X", "hello" } };
            }
            return Task.FromResult(result);
        }

        public Task<User> SayHello(string greeting, GreetType type = (GreetType)1)
        {
            return Task.FromResult(new User
            {
                Name = $"Hello, {greeting}!",
                Sex = 18,
                ExtensionValues = new Dictionary<string, object>
            {
                { "1",11},
                { "2",new User{ Name="hello",Sex=99} },
                { "3","33"}
            }
            });
        }

        public Task<User> SayHello(Greet greeting)
        {
            return Task.FromResult(new User
            {
                Name = $"Hello, {greeting}!",
                Sex = 18,
                ExtensionValues = new Dictionary<string, object>
            {
                { "1",11},
                { "2",new User{ Name="hello",Sex=99} },
                { "3","33"}
            }
            });
        }

        public Task<List<User>> SayHelloList(string[] greeting, GenericValue<GreetType>[] types, KeyValuePair<GreetType, int> kv)
        {
            var list = new List<User>
            {
                new User
                {
                    Name = $"Hello, {greeting}!",
                    Sex = 18,
                    ExtensionValues = new Dictionary<string, object>
                    {
                        { "1",11},
                        { "2",new User{ Name="hello",Sex=99} },
                        { "3","33"}
                    }
                }
            };
            return Task.FromResult(list);
        }
    }
}

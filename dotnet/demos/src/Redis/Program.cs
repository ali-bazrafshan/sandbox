using StackExchange.Redis;

var muxer = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
var db = muxer.GetDatabase();

// String
await db.StringSetAsync("user:1", "Ali Bazrafshan");
string? result = await db.StringGetAsync("user:1");
Console.WriteLine(result);

// Hash map
var map = new HashEntry[]
{
    new("first-name", "Ali"),
    new("last-name", "Bazrafshan")
};
await db.HashSetAsync("user:2", map);
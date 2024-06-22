using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Runtime.JSON;
using Alachisoft.NCache.Sample.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueriesTestApplication.TestCases.NewApi
{
    internal class JsonPatchBulkTest : NCacheTestBase
    {

        private const int COUNT = 50;

        [SetUp]
        public void Populate()
        {
            cache.Clear();

            Thread.Sleep(500);

            for(int i = 0; i < COUNT; i++)
            {
                cache.Insert($"key-{i}",GetProduct(i));
            }

            Thread.Sleep(500);
        }

        [Test]
        public void Test_AddOperation()
        {
            IJsonPatch patch = new JsonPatch().Add("ExtraField", "Value");

            var keylist = Enumerable.Range(0, COUNT).Select(i => "key-" + i).ToList();

            var result = cache.JsonPatchService.UpdateBulk(keylist, patch);

            Assert.That(result.Count, Is.EqualTo(0));

            var items = cache.GetBulk<JsonObject>(keylist);

            Assert.IsTrue(items.Values.All(jsonObj  => jsonObj.ContainsAttribute("ExtraField") && jsonObj["ExtraField"].Value.Equals("Value")));
        }


    }
}
